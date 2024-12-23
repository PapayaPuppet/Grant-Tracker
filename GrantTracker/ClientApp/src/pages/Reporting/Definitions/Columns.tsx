import { DateTimeFormatter, LocalDate } from '@js-joda/core'
import { Locale } from '@js-joda/locale_en-us'
import { Button } from 'react-bootstrap'

import Table, { Column, SortDirection } from 'components/BTable'
import { DateOnly } from 'Models/DateOnly'
import { AttendanceTimeRecordView } from 'Models/InstructorAttendance'
import { TimeOnly } from 'Models/TimeOnly'
import paths from 'utils/routing/paths'
import { Link, NavLink } from 'react-router-dom'

export const studentAttendanceColumns: Column[] = [
	{
	  label: 'Last Name',
	  attributeKey: 'lastName',
	  sortable: true
	},
	{
	  label: 'First Name',
	  attributeKey: 'firstName',
	  sortable: true
	},
	{
	  label: 'Matric Number',
	  attributeKey: 'matricNumber',
	  sortable: true
	},
	{
	  label: 'Grade',
	  attributeKey: 'grade',
	  sortable: true,
	  cellProps: {
		className: 'text-center'
	  }
	},
	{
	  label: 'Total Days',
	  attributeKey: 'totalDays',
	  sortable: true,
	  transform: (days: number) => (
		<div className='text-center'>{Math.floor(days * 100) / 100}</div>
	  ),
	  sortTransform: (days: number) => days,
	  cellProps: {
		className: 'text-center'
	  }
	},
	{
	  label: 'Total Hours',
	  attributeKey: 'totalHours',
	  sortable: true,
	  transform: (hours: number) => (
		<div className='text-center'>{Math.floor(hours * 100) / 100}</div>
	  ),
	  sortTransform: (hours: number) => hours,
	  cellProps: {
		className: 'text-center'
	  }
	},
]

const familyMemberColumns: Column[] = [
	{
	  label: '',
	  attributeKey: 'familyMember',
	  sortable: true,
	  cellProps: {
		class: 'px-2',
		style: {
		  width: '50%'
		}
	  }
	},
	{
	  label: '',
	  attributeKey: 'totalDays',
	  sortable: true,
	  transform: (days) => <div>{Math.floor(days * 100) / 100}</div>,
	  sortTransform: (days) => days,
	  headerProps: {
		class: 'text-center'
	  },
	  cellProps: {
		class: 'text-center px-2',
		style: {
		  width: '25%'
		}
	  }
	},
	{
	  label: '',
	  attributeKey: 'totalHours',
	  sortable: true,
	  transform: (hours) => <div >{Math.floor(hours * 100) / 100}</div>,
	  sortTransform: (hours) => hours,
	  headerProps: {
		class: 'text-center'
	  },
	  cellProps: {
		class: 'text-center px-2',
		style: {
		  width: '25%'
		}
	  }
	},
  ]
  
export const familyAttendanceColumns: Column[] = [
	{
	  label: 'Last Name',
	  attributeKey: 'lastName',
	  sortable: true
	},
	{
	  label: 'First Name',
	  attributeKey: 'firstName',
	  sortable: true
	},
	{
	  label: 'Matric Number',
	  attributeKey: 'matricNumber',
	  sortable: true
	},
	{
	  label: 'Grade',
	  attributeKey: 'grade',
	  sortable: true,
	  cellProps: {
		className: 'text-center'
	  }
	},
	{
	  label: 'Family Member',
	  attributeKey: 'familyAttendance',
	  sortable: false,
	  cellProps: {className: 'p-0'},
	  headerTransform: () => (
		<div className='d-flex p-0 align-items-end' style={{minWidth: 'fit-content'}}>
		  <div className='px-2' style={{width: '50%'}}><b>Family Member</b></div>
		  <div className='px-2 text-center' style={{width: '25%'}}><b>Total Days</b></div>
		  <div className='px-2 text-center' style={{width: '25%'}}><b>Total Hours</b></div>
		</div>
	  ),
	  transform: (familyAttendance) => (
		<Table 
		  columns={familyMemberColumns} 
		  dataset={familyAttendance} 
		  defaultSort={{index: 0, direction: SortDirection.Ascending}}
		  showHeader={false}
		  tableProps={{
			size: 'sm',
			className: 'm-0',
			bordered: false,
			style: {
			  border: '0px'
			}
		  }}
		/>
	  )
	}
  ]


