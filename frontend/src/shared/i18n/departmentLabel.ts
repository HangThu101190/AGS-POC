import { DEPT_DISPLAY } from "@/shared/constants/departments";

type TranslateFn = (key: string, options?: Record<string, unknown>) => string;

/** Human-readable department name for UI — never show raw codes like PVHK_DI. */
export function departmentLabel(code: string | null | undefined, t: TranslateFn): string {
  if (!code) return "—";
  return t(`dept.${code}`, { defaultValue: DEPT_DISPLAY[code] ?? code });
}

/** Label + internal code (tooltip / title only). */
export function departmentLabelWithCode(code: string | null | undefined, t: TranslateFn): string {
  if (!code) return "—";
  const name = departmentLabel(code, t);
  if (name === code) return name;
  return t("dept.codeTip", { name, code, defaultValue: `${name} (${code})` });
}

export function departmentLabelFromDto(
  dept: { code: string; name?: string | null } | null | undefined,
  t: TranslateFn,
): string {
  if (!dept?.code) return "—";
  return departmentLabel(dept.code, t);
}
