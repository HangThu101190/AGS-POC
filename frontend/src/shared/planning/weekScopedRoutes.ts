import type { AppRouteKey } from "@/shared/auth/roles";

/** Routes that share the global week selector (in-memory via WeekScopeProvider). */
export const WEEK_SCOPED_ROUTE_KEYS = new Set<AppRouteKey>([
  "dashboard",
  "flights",
  "dailyStaffing",
  "phanCongSlot",
  "reconcile",
]);

export function isWeekScopedRoute(key: AppRouteKey | null): boolean {
  return key != null && WEEK_SCOPED_ROUTE_KEYS.has(key);
}