export const activityReportColumns: Column[] = [
	{
	  label: 'Activity Type',
	  attributeKey: 'activity',
	  sortable: true
	},
	{
	  label: 'Total Attendees',
	  attributeKey: 'totalAttendees',
	  sortable: true,
	  cellProps: {
		className: 'text-center'
	  }
	},
	{
	  label: 'Total Hours',
	  attributeKey: 'totalHours',
	  sortable: true,
	  transform: (hours: number) => (
		<div className='text-center'>{Math.floor(hours * 100) / 100}</div>
	  ),
	  sortTransform: (hours: number) => hours,
	}
]


export const siteSessionsColumns = [
	{
		label: 'Name',
		attributeKey: 'sessionName',
		sortable: true
	},
	{
		label: 'Instance Date',
		attributeKey: 'instanceDate',
		sortable: true,
		transform: (date: DateOnly) => DateOnly.toLocalDate(date).toString()
	},
	{
		label: 'Activity\nType',
		attributeKey: 'activityType',
		sortable: true
	},
	{
		label: 'Objective',
		attributeKey: 'objectives',
		sortable: false,
		  transform: (objectives) => <div className='d-flex flex-column align-items-center'>
			  {objectives.map(objective => (<div>{objective}</div>))}
		  </div>
	},
	{
		label: 'Session\nType',
		attributeKey: 'sessionType',
		sortable: true
	},
	{
		label: 'Funding\nSource',
		attributeKey: 'fundingSource',
		sortable: true
	},
	{
		label: 'Partnership\nType',
		attributeKey: 'partnershipType',
		sortable: true
	},
	{
		label: 'Organization\nType',
		attributeKey: 'organizationType',
		sortable: true
	},
	{
		label: 'Attendee\nCount',
		attributeKey: 'attendeeCount',
		sortable: true,
		cellProps: {className: 'text-center'}
	},
	{
		label: 'Grades',
		attributeKey: 'grades',
		sortable: false,
		transform: (grades: string) => grades == '{}' || grades == '{ }' ? 'N/A' : grades.substring(1, grades.length - 1),
		cellProps: {
		class: 'text-center'
		}
	},
	{
		label: 'Instructors',
		attributeKey: 'instructors',
		sortable: false,
		transform: (instructors: any[]) => (
		<div style={{minWidth: 'fit-content'}}>
			{instructors.map((instructor, index) => (
			<>
				<p className='my-0 px-3'  style={{minWidth: 'fit-content'}}>{instructor.firstName && instructor.lastName ? `${instructor.firstName} ${instructor.lastName}` : 'N/A'}</p>
				{index === instructors.length - 1 ? null : <hr className='m-1'/>}
			</>
			))}
		</div>
		)
	}
]

