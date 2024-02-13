import { DateTimeFormatter, LocalDate, LocalTime } from "@js-joda/core";
import { Locale } from "@js-joda/locale_en-us";

import { useQuery } from "@tanstack/react-query";
import { DateOnly } from "Models/DateOnly";
import { Session, SessionDomain, SessionView } from "Models/Session";
import React, { ReactElement, useEffect, useReducer, useState } from "react"
import { useSearchParams } from "react-router-dom";
import { TimeInput } from "components/TimeRangeSelector";
import { DayOfWeek } from "Models/DayOfWeek";
import { TimeScheduleView } from "Models/TimeSchedule";
import { AttendanceForm, AttendanceForm as AttendanceFormState, ReducerAction, reducer } from './state'
import { StudentRegistration, StudentRegistrationDomain } from "Models/StudentRegistration";

import { InstructorAttendance } from './InstructorAttendance'
import { StudentAttendance } from './StudentAttendance'
import { AttendanceSummary } from './Summary'
import api from "utils/api";


enum FormState {
	DateTimeSelect = 0,
	AttendanceRecords = 1,
	Review = 2
}

export default (): React.ReactElement => {
	const [searchParams] = useSearchParams();
	const sessionGuid = searchParams.get('session')
	const attendanceGuid = searchParams.get('attendanceId')

	const [formState, setFormState] = useState<FormState>(FormState.DateTimeSelect)

	const [date, setDate] = useState<LocalDate | undefined>(undefined)
	const [timeSchedules, setTimeSchedules] = useState<TimeScheduleView[] | undefined>(undefined)
	const [attendanceState, dispatch] = useReducer(reducer, {
		defaultTimeSchedule: [],
		studentRecords: [],
		instructorRecords: []
	})

	const { isPending: fetchingSession, data: session, error: sessionError } = useQuery({
		queryKey: [`session/${sessionGuid}`],
		select: (session: SessionDomain) => Session.toViewModel(session),
		retry: false,
		staleTime: Infinity
	})

	const { isPending: fetchingStudentRegs, data: studentRegs, error: studentRegError } = useQuery({
		queryKey: [`session/${sessionGuid}/registration?dayOfWeek=${date?.dayOfWeek().value()}`],
		select: (regs: StudentRegistrationDomain[]) => regs.map(reg =>StudentRegistration.toViewModel(reg)),
		enabled: !!date,
		retry: false,
		staleTime: Infinity
	})

	const { data: priorAttendance } = useQuery({
		queryKey: [`session/${sessionGuid}/attendance/${attendanceGuid}`],
		enabled: !!attendanceGuid,
		retry: false,
		staleTime: Infinity
	})

	function handleFormStateChange(newState: FormState) {
		const prevState: FormState = formState

		switch (newState) {
			case FormState.AttendanceRecords:
				if (prevState === FormState.DateTimeSelect) 
				{
					if (timeSchedules) {
						dispatch({ type: 'setDefaultTimeSchedules', payload: timeSchedules })
					}

					if (session && studentRegs && timeSchedules) {
						if (!attendanceGuid) {
							dispatch({ type: 'populateInstructors', payload: { instructors: session?.instructors, times: timeSchedules }})
							dispatch({ type: 'populateStudents', payload: { students: studentRegs.map(reg => reg.studentSchoolYear), times: timeSchedules }})
						}
						else { 
							dispatch({ type: 'populateExistingRecords', payload: { instructorAttendance: priorAttendance.instructorAttendanceRecords, studentAttendance: priorAttendance.studentAttendanceRecords }})
						}
					}
				}

				break;
		}

		setFormState(newState);
	}

	function renderForm(): React.ReactElement {
		switch (formState) {
			case FormState.DateTimeSelect:
				return (
					<DateTimeSelection 
						session={session}
						date={date}
						onDateChange={setDate}
						times={timeSchedules}
						onTimeChange={setTimeSchedules}
						progressFormState={() => handleFormStateChange(FormState.AttendanceRecords)}
						isNewAttendance={attendanceGuid === null}
						originalAttendDate={priorAttendance ? DateOnly.toLocalDate(priorAttendance.instanceDate) : undefined}
					/>
				);
			case FormState.AttendanceRecords:
				return (
					<AttendanceForm 
						session={session!}
						attendanceGuid={attendanceGuid}
						date={date!}
						state={attendanceState}
						dispatch={dispatch}
					/>
				)
			default:
				return <></>
		}
	}

	if (!session)
		return <span>Loading...</span>

	const subHeading: ReactElement | null = priorAttendance?.instanceDate 
		? <h5 className='text-secondary'>Original attendance date - {DateOnly.toLocalDate(priorAttendance.instanceDate).format(DateTimeFormatter.ofPattern('eeee, MMMM dd').withLocale(Locale.ENGLISH))}</h5>
		: null;

	return (
		<div className='w-100'>
			<h3>{session.name}</h3>
			{subHeading}

			<main>
				{renderForm()}
			</main>
		</div>
	)
}

