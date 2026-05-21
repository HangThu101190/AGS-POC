export type PlanStatusKey = "draft" | "published" | "inProgress" | "closed";

/** Normalize API enum / camelCase to i18n `planStatus.*` keys. */
export function normalizePlanStatus(status: string | undefined | null): PlanStatusKey {
  const s = (status ?? "draft").toLowerCase().replace(/_/g, "");
  if (s === "inprogress") return "inProgress";
  if (s === "published") return "published";
  if (s === "closed") return "closed";
  return "draft";
}

export function isPlanPublished(status: string | undefined | null): boolean {
  const k = normalizePlanStatus(status);
  return k === "published" || k === "inProgress" || k === "closed";
}
