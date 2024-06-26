﻿using GrantTracker.Dal.Models.DTO;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Repositories.DevRepository;
using GrantTracker.Dal.Schema;
using GrantTracker.Dal.SynergySchema;
using GrantTracker.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace GrantTracker.Dal.Repositories.StudentRepository;

public class StudentFilter
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public List<string> SynergyGrades { get; set; }
	public List<string> GrantTrackerGrades { get; set; }
	public string MatricNumber { get; set; }
	public Guid OrganizationYearGuid { get; set; }
}

public class StudentRepository : IStudentRepository
{
	private readonly SynergyEODContext _synergy;
    protected readonly GrantTrackerContext _grantContext;
    protected readonly ClaimsPrincipal _user;

    public StudentRepository(GrantTrackerContext grantContext, IHttpContextAccessor httpContextAccessor, SynergyEODContext synergyContext)
	{
		_synergy = synergyContext;
        _grantContext = grantContext;
        _user = httpContextAccessor.HttpContext.User;
    }

	public async Task<StudentViewModel> CreateIfNotExistsAsync(StudentDTO newStudent)
	{
		var student = await _grantContext
			.Students
			.AsNoTracking()
			.Where(s => s.MatricNumber == newStudent.MatricNumber)
			.FirstOrDefaultAsync();

		if (student == null)
			return await this.CreateAsync(newStudent);

		return StudentViewModel.FromDatabase(student);
	}

	public async Task<StudentViewModel> CreateAsync(StudentDTO newStudent)
	{
		Guid PersonGuid = Guid.NewGuid();

		Student dbStudent = new()
		{
			PersonGuid = PersonGuid,
			FirstName = newStudent.FirstName,
			LastName = newStudent.LastName,
			MatricNumber = newStudent.MatricNumber,
		};

		await _grantContext.Students.AddAsync(dbStudent);
		await _grantContext.SaveChangesAsync();

		return await this.GetAsync(PersonGuid);
	}

	public async Task<StudentViewModel> GetAsync(Guid studentGuid)
	{
		var student = await _grantContext
			.Students
			.FindAsync(studentGuid);

		return StudentViewModel.FromDatabase(student);
	}

	//todo
	//add
	public async Task<List<StudentSchoolYearViewModel>> GetAsync(string name, Guid organizationGuid, Guid yearGuid)
	{
		return await _grantContext.StudentSchoolYears
			.Include(ssy => ssy.Student)
			.Include(ssy => ssy.AttendanceRecords)
			.Where(ssy => ssy.OrganizationYear.OrganizationGuid == organizationGuid && ssy.OrganizationYear.YearGuid == yearGuid)
			.Select(ssy => StudentSchoolYearViewModel.FromDatabase(ssy))
			.ToListAsync();
	}

	public async Task<StudentSchoolYearWithRecordsViewModel> GetAsync(Guid studentYearGuid, Guid organizationYearGuid = new Guid())
	{
		return await _grantContext.StudentSchoolYears
			.AsNoTracking()
			.Where(ssy => ssy.StudentSchoolYearGuid == studentYearGuid)
			.Include(ssy => ssy.Student)
			.Include(ssy => ssy.SessionRegistrations)
			.Include(ssy => ssy.OrganizationYear).ThenInclude(oy => oy.Organization)
			.Include(ssy => ssy.OrganizationYear).ThenInclude(oy => oy.Year)
			.Include(ssy => ssy.SessionRegistrations).ThenInclude(reg => reg.DaySchedule).ThenInclude(day => day.Session)
			.Include(ssy => ssy.SessionRegistrations).ThenInclude(reg => reg.DaySchedule).ThenInclude(day => day.TimeSchedules)
			.Include(ssy => ssy.AttendanceRecords).ThenInclude(sa => sa.AttendanceRecord).ThenInclude(ar => ar.Session)
			.Include(ssy => ssy.AttendanceRecords).ThenInclude(sa => sa.TimeRecords).Select(ssy => StudentSchoolYearWithRecordsViewModel.FromDatabase(ssy))
			.FirstOrDefaultAsync();
	}

