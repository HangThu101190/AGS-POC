import { PLANNING_DEPT_CODES } from "@/shared/constants/departments";

/** Operational dept for daily staffing API — HR/TBDH home dept is HCNS, not PVHK. */
export function resolveStaffingDepartmentCode(userDepartmentCode?: string | null): string {
  const code = (userDepartmentCode ?? "").trim().toUpperCase();
  if ((PLANNING_DEPT_CODES as readonly string[]).includes(code)) return code;
  return "PVHK_DI";
}
