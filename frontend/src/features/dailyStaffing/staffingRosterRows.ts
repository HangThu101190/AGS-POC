import type { StaffingDay } from "@/shared/api/staffingApi";
import { isQnSegment, isQtSegment } from "@/features/dailyStaffing/staffingSegment";

export type RosterQnRow = {
  key: string;
  lineId: string | null;
  stt: number;
  flightNo: string;
  dest: string;
  aircraft: string;
  etd: string;
  counter: string;
  gate: string;
  counterEmployeeId: string | null;
  gateEmployeeId: string | null;
  counterAssignmentId: string | null;
  gateAssignmentId: string | null;
  manningLabel: string;
  rowStatus: "empty" | "partial" | "full";
};

export type RosterQtRow = {
  key: string;
  lineId: string;
  stt: number;
  flightNo: string;
  dest: string;
  etd: string;
  sup: string;
  supEmployeeId: string | null;
  supAssignmentId: string | null;
  staffCount: string;
};

/** One physical row in PVHK Excel (QN cols 1–7 + QT cols 8–13). */
export type StaffingExcelCombinedRow = {
  key: string;
  qnStt: number | null;
  qnLineId: string | null;
  qnFlightNo: string;
  qnDest: string;
  qnAircraft: string;
  qnEtd: string;
  qnCounterEmployeeId: string | null;
  qnGateEmployeeId: string | null;
  qnCounterAssignmentId: string | null;
  qnGateAssignmentId: string | null;
  qtStt: number | null;
  qtLineId: string | null;
  qtFlightNo: string;
  qtDest: string;
  qtEtd: string;
  qtSupEmployeeId: string | null;
  qtSupAssignmentId: string | null;
  qtStaffCount: string;
};

export function destFromRoute(route: string): string {
  const parts = route.split("-").map((p) => p.trim()).filter(Boolean);
  return parts.length >= 2 ? (parts.at(-1) ?? route) : route;
}

export function formatAircraft(aircraft?: string | null): string {
  if (!aircraft?.trim()) return "";
  const a = aircraft.trim().toUpperCase();
  return a.startsWith("A") ? a : `A${a}`;
}

function assignmentForRole(
  day: StaffingDay,
  lineId: string,
  role: "Counter" | "Gate" | "Sup",
) {
  return day.assignments.find(
    (a) => a.staffingLineId === lineId && a.role === role,
  );
}

/** Matches PvhkDailyAssignmentExporter.FormatEtd. */
export function formatStaffingEtd(
  flight: StaffingDay["flights"][number] | undefined,
): string {
  if (!flight) return "—";
  const etd = flight.etd?.trim() || flight.std;
  if (
    flight.isDelayed ||
    (flight.etdDelayMinutes ?? 0) > 0 ||
    (flight.delayMinutes ?? 0) > 0
  ) {
    return `${etd} đc`;
  }
  return etd;
}

function qnRowStatus(
  line: StaffingDay["lines"][number],
  assigns: StaffingDay["assignments"],
): RosterQnRow["rowStatus"] {
  const onLine = assigns.filter((a) => a.staffingLineId === line.id);
  const hasCounter = onLine.some((a) => a.role === "Counter");
  const hasGate = onLine.some((a) => a.role === "Gate");
  const need = Math.max(line.targetManning, 2);
  const filled = (hasCounter ? 1 : 0) + (hasGate ? 1 : 0) + onLine.filter((a) => a.role === "General" || a.role === "Sup").length;
  if (filled >= need || (hasCounter && hasGate)) return "full";
  if (filled > 0 || hasCounter || hasGate) return "partial";
  return "empty";
}

/** Mirrors PVHK Excel QN block (STT, SHCB, DEST, A/C, ETD, Quầy, Gate). */
export function buildQnRosterRows(day: StaffingDay): RosterQnRow[] {
  const flightById = new Map(day.flights.map((f) => [f.id, f]));
  const qnLines = day.lines
    .filter((l) => isQnSegment(l.segment as string | number))
    .sort((a, b) => a.sortOrder - b.sortOrder);

  if (qnLines.length > 0) {
    return qnLines.map((line, i) => {
      const flight = flightById.get(line.flightId);
      const flt = flight?.departureFlightNo ?? flight?.flightNo ?? "—";
      const assigned = day.assignments.filter((a) => a.staffingLineId === line.id).length;
      const counterA = assignmentForRole(day, line.id, "Counter");
      const gateA = assignmentForRole(day, line.id, "Gate");
      return {
        key: line.id,
        lineId: line.id,
        stt: i + 1,
        flightNo: flt,
        dest: flight ? destFromRoute(flight.route) : "—",
        aircraft: formatAircraft(flight?.aircraft),
        etd: formatStaffingEtd(flight),
        counter: counterA?.employeeName ?? "",
        gate: gateA?.employeeName ?? "",
        counterEmployeeId: counterA?.employeeId ?? null,
        gateEmployeeId: gateA?.employeeId ?? null,
        counterAssignmentId: counterA?.id ?? null,
        gateAssignmentId: gateA?.id ?? null,
        manningLabel: `${assigned}/${line.targetManning}`,
        rowStatus: qnRowStatus(line, day.assignments),
      };
    });
  }

  return [...day.flights]
    .sort((a, b) => a.std.localeCompare(b.std))
    .map((flight, i) => ({
      key: flight.id,
      lineId: null,
      stt: i + 1,
      flightNo: flight.departureFlightNo ?? flight.flightNo,
      dest: destFromRoute(flight.route),
      aircraft: formatAircraft(flight.aircraft),
      etd: formatStaffingEtd(flight),
      counter: "",
      gate: "",
      counterEmployeeId: null,
      gateEmployeeId: null,
      counterAssignmentId: null,
      gateAssignmentId: null,
      manningLabel: `0/${flight.manning || 0}`,
      rowStatus: "empty" as const,
    }));
}

