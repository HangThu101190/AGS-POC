type TranslateFn = (key: string, options?: Record<string, unknown>) => string;

/** Human-readable label for an audit row from HTTP method + API path. */
export function auditActionLabel(method: string, path: string, t: TranslateFn): string {
  const m = method.toUpperCase();
  const p = path.toLowerCase();

  if (p.includes("/auth/me/change-password")) return t("audit.actionChangePassword");
  if (p.includes("/auth/me")) return t("audit.actionProfile");
  if (p.includes("/auth/login")) return t("audit.actionLogin");
  if (p.includes("/auth/logout")) return t("audit.actionLogout");
  if (p.includes("/flights/import")) return t("audit.actionFlightImport");
  if (p.includes("/delay")) return t("audit.actionFlightDelay");
  if (p.includes("/flights")) return m === "POST" ? t("audit.actionFlightCreate") : t("audit.actionFlightMutate");
  if (p.includes("/plans/") && p.includes("/publish")) return t("audit.actionPlanPublish");
  if (p.includes("/plans/") && p.includes("/generate")) return t("audit.actionPlanGenerate");
  if (p.includes("/plans/") && p.includes("/reset")) return t("audit.actionPlanReset");
  if (p.includes("/assignments")) return t("audit.actionAssignment");
  if (p.includes("/attendance/check-in")) return t("audit.actionCheckIn");
  if (p.includes("/attendance/check-out")) return t("audit.actionCheckOut");
  if (p.includes("/work-zones")) return t("audit.actionWorkZone");
  if (p.includes("/employees") && p.includes("/reset-password")) return t("audit.actionResetPassword");
  if (p.includes("/employees") && p.includes("/user")) return t("audit.actionCreateUser");
  if (p.includes("/employees")) return m === "POST" ? t("audit.actionEmployeeCreate") : t("audit.actionEmployeeMutate");
  if (p.includes("/departments")) return m === "POST" ? t("audit.actionDeptCreate") : t("audit.actionDeptMutate");
  if (p.includes("/sync-proposals") && p.includes("/confirm")) return t("audit.actionSyncConfirm");
  if (p.includes("/reconcile")) return t("audit.actionReconcile");

  return t("audit.actionGeneric", { method: m });
}
