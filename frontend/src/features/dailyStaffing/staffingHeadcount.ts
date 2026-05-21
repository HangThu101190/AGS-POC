import type { EventInput } from "@fullcalendar/core";
import type { StaffingDay } from "@/shared/api/staffingApi";
import { calendarDateIso, toEventDateTime } from "@/features/dailyStaffing/staffingCalendar";
import {
  buildHourCellHtml,
  HOUR_SLOT_THEME,
  type HourSlotMix,
  type HourSlotStats,
} from "@/features/dailyStaffing/staffingHourCell";
import {
  inferSegmentFromFlight,
  isQnSegment,
  isQtSegment,
} from "@/features/dailyStaffing/staffingSegment";
import {
  clampTimeToShift,
  overlapsShiftSlot,
  SHIFT_SLOT_START_HOURS,
  shiftSlotEndHour,
} from "@/features/dailyStaffing/staffingShiftSlots";
import type { WeekMeta } from "@/shared/planning/weekCalendar";

export type { HourSlotStats, HourSlotMix } from "@/features/dailyStaffing/staffingHourCell";

/** @deprecated Giữ tên export — `hour` = giờ bắt đầu ca 8h (0 | 8 | 16). */
function overlapsHour(workStart: string, workEnd: string, shiftStartHour: number): boolean {
  return overlapsShiftSlot(workStart, workEnd, shiftStartHour);
}

function flightActiveWindow(f: StaffingDay["flights"][number]): { start: string; end: string } | null {
  const start = (f.eta?.trim() || f.sta)?.trim();
  const end = (f.etd?.trim() || f.std)?.trim();
  if (!start || !end) return null;
  return { start, end };
}

export function flightsInHourSlot(
  day: StaffingDay,
  shiftStartHour: number,
): StaffingDay["flights"] {
  return day.flights.filter((f) => {
    const w = flightActiveWindow(f);
    return w ? overlapsShiftSlot(w.start, w.end, shiftStartHour) : false;
  });
}

export function linesInHourSlot(day: StaffingDay, shiftStartHour: number): StaffingDay["lines"] {
  const flightIds = new Set(flightsInHourSlot(day, shiftStartHour).map((f) => f.id));
  return day.lines.filter((l) => flightIds.has(l.flightId));
}

/** Line ưu tiên còn thiếu NV trong ca 8h. */
export function preferredLineIdForHour(day: StaffingDay, shiftStartHour: number): string | null {
  const lines = linesInHourSlot(day, shiftStartHour);
  if (!lines.length) return null;
  for (const line of lines) {
    const assigned = day.assignments.filter((a) => a.staffingLineId === line.id).length;
    const need = Math.max(line.targetManning, line.proposedManning, 1);
    if (assigned < need) return line.id;
  }
  return lines[0]!.id;
}

export function assignmentsInHourSlot(
  day: StaffingDay,
  shiftStartHour: number,
): StaffingDay["assignments"] {
  return day.assignments.filter((a) => overlapsHour(a.workStart, a.workEnd, shiftStartHour));
}

/** Giờ làm mặc định khi gán NV nhanh từ dropdown (kẹp trong ca 8h). */
export function defaultWorkWindowForLineHour(
  day: StaffingDay,
  lineId: string,
  shiftStartHour: number,
): { workStart: string; workEnd: string } {
  const line = day.lines.find((l) => l.id === lineId);
  const flight = line ? day.flights.find((f) => f.id === line.flightId) : undefined;
  const pad = (n: number) => String(n).padStart(2, "0");
  const shiftEnd = shiftSlotEndHour(shiftStartHour);
  const shiftEndLabel = shiftEnd >= 24 ? "23:59" : `${pad(shiftEnd)}:00`;
  let workStart = flight?.sta?.trim() || `${pad(shiftStartHour)}:00`;
  let workEnd = flight?.std?.trim() || shiftEndLabel;
  if (!overlapsShiftSlot(workStart, workEnd, shiftStartHour)) {
    workStart = `${pad(shiftStartHour)}:00`;
    workEnd = shiftEndLabel;
  } else {
    workStart = clampTimeToShift(workStart, shiftStartHour);
    workEnd = clampTimeToShift(workEnd, shiftStartHour);
  }
  if (workEnd <= workStart) workEnd = shiftEndLabel;
  return { workStart, workEnd };
}

export function segmentMixForHour(day: StaffingDay, shiftStartHour: number): HourSlotMix {
  const segments = new Set<"Qn" | "Qt">();

  for (const f of flightsInHourSlot(day, shiftStartHour)) {
    segments.add(inferSegmentFromFlight(f));
  }

  for (const a of assignmentsInHourSlot(day, shiftStartHour)) {
    const line = day.lines.find((l) => l.id === a.staffingLineId);
    if (!line) continue;
    if (isQnSegment(line.segment)) segments.add("Qn");
    if (isQtSegment(line.segment)) segments.add("Qt");
  }

  if (segments.has("Qn") && segments.has("Qt")) return "both";
  if (segments.has("Qt")) return "qt";
  if (segments.has("Qn")) return "qn";
  return "none";
}

