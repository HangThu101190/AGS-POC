/** ISO week helpers — aligned with backend `WeekCalendar` (UTC+7 ops day). */

import type { ConfigType } from "@/shared/utils/dayjs";
import { isoWeekMonday, opsDay } from "@/shared/utils/dayjs";

export type WeekKind = "past" | "current" | "future";

export type WeekMeta = {
  weekId: string;
  isoYear: number;
  isoWeek: number;
  weekDates: string[];
  dateFrom: string;
  dateTo: string;
  calendarYear: number;
  kind: WeekKind;
  todayIdx: number;
};

/** Local calendar date for CXR ops (UTC+7), date-only — `Date` at ops midnight. */
export function vietnamLocalDate(base?: ConfigType): Date {
  return opsDay(base).toDate();
}

export function currentWeekId(base?: ConfigType): string {
  const d = opsDay(base);
  return formatWeekId(d.isoWeekYear(), d.isoWeek());
}

export function parseWeekId(weekId: string): { year: number; week: number } | null {
  const m = /^(\d{4})-W(\d{2})$/i.exec(weekId.trim());
  if (!m) return null;
  const year = Number(m[1]);
  const week = Number(m[2]);
  if (!Number.isFinite(year) || week < 1 || week > 53) return null;
  return { year, week };
}

export function formatWeekId(year: number, week: number): string {
  return `${year}-W${String(week).padStart(2, "0")}`;
}

export function addWeeks(weekId: string, delta: number): string {
  const parsed = parseWeekId(weekId) ?? parseWeekId(currentWeekId())!;
  const shifted = isoWeekMonday(parsed.year, parsed.week).add(delta, "week");
  return formatWeekId(shifted.isoWeekYear(), shifted.isoWeek());
}

/** DD/MM/YYYY for modals and exports (weekDates are DD/MM only). */
export function formatWeekDayFullDate(weekMeta: WeekMeta, dayIdx: number): string {
  const label = weekMeta.weekDates[dayIdx] ?? weekMeta.weekDates[0] ?? "";
  const [d, m] = label.split("/");
  if (!d || !m) return label;
  return `${d}/${m}/${weekMeta.calendarYear}`;
}

export function buildWeekMeta(weekId: string, reference?: ConfigType): WeekMeta | null {
  const parsed = parseWeekId(weekId);
  if (!parsed) return null;

  const ref = opsDay(reference);
  const monday = isoWeekMonday(parsed.year, parsed.week);
  const sunday = monday.add(6, "day");

  const weekDates = Array.from({ length: 7 }, (_, i) =>
    monday.add(i, "day").format("DD/MM"),
  );

  const current = parseWeekId(currentWeekId(reference))!;
  let kind: WeekKind = "current";
  if (parsed.year < current.year || (parsed.year === current.year && parsed.week < current.week)) {
    kind = "past";
  } else if (parsed.year > current.year || (parsed.year === current.year && parsed.week > current.week)) {
    kind = "future";
  }

  let todayIdx: number;
  if (ref.isBefore(monday, "day")) {
    todayIdx = -1;
  } else if (ref.isAfter(sunday, "day")) {
    todayIdx = 7;
  } else {
    todayIdx = ref.isoWeekday() - 1;
  }

  return {
    weekId: formatWeekId(parsed.year, parsed.week),
    isoYear: parsed.year,
    isoWeek: parsed.week,
    weekDates,
    dateFrom: weekDates[0],
    dateTo: weekDates[6],
    calendarYear: sunday.year(),
    kind,
    todayIdx,
  };
}

/** Past / current / future week options for pickers. */
export function listWeekOptions(
  reference?: ConfigType,
  pastCount = 16,
  futureCount = 8,
): { past: WeekMeta[]; current: WeekMeta; future: WeekMeta[] } {
  const currentId = currentWeekId(reference);
  const past: WeekMeta[] = [];
  for (let i = pastCount; i >= 1; i -= 1) {
    const meta = buildWeekMeta(addWeeks(currentId, -i), reference);
    if (meta) past.push(meta);
  }
  const current = buildWeekMeta(currentId, reference)!;
  const future: WeekMeta[] = [];
  for (let i = 1; i <= futureCount; i += 1) {
    const meta = buildWeekMeta(addWeeks(currentId, i), reference);
    if (meta) future.push(meta);
  }
  return { past, current, future };
}

export function isPastDay(dayIdx: number, todayIdx: number): boolean {
  return Number.isInteger(dayIdx) && Number.isInteger(todayIdx) && dayIdx < todayIdx;
}

export function isFutureDay(dayIdx: number, todayIdx: number): boolean {
  return Number.isInteger(dayIdx) && Number.isInteger(todayIdx) && todayIdx >= 0 && dayIdx > todayIdx;
}

/** TBĐH may add/import/edit flights on future days even after schedule publish. */
export function isFutureFlightContext(
  weekKind: WeekKind,
  allWeek: boolean,
  selectedDay: number,
  todayIdx: number,
): boolean {
  if (weekKind === "future") return true;
  return !allWeek && isFutureDay(selectedDay, todayIdx);
}

/** Day index for slot filters when the viewed week is not the operational current week. */
export function slotCutoffDayIdx(todayIdx: number): number {
  return todayIdx >= 0 && todayIdx <= 6 ? todayIdx : 0;
}

const DAY_SHORT_VI = ["T2", "T3", "T4", "T5", "T6", "T7", "CN"] as const;

export function dayShortLabel(dayIdx: number): string {
  const i = Math.max(0, Math.min(6, Number(dayIdx) || 0));
  return DAY_SHORT_VI[i];
}
