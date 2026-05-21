import type { ConfigType } from "@/shared/utils/dayjs";
import { dayjs, dayjsLocaleForLang, withDayjsLocale } from "@/shared/utils/dayjs";

/** BCP 47 locale for Intl (legacy / third-party). */
export function dateLocaleForLang(lang?: string): string {
  return dayjsLocaleForLang(lang) === "en" ? "en-US" : "vi-VN";
}

/** `DD/MM/YYYY HH:mm` — matches prior `toLocaleString` vi-VN / en-US display. */
export function formatLocaleDateTime(value: ConfigType, lang?: string): string {
  return withDayjsLocale(value, lang).format("DD/MM/YYYY HH:mm");
}

export function formatLocaleDate(value: ConfigType, lang?: string): string {
  return withDayjsLocale(value, lang).format("DD/MM");
}

export function formatLocaleTime(value: ConfigType, lang?: string): string {
  return withDayjsLocale(value, lang).format("HH:mm");
}

/** `<input type="datetime-local" />` value from ISO UTC string. */
export function toDatetimeLocalValue(isoUtc: string): string {
  return dayjs(isoUtc).format("YYYY-MM-DDTHH:mm");
}

/** Parse datetime-local input → ISO UTC. */
export function datetimeLocalToUtcIso(localValue: string): string {
  return dayjs(localValue).toISOString();
}

/** Start of browser-local today as ISO UTC (audit filter default). */
export function startOfTodayUtcIso(): string {
  return dayjs().startOf("day").utc().toISOString();
}
