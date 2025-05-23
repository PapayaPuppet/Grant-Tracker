﻿using GrantTracker.Dal.Models.DTO;
using GrantTracker.Dal.Repositories.InstructorRepository;
using GrantTracker.Dal.Repositories.InstructorSchoolYearRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Schema;

//This is where Instructors are grabbed from Synergy and added to Grant Tracker
namespace GrantTracker.Dal.Controllers;

[ApiController]
[Authorize(Policy = "Teacher")]
[Route("instructor")]
public class InstructorController : ControllerBase
{
	private readonly IInstructorRepository _instructorRepository;
	private readonly IInstructorSchoolYearRepository _instructorSchoolYearRepository;
    private readonly ILogger<IInstructorRepository> _logger;

    public InstructorController(IInstructorRepository instructorRepository, IInstructorSchoolYearRepository instructorSchoolYearRepository, ILogger<IInstructorRepository> logger)
	{
		_instructorRepository = instructorRepository;
		_instructorSchoolYearRepository = instructorSchoolYearRepository;
		_logger = logger;
	}

	[HttpGet("")]
	public async Task<ActionResult<List<InstructorSchoolYearViewModel>>> GetInstructors(Guid orgYearGuid)
	{
		var instructors = await _instructorRepository.GetInstructorsAsync(orgYearGuid);
		return Ok(instructors);
	}

	//should we in some way umbrella this under Organization/OrganizationYear control?
	[HttpGet("{instructorSchoolYearGuid:guid}")]
	public async Task<ActionResult<InstructorSchoolYearViewModel>> GetInstructorSchoolYear(Guid instructorSchoolYearGuid)
	{
		var instructorSchoolYear = await _instructorSchoolYearRepository.GetAsync(instructorSchoolYearGuid);
		return Ok(instructorSchoolYear);
	}

	[HttpGet("search")]
	public async Task<ActionResult<List<EmployeeDto>>> SearchAllDistrictEmployees(string name = "", string badgeNumber = "")
	{
		return await _instructorRepository.SearchSynergyStaffAsync(name, badgeNumber);
	}

	[HttpPost("add")] 
	public async Task<ActionResult> AddInstructor(InstructorDto instructor, Guid organizationYearGuid)
	{
		try
        {
            Guid instructorSchoolYearGuid = await _instructorRepository.CreateAsync(instructor, organizationYearGuid);
            return Ok(instructorSchoolYearGuid);
        }
		catch (Exception ex)
		{
			_logger.LogError(ex, "");
			return StatusCode(500);
		}
	}

	[HttpPost("/instructorSchoolYear/{instructorSchoolYearGuid:guid}/studentGroup/{studentGroupGuid:guid}")]
	public async Task<IActionResult> AttachStudentGroup(Guid instructorSchoolYearGuid, Guid studentGroupGuid)
	{
		try
		{
			await _instructorSchoolYearRepository.AttachStudentGroupAsync(instructorSchoolYearGuid, studentGroupGuid);
			return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
            return StatusCode(500);
        }
    }

    [HttpDelete("/instructorSchoolYear/{instructorSchoolYearGuid:guid}/studentGroup/{studentGroupGuid:guid}")]
    public async Task<IActionResult> DetachStudentGroup(Guid instructorSchoolYearGuid, Guid studentGroupGuid)
    {
        try
        {
            await _instructorSchoolYearRepository.DetachStudentGroupAsync(instructorSchoolYearGuid, studentGroupGuid);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
            return StatusCode(500);
        }
    }

    public class PatchStatusProps
	{
		public Guid InstructorSchoolYearGuid { get; set; }
		public Guid StatusGuid { get; set; }
	}

	[HttpPatch("{instructorSchoolYearGuid:guid}/status")]
	public async Task<IActionResult> AlterInstructorStatus(Guid instructorSchoolYearGuid, [FromBody] InstructorSchoolYearViewModel instructorSchoolYear)
    {
        try
        {
            await _instructorRepository.UpdateInstructorAsync(instructorSchoolYearGuid, instructorSchoolYear);
		    return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
            return StatusCode(500);
        }
    }

    [HttpPatch("{instructorSchoolYearGuid:guid}/deletion")]
    public async Task<IActionResult> ToggleInstructorDeletion(Guid instructorSchoolYearGuid)
    {
        try
        {
            await _instructorSchoolYearRepository.ToggleDeletionAsync(instructorSchoolYearGuid);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "");
            return StatusCode(500);
        }
    }
}