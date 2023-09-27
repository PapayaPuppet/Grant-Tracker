﻿using GrantTracker.Dal.Models.Dto;
using GrantTracker.Dal.Models.Views;
using GrantTracker.Dal.Schema;

namespace GrantTracker.Dal.Repositories.AttendanceRepository
{
	public interface IAttendanceRepository
	{
		public Task<List<DateOnly>> GetAttendanceDatesAsync(Guid sessionGuid);
		public Task<List<SimpleAttendanceViewModel>> GetAttendanceOverviewAsync(Guid sessionGuid);
		public Task<AttendanceViewModel> GetAttendanceRecordAsync(Guid attendanceGuid);
		Task AddAttendanceAsync(AttendanceRecord Record);
        public Task AddAttendanceAsync(Guid sessionGuid, SessionAttendanceDto sessionAttendance);
		Task<AttendanceRecord> DeleteAttendanceRecordAsync(Guid AttendanceGuid);
        public Task EditAttendanceAsync(Guid attendanceGuid, SessionAttendanceDto sessionAttendance);


		//public Task<List<StudentAttendance>> GetSessionAttendance(Guid sessionGuid, DateOnly date);

		//edit and delete attendance
	}
}