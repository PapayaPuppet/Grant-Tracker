﻿using GrantTracker.Dal.Models.DTO;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Schema;

namespace GrantTracker.Dal.Repositories.InstructorRepository
{
	public interface IInstructorRepository
	{
		public Task<List<EmployeeDto>> SearchSynergyStaffAsync(string name, string badgeNumber);

		public Task<List<InstructorSchoolYearViewModel>> GetInstructorsAsync(Guid? orgYearGuid = null);

		public Task<Guid> CreateAsync(InstructorDto person, Guid organizationYearGuid);

		public Task CreateAsync(List<InstructorSchoolYear> instructorSchoolYears); //create an instructorOrgYear repo for this

		public Task UpdateInstructorAsync(Guid instructorSchoolYearGuid, InstructorSchoolYearViewModel instructor);
		Task DeleteInstructorAsync(Guid instructorGuid);
    }
}