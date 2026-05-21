import type { StaffingDay } from "@/shared/api/staffingApi";
import { isQnSegment, isQtSegment } from "@/features/dailyStaffing/staffingSegment";

export type RosterQnRow = {
  key: string;
  lineId: string | null;
  stt: number;
  flightNo: string;
  dest: string;
  aircraft: string;
  std: string;
  counter: string;
  gate: string;
  manningLabel: string;
  rowStatus: "empty" | "partial" | "full";
};

export type RosterQtRow = {
  key: string;
  lineId: string;
  stt: number;
  flightNo: string;
  dest: string;
  std: string;
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

function nameForRole(
  day: StaffingDay,
  lineId: string,
  role: "Counter" | "Gate",
): string {
  const match = day.assignments.find(
    (a) => a.staffingLineId === lineId && a.role === role,
  );
  return match?.employeeName ?? "";
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
      return {
        key: line.id,
        lineId: line.id,
        stt: i + 1,
        flightNo: flt,
        dest: flight ? destFromRoute(flight.route) : "—",
        aircraft: formatAircraft(flight?.aircraft),
        std: flight?.std ?? "—",
        counter: nameForRole(day, line.id, "Counter"),
        gate: nameForRole(day, line.id, "Gate"),
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
      std: flight.std,
      counter: "",
      gate: "",
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
      return {
        key: line.id,
        lineId: line.id,
        stt: i + 1,
        flightNo: flight?.departureFlightNo ?? flight?.flightNo ?? "—",
        dest: flight ? destFromRoute(flight.route) : "—",
        std: flight?.std ?? "—",
      };
    });
}
