import { useCallback, useEffect, useMemo, useState, type CSSProperties } from "react";
import { useTranslation } from "react-i18next";
import { FlightAssignDropdown } from "@/features/phanCongSlot/FlightAssignDropdown";
import { WeekDayChips } from "@/features/planning/WeekDayChips";
import {
  assignToSlot,
  fetchPhanCongSlot,
  linkAssignmentFlights,
  removeAssignment,
  type PhanCongSlotDto,
} from "@/shared/api/phanCongSlotApi";
import { fetchDepartmentsPage } from "@/shared/api/departmentsApi";
import { fetchEmployeesPage } from "@/shared/api/employeesApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import type { DepartmentDto, EmployeeDto } from "@/shared/types/api";
import { flightOverlapsSlotSegments } from "@/shared/planning/flightSlotOverlap";
import { isPastDay } from "@/shared/planning/weekCalendar";
import {
  StatusBanner,
  WeekPageShell,
  chrome,
} from "@/shared/webChrome";
import { PLANNING_DEPT_CODES } from "@/shared/constants/departments";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import sbStyles from "./phanCongSlotPage.module.css";

function avatarInitial(name: string): string {
  const parts = name.trim().split(/\s+/);
  return parts.length ? parts[parts.length - 1]![0]! : "?";
}