interface AttendanceFormProps {
	session: SessionView
	attendanceGuid: string | null
	date: LocalDate
	state: AttendanceForm
	dispatch: React.Dispatch<ReducerAction>
}



const AttendanceForm = ({session, attendanceGuid, date, state, dispatch}: AttendanceFormProps): ReactElement => {
	return (
		<div>
			<h5 className='text-secondary'>Attendance for {date.format(DateTimeFormatter.ofPattern('eeee, MMMM dd').withLocale(Locale.ENGLISH))}</h5>
			<hr />

			<section>
            	<h5>Instructors</h5>

				<InstructorAttendance orgYearGuid={session.organizationYear.guid} state={state} dispatch={dispatch} />
			</section>

			<section>
            	<h5>Students</h5>

				<StudentAttendance orgYearGuid={session.organizationYear.guid} state={state} dispatch={dispatch} sessionType={session.sessionType.label} />
			</section>

			<section>
				<h5>Summary</h5>
				<hr />
				<AttendanceSummary sessionGuid={session.guid} sessionType={session.sessionType.label} attendanceGuid={attendanceGuid} date={date} state={state} />
			</section>
		</div>
	)
}

const DateTimeSelection = ({session, date, onDateChange, times, onTimeChange, progressFormState, isNewAttendance, originalAttendDate}): ReactElement => {
	const [editDateInitialized, setEditDateInitialized] = useState<boolean>(false)


	const { isFetching: fetchingDates, data: dates, error: dateError } = useQuery({ 
		queryKey: [`session/${session.guid}/attendance/openDates`],
		select: (dates: DateOnly[]) => dates.map(date => DateOnly.toLocalDate(date)),
		retry: false,
		staleTime: Infinity
	})

	function handleDateChange(date: LocalDate) {
		onDateChange(date)
	}

	function setTimeScheduleStartTime(id: string, time: LocalTime) {
		if (!times)
			return

		let schedule: TimeScheduleView = times.find(x => x.guid === id)!
		schedule.startTime = time;
		onTimeChange([...times])
	}

	function setTimeScheduleEndTime(id: string, time: LocalTime) {
		if (!times)
			return

		let schedule: TimeScheduleView = times.find(x => x.guid === id)!
		schedule.endTime = time;
		onTimeChange([...times])
	}

	useEffect(() => {
		if (dates && dates.length > 0 && !date && !originalAttendDate)
			handleDateChange(dates[0])
	}, [dates?.length])

	useEffect(() => {
		if (session && date)
		{
			let timeScheduleForDate = session.daySchedules
				.find(d => DayOfWeek.toInt(d.dayOfWeek) == date.dayOfWeek().value())
				?.timeSchedules;

			if (timeScheduleForDate)
				onTimeChange(timeScheduleForDate)
		}
	}, [session?.guid, date?.toString()])

	useEffect(() => {
		if (originalAttendDate && !editDateInitialized) {
			handleDateChange(originalAttendDate)
			setEditDateInitialized(true)
		}
	}, [dates?.length, originalAttendDate])

	const dateFormatter = DateTimeFormatter.ofPattern('eeee, MMMM dd').withLocale(Locale.ENGLISH)
	const continueDisabled: boolean = !dates || !date
		|| !times || times.length <= 0
		|| (!isNewAttendance && !originalAttendDate)

	return (
		<div className='row'>
			<div className='col-xl-2 col-md-4 col-12'>
				<label className='form-label' htmlFor='date-select'>Date</label>
				<select className='form-select' aria-label='Select attendance date' value={date?.toString()} onChange={e => handleDateChange(LocalDate.parse(e.target.value))}>
					{originalAttendDate ? <option className='text-primary' value={originalAttendDate.toString()}>{originalAttendDate.format(dateFormatter)}</option> : null}
					{dates?.map(date => (<option value={date.toString()}>{date.format(dateFormatter)}</option>))}
				</select>
			</div>

			<div className='col-xl-2 col-md-4 col-12'>
				<label className='form-label'>Start Time</label>
				{times?.map(schedule => (
					<TimeInput 
						id={'start-time-' + schedule.guid} 
						value={schedule.startTime} 
						onChange={(time) => setTimeScheduleStartTime(schedule.guid, time)} 
					/>
				))}
			</div>

			<div className='col-xl-2 col-md-4 col-12'>
				<label className='form-label'>End Time</label>
				{times?.map(schedule => (
					<TimeInput 
						id={'end-time-' + schedule.guid} 
						value={schedule.endTime} 
						onChange={(time) => setTimeScheduleEndTime(schedule.guid, time)} 
					/>
				))}
			</div>

			<div className='col-xl-2 d-flex align-items-end'>
				<button className='btn btn-primary' onClick={progressFormState} disabled={continueDisabled}>Continue</button>
			</div>	
		</div>
	)
}