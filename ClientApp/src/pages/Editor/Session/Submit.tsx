import { useState, useEffect } from 'react'
import { Row, Col, Container, ListGroup, Button, Form } from 'react-bootstrap'
import { convert, DateTimeFormatter } from '@js-joda/core'
import { Locale } from '@js-joda/locale_en-us'

import ListItem from 'components/Item'
import { GradeView } from 'Models/Grade'
import { WeeklySchedule } from 'Models/DaySchedule'
import { DropdownOption } from 'types/Session'

import { useSession, Context } from '../index'
import api from 'utils/api'

function formatScheduling (scheduling: WeeklySchedule): JSX.Element[] {
  return scheduling.map(weekday => {
    if (weekday.recurs)
      return (
        <div className='mb-2'>
          <p className='my-0'>{`${weekday.dayOfWeek}: `}</p>
          {weekday.timeSchedules.map(schedule => (
            <p className='m-0'>
              {`${schedule.startTime.format(
                DateTimeFormatter.ofPattern('hh:mm a').withLocale(
                  Locale.ENGLISH
                )
              )}
								to
								${schedule.endTime.format(
                  DateTimeFormatter.ofPattern('hh:mm a').withLocale(
                    Locale.ENGLISH
                  )
                )}`}
            </p>
          ))}
        </div>
      )

    return <></>
  })
}

//Create component to replace Item & inner HTML if a refactor is ever needed
interface SDisplayProps extends Context {
  grades: GradeView[]
}

//we can use errors to display errors as needed and desired, then add a subscript to ListItem if desired
const SessionDisplay = ({
  reducerDispatch,
  dropdownData,
  values,
  errors,
  grades
}: SDisplayProps): JSX.Element => (
  <Col>
    <ListGroup>
      <h3>Session</h3>
      <ListItem label='Name:' value={values.name} />
      <ListItem
        label='Type'
        value={
          dropdownData.sessionTypes.find(e => e.guid === values.type)?.label
        }
      />
      <ListItem
        label='Activity:'
        value={
          dropdownData.activities.find(e => e.guid === values.activity)?.label
        }
      />
      <ListItem
        label='Objective:'
        value={
          dropdownData.objectives.find(e => e.guid === values.objective)?.label
        }
      />
      <ListItem
        label='Grade Levels:'
        value={
          <p>
            {grades?.length !== 0
              ? grades.map((grade, index) =>
                  index !== grades.length - 1 ? grade.value + ', ' : grade.value
                )
              : 'All Grade Levels'}
          </p>
        }
      />
    </ListGroup>
  </Col>
)

const SchedulingDisplay = ({
  reducerDispatch,
  dropdownData,
  values,
  errors
}: Context): JSX.Element => {
  const schedule = values.recurring
    ? values.scheduling
    : values.scheduling.find(s => s.timeSchedules.length !== 0)
  console.log(values.recurring)
  return (
    <Col>
      <ListGroup>
        <h3>Scheduling</h3>
        <ListItem
          label={
            values.recurring ? 'Series Start Date:' : 'First Session Date:'
          }
          value={convert(values.firstSessionDate)
            .toDate()
            .toLocaleDateString('en-US', {
              weekday: 'long',
              year: 'numeric',
              month: 'long',
              day: 'numeric'
            })}
        />
        {
          values.recurring 
          ?
          <ListItem
            label={'Series End Date:'}
            value={convert(values.lastSessionDate)
              .toDate()
              .toLocaleDateString('en-US', {
                weekday: 'long',
                year: 'numeric',
                month: 'long',
                day: 'numeric'
              })}
          />
          : <></>
        }
        <ListItem
          label={values.recurring ? 'Weekly Schedule:' : 'Time Schedule:'}
          value={
            values.recurring ? (
              <div className='d-flex flex-column'>
                {formatScheduling(schedule)}
              </div>
            ) : (
              <div className='d-flex flex-column'>
                {schedule?.timeSchedules?.map(time => (
                  <div>
                    {time.startTime?.format(
                      DateTimeFormatter.ofPattern('hh:mm a').withLocale(
                        Locale.ENGLISH
                      )
                    )}{' '}
                    to{' '}
                    {time.endTime?.format(
                      DateTimeFormatter.ofPattern('hh:mm a').withLocale(
                        Locale.ENGLISH
                      )
                    )}
                  </div>
                ))}
              </div>
            )
          }
        />
      </ListGroup>
    </Col>
  )
}

const OrganizerDisplay = ({
  reducerDispatch,
  dropdownData,
  values,
  errors
}: Context): JSX.Element => (
  <Col>
    <ListGroup>
      <h3>Organizer</h3>
      <ListItem
        label='Funding Source:'
        value={
          dropdownData.fundingSources.find(e => e.guid === values.fundingSource)
            ?.label
        }
      />
      <ListItem
        label='Organization Type:'
        value={
          dropdownData.organizationTypes.find(
            e => e.guid === values.organizationType
          )?.label
        }
      />
      <ListItem
        label='Partnership Type:'
        value={
          dropdownData.partnershipTypes.find(
            e => e.guid === values.partnershipType
          )?.label
        }
      />
    </ListGroup>
  </Col>
)

const InstructorDisplay = ({
  reducerDispatch,
  dropdownData,
  values,
  errors
}: Context): JSX.Element => (
  <Col>
    <h3>Instructors</h3>
    <ListGroup className='d-flex flex-row flex-wrap'>
      {values.instructors.map(instructor => (
        <div className='w-50'>
          <ListGroup.Item>{instructor.label}</ListGroup.Item>
        </div>
      ))}
    </ListGroup>
  </Col>
)

//Don't allow '#' in session names, and other special characters probably
export default (): JSX.Element => {
  const props: Context = useSession()
  const [grades, setGrades] = useState<GradeView[]>([])

  if (!props.values)
    return (
      <p style={{ textAlign: 'center' }}>Error in loading Session details...</p>
    )

  document.title = `${props.values.guid ? 'Edit' : 'New'} Session - Submit`

  useEffect(() => {
    api
      .get<DropdownOption[]>('dropdown/view/grades')
      .then(res => {
        console.log(res.data)
        let gradeViews: GradeView[] = res.data
          .filter(option => props.values.grades.includes(option.guid))
          .map(
            grade =>
              ({
                value: grade.abbreviation,
                description: grade.label
              } as GradeView)
          )

        setGrades(gradeViews)
      })
      .catch(err => {
        console.warn(err)
      })
  }, [])

  //on click, take user to appropriate page and focus the appropriate element
  return (
    <Container className='m-3'>
      <Row lg={2} className='m-3'>
        <SessionDisplay {...props} grades={grades} />
        <OrganizerDisplay {...props} />
      </Row>
      <Row lg={2} className='m-3'>
        <SchedulingDisplay {...props} />
        <InstructorDisplay {...props} />
      </Row>
      <Row lg={3}>
        <Col />
        <Col className='d-flex justify-content-center'>
          <Button variant='primary' type='submit'>
            Submit
          </Button>
        </Col>
        <Col />
      </Row>
    </Container>
  )
}
