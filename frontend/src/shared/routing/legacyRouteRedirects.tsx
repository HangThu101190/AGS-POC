import { Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { AppRouteKey } from "@/shared/auth/roles";
import { routePathFor } from "@/shared/routing/routePaths";

export function LegacyRouteRedirect({ routeKey }: { routeKey: AppRouteKey }) {
  const { i18n } = useTranslation();
  return <Navigate to={routePathFor(routeKey, i18n.language)} replace />;
}

/** Old URL segments → current routes (bookmarks / docs). */
export const LEGACY_ROUTE_REDIRECTS: { path: string; routeKey: AppRouteKey }[] = [
  { path: "lap-ke-hoach-ca", routeKey: "dailyStaffing" },
  { path: "planner", routeKey: "dailyStaffing" },
  { path: "supboard", routeKey: "phanCongSlot" },
];
