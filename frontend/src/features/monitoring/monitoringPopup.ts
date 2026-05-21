import type { MonitoringMarkerDto } from "@/shared/api/monitoringApi";
import i18n from "@/shared/i18n";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { formatLocaleDateTime } from "@/shared/utils/dateLocale";

function fmtUtc(iso: string) {
  try {
    return formatLocaleDateTime(iso, i18n.language);
  } catch {
    return iso;
  }
}

export function buildMonitoringMarkerPopup(m: MonitoringMarkerDto): string {
  const shift =
    m.shiftSegments.length > 0 ? m.shiftSegments.join(" · ") : "—";
  const flights =
    m.flightNos.length > 0 ? m.flightNos.join(", ") : "—";
  const lines = [
    `<strong>${m.employeeName}</strong> (${m.employeeCode})`,
    `${departmentLabel(m.departmentCode, i18n.t)} · Ca: ${shift}`,
    `Chuyến: ${flights}`,
    m.checkedOut
      ? `<span style="color:#64748b">Đã ra ca ${m.checkOutUtc ? fmtUtc(m.checkOutUtc) : ""}</span>`
      : m.inZone
        ? "Trong vùng"
        : '<span style="color:#b91c1c;font-weight:600">Ngoài vùng</span>',
  ];
  if (m.geoNote) {
    lines.push(`Ghi chú: ${m.geoNote}`);
  }
  return lines.join("<br/>");
}
