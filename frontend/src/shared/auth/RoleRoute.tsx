import { Navigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/shared/auth/AuthContext";
import { canAccessRoute, getRoleHomePath, pathnameToRouteKey } from "@/shared/auth/roles";
export function RoleRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();
  const location = useLocation();
  const { i18n } = useTranslation();

  if (isLoading || !user) {
    return null;
  }

  const routeKey = pathnameToRouteKey(location.pathname);
  if (routeKey && !canAccessRoute(user.role, routeKey, user.permissions)) {
    return <Navigate to={getRoleHomePath(user.role, i18n.language)} replace />;
  }

  return children;
}
