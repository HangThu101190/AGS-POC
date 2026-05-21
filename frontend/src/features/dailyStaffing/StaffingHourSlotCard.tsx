import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { StaffingHourSlotHeadDropdown } from "@/features/dailyStaffing/StaffingHourSlotHeadDropdown";
import {
  buildHourSlotFlightGroups,
  hourSlotFlightGroupsForCard,
} from "@/features/dailyStaffing/staffingHourSlotFlightView";
import {
  flightsInHourSlot,
  isShiftUnderstaffed,
  type HourSlotStats,
} from "@/features/dailyStaffing/staffingHeadcount";
import type { StaffingDay, StaffingRosterEntry } from "@/shared/api/staffingApi";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { flightEtaEtdDisplayTime } from "@/shared/planning/flightClock";

type Props = {
  day: StaffingDay;
  hour: number;
  stats: HourSlotStats;
  roster?: StaffingRosterEntry[];
  rosterLoading?: boolean;
  canAssign: boolean;
  isSaving?: boolean;
  onRemove: (assignmentId: string) => void;
  onToggleFlight: (employeeId: string, flightId: string) => void;
};

function hourTargetStaff(day: StaffingDay, shiftStartHour: number): number {
  let fromLines = 0;
  const slotStart = shiftStartHour * 60;
  const slotEnd = shiftStartHour + 8 * 60;
  const parse = (hhmm: string) => {
    const [h, m] = hhmm.split(":").map((x) => Number(x));
    return (h ?? 0) * 60 + (m ?? 0);
  };
  for (const line of day.lines) {
    const flight = day.flights.find((f) => f.id === line.flightId);
    const start = (flight?.eta?.trim() || flight?.sta)?.trim();
    const end = (flight?.etd?.trim() || flight?.std)?.trim();
    if (!start || !end) continue;
    const startM = parse(start);
    let endM = parse(end);
    if (endM <= startM) endM += 24 * 60;
    if (startM >= slotEnd || endM <= slotStart) continue;
    fromLines += Math.max(line.proposedManning, line.targetManning, 0);
  }
  if (fromLines > 0) return fromLines;
  const fromFlights = flightsInHourSlot(day, shiftStartHour).reduce(
    (sum, f) => sum + (f.manning || 0),
    0,
  );
  return fromFlights > 0 ? fromFlights : 1;
}

function badgeClass(filled: number, need: number): string {
  if (need <= 0) return "staffing-slot-card__badge--neutral";
  if (filled >= need) return "staffing-slot-card__badge--full";
  if (filled === 0) return "staffing-slot-card__badge--empty";
  return "staffing-slot-card__badge--partial";
}

function stopCardEvent(e: React.SyntheticEvent) {
  e.stopPropagation();
}

function roleLabel(t: (k: string) => string, role: string): string {
  const key = `staffing.roles.${role}`;
  const translated = t(key);
  return translated === key ? role : translated;
}

function staffMetaLabel(
  t: (k: string) => string,
  role: string,
  employeeDepartmentCode: string,
): string {
  const normalized = role.trim();
  if (
    (normalized === "General" || normalized === "general") &&
    employeeDepartmentCode
  ) {
    return departmentLabel(employeeDepartmentCode, t);
  }
  return roleLabel(t, role);
}

function manningChipClass(assigned: number, need: number): string {
  if (need <= 0) return "staffing-slot-card__flt-manning--neutral";
  if (assigned >= need) return "staffing-slot-card__flt-manning--full";
  if (assigned === 0) return "staffing-slot-card__flt-manning--empty";
  return "staffing-slot-card__flt-manning--partial";
}

