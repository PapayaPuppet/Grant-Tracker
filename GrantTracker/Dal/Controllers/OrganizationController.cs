﻿using GrantTracker.Dal.Repositories.OrganizationRepository;
using GrantTracker.Dal.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Repositories.OrganizationYearRepository;
using GrantTracker.Utilities;
using Microsoft.EntityFrameworkCore;
using GrantTracker.Dal.Models.DTO.Attendance;
using GrantTracker.Dal.Repositories.AttendanceRepository;
using GrantTracker.Dal.Models.DTO.SessionDTO;
using GrantTracker.Dal.Repositories.SessionRepository;

namespace GrantTracker.Dal.Controllers
{
	[ApiController]
	[Authorize(Policy = "Teacher")]
	[Route("")]
	public class OrganizationController : ControllerBase
	{
		private readonly IOrganizationRepository _organizationRepository;
        private readonly IOrganizationYearRepository _organizationYearRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(IOrganizationRepository OrganizationRepository, IOrganizationYearRepository OrganizationYearRepository, IAttendanceRepository attendRepo, ISessionRepository sessionRepo, ILogger<OrganizationController> logger)
		{
			_organizationRepository = OrganizationRepository;
			_organizationYearRepository = OrganizationYearRepository;
            _attendanceRepository = attendRepo;
            _sessionRepository = sessionRepo;
            _logger = logger;
		}

        [HttpGet("organization")]
        [Authorize(Policy = "Administrator")]
        public async Task<ActionResult<List<OrganizationView>>> GetOrganizations()
        {
            try
            {
                var organizations = await _organizationRepository.GetOrganizationsAsync();
                return Ok(organizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpGet("organization/{OrganizationGuid:Guid}")]
		public async Task<ActionResult<OrganizationView>> GetOrganizationYears(Guid OrganizationGuid)
		{
			try
            {
                var organization = await _organizationRepository.GetYearsAsync(OrganizationGuid);
                return Ok(organization);
            }
			catch (Exception ex)
			{
				return StatusCode(500);
			}
		}

		[HttpGet("organizationYear")]
		public async Task<ActionResult<OrganizationYearView>> GetOrganizationYear(Guid organizationYearGuid)
		{
			try
            {
                var organizationYear = await _organizationRepository.GetOrganizationYearAsync(organizationYearGuid);
                return Ok(organizationYear);
            }
			catch (Exception ex)
			{
				return StatusCode(500);
			}
        }

        [HttpGet("organizationYear/{OrganizationYearGuid:Guid}/Attendance/Missing")]
        public async Task<ActionResult<List<AttendanceRecord>>> GetMissingAttendanceRecordsAsync(Guid organizationYearGuid)
        {
            try
            {
                var orgYear = await _organizationYearRepository.GetOrganizationYear(organizationYearGuid).FirstAsync();
                var blackoutDates = await _organizationRepository.GetBlackoutDatesAsync(orgYear.OrganizationGuid);
                var sessionBlackoutDates = await _organizationYearRepository.GetSessionBlackoutDatesAsync(organizationYearGuid);

                var missingRecords = await _organizationYearRepository.GetOrganizationYear(organizationYearGuid)
                    .WithSessions().WithAttendanceRecords()
                    .GetMissingAttendanceRecordsAsync(blackoutDates, sessionBlackoutDates);

                return Ok(missingRecords);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unknown error occured while fetching missing attendance.");
            }
        }

        [HttpGet("organization/{organizationGuid:guid}/attendance/issues")]
        public async Task<ActionResult<List<AttendanceIssueDTO>>> GetAttendanceIssuesAsync(Guid organizationGuid)
        {
            try
            {
                if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(organizationGuid))
                    return Unauthorized();

                return Ok(await _attendanceRepository.GetIssuesAsync(organizationGuid));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unknown error occured while fetching attendance issues.");
            }
        }

        [HttpGet("organizationYear/{organizationYearGuid:Guid}/session/issues")]
        public async Task<ActionResult<List<SessionIssuesDTO>>> GetSessionIssues(Guid organizationYearGuid)
        {
            try
            {
                Guid orgGuid = (await _organizationRepository.GetOrganizationYearAsync(organizationYearGuid)).Organization.Guid;

                if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(orgGuid))
                    return Unauthorized();

                return Ok(await _sessionRepository.GetIssues(organizationYearGuid));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unknown error occured while fetching session issues.");
            }
        }

