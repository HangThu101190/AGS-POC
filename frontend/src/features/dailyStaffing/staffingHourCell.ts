/** Inline SVG (no @mui/icons-material) for FullCalendar eventContent HTML. */
const ICON_PEOPLE = `<svg class="staffing-hour-cell__icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M16 11c1.66 0 2.99-1.34 2.99-3S17.66 5 16 5s-3 1.34-3 3 1.34 3 3 3zm-8 0c1.66 0 2.99-1.34 2.99-3S9.66 5 8 5 5 6.34 5 8s1.34 3 3 3zm0 2c-2.33 0-7 1.17-7 3.5V19h14v-2.5c0-2.33-4.67-3.5-7-3.5zm8 0c-.29 0-.62.02-.97.05 1.16.84 1.97 1.97 1.97 3.45V19h6v-2.5c0-2.33-4.67-3.5-7-3.5z"/></svg>`;

const ICON_PLANE = `<svg class="staffing-hour-cell__icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M21 16v-2l-8-5V3.5c0-.83-.67-1.5-1.5-1.5S10 2.67 10 3.5V9l-8 5v2l8-2.5V19l-2 1.5V22l3.5-1 3.5 1v-1.5L13 19v-5.5l8 2.5z"/></svg>`;

export type HourSlotMix = "qn" | "qt" | "both" | "none";

export type HourSlotStats = {
  dayIdx: number;
  hour: number;
  staffCount: number;
  flightCount: number;
  mix: HourSlotMix;
};

/** Quốc tế — vàng nhạt kiểu PVHK Excel (#FFF5CC nền, viền amber). */
export const STAFFING_QT_COLORS = {
  fill: "#FFF5CC",
  accent: "#D97706",
  border: "#FCD34D",
  text: "#5c4a14",
} as const;

export const HOUR_SLOT_THEME: Record<
  HourSlotMix,
  { backgroundColor: string; borderColor: string; textColor: string; className: string }
> = {
  qn: {
    backgroundColor: "#4f8ff7",
    borderColor: "transparent",
    textColor: "#ffffff",
    className: "staffing-mix-qn",
  },
  qt: {
    backgroundColor: STAFFING_QT_COLORS.fill,
    borderColor: STAFFING_QT_COLORS.accent,
    textColor: STAFFING_QT_COLORS.text,
    className: "staffing-mix-qt",
  },
  both: {
    backgroundColor: "#9b7aea",
    borderColor: "transparent",
    textColor: "#ffffff",
    className: "staffing-mix-both",
  },
  none: {
    backgroundColor: "#94a3b8",
    borderColor: "transparent",
    textColor: "#ffffff",
    className: "staffing-mix-none",
  },
};

export function buildHourCellHtml(stats: HourSlotStats): string {
  return `<div class="staffing-hour-cell" role="presentation">
  <div class="staffing-hour-cell__line">${ICON_PEOPLE}<span class="staffing-hour-cell__num">${stats.staffCount}</span></div>
  <div class="staffing-hour-cell__line">${ICON_PLANE}<span class="staffing-hour-cell__num">${stats.flightCount}</span></div>
</div>`;
}
