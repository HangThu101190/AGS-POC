import type { StaffingRosterEntry } from "@/shared/api/staffingApi";

export type RosterPickOption = {
  employeeId: string;
  label: string;
  eligible: boolean;
  eligibleAsOvertime: boolean;
  disabled: boolean;
  assignedOnLine: boolean;
};

export function rosterOptionsForLine(
  roster: StaffingRosterEntry[] | undefined,
  lineId: string,
  opts?: { assigned?: number; target?: number },
): RosterPickOption[] {
  if (!roster) return [];
  const atCapacity =
    opts?.assigned != null && opts?.target != null && opts.assigned >= opts.target;

  return roster.map((e) => {
    const assignedOnLine = e.assignedLineIds.includes(lineId);
    const disabled = !e.eligible && !e.eligibleAsOvertime;
    return {
      employeeId: e.employeeId,
      label: `${e.code} — ${e.name}`,
      eligible: e.eligible,
      eligibleAsOvertime: e.eligibleAsOvertime,
      disabled: disabled || (atCapacity && !assignedOnLine),
      assignedOnLine,
    };
  });
}