export const summaryOfClassesColumns: Column[] = [
	{
	  label: 'Name',
	  attributeKey: '',
	  transform: (ses: any) => <NavLink to={`/home/admin/sessions/${ses.sessionGuid}?oyGuid=${ses.organizationYearGuid}`}>
		{ses.sessionName}
	  </NavLink>,
	  sortable: true
	},
	{
	  label: 'Activity Type',
	  attributeKey: 'activityType',
	  sortable: true
	},
	{
	  label: 'Funding Source',
	  attributeKey: 'fundingSource',
	  sortable: true
	},
	{
	  label: 'Objective',
	  attributeKey: 'objectives',
	  sortable: false,
		transform: (objectives) => <div className='d-flex flex-column align-items-center'>
			{objectives.map(objective => (<div>{objective}</div>))}
		</div>
	},
	{
	  label: 'Start Date',
	  attributeKey: 'firstSession',
	  sortable: true,
	  transform: (date: DateOnly) => DateOnly.toLocalDate(date).format(DateTimeFormatter.ofPattern('MMMM d, yyyy').withLocale(Locale.ENGLISH))
	},
	{
	  label: 'End Date',
	  attributeKey: 'lastSession',
	  sortable: true,
	  transform: (date: DateOnly) => DateOnly.toLocalDate(date).format(DateTimeFormatter.ofPattern('MMMM d, yyyy').withLocale(Locale.ENGLISH))
	},
	{
	  label: 'Instructors',
	  attributeKey: 'instructors',
	  sortable: false,
	  transform: (instructors: any[]) => (
		<div style={{minWidth: 'fit-content'}}>
		  {instructors.map((instructor, index) => (
			<>
			  <p className='my-0 px-3' style={{minWidth: 'fit-content'}}>{instructor.firstName && instructor.lastName ? `${instructor.firstName} ${instructor.lastName}` : 'N/A'}</p>
			  {index === instructors.length - 1 ? null : <hr className='m-1'/>}
			</>
		  ))}
		</div>
	  )
	},
	{
	  label: 'Weeks to Date',
	  attributeKey: 'weeksToDate',
	  sortable: true,
	  transform: (weeks: number) => (
		<div className='text-center'>{Math.floor(weeks * 100) / 100}</div>
	  ),
	  sortTransform: (weeks: number) => weeks
	},
	{
	  label: 'Avg Hours Per Day',
	  attributeKey: 'avgHoursPerDay',
	  sortable: true,
	  transform: (hours: number) => (
		<div className='text-center'>{Math.floor(hours * 100) / 100}</div>
	  ),
	  sortTransform: (hours: number) => hours
	},
	{
	  label: 'Avg Attendees',
	  attributeKey: 'avgDailyAttendance',
	  sortable: true,
	  transform: (count: number) => (
		<div className='text-center'>{Math.floor(count * 100) / 100}</div>
	  ),
	  sortTransform: (count: number) => count
	}
]


export const programOverviewColumns: Column[] = [
	{
	  label: 'Organization',
	  attributeKey: 'organizationName',
	  sortable: true
	},
	{
	  label: '# of\nRegular Students',
	  attributeKey: 'regularStudentCount',
	  sortable: true,
	  transform: (attendees: number) => (
		<div className='text-center'>{attendees}</div>
	  ),
	  headerProps: {
		className: 'text-center'
	  }
	},
	{
	  label: '# of\nFamily Attendees',
	  attributeKey: 'familyAttendanceCount',
	  sortable: true,
	  transform: (attendees: number) => (
		<div className='text-center'>{attendees}</div>
	  ),
	  headerProps: {
		className: 'text-center'
	  }
	},
	{
		label: 'Student Days\nOffered',
		attributeKey: 'studentDaysOfferedCount',
		sortable: true,
		transform: (count: number) => (
		  <div className='text-center'>{count}</div>
		),
		headerProps: {
		  className: 'text-center'
		}
	},
	{
		label: 'Avg Daily\nUnique Student Attendance',
		attributeKey: 'avgStudentDailyAttendance',
		sortable: true,
		transform: (count: number) => (
		  <div className='text-center'>{Math.floor(count * 100) / 100}</div>
		),
		headerProps: {
		  className: 'text-center'
		},
		sortTransform: (count: number) => count,
	},
	{
	  label: 'Avg Attendance\nDays Per Week',
	  attributeKey: 'avgStudentAttendDaysPerWeek',
	  sortable: true,
	  transform: (days: number) => (
		<div className='text-center'>{Math.floor(days * 100) / 100}</div>
	  ),
	  sortTransform: (days: number) => days,
	},
	{
	  label: 'Avg Attendance\nHours Per Week',
	  attributeKey: 'avgStudentAttendHoursPerWeek',
	  sortable: true,
	  transform: (hours: number) => (
		<div className='text-center'>{Math.floor(hours * 100) / 100}</div>
	  ),
	  sortTransform: (hours: number) => hours,
	}
]