export function computeHourSlotStats(
  day: StaffingDay,
  dayIdx: number,
  shiftStartHour: number,
): HourSlotStats {
  const byEmployee = new Set<string>();
  for (const a of day.assignments) {
    if (!overlapsHour(a.workStart, a.workEnd, shiftStartHour)) continue;
    byEmployee.add(a.employeeId);
  }
  return {
    dayIdx,
    hour: shiftStartHour,
    staffCount: byEmployee.size,
    flightCount: flightsInHourSlot(day, shiftStartHour).length,
    mix: segmentMixForHour(day, shiftStartHour),
  };
}

/**
 * Mỗi ô ca 8h trên lịch tuần (0–8, 8–16, 16–24).
 */
export function buildHeadcountWeekEvents(
  daysByIdx: Map<number, StaffingDay>,
  weekMeta: WeekMeta,
): EventInput[] {
  const events: EventInput[] = [];

  for (let dayIdx = 0; dayIdx < 7; dayIdx += 1) {
    const day = daysByIdx.get(dayIdx);
    if (!day) continue;
    if (day.flights.length === 0 && day.assignments.length === 0) continue;

    const dateIso = calendarDateIso(weekMeta, dayIdx);

    for (const shiftStartHour of SHIFT_SLOT_START_HOURS) {
      const stats = computeHourSlotStats(day, dayIdx, shiftStartHour);
      if (stats.staffCount === 0 && stats.flightCount === 0) continue;

      const theme = HOUR_SLOT_THEME[stats.mix];
      const hh = String(shiftStartHour).padStart(2, "0");
      const hhEnd = String(shiftSlotEndHour(shiftStartHour)).padStart(2, "0");

      events.push({
        id: `slot-${dayIdx}-${shiftStartHour}`,
        title: "",
        start: toEventDateTime(dateIso, `${hh}:00`),
        end: toEventDateTime(dateIso, `${hhEnd}:00`),
        classNames: ["staffing-hour-slot-event", theme.className],
        backgroundColor: "transparent",
        borderColor: "transparent",
        textColor: "#0f172a",
        editable: false,
        extendedProps: { kind: "hour-slot", stats, dayIdx, hour: shiftStartHour },
      });
    }
  }

  return events;
}

export function hourCellContentHtml(stats: HourSlotStats): string {
  return buildHourCellHtml(stats);
}

export function flightLabelForAssignment(day: StaffingDay, staffingLineId: string): string {
  const line = day.lines.find((l) => l.id === staffingLineId);
  const flight = line ? day.flights.find((f) => f.id === line.flightId) : undefined;
  return flight?.departureFlightNo ?? flight?.flightNo ?? "—";
}

export function flightNoLabel(f: StaffingDay["flights"][number]): string {
  return f.departureFlightNo ?? f.flightNo;
}

export function lineForFlight(
  day: StaffingDay,
  flightId: string,
): StaffingDay["lines"][number] | undefined {
  return day.lines.find((l) => l.flightId === flightId);
}

export function requiredManningForLine(
  line: StaffingDay["lines"][number],
  flight: StaffingDay["flights"][number] | undefined,
): number {
  return Math.max(line.targetManning, line.proposedManning, flight?.manning ?? 0, 1);
}

export function assignedCountOnLine(day: StaffingDay, lineId: string): number {
  return day.assignments.filter((a) => a.staffingLineId === lineId).length;
}

/** Chuyến trong ca 8h chưa đủ NV gán trên dòng phân công tương ứng. */
export function flightsWithCoverageGap(
  day: StaffingDay,
  shiftStartHour: number,
): StaffingDay["flights"] {
  return flightsInHourSlot(day, shiftStartHour).filter((f) => {
    const line = lineForFlight(day, f.id);
    if (!line) return true;
    const need = requiredManningForLine(line, f);
    return assignedCountOnLine(day, line.id) < need;
  });
}

export function isShiftUnderstaffed(day: StaffingDay, shiftStartHour: number): boolean {
  return flightsWithCoverageGap(day, shiftStartHour).length > 0;
}

export type EmployeeShiftGroup = {
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  role: string;
  employeeDepartmentCode: string;
  assignments: StaffingDay["assignments"];
  flightNos: string[];
};

/** Gộp phân công trong ca theo nhân viên (một dòng UI / NV). */
export function groupAssignmentsByEmployeeInShift(
  day: StaffingDay,
  shiftStartHour: number,
): EmployeeShiftGroup[] {
  const inSlot = assignmentsInHourSlot(day, shiftStartHour);
  const byEmp = new Map<string, EmployeeShiftGroup>();
  for (const a of inSlot) {
    const flt = flightLabelForAssignment(day, a.staffingLineId);
    const existing = byEmp.get(a.employeeId);
    if (existing) {
      existing.assignments.push(a);
      if (flt !== "—" && !existing.flightNos.includes(flt)) existing.flightNos.push(flt);
      continue;
    }
    byEmp.set(a.employeeId, {
      employeeId: a.employeeId,
      employeeName: a.employeeName,
      employeeCode: a.employeeCode,
      role: a.role,
      employeeDepartmentCode: a.employeeDepartmentCode?.trim() ?? "",
      assignments: [a],
      flightNos: flt !== "—" ? [flt] : [],
    });
  }
  return [...byEmp.values()].sort((a, b) => a.employeeName.localeCompare(b.employeeName, "vi"));
}
