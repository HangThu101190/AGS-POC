/**
 * Configured dayjs — import from here, not `dayjs` directly.
 * @see https://day.js.org/docs/en/installation/installation
 */
import dayjsLib, { type ConfigType, type Dayjs } from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import isoWeekPlugin from "dayjs/plugin/isoWeek";
import timezone from "dayjs/plugin/timezone";
import utc from "dayjs/plugin/utc";
import "dayjs/locale/en";
import "dayjs/locale/vi";

/** CXR ops timezone (aligned with backend UTC+7 day boundary). */
export const OPS_TZ = "Asia/Ho_Chi_Minh";

dayjsLib.extend(utc);
dayjsLib.extend(timezone);
dayjsLib.extend(isoWeekPlugin);
dayjsLib.extend(customParseFormat);

export type { ConfigType, Dayjs };
export const dayjs = dayjsLib;

export function dayjsLocaleForLang(lang?: string): "en" | "vi" {
  return lang?.startsWith("en") ? "en" : "vi";
}

export function withDayjsLocale(input: ConfigType | undefined, lang?: string): Dayjs {
  const locale = dayjsLocaleForLang(lang);
  return (input == null ? dayjs() : dayjs(input)).locale(locale);
}

/** Now in ops timezone. */
export function opsNow(): Dayjs {
  return dayjs().tz(OPS_TZ);
}

/** Start of calendar day in ops timezone. */
export function opsDay(input?: ConfigType): Dayjs {
  const base = input == null ? opsNow() : dayjs(input);
  return base.tz(OPS_TZ).startOf("day");
}

type DayjsWithIsoWeek = Dayjs & {
  isoWeekYear(year: number): Dayjs;
  isoWeek(week: number): Dayjs;
};

/** Monday 00:00 of an ISO week in ops timezone. */
export function isoWeekMonday(isoYear: number, isoWeekNum: number): Dayjs {
  const d = dayjs().tz(OPS_TZ) as DayjsWithIsoWeek;
  d.isoWeekYear(isoYear);
  return d.isoWeek(isoWeekNum).startOf("isoWeek");
}
