import { useMemo } from "react";
import { useAuth } from "@/shared/auth/AuthContext";
import { getRolePermissions, type RolePermissions } from "@/shared/auth/permissions";

export function usePermissions(): RolePermissions | null {
  const { user } = useAuth();
  return useMemo(
    () => (user ? getRolePermissions(user.role, user.permissions) : null),
    [user],
  );
}
