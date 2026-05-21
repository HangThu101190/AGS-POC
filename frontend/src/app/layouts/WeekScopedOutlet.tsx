import { Outlet, useLocation } from "react-router-dom";
import { WeekScopeProvider } from "@/shared/planning/WeekScopeContext";
import { isWeekScopedRoute } from "@/shared/planning/weekScopedRoutes";
import { routeKeyFromPathname } from "@/shared/routing/routePaths";

export function WeekScopedOutlet() {
  const location = useLocation();
  const routeKey = routeKeyFromPathname(location.pathname);

  if (!isWeekScopedRoute(routeKey)) {
    return <Outlet />;
  }

  return (
    <WeekScopeProvider>
      <Outlet />
    </WeekScopeProvider>
  );
}