export function PhanCongSlotPage() {
  const { t } = useTranslation();
  const { weekId, weekMeta, plan, refreshPlan } = useWeekScope();
  const [board, setBoard] = useState<PhanCongSlotDto | null>(null);
  const [dayIdx, setDayIdx] = useState(0);
  const [deptCode, setDeptCode] = useState("PVHK_DI");
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [pickingForSlot, setPickingForSlot] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      const data = await fetchPhanCongSlot({ weekId, dayIdx, departmentCode: deptCode });
      setBoard(data);
    } catch {
      setError(t("phanCongSlot.loadFailed"));
    }
  }, [weekId, dayIdx, deptCode, t]);

  useEffect(() => {
    const idx = weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;
    setDayIdx(idx);
  }, [weekId, weekMeta.todayIdx]);

  useEffect(() => {
    void fetchDepartmentsPage({ page: 1, pageSize: 20 }).then((p) => setDepartments(p.items));
    void fetchEmployeesPage({ page: 1, pageSize: 100 }).then((p) => setEmployees(p.items));
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const todayIdx = weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;
  const isPast = isPastDay(dayIdx, todayIdx);
  const dept = departments.find((d) => d.code === deptCode);
  const dayFlights = board?.flights ?? [];
  const planStatus = board?.planStatus ?? plan?.status ?? "draft";
  const isDraft = planStatus === "draft";

  const staffForDept = useMemo(
    () =>
      employees.filter(
        (e) => e.departmentId === dept?.id && e.role.toLowerCase() === "staff",
      ),
    [employees, dept?.id],
  );

  const onAssign = async (slotId: string, empId: string) => {
    if (!dept) return;
    setError(null);
    try {
      await assignToSlot({
        weekId,
        departmentId: dept.id,
        slotId,
        employeeId: empId,
      });
      setMessage(t("phanCongSlot.assigned"));
      setPickingForSlot(null);
      await load();
      refreshPlan();
    } catch {
      setError(t("phanCongSlot.actionFailed"));
    }
  };

  const toggleFlight = async (assignmentId: string, flightNo: string, current: string[]) => {
    if (isPast) return;
    setError(null);
    const next = current.includes(flightNo)
      ? current.filter((f) => f !== flightNo)
      : [...current, flightNo];
    try {
      await linkAssignmentFlights(assignmentId, { weekId, flightNos: next });
      await load();
    } catch {
      setError(t("phanCongSlot.actionFailed"));
    }
  };

  if (isDraft) {
    return (
      <WeekPageShell>
        <DraftEmpty t={t} />
      </WeekPageShell>
    );
  }

  const deptName = departmentLabel(deptCode, t);
  const dayLabel = weekMeta.weekDates[dayIdx] ?? "";

  return (
    <WeekPageShell>
      <WeekDayChips
        weekDates={weekMeta.weekDates}
        todayIdx={weekMeta.todayIdx}
        value={dayIdx}
        onChange={setDayIdx}
      />

      <DeptTabs deptCode={deptCode} onChange={setDeptCode} label={t("phanCongSlot.dept")} t={t} />

      {message ? <StatusBanner variant="success">{message}</StatusBanner> : null}
      {error ? <StatusBanner variant="warn">{error}</StatusBanner> : null}

      <div className={sbStyles.grid}>
        <SlotsColumn
          board={board}
          deptName={deptName}
          dayLabel={dayLabel}
          dayFlights={dayFlights}
          isPast={isPast}
          pickingForSlot={pickingForSlot}
          staffForDept={staffForDept}
          onPickSlot={setPickingForSlot}
          onAssign={onAssign}
          onRemove={(id) => void removeAssignment(id, weekId).then(load)}
          onToggleFlight={toggleFlight}
          t={t}
        />
        <FlightsColumn flights={dayFlights} deptName={deptName} t={t} />
      </div>
    </WeekPageShell>
  );
}

function DraftEmpty({ t }: { t: (k: string) => string }) {
  return (
    <div style={chrome.emptyState}>
      <div style={{ fontSize: 32, marginBottom: 8 }}>○</div>
      <div style={{ fontSize: 16, fontWeight: 600,  }}>
        {t("phanCongSlot.draftEmptyTitle")}
      </div>
    </div>
  );
}

function DeptTabs({
  deptCode,
  onChange,
  label,
  t,
}: {
  deptCode: string;
  onChange: (code: string) => void;
  label: string;
  t: (key: string) => string;
}) {
  return (
    <div style={{ marginBottom: 20 }}>
      <div style={chrome.fieldLabel}>{label}</div>
      <div style={{ display: "flex", gap: 6, flexWrap: "wrap" }}>
        {PLANNING_DEPT_CODES.map((d) => {
          const name = departmentLabel(d, t);
          const selected = deptCode === d;
          return (
            <button
              key={d}
              type="button"
              onClick={() => onChange(d)}
              style={{ ...chrome.dayPill, ...(selected ? chrome.dayPillActive : {}) }}
            >
              {name}
            </button>
          );
        })}
      </div>
    </div>
  );
}

function SlotsColumn({
  board,
  deptName,
  dayLabel,
  dayFlights,
  isPast,
  pickingForSlot,
  staffForDept,
  onPickSlot,
  onAssign,
  onRemove,
  onToggleFlight,
  t,
}: {
  board: PhanCongSlotDto | null;
  deptName: string;
  dayLabel: string;
  dayFlights: PhanCongSlotDto["flights"];
  isPast: boolean;
  pickingForSlot: string | null;
  staffForDept: EmployeeDto[];
  onPickSlot: (id: string | null) => void;
  onAssign: (slotId: string, empId: string) => void;
  onRemove: (assignmentId: string) => void;
  onToggleFlight: (assignmentId: string, flightNo: string, current: string[]) => void;
  t: (k: string, o?: Record<string, string | number>) => string;
}) {
  const slots = board?.slots ?? [];

  return (
    <div>
      <div style={chrome.fieldLabel}>
        {t("phanCongSlot.slotHeader", { dept: deptName, day: dayLabel })}
      </div>
      {slots.length === 0 ? <div style={chrome.emptyState}>{t("phanCongSlot.emptySlots")}</div> : null}
      {slots.map((slot) => {
        const filled = slot.assignments.length;
        const need = slot.headcount;
        const isFull = filled >= need;
        const segLabel = slot.segments.map((s) => s.replace("-", "h-") + "h").join(" + ");
        const ratioStyle = ratioBadgeStyle(filled, need, isFull);

        return (
          <div key={slot.id} style={chrome.slotCard}>
            <div
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                marginBottom: 12,
              }}
            >
              <div style={{ fontSize: 16, fontWeight: 600 }}>{segLabel}</div>
              <span style={ratioStyle}>{t("phanCongSlot.slotRatio", { a: filled, b: need })}</span>
            </div>

            <div style={{ display: "flex", flexDirection: "column", gap: 6 }}>
              {slot.assignments.map((a) => (
                <div key={a.id} style={chrome.assignmentRow}>
                  <div style={chrome.avatarSm}>{avatarInitial(a.employeeName)}</div>
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div style={{ fontSize: 13, fontWeight: 500 }}>{a.employeeName}</div>
                    <div style={{ fontSize: 10, color: "#64748b" }}>{a.employeeCode}</div>
                    {a.flightNos.length > 0 ? (
                      <div style={{ display: "flex", gap: 4, marginTop: 4, flexWrap: "wrap" }}>
                        {a.flightNos.map((fn) => (
                          <span key={fn} style={chrome.flightChip}>
                            {fn}
                          </span>
                        ))}
                      </div>
                    ) : null}
                  </div>
                  {!isPast ? (
                    <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
                      <FlightAssignDropdown
                        flightNos={a.flightNos}
                        flights={dayFlights.filter(
                          (f) =>
                            a.flightNos.includes(f.flightNo) ||
                            flightOverlapsSlotSegments(f, slot.segments),
                        )}
                        onToggle={(fn) => onToggleFlight(a.id, fn, a.flightNos)}
                      />
                      <button type="button" onClick={() => onRemove(a.id)} style={chrome.btnSmGhost}>
                        ✕
                      </button>
                    </div>
                  ) : null}
                </div>
              ))}

              {!isPast && filled < need ? (
                <button
                  type="button"
                  onClick={() => onPickSlot(pickingForSlot === slot.id ? null : slot.id)}
                  style={chrome.addBtn}
                >
                  {t("phanCongSlot.addNv", { n: need - filled })}
                </button>
              ) : null}

              {!isPast && pickingForSlot === slot.id ? (
                <EmpPicker
                  staff={staffForDept}
                  assignedIds={slot.assignments.map((a) => a.employeeId)}
                  onPick={(empId) => void onAssign(slot.id, empId)}
                  t={t}
                />
              ) : null}
            </div>
          </div>
        );
      })}
    </div>
  );
}

