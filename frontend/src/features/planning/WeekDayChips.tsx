import type { CSSProperties } from "react";
import { useTranslation } from "react-i18next";
import { chrome } from "@/shared/webChrome/styles";
import { isPastDay } from "@/shared/planning/weekCalendar";
import { ALL_WEEK_DAY_IDX, isAllWeekDay } from "@/shared/planning/weekDayFilter";
import styles from "./weekDayChips.module.css";

type WeekDayChipsProps = {
  weekDates: string[];
  todayIdx: number;
  value: number;
  onChange: (dayIdx: number) => void;
  /** When set, renders a “Cả tuần” pill first (chrometype FlightsModule). */
  allWeekCount?: number;
  /** No bottom margin — use inside toolbar with WeekPicker. */
  embedded?: boolean;
  /** Single horizontal row with scroll (Flights toolbar). */
  scrollable?: boolean;
  /** Smaller pills; “Cả tuần” uses short label when set. */
  compact?: boolean;
};

/** Day chips — chọn ngày (Bảng phân ca / Phân công slot). */
export function WeekDayChips({
  weekDates,
  todayIdx,
  value,
  onChange,
  allWeekCount,
  embedded = false,
  scrollable = false,
  compact = false,
}: WeekDayChipsProps) {
  const { t } = useTranslation();

  const rowClass = [
    styles.row,
    embedded ? styles.rowEmbedded : "",
    scrollable ? styles.rowScroll : "",
  ]
    .filter(Boolean)
    .join(" ");

  const chipStyle = (selected: boolean, past: boolean): CSSProperties => ({
    ...chrome.dayPill,
    ...(compact ? { padding: "6px 10px", fontSize: 12 } : {}),
    ...(selected ? chrome.dayPillActive : {}),
    opacity: past && !selected ? 0.6 : 1,
  });

  return (
    <div className={rowClass}>
      {allWeekCount != null ? (
        <button
          type="button"
          title={t("flights.allWeek", { n: allWeekCount })}
          className={styles.chip}
          onClick={() => onChange(ALL_WEEK_DAY_IDX)}
          style={chipStyle(isAllWeekDay(value), false)}
        >
          {compact ? t("flights.allWeekShort", { n: allWeekCount }) : t("flights.allWeek", { n: allWeekCount })}
        </button>
      ) : null}
      {weekDates.map((label, dayIdx) => {
        const past = isPastDay(dayIdx, todayIdx);
        const isToday = dayIdx === todayIdx;
        const selected = !isAllWeekDay(value) && dayIdx === value;

        return (
          <button
            key={dayIdx}
            type="button"
            title={label}
            className={[
              styles.chip,
              compact ? styles.chipCompact : "",
              isToday && !selected ? styles.chipToday : "",
            ]
              .filter(Boolean)
              .join(" ")}
            onClick={() => onChange(dayIdx)}
            style={chipStyle(selected, past)}
          >
            {label}
          </button>
        );
      })}
    </div>
  );
}