export const staffingColumns: Column[] = [
	{
	  label: 'Last Name',
	  attributeKey: 'lastName',
	  sortable: true
	},
	{
	  label: 'First Name',
	  attributeKey: 'firstName',
	  sortable: true
	},
	{
	  label: '',
	  attributeKey: 'instructorSchoolYearGuid',
	  sortable: false,
	  transform: (guid: string) => (
		<div className='d-flex justify-content-center'>
		  <Button size='sm' href={`/home/admin/staff/${guid}`}>View</Button>
		</div>
	  )
	}
  ]


  export const studentSurveyColumns: Column[] = [
	{
	  label: 'Last Name',
	  attributeKey: 'lastName',
	  sortable: true
	},
	{
	  label: 'First Name',
	  attributeKey: 'firstName',
	  sortable: true
	},
	{
	  label: 'Objective',
	  attributeKey: 'objective',
	  sortable: true
	},
	{
	  label: 'Matric Number',
	  attributeKey: 'matricNumber',
	  sortable: true
	},
	{
	  label: 'Grade',
	  attributeKey: 'grade',
	  sortable: true,
	  cellProps: {
		className: 'text-center'
	  }
	},
	{
	  label: 'Total Days',
	  attributeKey: 'totalDays',
	  sortable: true,
	  transform: (days: number) => (
		<div className='text-center'>{Math.floor(days * 100) / 100}</div>
	  ),
	  sortTransform: (days: number) => days,
	},
	{
	  label: 'Total Hours',
	  attributeKey: 'totalHours',
	  sortable: true,
	  transform: (hours: number) => (
		<div className='text-center'>{Math.floor(hours * 100) / 100}</div>
	  ),
	  sortTransform: (hours: number) => hours,
	}
]


const TimeRecordDisplay = ({timeRecords}: {timeRecords}): JSX.Element => {
	timeRecords = timeRecords
    .sort((first, second) => {
      if (first.startTime.isBefore(second.startTime))
        return -1
      if (first.endTime.isAfter(second.endTime))
        return 1
      return 0
    })
  
	return (
	  <div className='row'>
		{ 
		  timeRecords.map(record => (
			<>
			  <span className='col-6 p-0 text-center'>{record.startTime.format(DateTimeFormatter.ofPattern('h:mm a').withLocale(Locale.ENGLISH))}</span>
			  <span className='col-6 p-0 text-center'>{record.endTime.format(DateTimeFormatter.ofPattern('h:mm a').withLocale(Locale.ENGLISH))}</span>
			</>
		  ))
		}
	  </div>
	)
}


//<Link to={`${paths.Admin.path}/${paths.Admin.Tabs.Sessions.path}/${attendanceCheck.sessionGuid}`}>{attendanceCheck.className}</Link>
export const attendanceCheckColumns: Column[] = [
	{
		label: 'School',
		attributeKey: 'school',
		sortable: false
	},
	{
		label: 'Class',
		attributeKey: '',
		sortable: false,
		transform: (attendanceCheck) => {
			return (
				<Link to={`/home/admin/sessions/${attendanceCheck.sessionGuid}`}>
					{attendanceCheck.className}
				</Link>
			)
		}
	},
	{
		label: 'Weekday',
		attributeKey: 'instanceDate',
		transform: (date: LocalDate) => date.format(DateTimeFormatter.ofPattern('eeee').withLocale(Locale.ENGLISH)),
		sortable: false
	},
	{
		label: 'Date',
		attributeKey: 'instanceDate',
		transform: (date: LocalDate) => date.toString(),
		sortable: false
	},
	{
		label: 'Time Bounds',
		attributeKey: 'timeBounds',
		transform: (bounds) => <TimeRecordDisplay timeRecords={bounds} />,
		sortable: false
	},
	{
		label: 'Instructors',
		attributeKey: 'instructors',
    	transform: (instructors) => <>{instructors.map(i => <div className='d-flex justify-content-around'><span>{i.firstName}</span> &nbsp; <span>{i.lastName}</span></div>)}</>,
		sortable: false
	},
	{
		label: 'Entry?',
		attributeKey: 'attendanceEntry',
		cellProps: { className: 'text-center'},
		transform: (attendanceEntry: boolean) => attendanceEntry ? <span className='text-success fw-bold'>Y</span> : <span className='text-danger fw-bold'>N</span>,
		sortable: true,
		sortTransform: (attendanceEntry: boolean) => attendanceEntry ? "1" : "0"
	}
]


