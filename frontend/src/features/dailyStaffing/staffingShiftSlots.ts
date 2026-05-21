/** Lưới lịch tuần: 3 ca × 8h/ngày (0–8, 8–16, 16–24). `shiftStartHour` = giờ bắt đầu ca. */

export const SHIFT_SLOT_DURATION_HOURS = 8;

export const SHIFT_SLOT_START_HOURS = [0, 8, 16] as const;

export type ShiftSlotStartHour = (typeof SHIFT_SLOT_START_HOURS)[number];

export function shiftSlotEndHour(shiftStartHour: number): number {
  return shiftStartHour + SHIFT_SLOT_DURATION_HOURS;
}

export function shiftSlotTimeLabel(shiftStartHour: number): string {
  const end = shiftSlotEndHour(shiftStartHour);
  const startLabel = String(shiftStartHour).padStart(2, "0");
  const endLabel = end >= 24 ? "24" : String(end).padStart(2, "0");
  return `${startLabel}h–${endLabel}h`;
}

function parseMinutes(hhmm: string): number {
  const [h, m] = hhmm.split(":").map((x) => Number(x));
  return (h ?? 0) * 60 + (m ?? 0);
}

/** STA/STD, workStart/workEnd, assignment… có giao với ca 8h bắt đầu tại `shiftStartHour`. */
export function overlapsShiftSlot(
  rangeStart: string,
  rangeEnd: string,
  shiftStartHour: number,
): boolean {
  const startM = parseMinutes(rangeStart);
  let endM = parseMinutes(rangeEnd);
  if (endM <= startM) endM += 24 * 60;
  const slotStart = shiftStartHour * 60;
  const slotEnd = shiftSlotEndHour(shiftStartHour) * 60;
  return startM < slotEnd && endM > slotStart;
}

export function currentShiftStartHour(now = new Date()): ShiftSlotStartHour {
  const h = now.getHours();
  if (h < 8) return 0;
  if (h < 16) return 8;
  return 16;
}

export function isCurrentShiftSlot(slotDate: Date, now = new Date()): boolean {
  return slotDate.getHours() === currentShiftStartHour(now);
}

export function scrollTimeForShiftGrid(): string {
  const start = currentShiftStartHour();
  return `${String(start).padStart(2, "0")}:00:00`;
}

export function clampTimeToShift(
  hhmm: string,
  shiftStartHour: number,
): string {
  const m = parseMinutes(hhmm);
  const min = shiftStartHour * 60;
  const max = shiftSlotEndHour(shiftStartHour) * 60;
  const clamped = Math.max(min, Math.min(max - 1, m));
  const h = Math.floor(clamped / 60);
  const mm = clamped % 60;
  return `${String(h).padStart(2, "0")}:${String(mm).padStart(2, "0")}`;
}
