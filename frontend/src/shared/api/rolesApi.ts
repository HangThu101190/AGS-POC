import { apiClient } from "@/shared/api/client";

export interface RoleCatalogPermission {
  code: string;
  module: string;
  nameVi: string;
  nameEn: string;
}

export interface RoleCatalogRole {
  code: string;
  nameVi: string;
  nameEn: string;
  description?: string;
  permissions: string[];
}

export interface RoleCatalog {
  roles: RoleCatalogRole[];
  permissions: RoleCatalogPermission[];
}

export async function fetchRoleCatalog(): Promise<RoleCatalog> {
  const { data } = await apiClient.get<RoleCatalog>("/api/v1/roles/catalog");
  return data;
}

export interface UpdateRoleCatalogPermissionsBody {
  roles: Record<string, string[]>;
}

export async function updateRoleCatalogPermissions(
  body: UpdateRoleCatalogPermissionsBody,
): Promise<RoleCatalog> {
  const { data } = await apiClient.put<RoleCatalog>("/api/v1/roles/catalog/permissions", body);
  return data;
}
