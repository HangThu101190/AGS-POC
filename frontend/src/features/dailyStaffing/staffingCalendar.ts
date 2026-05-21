import type { EventInput } from "@fullcalendar/core";
import type { StaffingDay } from "@/shared/api/staffingApi";
import { HOUR_SLOT_THEME, type HourSlotMix } from "@/features/dailyStaffing/staffingHourCell";
import {
  inferSegmentFromFlight,
  isQnSegment,
  isQtSegment,
} from "@/features/dailyStaffing/staffingSegment";
import type { WeekMeta } from "@/shared/planning/weekCalendar";

export function calendarDateIso(weekMeta: WeekMeta, dayIdx: number): string {
  const label = weekMeta.weekDates[dayIdx] ?? weekMeta.weekDates[0];
  const [d, m] = label.split("/");
  return `${weekMeta.calendarYear}-${m.padStart(2, "0")}-${d.padStart(2, "0")}`;
}

export function toEventDateTime(dateIso: string, hhmm: string): string {
  const [h, m] = hhmm.split(":").map((x) => Number(x));
  return `${dateIso}T${String(h).padStart(2, "0")}:${String(m ?? 0).padStart(2, "0")}:00`;
}

export function staffingPlanStatusKey(status: string): string {
  const normalized = status.replace(/\s/g, "").toLowerCase();
  if (normalized.includes("confirmed")) return "staffing.status.confirmed";
  if (normalized.includes("crewdraft") || normalized.includes("crew")) return "staffing.status.crewDraft";
  if (normalized.includes("proposed")) return "staffing.status.proposed";
  return "staffing.status.tbdhDraft";
}

/** Flight STA→STD windows when lines are not proposed yet. */
export function buildStaffingFlightCalendarEvents(
  flights: StaffingDay["flights"],
  weekMeta: WeekMeta,
  dayIdx: number,
): EventInput[] {
  const dateIso = calendarDateIso(weekMeta, dayIdx);
  return flights.flatMap((flight) => {
    if (!flight.sta || !flight.std) return [];
    const flt = flight.departureFlightNo ?? flight.flightNo;
    return [
      {
        id: `flight-${flight.id}`,
        title: flt,
        start: toEventDateTime(dateIso, flight.sta),
        end: toEventDateTime(dateIso, flight.std),
        display: "background",
        classNames: ["staffing-flight-bg"],
        backgroundColor: "#bfdbfe",
        borderColor: "#2563eb",
        extendedProps: { dayIdx, flightId: flight.id, kind: "flight" },
      },
    ];
  });
}

/** Flight windows from staffing lines (visible after Đề xuất định biên). */
export function buildStaffingLineCalendarEvents(
  day: StaffingDay,
  weekMeta: WeekMeta,
  dayIdx: number,
): EventInput[] {
  const dateIso = calendarDateIso(weekMeta, dayIdx);
  const flightById = new Map(day.flights.map((f) => [f.id, f]));
  return day.lines.flatMap((line) => {
    const flight = flightById.get(line.flightId);
    if (!flight?.sta || !flight?.std) return [];
    const flt = flight.departureFlightNo ?? flight.flightNo;
    return [
      {
        id: `line-${line.id}`,
        title: `${flt} · ${line.proposedManning}/${line.targetManning}`,
        start: toEventDateTime(dateIso, flight.sta),
        end: toEventDateTime(dateIso, flight.std),
        classNames: ["staffing-line-event"],
        backgroundColor: "#1d4ed8",
        borderColor: "#1e3a8a",
        textColor: "#ffffff",
        extendedProps: { dayIdx, staffingLineId: line.id, kind: "line" },
      },
    ];
  });
}

function assignmentSegmentMix(
  day: StaffingDay,
  assignment: StaffingDay["assignments"][number],
): HourSlotMix {
  const line = day.lines.find((l) => l.id === assignment.staffingLineId);
  if (line) {
    if (isQnSegment(line.segment)) return "qn";
    if (isQtSegment(line.segment)) return "qt";
  }
  const flight = line ? day.flights.find((f) => f.id === line.flightId) : undefined;
  if (flight) return inferSegmentFromFlight(flight) === "Qn" ? "qn" : "qt";
  return "none";
}

