import type { EventInput } from "@fullcalendar/core";
import type { StaffingDay } from "@/shared/api/staffingApi";
import { buildHeadcountWeekEvents } from "@/features/dailyStaffing/staffingHeadcount";
import type { WeekMeta } from "@/shared/planning/weekCalendar";

/** Ô giờ trên lịch tuần — NV đã gán hiển thị trong card (dropdown), không vẽ pill FC riêng. */
export function buildWeekStaffingEvents(
  daysByIdx: Map<number, StaffingDay>,
  weekMeta: WeekMeta,
): EventInput[] {
  return buildHeadcountWeekEvents(daysByIdx, weekMeta);
}