        [HttpGet("organization/{OrganizationGuid:Guid}/blackout")]
        public async Task<ActionResult<List<OrganizationBlackoutDate>>> GetBlackoutDates(Guid OrganizationGuid)
        {
            try
            {
                if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(OrganizationGuid))
                    return Unauthorized();

                var dates = await _organizationRepository.GetBlackoutDatesAsync(OrganizationGuid);
                return Ok(dates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unknown error occured while fetching blackout dates.");
            }
        }

        [HttpPost("organization/{OrganizationGuid:Guid}/blackout")]
        public async Task<IActionResult> AddBlackoutDate(Guid OrganizationGuid, [FromBody] DateOnly BlackoutDate)
        {
            try
            {
                if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(OrganizationGuid))
                    return Unauthorized();

                if ((await _organizationRepository.GetBlackoutDatesAsync(OrganizationGuid)).Any(x => x.Date.CompareTo(BlackoutDate) == 0))
                    return Conflict();

                await _organizationRepository.AddBlackoutDateAsync(OrganizationGuid, BlackoutDate);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unknown error occured while adding the blackout date {BlackoutDate.ToShortDateString()}.");
            }
        }

        [HttpDelete("organization/{OrganizationGuid:Guid}/blackout/{BlackoutDateGuid:Guid}")]
        public async Task<IActionResult> DeleteBlackoutDate(Guid OrganizationGuid, Guid BlackoutDateGuid)
        {
            try
            {
                if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(OrganizationGuid))
                    return Unauthorized();

                if (!(await _organizationRepository.GetBlackoutDatesAsync(OrganizationGuid)).Any(x => x.Guid == BlackoutDateGuid))
                    return NotFound("A blackout date with the given identifier could not be found.");

                await _organizationRepository.DeleteBlackoutDateAsync(BlackoutDateGuid);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unknown error occured while deleting blackout date.");
            }
        }

        [Authorize(Policy = "Administrator")]
        [HttpDelete("organization/{OrganizationGuid:Guid}")]
        public async Task<IActionResult> DeleteOrganization(Guid OrganizationGuid)
		{
			try
			{
                await _organizationRepository.DeleteOrganizationAsync(OrganizationGuid);
                return NoContent();
            }
			catch (Exception ex)
			{
                return StatusCode(500);
            }
        }

        [Authorize(Policy = "Administrator")]
        [HttpDelete("organizationYear/{OrganizationYearGuid:Guid}")]
        public async Task<IActionResult> DeleteOrganizationYear(Guid OrganizationYearGuid)
        {
            try
            {
				await _organizationYearRepository.DeleteOrganizationYearAsync(OrganizationYearGuid);
				return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(500);
            }
        }

        [HttpGet("organizationYear/{OrganizationYearGuid:Guid}/studentGroup")]
        public async Task<ActionResult<List<StudentGroupView>>> GetStudentGroupsAsync(Guid organizationYearGuid, [FromQuery] string? fields = null)
        {
            try
            {
                var groups = await _organizationYearRepository.GetStudentGroupsAsync(organizationYearGuid, fields);
                return Ok(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(500);
            }
        }

        [HttpGet("organizationYear/{OrganizationYearGuid:Guid}/studentGroup/{studentGroupGuid:guid?}")]
        public async Task<ActionResult<StudentGroupView>> GetStudentGroupAsync(Guid studentGroupGuid, [FromQuery] string? fields = null)
        {
            try
            {
                var group = await _organizationYearRepository.GetStudentGroupAsync(studentGroupGuid, fields);
                return Ok(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(500);
            }
        }

        [HttpPost("organizationYear/{OrganizationYearGuid:Guid}/studentGroup")]
        public async Task<IActionResult> CreateStudentGroupAsync(Guid organizationYearGuid, string name)
        {
            try
            {
                await _organizationYearRepository.CreateStudentGrouping(organizationYearGuid, name);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(500);
            }
        }

        [HttpDelete("organizationYear/studentGroup/{studentGroupGuid:guid}")]
        public async Task<IActionResult> DeleteStudentGroupAsync(Guid studentGroupGuid)
        {
            try
            {
                await _organizationYearRepository.DeleteStudentGroup(studentGroupGuid);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                return StatusCode(500);
            }
        }
    }
}