function assignmentColors(
  day: StaffingDay,
  assignment: StaffingDay["assignments"][number],
): { backgroundColor: string; borderColor: string } {
  const att = assignment.attendance?.status;
  if (att === "late") return { backgroundColor: "#f87171", borderColor: "transparent" };
  if (att === "early_checkout") return { backgroundColor: "#fbbf24", borderColor: "transparent" };
  if (att === "checked_in") return { backgroundColor: "#4ade80", borderColor: "transparent" };
  const theme = HOUR_SLOT_THEME[assignmentSegmentMix(day, assignment)];
  return { backgroundColor: theme.backgroundColor, borderColor: theme.borderColor };
}

export function buildStaffingCalendarEvents(
  day: StaffingDay,
  weekMeta: WeekMeta,
  dayIdx: number,
  assignmentEditable = false,
): EventInput[] {
  const dateIso = calendarDateIso(weekMeta, dayIdx);
  const flightById = new Map(day.flights.map((f) => [f.id, f]));
  return day.assignments.map((a) => {
    const line = day.lines.find((l) => l.id === a.staffingLineId);
    const flight = line ? flightById.get(line.flightId) : undefined;
    const flt = flight?.departureFlightNo ?? flight?.flightNo ?? "";
    const title = `${a.employeeName}\n${a.workStart}–${a.workEnd}${flt ? ` · ${flt}` : ""}`;
    const colors = assignmentColors(day, a);
    const mix = assignmentSegmentMix(day, a);
    const theme = HOUR_SLOT_THEME[mix];
    return {
      id: a.id,
      title,
      start: toEventDateTime(dateIso, a.workStart),
      end: toEventDateTime(dateIso, a.workEnd),
      classNames: ["staffing-assignment-event", "staffing-pill-assignment", theme.className],
      backgroundColor: colors.backgroundColor,
      borderColor: colors.borderColor,
      textColor: theme.textColor,
      editable: assignmentEditable,
      durationEditable: assignmentEditable,
      startEditable: assignmentEditable,
      extendedProps: {
        kind: "assignment",
        dayIdx,
        staffingLineId: a.staffingLineId,
        assignmentId: a.id,
        role: a.role,
        isOvertime: a.isOvertime,
      },
    };
  });
}

/** Block NV kéo dài theo workStart–workEnd trên lịch tuần. */
export function buildAssignmentWeekEvents(
  daysByIdx: Map<number, StaffingDay>,
  weekMeta: WeekMeta,
  assignmentEditable: boolean,
): EventInput[] {
  const events: EventInput[] = [];
  for (let dayIdx = 0; dayIdx < 7; dayIdx += 1) {
    const day = daysByIdx.get(dayIdx);
    if (!day?.assignments.length) continue;
    events.push(...buildStaffingCalendarEvents(day, weekMeta, dayIdx, assignmentEditable));
  }
  return events;
}

export function timeOnlyFromDate(d: Date): string {
  return `${String(d.getHours()).padStart(2, "0")}:${String(d.getMinutes()).padStart(2, "0")}`;
}

/** Ô slot cùng ca 8h với thời điểm hiện tại (highlight hàng ngang trên lịch tuần). */
export function isCurrentHourSlot(slotDate: Date, now = new Date()): boolean {
  const h = now.getHours();
  const shiftStart = h < 8 ? 0 : h < 16 ? 8 : 16;
  return slotDate.getHours() === shiftStart;
}

/** FullCalendar `scrollTime` — giờ hiện tại, kẹp trong khung slot (mặc định 05:00–24:00). */
export function scrollTimeNow(slotMinHour = 5, slotMaxHour = 24): string {
  const now = new Date();
  let totalMin = now.getHours() * 60 + now.getMinutes();
  const minMin = slotMinHour * 60;
  const maxMin = slotMaxHour * 60 - 30;
  totalMin = Math.max(minMin, Math.min(maxMin, totalMin));
  const h = Math.floor(totalMin / 60);
  const m = totalMin % 60;
  return `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}:00`;
}

export function dayIdxFromCalendarDate(weekMeta: WeekMeta, dateStr: string): number | null {
  for (let i = 0; i < 7; i += 1) {
    if (calendarDateIso(weekMeta, i) === dateStr) return i;
  }
  return null;
}
