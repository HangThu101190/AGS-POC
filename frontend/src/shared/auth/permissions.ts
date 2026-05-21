import type { AppRole } from "@/shared/auth/types";
import {
  canAccessAudit,
  canAccessConfig,
  canAccessMonitoring,
  canAccessReconcile,
  canAccessRoute,
  canAccessPhanCongSlot,
  canManageFlights,
  canManageMasterData,
  canManageWeeklyPlan,
  canViewFlights,
  canViewWeeklyPlan,
  type AppRouteKey,
} from "@/shared/auth/roles";

export type RolePermissions = {
  role: AppRole;
  canAccessRoute: (route: AppRouteKey) => boolean;
  canManageFlights: boolean;
  canManageWeeklyPlan: boolean;
  canViewWeeklyPlan: boolean;
  canViewFlights: boolean;
  canManageMasterData: boolean;
  canAccessMonitoring: boolean;
  canAccessReconcile: boolean;
  canAccessPhanCongSlot: boolean;
  canAccessAudit: boolean;
  canAccessConfig: boolean;
};

export function getRolePermissions(role: AppRole, permissions?: readonly string[]): RolePermissions {
  return {
    role,
    canAccessRoute: (route) => canAccessRoute(role, route, permissions),
    canManageFlights: canManageFlights(role),
    canManageWeeklyPlan: canManageWeeklyPlan(role),
    canViewWeeklyPlan: canViewWeeklyPlan(role),
    canViewFlights: canViewFlights(role),
    canManageMasterData: canManageMasterData(role),
    canAccessMonitoring: canAccessMonitoring(role),
    canAccessReconcile: canAccessReconcile(role),
    canAccessPhanCongSlot: canAccessPhanCongSlot(role),
    canAccessAudit: canAccessAudit(role),
    canAccessConfig: canAccessConfig(role),
  };
}
