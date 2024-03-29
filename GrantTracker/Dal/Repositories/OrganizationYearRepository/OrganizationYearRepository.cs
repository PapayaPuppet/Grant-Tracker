﻿using GrantTracker.Dal.Schema;
using GrantTracker.Utilities;
using GrantTracker.Dal.Repositories.DevRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using GrantTracker.Dal.Schema.Sprocs;
using GrantTracker.Dal.Models.Dto;
using GrantTracker.Dal.Schema.Sprocs.Reporting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GrantTracker.Dal.Repositories.OrganizationYearRepository;

public class OrganizationYearRepository : IOrganizationYearRepository
{
    protected readonly GrantTrackerContext _grantContext;
	protected readonly ClaimsPrincipal _user;
    public OrganizationYearRepository(GrantTrackerContext grantContext, IDevRepository devRepository, IHttpContextAccessor httpContextAccessor)
	{
		_grantContext = grantContext;
        _user = httpContextAccessor.HttpContext.User;
    }

	public async Task<List<OrganizationYear>> GetAsync()
	{
		return await _grantContext
			.OrganizationYears
			.Where(x => _user.IsAdmin() || _user.HomeOrganizationGuids().Contains(x.OrganizationGuid))
			.Include(oy => oy.Organization)
			.Include(oy => oy.Year)
			.ToListAsync();
	}

    public async Task<List<OrganizationYear>> GetAsync(Guid yearGuid)
    {
        return await _grantContext
            .OrganizationYears
            .AsNoTracking()
            .Where(oy => oy.YearGuid == yearGuid)
            .Include(oy => oy.Organization)
            .Include(oy => oy.Year)
            .ToListAsync();
    }

	public async Task<OrganizationYear> GetAsyncBySessionId(Guid sessionGuid)
	{
		return await _grantContext
			.OrganizationYears
			.AsNoTracking()
			.Where(x => _user.IsAdmin() || _user.HomeOrganizationGuids().Contains(x.OrganizationGuid))
			.Where(x => x.Sessions.Any(s => s.SessionGuid == sessionGuid))
			.Include(oy => oy.Organization)
			.Include(oy => oy.Year)
			.FirstAsync();
    }

	//this is silly
    public async Task<Guid> GetGuidAsync(Guid organizationGuid, Guid yearGuid)
	{
			//ensure they are authorized to access the specified organization
		if (!_user.IsAuthorizedToViewOrganization(organizationGuid))
		{
			throw new Exception("The requestor is not authorized to access this resource.");
		}

		return await _grantContext
		.OrganizationYears
		.AsNoTracking()
		.Where(oy => oy.YearGuid == yearGuid && oy.OrganizationGuid == organizationGuid)
		.Select(oy => oy.OrganizationYearGuid)
		.SingleAsync();
	}

	public async Task CreateAsync(List<Organization> organizations, Guid yearGuid)
	{
		List<OrganizationYear> newOrganizationYears = organizations
			.Select(org => new OrganizationYear()
			{
				OrganizationYearGuid = Guid.NewGuid(),
				OrganizationGuid = org.OrganizationGuid,
				YearGuid = yearGuid
			})
			.ToList();

		await _grantContext.OrganizationYears.AddRangeAsync(newOrganizationYears);
		await _grantContext.SaveChangesAsync();
	}

	public async Task DeleteOrganizationYearAsync(Guid OrganizationYearGuid)
	{
		var orgYearToDelete = await _grantContext.OrganizationYears.FindAsync(OrganizationYearGuid);
		_grantContext.Remove(orgYearToDelete);
		await _grantContext.SaveChangesAsync();
	}

    public IQueryable<OrganizationYear> GetOrganizationYear(Guid OrganizationYearGuid)
    {
        return _grantContext.OrganizationYears.Where(x => x.OrganizationYearGuid == OrganizationYearGuid);
    }

    public IQueryable<OrganizationYear> GetOrganizationYear(Guid OrganizationGuid, Guid YearGuid)
    {
        return _grantContext.OrganizationYears.Where(x => x.OrganizationGuid == OrganizationGuid && x.YearGuid == YearGuid).Include(x => x.Sessions).ThenInclude(s => s.AttendanceRecords);
    }
}

public static class OrganizationYearExtensions
{
    public static IIncludableQueryable<OrganizationYear, IEnumerable<Session>> WithSessions(this IQueryable<OrganizationYear> Query)
	{
		return Query.Include(x => x.Sessions);
	}

    public static IIncludableQueryable<OrganizationYear, IEnumerable<AttendanceRecord>> WithAttendanceRecords(this IIncludableQueryable<OrganizationYear, IEnumerable<Session>> Query)
	{
		return Query.ThenInclude(s => s.AttendanceRecords);
    }

    public static IIncludableQueryable<OrganizationYear, IEnumerable<SessionDaySchedule>> WithDaySchedules(this IIncludableQueryable<OrganizationYear, IEnumerable<Session>> Query)
    {
        return Query.ThenInclude(s => s.DaySchedules);
    }

    public static async Task<List<AttendanceRecord>> GetMissingAttendanceRecordsAsync(this IIncludableQueryable<OrganizationYear, IEnumerable<AttendanceRecord>> Query, List<OrganizationBlackoutDate> BlackoutDates)
	{
		List<AttendanceRecord> missingAttendanceRecords = new();
		var organizationYear = await Query.Include(x => x.Sessions).ThenInclude(x => x.DaySchedules).FirstAsync();
		var yesterday = DateOnly.FromDateTime(DateTime.Now).AddDays(-1);

		return organizationYear.Sessions.Select(session =>
		{
			List<AttendanceRecord> missingAttendance = new();
            DateOnly endDateBound = yesterday >= session.LastSession ? session.LastSession : yesterday;

            foreach (var daySchedule in session.DaySchedules)
			{
				int daysUntilNextDoW = ((int)daySchedule.DayOfWeek - (int)session.FirstSession.DayOfWeek + 7) % 7;

				var currentDate = session.FirstSession.AddDays(daysUntilNextDoW);
				while (currentDate <= endDateBound)
				{
					bool attendanceRecordExists = !session.AttendanceRecords.Any(ar => ar.InstanceDate == currentDate);
					bool isABlackoutDate = BlackoutDates.Any(blackout => blackout.Date == currentDate);

                    if (attendanceRecordExists && !isABlackoutDate)
					{
						missingAttendance.Add(new()
						{
							SessionGuid = session.SessionGuid,
							InstanceDate = currentDate
						});
					}

					currentDate = currentDate.AddDays(7);
				}
			}

			return missingAttendance;
		})
		.SelectMany(missingAttendances => missingAttendances)
		.ToList();
	}
}