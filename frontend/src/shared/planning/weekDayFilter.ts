/** Prototype `dayFilter === -1` — show flights for the whole week. */
export const ALL_WEEK_DAY_IDX = -1;

export function isAllWeekDay(dayIdx: number): boolean {
  return dayIdx === ALL_WEEK_DAY_IDX;
}
