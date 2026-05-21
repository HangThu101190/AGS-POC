import type { StaffingDay } from "@/shared/api/staffingApi";

export type OperationalSegment = "Qn" | "Qt";

/** Mirror backend OperationalSegmentHelper.FromFlight. */
export function inferSegmentFromFlight(
  flight: StaffingDay["flights"][number],
): OperationalSegment {
  const belt = flight.belt?.toUpperCase() ?? "";
  if (belt.includes("QN") || belt.includes("2 -")) return "Qn";
  if (belt.includes("QT") || belt.includes("1 -") || belt.includes("8 -")) return "Qt";
  const route = flight.route.toUpperCase();
  if (
    route.includes("CAN") ||
    route.includes("ICN") ||
    route.includes("SVO") ||
    route.includes("NRT")
  ) {
    return "Qt";
  }
  return "Qn";
}

export function isQnSegment(segment: string | number): boolean {
  return segment === "Qn" || segment === 0;
}

export function isQtSegment(segment: string | number): boolean {
  return segment === "Qt" || segment === 1;
}
