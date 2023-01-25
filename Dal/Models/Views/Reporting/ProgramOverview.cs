﻿namespace GrantTracker.Dal.Models.Views.Reporting;

public class ProgramViewModel
{
	public string OrganizationName { get; set; }
	public int RegularStudentCount { get; set; }
	public int FamilyAttendanceCount { get; set; }
	public int StudentDaysOfferedCount { get; set; }
	public double AvgStudentAttendHoursPerWeek { get; set; }
	public double AvgStudentAttendDaysPerWeek { get; set; }
}