function FlightsColumn({
  flights,
  deptName,
  t,
}: {
  flights: PhanCongSlotDto["flights"];
  deptName: string;
  t: (k: string) => string;
}) {
  return (
    <div>
      <div style={chrome.fieldLabel}>
        {t("phanCongSlot.dayFlights")} — {deptName}
      </div>
      <div style={chrome.flightsPanel}>
        {flights.length === 0 ? (
          <div style={{ padding: 16, fontSize: 12, color: "#64748b" }}>
            {t("phanCongSlot.noDayFlights")}
          </div>
        ) : (
          flights.map((f) => (
            <div
              key={f.id}
              style={{
                padding: "10px 12px",
                borderBottom: "1px solid #f1f5f9",
                background: f.isDelayed ? "#fef2f2" : "transparent",
              }}
            >
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "space-between",
                  marginBottom: 2,
                }}
              >
                <span style={{ fontWeight: 600, fontSize: 13 }}>{f.flightNo}</span>
                <span style={{ fontSize: 10, color: "#64748b" }}>{f.route}</span>
              </div>
              <div style={{ fontSize: 11, color: "#475569" }}>
                STA {f.sta} → STD {f.std}
              </div>
              {f.isDelayed && f.delayMinutes > 0 ? (
                <div style={{ fontSize: 11, color: "#dc2626", fontWeight: 600, marginTop: 2 }}>
                  Delay {f.delayMinutes}&apos;
                </div>
              ) : null}
            </div>
          ))
        )}
      </div>
    </div>
  );
}

function EmpPicker({
  staff,
  assignedIds,
  onPick,
  t,
}: {
  staff: EmployeeDto[];
  assignedIds: string[];
  onPick: (empId: string) => void;
  t: (k: string) => string;
}) {
  return (
    <div style={chrome.empPicker}>
      <div
        style={{
          fontSize: 11,
          color: "#64748b",
          marginBottom: 8,
          textTransform: "uppercase",
          letterSpacing: 1,
        }}
      >
        {t("phanCongSlot.pickNv")}
      </div>
      {staff.map((e) => {
        const taken = assignedIds.includes(e.id);
        return (
          <button
            key={e.id}
            type="button"
            disabled={taken}
            onClick={() => onPick(e.id)}
            style={{
              ...chrome.empPickerItem,
              opacity: taken ? 0.4 : 1,
              cursor: taken ? "not-allowed" : "pointer",
            }}
          >
            <div style={chrome.avatarSm}>{avatarInitial(e.name)}</div>
            <div style={{ flex: 1, textAlign: "left" }}>
              <div style={{ fontSize: 13, fontWeight: 500 }}>{e.name}</div>
              <div style={{ fontSize: 10, color: "#64748b" }}>{e.code}</div>
            </div>
          </button>
        );
      })}
    </div>
  );
}

function ratioBadgeStyle(filled: number, _need: number, isFull: boolean): CSSProperties {
  const bg = isFull ? "#dcfce7" : filled === 0 ? "#fee2e2" : "#fef3c7";
  const color = isFull ? "#15803d" : filled === 0 ? "#dc2626" : "#92400e";
  return {
    padding: "4px 10px",
    borderRadius: 4,
    fontSize: 11,
    fontWeight: 600,
    background: bg,
    color,
  };
}

