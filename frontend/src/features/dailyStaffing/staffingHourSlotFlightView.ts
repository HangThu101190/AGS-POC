import {
  assignedCountOnLine,
  assignmentsInHourSlot,
  flightNoLabel,
  flightsInHourSlot,
  lineForFlight,
  requiredManningForLine,
} from "@/features/dailyStaffing/staffingHeadcount";
import type { StaffingDay, StaffingRosterEntry } from "@/shared/api/staffingApi";

/** Một NV trên một chuyến trong ca (có thể nhiều assignment id khi gộp). */
export type HourSlotFlightStaffRow = {
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  role: string;
  employeeDepartmentCode: string;
  workStart: string;
  workEnd: string;
  assignmentIds: string[];
};

/** Khối hiển thị / phân công theo chuyến bay trong ca 8h. */
export type HourSlotFlightGroup = {
  flight: StaffingDay["flights"][number];
  line: StaffingDay["lines"][number] | null;
  need: number;
  assigned: number;
  staff: HourSlotFlightStaffRow[];
};

export type StaffPickOption = {
  value: string;
  name: string;
  code: string;
  hint?: string;
  disabled: boolean;
  assignedOnLine?: boolean;
};

/** Danh sách chuyến trong ca + NV đã gán từng dòng (nguồn chung card + dropdown). */
export function buildHourSlotFlightGroups(
  day: StaffingDay,
  shiftStartHour: number,
): HourSlotFlightGroup[] {
  const flights = flightsInHourSlot(day, shiftStartHour);
  const inSlot = assignmentsInHourSlot(day, shiftStartHour);

  return flights.map((flight) => {
    const line = lineForFlight(day, flight.id) ?? null;
    const lineId = line?.id;
    const onLine = lineId ? inSlot.filter((a) => a.staffingLineId === lineId) : [];

    const byEmp = new Map<string, HourSlotFlightStaffRow>();
    for (const a of onLine) {
      const existing = byEmp.get(a.employeeId);
      if (existing) {
        existing.assignmentIds.push(a.id);
        continue;
      }
      byEmp.set(a.employeeId, {
        employeeId: a.employeeId,
        employeeName: a.employeeName,
        employeeCode: a.employeeCode,
        role: a.role,
        employeeDepartmentCode: a.employeeDepartmentCode?.trim() ?? "",
        workStart: a.workStart,
        workEnd: a.workEnd,
        assignmentIds: [a.id],
      });
    }

    const staff = [...byEmp.values()].sort((a, b) =>
      a.employeeName.localeCompare(b.employeeName, "vi"),
    );
    const need = line ? requiredManningForLine(line, flight) : 0;
    const assigned = line ? assignedCountOnLine(day, line.id) : 0;

    return { flight, line, need, assigned, staff };
  });
}

/** Chuyến có NV trong ca hoặc còn thiếu định biên — hiển thị trên thẻ. */
export function hourSlotFlightGroupsForCard(groups: HourSlotFlightGroup[]): HourSlotFlightGroup[] {
  return groups.filter(
    (g) => g.staff.length > 0 || (g.line != null && g.need > 0 && g.assigned < g.need),
  );
}

export function rosterPickOptionsForLine(
  roster: StaffingRosterEntry[] | undefined,
  lineId: string | null,
): StaffPickOption[] {
  if (!lineId) return [];
  return (roster ?? []).map((e) => {
    const onLine = e.assignedLineIds.includes(lineId);
    const disabled = (!e.eligible && !e.eligibleAsOvertime) || onLine;
    let hint = "";
    if (!onLine && e.availabilityStatus) {
      hint = `${e.availabilityStatus}${e.leaveTypeCode ? ` · ${e.leaveTypeCode}` : ""}`;
    }
    return {
      value: e.employeeId,
      name: e.name,
      code: e.code,
      hint: hint || undefined,
      disabled,
      assignedOnLine: onLine,
    };
  });
}

export function hourSlotFlightNo(group: HourSlotFlightGroup): string {
  return flightNoLabel(group.flight);
}
