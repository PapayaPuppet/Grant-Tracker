﻿using GrantTracker.Dal.Schema;
using GrantTracker.Utilities;
using Microsoft.EntityFrameworkCore;
using GrantTracker.Dal.Repositories.DevRepository;
using GrantTracker.Dal.Models.Views;

namespace GrantTracker.Dal.Repositories.DropdownRepository
{
	public class DropdownRepository : IDropdownRepository
	{
        private readonly GrantTrackerContext _grantContext;
        private readonly IDevRepository _devRepository;

        private async Task<List<DropdownOption>> GetDropdownOptionsAsync<T>(DbSet<T> table) where T : class, IDropdown
		{
			return await table
				.AsNoTracking()
				.Select(t => new DropdownOption
				{
					Guid = t.Guid,
					Abbreviation = t.Abbreviation,
					Label = t.Label,
					Description = t.Description
				})
				.OrderBy(t => t.Abbreviation ?? t.Label)
			.ToListAsync();
		}

		public DropdownRepository(IDevRepository devRepository, GrantTrackerContext grantContext)
		{
			_devRepository = devRepository;
			_grantContext = grantContext;
		}

		public async Task<SessionDropdownOptions> GetAllSessionDropdownOptionsAsync()
		{
			return new SessionDropdownOptions()
			{
				SessionTypes = await GetDropdownOptionsAsync(_grantContext.SessionTypes),
				Activities = await GetDropdownOptionsAsync(_grantContext.Activities),
				Objectives = await GetDropdownOptionsAsync(_grantContext.Objectives),
				FundingSources = await GetDropdownOptionsAsync(_grantContext.FundingSources),
				OrganizationTypes = await GetDropdownOptionsAsync(_grantContext.OrganizationTypes),
				PartnershipTypes = await GetDropdownOptionsAsync(_grantContext.Partnerships)
			};
		}

		public async Task<DropdownOptions> GetAllDropdownOptionsAsync()
		{
			return new DropdownOptions()
			{
				SessionTypes = await GetDropdownOptionsAsync(_grantContext.SessionTypes),
				Activities = await GetDropdownOptionsAsync(_grantContext.Activities),
				Objectives = await GetDropdownOptionsAsync(_grantContext.Objectives),
				InstructorStatuses = await GetDropdownOptionsAsync(_grantContext.InstructorStatuses),
				FundingSources = await GetDropdownOptionsAsync(_grantContext.FundingSources),
				OrganizationTypes = await GetDropdownOptionsAsync(_grantContext.OrganizationTypes),
				PartnershipTypes = await GetDropdownOptionsAsync(_grantContext.Partnerships),
				Grades = await GetGradesAsync()
			};
		}

		public async Task<List<DropdownOption>> GetInstructorStatusesAsync()
		{
			return await GetDropdownOptionsAsync(_grantContext.InstructorStatuses);
		}

		public async Task<List<DropdownOption>> GetGradesAsync()
		{
			var grades = await _grantContext.LookupDefinitions
				.AsNoTracking()
				.Include(def => def.Values)
				.Where(def => def.Name == "Grade")
				.Select(def => new List<LookupValue>(def.Values))
				.FirstOrDefaultAsync();

			return grades.Select(grade => new DropdownOption()
				{
					Guid = grade.Guid,
					Abbreviation = grade.Value,
					Label = grade.Description,
					Description = ""
				})
				.OrderBy(g => g.Abbreviation, new SemiNumericComparer())
				.ToList();
		}

		//Used solely for dropdown selections
		public async Task<List<OrganizationView>> GetOrganizationsAsync(bool UserIsAdmin, List<Guid> HomeOrganizationGuids)
		{
			//Coordinators only receive their organization, administrators receive the full list
			var organizations = await _grantContext
				.Organizations
				.AsNoTracking()
                .Where(org => UserIsAdmin || HomeOrganizationGuids.Contains(org.OrganizationGuid))
                .OrderBy(org => org.Name)
				.ToListAsync();

			return organizations.Select(OrganizationView.FromDatabase).ToList();
		}

		public async Task<List<YearView>> GetYearsAsync(Guid? organizationGuid = null)
		{
			var years = await _grantContext
				.Organizations
				.AsNoTracking()
				.Where(org => organizationGuid == null || org.OrganizationGuid == organizationGuid)
				.Include(org => org.Years).ThenInclude(oy => oy.Year)
				.SelectMany(org => org.Years)
				.Select(oy => oy.Year)
				.ToListAsync();

			return years.Select(YearView.FromDatabase).ToList();
		}
	}
}