export function StaffingHourSlotCard({
  day,
  hour,
  stats,
  roster,
  rosterLoading,
  canAssign,
  isSaving,
  onRemove,
  onToggleFlight,
}: Props) {
  const { t } = useTranslation();

  const flightGroups = useMemo(() => buildHourSlotFlightGroups(day, hour), [day, hour]);
  const cardFlightGroups = useMemo(
    () => hourSlotFlightGroupsForCard(flightGroups),
    [flightGroups],
  );
  const maxFlightsOnCard = 4;
  const visibleFlightGroups = cardFlightGroups.slice(0, maxFlightsOnCard);
  const hiddenFlightCount = Math.max(0, cardFlightGroups.length - visibleFlightGroups.length);
  const showPerFlightGapHint = cardFlightGroups.length <= 2;
  const flights = useMemo(() => flightsInHourSlot(day, hour), [day, hour]);
  const danger = useMemo(() => isShiftUnderstaffed(day, hour), [day, hour]);
  const need = hourTargetStaff(day, hour);
  const badgeNeed = Math.max(need, stats.staffCount, 1);
  const badgeText = t("staffing.hourSlotStaffBadge", {
    filled: stats.staffCount,
    need: badgeNeed,
  });
  const badgeCls = badgeClass(stats.staffCount, badgeNeed);

  const deptByEmployeeId = useMemo(() => {
    const m = new Map<string, string>();
    for (const e of roster ?? []) {
      const code = e.departmentCode?.trim();
      if (code) m.set(e.employeeId, code);
    }
    return m;
  }, [roster]);

  const cardClass = [
    "staffing-slot-card",
    "staffing-slot-card--shift",
    danger ? "staffing-slot-card--danger" : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div
      className={cardClass}
      role="presentation"
      onClick={stopCardEvent}
      onMouseDown={stopCardEvent}
    >
      <div className="staffing-slot-card__head staffing-slot-card__head--badge">
        {canAssign && flights.length > 0 ? (
          <StaffingHourSlotHeadDropdown
            flightGroups={flightGroups}
            roster={roster}
            badgeText={badgeText}
            badgeClassName={badgeCls}
            disabled={isSaving || rosterLoading}
            onToggleFlight={onToggleFlight}
          />
        ) : (
          <span className={`staffing-slot-card__badge staffing-slot-card__badge--block ${badgeCls}`}>
            <span className="staffing-slot-card__nv-btn-icon" aria-hidden>
              ✈
            </span>
            {badgeText}
          </span>
        )}
      </div>

      {visibleFlightGroups.length > 0 ? (
        <div className="staffing-slot-card__flights staffing-slot-card__flights--with-staff">
          {visibleFlightGroups.map((group) => {
            const time =
              flightEtaEtdDisplayTime(group.flight, "etd") || group.flight.std || "";
            const manningCls = manningChipClass(group.assigned, group.need);
            const gap = group.line != null && group.need > 0 && group.assigned < group.need;

            return (
              <section
                key={group.flight.id}
                className={`staffing-slot-card__flight-group${gap ? " staffing-slot-card__flight-group--gap" : ""}`}
              >
                <div className="staffing-slot-card__flight-head">
                  <div className="staffing-slot-card__flt-row">
                    <span className="staffing-slot-card__flt">{group.flight.departureFlightNo ?? group.flight.flightNo}</span>
                    {time ? (
                      <span className="staffing-slot-card__flt-time">{time}</span>
                    ) : null}
                  </div>
                  {group.line && group.need > 0 ? (
                    <span
                      className={`staffing-slot-card__flt-manning ${manningCls}`}
                      title={t("staffing.hourSlotFlightManning", {
                        assigned: group.assigned,
                        need: group.need,
                      })}
                    >
                      {group.assigned}/{group.need}
                    </span>
                  ) : null}
                </div>

                {group.staff.length > 0 ? (
                  <div className="staffing-slot-card__flight-staff">
                    {group.staff.map((row) => (
                      <div
                        key={`${group.flight.id}-${row.employeeId}`}
                        className="staffing-slot-card__staff-row"
                      >
                        <div className="staffing-slot-card__staff-body">
                          <div className="staffing-slot-card__staff-name">{row.employeeName}</div>
                          <div className="staffing-slot-card__staff-meta">
                            {staffMetaLabel(
                              t,
                              row.role,
                              row.employeeDepartmentCode ||
                                deptByEmployeeId.get(row.employeeId) ||
                                "",
                            )}
                            {` · ${row.workStart}–${row.workEnd}`}
                          </div>
                        </div>
                        {canAssign ? (
                          <div className="staffing-slot-card__staff-actions">
                            <button
                              type="button"
                              className="staffing-slot-card__remove"
                              disabled={isSaving}
                              aria-label={t("staffing.removeAssign")}
                              onClick={() => {
                                for (const id of row.assignmentIds) onRemove(id);
                              }}
                            >
                              ✕
                            </button>
                          </div>
                        ) : null}
                      </div>
                    ))}
                  </div>
                ) : gap && showPerFlightGapHint ? (
                  <p className="staffing-slot-card__flight-gap">{t("staffing.hourSlotFlightGap")}</p>
                ) : null}
              </section>
            );
          })}
          {hiddenFlightCount > 0 ? (
            <p className="staffing-slot-card__more">
              {t("staffing.hourSlotMoreFlights", { count: hiddenFlightCount })}
            </p>
          ) : null}
        </div>
      ) : null}
    </div>
  );
}
