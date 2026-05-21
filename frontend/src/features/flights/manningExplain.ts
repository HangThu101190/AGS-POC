/** Dev-only manning_explain prefixes — never show in UI tooltips. */
const DEV_MANNING_EXPLAIN = /^(mock seed:|excel import:)/i;

export function userFacingManningExplain(value: string | null | undefined): string | undefined {
  const text = value?.trim();
  if (!text || DEV_MANNING_EXPLAIN.test(text)) return undefined;
  return text;
}
