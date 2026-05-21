import type { AppRole } from "@/shared/auth/types";
import {
  appLangFromI18n,
  routeKeyFromPathname,
  routePathFor,
} from "@/shared/routing/routePaths";

/** Nav / route keys aligned with HTML Web + Shift Leader tablet modules. */
export type AppRouteKey =
  | "dashboard"
  | "flights"
  | "dailyStaffing"
  | "phanCongSlot"
  | "monitoring"
  | "reconcile"
  | "people"
  | "audit"
  | "config"
  | "profile"
  | "settings";

export function isHrRole(role: AppRole): boolean {
  return role === "hr";
}

export function isSupRole(role: AppRole): boolean {
  return role === "sup";
}

export function isTbdhRole(role: AppRole): boolean {
  return role === "tbdh";
}

export function isShiftLeaderRole(role: AppRole): boolean {
  return role === "shift_leader";
}

export function isStaffRole(role: AppRole): boolean {
  return role === "staff";
}

/** Tầng 0 — TBĐH only (upload / delay / CRUD). */
export function canManageFlights(role: AppRole): boolean {
  return isTbdhRole(role);
}

/** Tầng 1 — Shift Leader only (WeeklyPlan wizard). */
export function canManageWeeklyPlan(role: AppRole): boolean {
  return isSupRole(role);
}

/** Read-only lịch ca: HR đối soát, TBĐH giám sát, Sup quản lý. */
export function canViewWeeklyPlan(role: AppRole): boolean {
  return isSupRole(role) || isHrRole(role) || isTbdhRole(role);
}

export function canViewFlights(role: AppRole): boolean {
  return isTbdhRole(role) || isHrRole(role) || isSupRole(role) || isShiftLeaderRole(role);
}

export function canManageMasterData(role: AppRole): boolean {
  return isHrRole(role);
}

export function canAccessMonitoring(role: AppRole): boolean {
  return isHrRole(role);
}

export function canAccessReconcile(role: AppRole): boolean {
  return isHrRole(role);
}

/** Phân công slot (Sup) — web `/phan-cong-slot`. */
export function canAccessPhanCongSlot(role: AppRole): boolean {
  return isSupRole(role);
}

export function canAccessAudit(role: AppRole): boolean {
  return isHrRole(role);
}

/** Cấu hình: quy tắc định biên, geofence, giờ check-in — không phải lịch phân công ngày. */
export function canAccessConfig(role: AppRole): boolean {
  return isHrRole(role);
}

/** Bảng phân ca (PVHK) — lịch tuần, đề xuất định biên, gán NV. */
export function canViewStaffingWeek(role: AppRole): boolean {
  return isShiftLeaderRole(role) || isHrRole(role) || isTbdhRole(role);
}

export function canManageDailyStaffing(role: AppRole): boolean {
  return isShiftLeaderRole(role) || isHrRole(role);
}

export function canEditStaffingManning(role: AppRole): boolean {
  return isTbdhRole(role) || isHrRole(role);
}

/** Route → any of these permission codes grants access (DB catalog). */
const ROUTE_PERMISSIONS: Partial<Record<AppRouteKey, readonly string[]>> = {
  flights: ["flights.import", "flights.delay", "flights.crud"],
  dailyStaffing: ["plan.publish", "staffing.view"],
  phanCongSlot: ["assignments.mutate"],
  monitoring: ["monitoring.view"],
  reconcile: ["reconcile.export"],
  people: ["master.employees", "master.departments"],
  audit: ["audit.view"],
  config: ["config.workzone", "config.manning_rules", "config.shift_attendance", "staffing.view"],
};

export function hasPermission(permissions: readonly string[] | undefined, code: string): boolean {
  return permissions?.includes(code) ?? false;
}

export function hasAnyPermission(
  permissions: readonly string[] | undefined,
  codes: readonly string[],
): boolean {
  if (!permissions?.length) return false;
  return codes.some((c) => permissions.includes(c));
}

function canAccessRouteByPermissions(permissions: readonly string[] | undefined, route: AppRouteKey): boolean | null {
  const codes = ROUTE_PERMISSIONS[route];
  if (!codes) return null;
  if (!permissions?.length) return null;
  return hasAnyPermission(permissions, codes);
}

/** Web route access — uses DB permissions when present, else role matrix fallback. */
export function canAccessRoute(
  role: AppRole,
  route: AppRouteKey,
  permissions?: readonly string[],
): boolean {
  const fromCatalog = canAccessRouteByPermissions(permissions, route);
  if (fromCatalog !== null) return fromCatalog;

  switch (route) {
    case "dashboard":
      return true;
    case "flights":
      return canViewFlights(role);
    case "dailyStaffing":
      return canViewStaffingWeek(role);
    case "phanCongSlot":
      return canAccessPhanCongSlot(role);
    case "monitoring":
      return canAccessMonitoring(role);
    case "reconcile":
      return canAccessReconcile(role);
    case "people":
      return canManageMasterData(role);
    case "audit":
      return canAccessAudit(role);
    case "config":
      return canAccessConfig(role);
    case "profile":
    case "settings":
      return true;
    default:
      return false;
  }
}

/** Default landing after login (matches primary module per actor). */
export function getRoleHomePath(role: AppRole, lang?: string): string {
  const l = appLangFromI18n(lang ?? (typeof localStorage !== "undefined" ? localStorage.getItem("ags.lang") ?? "vi" : "vi"));
  switch (role) {
    case "tbdh":
      return routePathFor("flights", l);
    case "shift_leader":
      return routePathFor("dailyStaffing", l);
    case "sup":
      return "/mobile/plan";
    case "hr":
      return routePathFor("people", l);
    case "staff":
      return "/mobile/today";
    default:
      return routePathFor("dashboard", l);
  }
}

export function isMobilePath(pathname: string): boolean {
  return pathname === "/mobile" || pathname.startsWith("/mobile/");
}

export function pathnameToRouteKey(pathname: string): AppRouteKey | null {
  return routeKeyFromPathname(pathname);
}
