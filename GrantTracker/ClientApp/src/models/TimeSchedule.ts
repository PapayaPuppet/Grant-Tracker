import { LocalTime } from "@js-joda/core"
import { TimeOnly } from "./TimeOnly"


export interface TimeScheduleDomain {
  guid: string
  dayScheduleGuid: string
  startTime: TimeOnly
  endTime: TimeOnly
}

//why did I make both the next two interfaces..?
export interface TimeScheduleView {
  guid: string
  startTime: LocalTime
  endTime: LocalTime
}

export interface TimeScheduleForm {
  guid: string | undefined
  startTime: LocalTime
  endTime: LocalTime
}

export function timeScheduleComparer(first: TimeScheduleView, second: TimeScheduleView): -1 | 0 | 1 {
  if (first.startTime.isBefore(second.startTime)) return -1
  if (first.startTime.isAfter(second.startTime)) return 1
  return 0
}

export abstract class TimeSchedule {
  public static toViewModel(obj: TimeScheduleDomain): TimeScheduleView {
    return {
      guid: obj.guid,
      startTime: TimeOnly.toLocalTime(obj.startTime),
      endTime: TimeOnly.toLocalTime(obj.endTime)
    }
  }

  public static toFormModel(obj: TimeScheduleDomain): TimeScheduleForm {
    return {
      guid: obj.guid,
      startTime: TimeOnly.toLocalTime(obj.startTime),
      endTime: TimeOnly.toLocalTime(obj.endTime)
    }
  }
}