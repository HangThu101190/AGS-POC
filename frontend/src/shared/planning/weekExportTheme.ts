/** Excel TH CONG segment cell colors — from prototype WEEK_EXPORT_THEME. */

export const WEEK_EXPORT_THEME = {
  header: "F2E5BC",
  night_shift: "50D092",
  full_shift: "F0B000",
  afternoon_shift: "00C0FF",
  special_shift: "A03070",
  light_bg: "D6E4FC",
  primary: "1159C6",
  white: "FFFFFF",
  weekoff: "CCF2FF",
  holiday: "ADCBF8",
  titleRed: "B91C1C",
} as const;

function relLuminance(hex: string): number {
  const h = hex.replace("#", "").trim();
  if (h.length !== 6) return 1;
  const r = parseInt(h.slice(0, 2), 16) / 255;
  const g = parseInt(h.slice(2, 4), 16) / 255;
  const b = parseInt(h.slice(4, 6), 16) / 255;
  const lin = (v: number) => (v <= 0.03928 ? v / 12.92 : ((v + 0.055) / 1.055) ** 2.4);
  return 0.2126 * lin(r) + 0.7152 * lin(g) + 0.0722 * lin(b);
}

export function weekExportSegTextColor(bgHex: string): string {
  const raw = bgHex.replace("#", "").trim();
  const h = raw.length === 6 ? raw : WEEK_EXPORT_THEME.white;
  return relLuminance(`#${h}`) < 0.52 ? "#ffffff" : "#0f172a";
}

function segmentStats(seg: string) {
  const [sStr, eRaw] = seg.split("-");
  const s = parseInt(sStr, 10);
  let e = parseInt(String(eRaw).replace("+", ""), 10);
  if (!Number.isFinite(s) || !Number.isFinite(e)) return { dayH: 0, nightH: 0, start: 0, span: 0 };
  if (e <= s) e += 24;
  let dayH = 0;
  let nightH = 0;
  for (let h = s; h < e; h++) {
    const hour = h % 24;
    if (hour >= 22 || hour < 6) nightH++;
    else dayH++;
  }
  return { dayH, nightH, start: ((s % 24) + 24) % 24, span: e - s };
}

export type ReconcilePreviewDay = {
  actualCode: string;
  actualType: string;
  plannedSegments: string[];
  actualSegments: string[];
  hasRevisionFlag: boolean;
};

/** Background hex for segment cell (prototype weekExportShiftFillHex). */
export function weekExportShiftFillHex(day: ReconcilePreviewDay | undefined): string {
  if (!day) return `#${WEEK_EXPORT_THEME.white}`;
  if (day.actualType === "weekoff") return `#${WEEK_EXPORT_THEME.weekoff}`;
  if (day.actualType === "holiday") return `#${WEEK_EXPORT_THEME.holiday}`;
  const segs = (day.actualSegments.length ? day.actualSegments : day.plannedSegments).filter(Boolean);
  if (!segs.length) return `#${WEEK_EXPORT_THEME.white}`;
  if (segs.length > 1) return `#${WEEK_EXPORT_THEME.special_shift}`;
  const { dayH, nightH, start, span } = segmentStats(segs[0]);
  if (nightH > dayH) return `#${WEEK_EXPORT_THEME.night_shift}`;
  if (start >= 12) return `#${WEEK_EXPORT_THEME.afternoon_shift}`;
  if (span >= 10 || (start <= 8 && span >= 8)) return `#${WEEK_EXPORT_THEME.full_shift}`;
  return `#${WEEK_EXPORT_THEME.full_shift}`;
}

export function formatSegCell(day: ReconcilePreviewDay | undefined): string {
  if (!day) return "";
  if (day.actualType === "weekoff") return "T";
  if (day.actualType === "holiday") return "L";
  const segs = day.actualSegments.length ? day.actualSegments : day.plannedSegments;
  if (!segs.length) return day.actualCode || "";
  const s = segs
    .map((seg) => {
      const [a, b] = seg.split("-");
      const aa = String(a).padStart(2, "0");
      const bb = String(b).replace("+", "");
      return `${aa}h00-${String(bb).padStart(2, "0")}h00`;
    })
    .join(" / ");
  return day.hasRevisionFlag ? `${s} SĐ` : s;
}