/** Mirrors PVHK Excel QT block (STT, SHCB, DEST, ETD). */
export function buildQtRosterRows(day: StaffingDay): RosterQtRow[] {
  const flightById = new Map(day.flights.map((f) => [f.id, f]));
  return day.lines
    .filter((l) => isQtSegment(l.segment as string | number))
    .sort((a, b) => a.sortOrder - b.sortOrder)
    .map((line, i) => {
      const flight = flightById.get(line.flightId);
      const supA = assignmentForRole(day, line.id, "Sup");
      const onLine = day.assignments.filter((a) => a.staffingLineId === line.id);
      return {
        key: line.id,
        lineId: line.id,
        stt: i + 1,
        flightNo: flight?.departureFlightNo ?? flight?.flightNo ?? "—",
        dest: flight ? destFromRoute(flight.route) : "—",
        etd: formatStaffingEtd(flight),
        sup: supA?.employeeName ?? "",
        supEmployeeId: supA?.employeeId ?? null,
        supAssignmentId: supA?.id ?? null,
        staffCount: onLine.length > 0 ? String(onLine.length) : String(line.targetManning),
      };
    });
}

/** Align QN and QT blocks on the same row index like the Excel template. */
export function buildCombinedExcelRows(day: StaffingDay): StaffingExcelCombinedRow[] {
  const qn = buildQnRosterRows(day);
  const qt = buildQtRosterRows(day);
  const n = Math.max(qn.length, qt.length, 1);
  const emptyQn = (): Pick<
    StaffingExcelCombinedRow,
    | "qnStt"
    | "qnLineId"
    | "qnFlightNo"
    | "qnDest"
    | "qnAircraft"
    | "qnEtd"
    | "qnCounterEmployeeId"
    | "qnGateEmployeeId"
    | "qnCounterAssignmentId"
    | "qnGateAssignmentId"
  > => ({
    qnStt: null,
    qnLineId: null,
    qnFlightNo: "",
    qnDest: "",
    qnAircraft: "",
    qnEtd: "",
    qnCounterEmployeeId: null,
    qnGateEmployeeId: null,
    qnCounterAssignmentId: null,
    qnGateAssignmentId: null,
  });
  const emptyQt = (): Pick<
    StaffingExcelCombinedRow,
    | "qtStt"
    | "qtLineId"
    | "qtFlightNo"
    | "qtDest"
    | "qtEtd"
    | "qtSupEmployeeId"
    | "qtSupAssignmentId"
    | "qtStaffCount"
  > => ({
    qtStt: null,
    qtLineId: null,
    qtFlightNo: "",
    qtDest: "",
    qtEtd: "",
    qtSupEmployeeId: null,
    qtSupAssignmentId: null,
    qtStaffCount: "",
  });

  return Array.from({ length: n }, (_, i) => {
    const q = qn[i];
    const t = qt[i];
    return {
      key: `excel-${i}-${q?.key ?? ""}-${t?.key ?? ""}`,
      ...(q
        ? {
            qnStt: q.stt,
            qnLineId: q.lineId,
            qnFlightNo: q.flightNo,
            qnDest: q.dest,
            qnAircraft: q.aircraft,
            qnEtd: q.etd,
            qnCounterEmployeeId: q.counterEmployeeId,
            qnGateEmployeeId: q.gateEmployeeId,
            qnCounterAssignmentId: q.counterAssignmentId,
            qnGateAssignmentId: q.gateAssignmentId,
          }
        : emptyQn()),
      ...(t
        ? {
            qtStt: t.stt,
            qtLineId: t.lineId,
            qtFlightNo: t.flightNo,
            qtDest: t.dest,
            qtEtd: t.etd,
            qtSupEmployeeId: t.supEmployeeId,
            qtSupAssignmentId: t.supAssignmentId,
            qtStaffCount: t.staffCount,
          }
        : emptyQt()),
    };
  });
}
