﻿using Castle.Core.Internal;
using GrantTracker.Dal.Models.Dto;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Repositories.AttendanceRepository;
using GrantTracker.Dal.Repositories.SessionRepository;
using GrantTracker.Dal.Repositories.StudentRepository;
using GrantTracker.Dal.Repositories.StudentSchoolYearRepository;
using GrantTracker.Dal.Repositories.InstructorRepository;
using GrantTracker.Dal.Repositories.InstructorSchoolYearRepository;
using GrantTracker.Dal.Repositories.OrganizationYearRepository;
using GrantTracker.Dal.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrantTracker.Dal.Repositories.OrganizationRepository;
using GrantTracker.Utilities;

namespace GrantTracker.Dal.Controllers;

[ApiController]
[Authorize(Policy = "AnyAuthorizedUser")]
[Route("session")]
public class SessionController : ControllerBase
{
	private readonly ISessionRepository _sessionRepository;
	private readonly IStudentRepository _studentRepository;
	private readonly IStudentSchoolYearRepository _studentSchoolYearRepository;
	private readonly IAttendanceRepository _attendanceRepository;
	private readonly IInstructorRepository _instructorRepository;
	private readonly IInstructorSchoolYearRepository _instructorSchoolYearRepository;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IOrganizationYearRepository _organizationYearRepository;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionRepository sessionRepository, IStudentRepository studentRepository, IStudentSchoolYearRepository studentSchoolYearRepository, 
		IAttendanceRepository attendanceRepository, IInstructorRepository instructorRepository, IOrganizationYearRepository organizationYearRepository, 
		IOrganizationRepository organizationRepository, IInstructorSchoolYearRepository instructorSchoolYearRepository
		, ILogger<SessionController> logger)
	{
		_sessionRepository = sessionRepository;
		_studentRepository = studentRepository;
		_studentSchoolYearRepository = studentSchoolYearRepository;
		_attendanceRepository = attendanceRepository;
		_instructorRepository = instructorRepository;
		_instructorSchoolYearRepository = instructorSchoolYearRepository;
		_organizationRepository = organizationRepository;
		_organizationYearRepository = organizationYearRepository;
		_logger = logger;
	}

	#region Get

	[HttpGet("")]
	public async Task<ActionResult<List<SimpleSessionView>>> GetAsync(string sessionName, [FromQuery(Name = "grades[]")] Guid[] grades, Guid organizationGuid, Guid yearGuid)
	{
		var organizationYearGuid = await _organizationYearRepository.GetGuidAsync(organizationGuid, yearGuid);
		var sessions = await _sessionRepository.GetAsync(sessionName, organizationYearGuid);
		return Ok(sessions);
	}

	//Users must be able to receive single sessions to view, edit, fill out attendance, and add students.
	[HttpGet("{sessionGuid:guid}")]
	public async Task<ActionResult<SessionView>> Get(Guid sessionGuid)
	{
		var session = await _sessionRepository.GetAsync(sessionGuid);
		session.Instructors = session.Instructors.Select(i =>
		{
			i.EnrollmentRecords = null;
			i.AttendanceRecords = null;
			return i;
		})
		.ToList();
		return Ok(session);
	}

	[HttpGet("{sessionGuid:guid}/status")]
	public async Task<ActionResult<bool>> GetSessionStatus(Guid sessionGuid)
	{
		return Ok(await _sessionRepository.GetStatusAsync(sessionGuid));
	}

	[HttpGet("{sessionGuid:Guid}/registration")]
	public async Task<ActionResult<List<StudentRegistrationView>>> GetStudents(Guid sessionGuid, int dayOfWeek = -1)
	{
		var students = await _sessionRepository.GetStudentRegistrationsAsync(sessionGuid, dayOfWeek);
		return Ok(students);
	}

	////
	//Get - Attendance Overview
	//

	[HttpGet("{sessionGuid}/attendance")]
	public async Task<ActionResult<SimpleAttendanceViewModel>> GetAttendanceOverview(Guid sessionGuid)
	{
		var simpleAttendanceViews = await _attendanceRepository.GetAttendanceOverviewAsync(sessionGuid);
		return Ok(simpleAttendanceViews);
	}

	////
	//Get - Full Attendance Details
	//

	[HttpGet("{sessionGuid:guid}/attendance/{attendanceGuid:guid}")]
	public async Task<ActionResult<AttendanceViewModel>> GetAttendanceRecords(Guid sessionGuid, Guid attendanceGuid)
	{
		var attendanceRecord = await _attendanceRepository.GetAttendanceRecordAsync(attendanceGuid);

		return Ok(attendanceRecord);
	}

	[HttpGet("{sessionGuid:guid}/attendance/openDates")]
	public async Task<ActionResult<List<DateOnly>>> GetOpenAttendanceDates(Guid sessionGuid, DayOfWeek dayOfWeek)
	{
		try
		{
			var session = await _sessionRepository.GetAsync(sessionGuid);

			if (!User.IsAdmin() && !User.HomeOrganizationGuids().Contains(session.OrganizationYear.Organization.Guid))
				return Unauthorized();

			DateOnly startDate = session.FirstSession;
			DateOnly endDate = session.LastSession;

			List<DateOnly> GetWeekdaysBetween(DayOfWeek dayOfWeek, DateOnly startDate, DateOnly endDate)
			{
				List<DateOnly> openDates = new();

				//check out yield return as an iterable
				DateOnly currentDate;
				if (startDate.DayOfWeek < dayOfWeek)
				{
					currentDate = startDate.AddDays(dayOfWeek - startDate.DayOfWeek);
				}
				else if (startDate.DayOfWeek > dayOfWeek)
				{
					currentDate = startDate.AddDays((int)DayOfWeek.Saturday - (int)startDate.DayOfWeek + (int)dayOfWeek + 1);
				}
				else
				{
					currentDate = startDate;
				}

				while (currentDate <= endDate)
				{
					openDates.Add(currentDate);
					currentDate = currentDate.AddDays(7);
				}

				openDates.Sort();
				return openDates;
			}

			IEnumerable<DateOnly> attendanceDates = await _attendanceRepository.GetAttendanceDatesAsync(sessionGuid);
			IEnumerable<DateOnly> blackoutDates = (await _organizationRepository.GetBlackoutDatesAsync(session.OrganizationYear.Organization.Guid)).Select(x => x.Date);

			var openDates = GetWeekdaysBetween(dayOfWeek, startDate, endDate)
				.Except(blackoutDates)
				.Except(attendanceDates)
				.ToList(); //filter coordinator-defined blackout dates and already existing attendance;

			return Ok(openDates);
		}
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(GetOpenAttendanceDates));
            return StatusCode(500);
        }
    }


	#endregion Get

	#region Post

	[HttpPost("")]
	public async Task<ActionResult<Guid>> AddSession([FromBody] FormSessionDto session)
	{
		await _sessionRepository.AddAsync(session);
		return Created("session", session);
	}


	[HttpPost("{sessionGuid:guid}/registration")]
	public async Task<ActionResult<List<SessionErrorMessage>>> RegisterStudent(Guid sessionGuid, [FromBody] StudentRegistrationDto newRegistration)
	{
		//Fetch basic session details
		var targetSession = await _sessionRepository.GetAsync(sessionGuid);

		if (targetSession == null) 
			return BadRequest("SessionGuid is invalid: " + sessionGuid);

		var targetStudent = await _studentRepository.CreateIfNotExistsAsync(newRegistration.Student);
		var targetStudentSchoolYear = await _studentSchoolYearRepository.CreateIfNotExistsAsync(targetStudent.Guid, targetSession.OrganizationYear.Guid);

		await _sessionRepository.RegisterStudentAsync(sessionGuid, newRegistration.DayScheduleGuids, targetStudentSchoolYear.Guid);

		return Created($"{sessionGuid}/registration", targetStudentSchoolYear.Guid);
	}



	[HttpPost("{destinationSessionGuid:guid}/registration/copy")]
	public async Task<ActionResult<List<string>>> CopyStudentRegistrations(Guid destinationSessionGuid, [FromBody] List<Guid> studentSchoolYearGuids)
	{
		//the student school years are assured to exist, otherwise they wouldn't have been registered already
		List<Guid> scheduleGuids = (await _sessionRepository.GetAsync(destinationSessionGuid)).DaySchedules.Select(ds => ds.DayScheduleGuid).ToList();

		//we cannot make a list of Tasks without making the requisite repositories transient. Otherwise, thread-unsafe operation occurs.
		foreach (var studentSchoolYearGuid in studentSchoolYearGuids)
		{
			await _sessionRepository.RegisterStudentAsync(destinationSessionGuid, scheduleGuids, studentSchoolYearGuid);
		}

		return Created($"{destinationSessionGuid}/registration/copy", studentSchoolYearGuids);
	}


	private async Task<List<InstructorAttendanceDto>> ConcatSubstituteRecordsAsync(List<InstructorAttendanceDto> instructorAttendance, List<SubstituteAttendanceDto> substituteAttendance, Guid OrganizationYearGuid)
	{
		List<InstructorAttendanceDto> recordsToAdd = new();
        foreach (var substituteRecord in substituteAttendance)
        {
            Guid instructorSchoolYearGuid = substituteRecord.InstructorSchoolYearGuid;

            //this needs to check the organization year specified by the session, not just try and find one for her
            bool instructorSchoolYearGuidExists = instructorSchoolYearGuid != Guid.Empty;

            if (!instructorSchoolYearGuidExists)
            {
                bool substituteHasBadgeNumber = !substituteRecord.Substitute.BadgeNumber.IsNullOrEmpty();
                if (substituteHasBadgeNumber)
                {
                    instructorSchoolYearGuid = await _instructorRepository.CreateAsync(substituteRecord.Substitute, OrganizationYearGuid);
                }

                if (instructorSchoolYearGuid == Guid.Empty)
                    instructorSchoolYearGuid = await _instructorRepository.CreateAsync(substituteRecord.Substitute, OrganizationYearGuid);
            }

            InstructorAttendanceDto instructorAttendanceRecord = new()
            {
                InstructorSchoolYearGuid = instructorSchoolYearGuid,
				IsSubstitute = true,
                Attendance = substituteRecord.Attendance
            };

            recordsToAdd.Add(instructorAttendanceRecord);
        }

        instructorAttendance.AddRange(recordsToAdd);
        return instructorAttendance.Distinct().ToList(); //ensure the substitutes haven't introduced a duplication error
    }

	private async Task<List<StudentAttendanceDto>> ValidateOrCreateStudentsFromAttendanceAsync(Guid OrganizationYearGuid, List<StudentAttendanceDto> StudentAttendance)
	{
		foreach(var record in StudentAttendance)
        {
            if (record.StudentGuid == default)
			{
                var student = await _studentRepository.CreateIfNotExistsAsync(record.Student);
                record.StudentGuid = student.Guid;
            }

			if (record.StudentSchoolYearGuid == default)
            {
                var studentSchoolYear = await _studentSchoolYearRepository.CreateIfNotExistsAsync(record.StudentGuid, OrganizationYearGuid);
				record.StudentSchoolYearGuid = studentSchoolYear.Guid;
            }
        }

		return StudentAttendance;
    }
	
	[HttpPost("{sessionGuid:guid}/attendance")]
	public async Task<IActionResult> SubmitAttendance(Guid sessionGuid, [FromBody] SessionAttendanceDto sessionAttendance)
	{
		try
		{
            //A lot of this could be moved to the repository side
            var organizationYearGuid = (await _sessionRepository.GetAsync(sessionGuid)).OrganizationYear.Guid;

            if (sessionAttendance.SessionGuid == Guid.Empty)
                throw new ArgumentException("SessionGuid cannot be empty.", nameof(sessionAttendance));

            sessionAttendance.InstructorRecords = await ConcatSubstituteRecordsAsync(sessionAttendance.InstructorRecords, sessionAttendance.SubstituteRecords, organizationYearGuid);
            sessionAttendance.StudentRecords = await ValidateOrCreateStudentsFromAttendanceAsync(organizationYearGuid, sessionAttendance.StudentRecords);

            var errorList = await _sessionRepository.ValidateStudentAttendanceAsync(sessionAttendance.Date, sessionAttendance.StudentRecords);
            sessionAttendance.StudentRecords = sessionAttendance.StudentRecords.ExceptBy(errorList.Select(x => x.StudentSchoolYearGuid), record => record.StudentSchoolYearGuid).ToList();

            await _attendanceRepository.AddAttendanceAsync(sessionGuid, sessionAttendance);

            if (errorList.Count > 0)
                return Conflict(errorList.Select(x => x.Error));

            return NoContent();
        }
		catch (Exception ex)
        {
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(SubmitAttendance));
            return StatusCode(500);
		}
	}

	#endregion Post

	#region Patch
	[HttpPatch("")]
	public async Task<ActionResult<Guid>> EditSession([FromBody] FormSessionDto session)
	{
		try
        {
            await _sessionRepository.UpdateAsync(session);
            return Ok(session.Guid);
        }
		catch (Exception ex)
		{
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(EditSession));
            return StatusCode(500);
        }
	}

	//We need a builder pattern for grabbing most variable info..
	//this endpoint needs to be secured and ensure the requestor can view the resources before doing any actions...
	[HttpPatch("attendance")]
	public async Task<IActionResult> EditAttendance(Guid attendanceGuid, [FromBody] SessionAttendanceDto sessionAttendance)
	{
		var existingAttendanceRecord = await _attendanceRepository.DeleteAttendanceRecordAsync(attendanceGuid);

		try
		{
            var organizationYearGuid = (await _sessionRepository.GetAsync(sessionAttendance.SessionGuid)).OrganizationYear.Guid;
            //instructors don't need validation as they are ensured to exist in the given orgYear
            //Ensure substitutes have all information required and are added to the session's organizationYear
            List<InstructorAttendanceDto> substituteAttendance = new();
            foreach (var substituteRecord in sessionAttendance.SubstituteRecords)
            {
                Guid instructorSchoolYearGuid = substituteRecord.InstructorSchoolYearGuid;

                bool substituteHasBadgeNumber = !substituteRecord.Substitute.BadgeNumber.IsNullOrEmpty();
                if (substituteHasBadgeNumber)
                    instructorSchoolYearGuid = (await _instructorSchoolYearRepository.GetInstructorSchoolYearAsync(substituteRecord.Substitute.BadgeNumber, organizationYearGuid))?.Guid ?? Guid.Empty;

                if (instructorSchoolYearGuid == Guid.Empty)
                    instructorSchoolYearGuid = await _instructorRepository.CreateAsync(substituteRecord.Substitute, organizationYearGuid);

                InstructorAttendanceDto instructorAttendanceRecord = new()
                {
                    InstructorSchoolYearGuid = instructorSchoolYearGuid,
                    IsSubstitute = true,
                    Attendance = substituteRecord.Attendance
                };

                substituteAttendance.Add(instructorAttendanceRecord);
            }

            //reassign with verified instructor records
            sessionAttendance.InstructorRecords.AddRange(substituteAttendance);
            sessionAttendance.InstructorRecords = sessionAttendance.InstructorRecords.Distinct().ToList(); //ensure the substitutes haven't introduced a duplication error
            sessionAttendance.StudentRecords = await ValidateOrCreateStudentsFromAttendanceAsync(organizationYearGuid, sessionAttendance.StudentRecords);

            var errorList = await _sessionRepository.ValidateStudentAttendanceAsync(sessionAttendance.Date, sessionAttendance.StudentRecords);
            sessionAttendance.StudentRecords = sessionAttendance.StudentRecords.ExceptBy(errorList.Select(x => x.StudentSchoolYearGuid), record => record.StudentSchoolYearGuid).ToList();

            await _attendanceRepository.EditAttendanceAsync(attendanceGuid, sessionAttendance);

            if (errorList.Count > 0)
			{
				await _attendanceRepository.AddAttendanceAsync(existingAttendanceRecord);
                return Conflict(errorList.Select(x => x.Error));
            }

            return Ok();
        }
		catch (Exception ex)
		{
			await _attendanceRepository.AddAttendanceAsync(existingAttendanceRecord);
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(EditAttendance));
            return StatusCode(500, "An unexpected error occurred while editing attendance. No changes were made.");
		}
	}

	#endregion Patch

	#region Delete

	[HttpDelete("{sessionGuid:Guid}")]
	public async Task<IActionResult> DeleteSession(Guid sessionGuid)
	{
		try
		{
            if (await _sessionRepository.GetStatusAsync(sessionGuid))
            {
                await _sessionRepository.DeleteAsync(sessionGuid);
            }
            return NoContent();
        }
		catch (Exception ex)
        {
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(DeleteSession));
            return StatusCode(500);
        }
	}

	[HttpDelete("registration")]
	public async Task<IActionResult> UnregisterStudent(Guid studentSchoolYearGuid, [FromQuery(Name = "dayScheduleGuid[]")] Guid[] dayScheduleGuids)
	{
		try
		{
            await _sessionRepository.RemoveStudentAsync(studentSchoolYearGuid, dayScheduleGuids.ToList());
            return Ok();
        }
		catch (Exception ex)
		{
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(UnregisterStudent));
            return StatusCode(500);
        }
	}

	[HttpDelete("attendance")]
	public async Task<IActionResult> DeleteAttendanceRecord(Guid attendanceGuid)
	{
		try
		{
            await _sessionRepository.RemoveAttendanceRecordAsync(attendanceGuid);
            return Ok();
        }
		catch (Exception ex)
		{
            _logger.LogError(ex, "{Function} - An unhandled error occured.", nameof(DeleteAttendanceRecord));
            return StatusCode(500);
        }
	}
	#endregion Delete
}