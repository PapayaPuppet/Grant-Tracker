//local components
import OverviewForm from './Overview'
import InstructorAttendance from './InstructorAttendance'
import SubstituteAttendance from './SubstituteAttendance'
import StudentAttendance from './StudentAttendance'

//Types and classes
import { AttendanceForm, ReducerAction } from '../state'
import { DropdownOption } from 'Models/Session'

interface Props {
  state: AttendanceForm
  dispatch: (action: ReducerAction) => void
  dateOptions: DropdownOption[],
  sessionType: string
}

export default ({ state, dispatch, dateOptions, sessionType }: Props): JSX.Element => {
  return (
    <>
      <OverviewForm state={state} dispatch={dispatch} dateOptions={dateOptions} />
      <InstructorAttendance state={state} dispatch={dispatch} />
      <SubstituteAttendance state={state} dispatch={dispatch} />
      <StudentAttendance state={state} dispatch={dispatch} sessionType={sessionType} />
    </>
  )
}

//
//<SubstituteAttendance state={state} dispatch={dispatch} />
//