const AttendanceCheckTimeRecordDisplay = ({timeRecords}: {timeRecords}): JSX.Element => {
	timeRecords = timeRecords
		.map(time => ({ startTime: TimeOnly.toLocalTime(time.startTime), endTime: TimeOnly.toLocalTime(time.endTime)}))
		.sort((first, second) => {
			if (first.startTime.isBefore(second.startTime))
				return -1
			if (first.endTime.isAfter(second.endTime))
				return 1
			return 0
		})
  
	return (
	  <div className='row h-100 align-items-center'>
		{ 
		  timeRecords.map(record => (
			<>
			  <span className='col-6 p-0'>{record.startTime.format(DateTimeFormatter.ofPattern('h:mm a').withLocale(Locale.ENGLISH))}</span>
			  <span className='col-6 p-0'>{record.endTime.format(DateTimeFormatter.ofPattern('h:mm a').withLocale(Locale.ENGLISH))}</span>
			</>
		  ))
		}
	  </div>
	)
}




const createPayrollAuditRegisteredInstructorColumns = (filter: string): Column[] => [
	{
		label: 'Name',
		attributeKey: '', 
		headerTransform: () => <th><small>Name</small></th>,
		transform: (record) => (
			<div className={filter.trim() !== '' && `${record.firstName} ${record.lastName}`.toLocaleLowerCase().includes(filter.toLocaleLowerCase()) ? 'text-success fw-bold' : ''}>
				{record.firstName} {record.lastName}
			</div>
		),
		sortable: true
	  }
]

const createPayrollAuditAttendingInstructorColumns = (filter: string): Column[] => [
	{
		label: 'Name',
		attributeKey: '', 
		headerTransform: () => <th><small>Name</small></th>,
		transform: (record) => (
			<div className={filter.trim() !== '' && `${record.firstName} ${record.lastName}`.toLocaleLowerCase().includes(filter.toLocaleLowerCase()) ? 'text-success fw-bold' : ''}>
				{record.firstName} {record.lastName}
			</div>
		),
		sortable: true
	},
	{
		label: 'Sub?',
		attributeKey: 'isSubstitute', 
		headerTransform: () => <th><small>Substitute?</small></th>,
		transform: (isSub: boolean) => isSub ? <span className='text-primary'>Yes</span> : '',
		sortable: true
	},
	{
		label: 'Total Time (m)',
		attributeKey: 'totalTime', 
		headerTransform: () => <th><small>Total Time</small></th>,
		sortable: true
	},
	{
		label: 'Time Records',
		attributeKey: 'timeRecords',
		sortable: false,
		headerTransform: () => (
		<th className=''>
			<div className='d-flex'>
			<div className='w-50'><small>Entered</small></div>
			<div className='w-50'><small>Exited</small></div>
			</div>
		</th>
		),
		transform: (timeRecord: AttendanceTimeRecordView[]) => <AttendanceCheckTimeRecordDisplay timeRecords={timeRecord} />
	}
]

export const createPayrollAuditColumns = (attendingFilter, registeredFilter): Column[] => [
  {
    label: 'School',
    attributeKey: 'school',
    sortable: true
  },
  {
    label: 'Class',
    attributeKey: 'className',
    sortable: true
  },
  {
    label: 'Date',
    attributeKey: 'instanceDate',
    transform: (date: DateOnly) => DateOnly.toLocalDate(date).toString(),
    sortable: true
  },
  {
	label: 'Registered Instructors',
	attributeKey: 'registeredInstructors',
	sortable: false,
    transform: (records) => {
      return (
        <Table columns={createPayrollAuditRegisteredInstructorColumns(registeredFilter)} dataset={records} bordered={false} className='m-0 border-0' />
      )
    },
    cellProps: {
      className: 'p-0 text-center'
    }
  },
  {
    label: 'Attending Instructors',
    attributeKey: 'attendingInstructorRecords',
    sortable: false,
    transform: (records) => {
      return (
        <Table columns={createPayrollAuditAttendingInstructorColumns(attendingFilter)} dataset={records} bordered={false} className='m-0 border-0' />
      )
    },
    cellProps: {
      className: 'p-0 text-center'
    }
  }
]