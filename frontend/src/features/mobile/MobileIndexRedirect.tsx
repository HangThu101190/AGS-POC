import { Navigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { getRoleHomePath } from "@/shared/auth/roles";

export function MobileIndexRedirect() {
  const { user } = useAuth();
  if (!user) return null;
  const home = getRoleHomePath(user.role);
  const sub = home.replace(/^\/mobile\/?/, "") || "today";
  return <Navigate to={sub} replace />;
}