	public async Task<StudentSchoolYearWithRecordsViewModel> GetSingleAsync(string matricNumber)
	{
		return await _grantContext.StudentSchoolYears
			.AsNoTracking()
			.Include(ssy => ssy.Student)
			.Include(ssy => ssy.SessionRegistrations)
			.Include(ssy => ssy.AttendanceRecords)
			.Include(ssy => ssy.OrganizationYear).ThenInclude(oy => oy.Organization)
			.Include(ssy => ssy.OrganizationYear).ThenInclude(oy => oy.Year)
			.Include(ssy => ssy.SessionRegistrations).ThenInclude(reg => reg.DaySchedule).ThenInclude(day => day.Session)
			.Include(ssy => ssy.SessionRegistrations).ThenInclude(reg => reg.DaySchedule).ThenInclude(day => day.TimeSchedules)
			.Include(ssy => ssy.AttendanceRecords).ThenInclude(sa => sa.AttendanceRecord).ThenInclude(ar => ar.Session)
			.Include(ssy => ssy.AttendanceRecords).ThenInclude(sa => sa.TimeRecords)
			.Where(ssy => ssy.Student.MatricNumber == matricNumber && ssy.OrganizationYear.Year.IsCurrentSchoolYear == true)
			.Select(ssy => StudentSchoolYearWithRecordsViewModel.FromDatabase(ssy))
			.FirstOrDefaultAsync();
	}



    public async Task<List<StudentSchoolYearViewModel>> SearchSynergyAsync(StudentFilter filter)
    {
        var gtYear = (await _grantContext
            .OrganizationYears
            .Include(oy => oy.Year)
            .FirstOrDefaultAsync(oy => oy.OrganizationYearGuid == filter.OrganizationYearGuid))
            .Year;

		Guid organizationGuid = (await _grantContext
			.OrganizationYears
			.AsNoTracking()
			.FirstAsync(oy => oy.OrganizationYearGuid == filter.OrganizationYearGuid)
			).OrganizationGuid;

        int synergyYear = gtYear.Quarter == Quarter.Summer && gtYear.IsCurrentSchoolYear ? gtYear.SchoolYear - 1 : gtYear.SchoolYear;
        var synergyExtension = gtYear.Quarter == Quarter.Summer && gtYear.IsCurrentSchoolYear ? "S" : "R";
        //&& (_identity.Claim == IdentityClaim.Administrator || ssy.OrganizationYear.OrganizationGu == filter.OrganizationYearGuid)

        var synergyResults = await _synergy.RevSSY
            .AsNoTracking()
            .Include(ssy => ssy.OrganizationYear).ThenInclude(org => org.Organization)
            .Include(ssy => ssy.Year)
            .Include(ssy => ssy.Student).ThenInclude(stu => stu.Person)
            .Where(ssy => ssy.Year.SchoolYear == synergyYear)
            .Where(ssy => ssy.Year.Extension == synergyExtension)
            .Where(ssy => ssy.OrganizationYear.OrganizationGu  == organizationGuid) //no admin exception
            .Where(ssy => string.IsNullOrEmpty(filter.FirstName) || ssy.Student.Person.FirstName.Contains(filter.FirstName))
            .Where(ssy => string.IsNullOrEmpty(filter.LastName) || ssy.Student.Person.LastName.Contains(filter.LastName))
            .Where(ssy => string.IsNullOrEmpty(filter.MatricNumber) || ssy.Student.SisNumber.Contains(filter.MatricNumber))
            .Where(ssy => filter.SynergyGrades.Count == 0 || filter.SynergyGrades.Contains(ssy.Grade))
            .Select(ssy => new StudentSchoolYearViewModel
            {
                Student = new StudentViewModel()
                {
                    MatricNumber = ssy.Student.SisNumber,
                    FirstName = ssy.Student.Person.FirstName,
                    LastName = ssy.Student.Person.LastName
                },
                Grade = GradeDto.FromSynergy(ssy.Grade)
            })
            .Take(100)
            .ToListAsync();


        //&& (_identity.Claim == IdentityClaim.Administrator || ssy.OrganizationYearGuid == filter.OrganizationYearGuid)
        var grantTrackerResults = await _grantContext.StudentSchoolYears
            .AsNoTracking()
            .Include(ssy => ssy.Student)
            .Include(ssy => ssy.OrganizationYear)
            .Where(x => x.OrganizationYearGuid == filter.OrganizationYearGuid)
            .Where(ssy => (
                (string.IsNullOrWhiteSpace(filter.FirstName) || ssy.Student.FirstName.Contains(filter.FirstName))
                && (string.IsNullOrWhiteSpace(filter.LastName) || ssy.Student.LastName.Contains(filter.LastName))
                && (string.IsNullOrWhiteSpace(filter.MatricNumber) || ssy.Student.MatricNumber.Contains(filter.MatricNumber))
                && (filter.GrantTrackerGrades != null || filter.GrantTrackerGrades.Count == 0 || filter.GrantTrackerGrades.Contains(ssy.Grade))))
            .Take(100)
            .ToListAsync();


        //return synergyResults;

        return grantTrackerResults
            .Select(ssy => StudentSchoolYearViewModel.FromDatabase(ssy))
            .Concat(synergyResults)
            .DistinctBy(ssy => new { ssy.Student.FirstName, ssy.Student.LastName, ssy.Student.MatricNumber })
			.OrderBy(ssy => ssy.Student.LastName)
            .ToList();
    }
}