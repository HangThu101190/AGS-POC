import type { ValueFormatterParams } from "ag-grid-community";

export function isEmptyCellValue(value: unknown): boolean {
  if (value == null) return true;
  if (typeof value === "string") return value.trim() === "";
  return false;
}

/** Plain text cells: null / "" → blank (do not substitute em dash). */
export function formatEmptyCell(value: unknown): string {
  return isEmptyCellValue(value) ? "" : String(value);
}

export function gridEmptyValueFormatter<T>(
  params: ValueFormatterParams<T, unknown>,
): string {
  return formatEmptyCell(params.value);
}

/** Run a formatter only when the raw value is present. */
export function gridValueFormatter<T>(
  params: ValueFormatterParams<T, unknown>,
  format: (params: ValueFormatterParams<T, unknown>) => string,
): string {
  if (isEmptyCellValue(params.value)) return "";
  return format(params);
}
