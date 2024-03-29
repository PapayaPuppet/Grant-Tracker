﻿using GrantTracker.Dal.Models.Dto;
using GrantTracker.Dal.Repositories.InstructorRepository;
using GrantTracker.Dal.Repositories.InstructorSchoolYearRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Schema;

//This is where Instructors are grabbed from Synergy and added to Grant Tracker
namespace GrantTracker.Dal.Controllers;

[ApiController]
[Authorize(Policy = "AnyAuthorizedUser")]
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
		var instructorSchoolYear = await _instructorSchoolYearRepository.GetInstructorSchoolYearAsync(instructorSchoolYearGuid);
		return Ok(instructorSchoolYear);
	}

	[HttpGet("search")]
	public async Task<ActionResult<List<EmployeeDto>>> SearchAllDistrictEmployees(string name = "", string badgeNumber = "")
	{
		return await _instructorRepository.SearchSynergyStaffAsync(name, badgeNumber);
	}

	[HttpPost("add")] //change this to new or something...
	public async Task<ActionResult> AddInstructor(InstructorDto instructor, Guid organizationYearGuid)
	{
		try
            {
                Guid instructorSchoolYearGuid = await _instructorRepository.CreateAsync(instructor, organizationYearGuid);
                return Ok(instructorSchoolYearGuid);
            }
		catch (Exception ex)
		{
			_logger.LogError("Unhandled", ex);
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
		await _instructorRepository.UpdateInstructorAsync(instructorSchoolYearGuid, instructorSchoolYear);
		return NoContent();
	}
}