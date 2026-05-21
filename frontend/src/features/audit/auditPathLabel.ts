import type { TFunction } from "i18next";

/** Human-readable Vietnamese/EN label for audit API path (sidebar-aligned names). */
export function auditPathLabel(path: string, t: TFunction): string {
  const p = path.split("?")[0].toLowerCase();

  if (p.includes("/auth/me/change-password")) return t("audit.pathChangePassword");
  if (p.includes("/auth/me")) return t("audit.pathProfile");
  if (p.includes("/auth/login")) return t("audit.pathLogin");
  if (p.includes("/auth/logout")) return t("audit.pathLogout");
  if (p.includes("/auth/refresh")) return t("audit.pathRefresh");
  if (p.includes("/flights/import")) return t("audit.pathFlightsImport");
  if (p.includes("/flights") && p.includes("/delay")) return t("audit.pathFlightsDelay");
  if (p.includes("/flights")) return t("audit.pathFlights");
  if (p.includes("/flight-schedules")) return t("audit.pathFlightSchedules");
  if (p.includes("/plans/") && p.includes("/publish")) return t("audit.pathPlanPublish");
  if (p.includes("/plans/") && p.includes("/generate")) return t("audit.pathPlanGenerate");
  if (p.includes("/plans/") && p.includes("/reset")) return t("audit.pathPlanReset");
  if (p.includes("/plans")) return t("audit.pathPlans");
  if (p.includes("/assignments")) return t("audit.pathAssignments");
  if (p.includes("/attendance/check-in")) return t("audit.pathCheckIn");
  if (p.includes("/attendance/check-out")) return t("audit.pathCheckOut");
  if (p.includes("/attendance")) return t("audit.pathAttendance");
  if (p.includes("/work-zones")) return t("audit.pathWorkZone");
  if (p.includes("/employees") && p.includes("/reset-password")) return t("audit.pathResetPassword");
  if (p.includes("/employees") && p.includes("/user")) return t("audit.pathCreateUser");
  if (p.includes("/employees")) return t("audit.pathEmployees");
  if (p.includes("/departments")) return t("audit.pathDepartments");
  if (p.includes("/roles/catalog")) return t("audit.pathRoles");
  if (p.includes("/sync-proposals")) return t("audit.pathSync");
  if (p.includes("/reconcile")) return t("audit.pathReconcile");
  if (p.includes("/monitoring")) return t("audit.pathMonitoring");
  if (p.includes("/notifications")) return t("audit.pathNotifications");

  const trimmed = p.replace(/^\/api\/v1\/?/, "/");
  return trimmed || path;
}
