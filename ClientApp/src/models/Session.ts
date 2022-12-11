import { LocalDate, LocalTime } from '@js-joda/core'

import { DateOnly } from './DateOnly'
import { TimeOnly } from './TimeOnly'
import { DayOfWeek, DayOfWeekNumeric } from './DayOfWeek'
import { Grade, GradeDomain, GradeView } from './Grade'
import { Instructor, InstructorSchoolYearView } from './Instructor'
import * as DaySchedule from './DaySchedule'
import { TimeSchedule } from './TimeSchedule'
import { InstructorRegistration } from './InstructorRegistration'

export interface DropdownOption {
  guid: string
  abbreviation?: string
  label: string
  description?: string
}

export interface SimpleSessionView {
  sessionGuid: string
  name: string
  sessionType: DropdownOption
  activity: DropdownOption
  recurring: boolean
  daySchedules: DaySchedule.DayScheduleView[]
  sessionGrades: GradeView[]
}

export interface SessionDomain {
  guid: string
  name: string
  firstSession: DateOnly
  lastSession: DateOnly
  recurring: boolean
  organization: DropdownOption
  sessionType: DropdownOption
  activity: DropdownOption
  objective: DropdownOption
  fundingSource: DropdownOption
  organizationType: DropdownOption
  partnershipType: DropdownOption
  daySchedules: DaySchedule.DayScheduleDomain[]
  instructors: InstructorSchoolYearView[]
  sessionGrades: GradeDomain[]
}

export interface SessionView {
  guid: string
  name: string
  firstSession: LocalDate
  lastSession: LocalDate
  recurring: boolean
  organization: DropdownOption
  sessionType: DropdownOption
  activity: DropdownOption
  objective: DropdownOption
  fundingSource: DropdownOption
  organizationType: DropdownOption
  partnershipType: DropdownOption
  daySchedules: DaySchedule.DayScheduleView[]
  instructors: InstructorSchoolYearView[]
  grades: GradeView[]
}

export interface SessionForm {
  guid?: string
  name: string
  type: string
  activity: string
  objective: string
  fundingSource: string
  organizationType: string
  partnershipType: string
  firstSessionDate: LocalDate
  lastSessionDate: LocalDate
  recurring: boolean
  scheduling: DaySchedule.WeeklySchedule
  grades: string[]
  instructors: { guid: string; label: string }[]
}

export abstract class Session {
  public static toViewModel (obj: SessionDomain): SessionView {
    return {
      ...obj,
      firstSession: DateOnly.toLocalDate(obj.firstSession),
      lastSession: DateOnly.toLocalDate(obj.lastSession),
      daySchedules: obj.daySchedules.map(day => DaySchedule.DaySchedule.toViewModel(day)),
      grades: obj.sessionGrades.map(grade => Grade.toViewModel(grade))
    }
  }

  public static toFormModel (obj: SessionDomain): SessionForm {
    const firstSession: LocalDate = DateOnly.toLocalDate(obj.firstSession)

    return {
      guid: obj.guid,
      name: obj.name,
      activity: obj.activity.guid,
      objective: obj.objective.guid,
      type: obj.sessionType.guid,
      instructors: obj.instructors.map(reg => ({
        guid: reg.guid,
        label: `${reg.instructor.firstName} ${reg.instructor.lastName}`
      })),
      fundingSource: obj.fundingSource.guid,
      organizationType: obj.organizationType.guid,
      partnershipType: obj.partnershipType.guid,
      firstSessionDate: firstSession,
      lastSessionDate: DateOnly.toLocalDate(obj.lastSession),
      recurring: obj.recurring,
      scheduling: DaySchedule.DaySchedule.createWeeklySchedule().map((day, index) => {
        if (!obj.recurring) {
          if (
            firstSession.dayOfWeek().value() === DayOfWeek.toInt(day.dayOfWeek)
          ) {
            if (!obj.daySchedules || obj.daySchedules.length === 0) {
              return {
                dayOfWeek: day.dayOfWeek,
                recurs: false,
                timeSchedules: [
                  {
                    startTime: LocalTime.MIDNIGHT,
                    endTime: LocalTime.MIDNIGHT
                  }
                ]
              }
              if (
                obj.daySchedules.length === 1 &&
                !obj.daySchedules[0].timeSchedules
              ) {
                obj.daySchedules[0].timeSchedules = [
                  {
                    startTime: LocalTime.MIDNIGHT,
                    endTime: LocalTime.MIDNIGHT
                  }
                ]
              }
            }
            return {
              dayOfWeek: day.dayOfWeek,
              recurs: false,
              timeSchedules: obj.daySchedules[0].timeSchedules.map(s =>
                TimeSchedule.toFormModel(s)
              )
            }
          }
          return day
        }

        const domainSchedule:
          | DaySchedule.DayScheduleDomain
          | undefined = obj.daySchedules.find(
          domainDay => DayOfWeek.toString(domainDay.dayOfWeek) === day.dayOfWeek
        )
        if (domainSchedule) {
          day.recurs = true
          day.timeSchedules = domainSchedule.timeSchedules.map(time =>
            TimeSchedule.toFormModel(time)
          )
        }
        return day
      }) as DaySchedule.WeeklySchedule,
      grades: obj.sessionGrades?.map(grade => grade.gradeGuid) || []
    }
  }

  public static createDefaultForm (): SessionForm {
    let baseSchedule = DaySchedule.DaySchedule.createWeeklySchedule()
    let today = LocalDate.now()
    /*baseSchedule[today.dayOfWeek().value()] = {
      dayOfWeek: DayOfWeek.toString(today.dayOfWeek().value()),
      recurs: false,
      timeSchedules: [
        {
          startTime: LocalTime.MIDNIGHT,
          endTime: LocalTime.MIDNIGHT
        }
      ]
    }*/

    return {
      name: '',
      type: '',
      activity: '',
      objective: '',
      instructors: [],
      fundingSource: '',
      organizationType: '',
      partnershipType: '',
      firstSessionDate: today,
      lastSessionDate: today,
      recurring: true,
      scheduling: baseSchedule,
      grades: []
    }
  }
}
