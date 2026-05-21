import { apiClient } from "@/shared/api/client";
import { normalizePagedList, toTableListQuery } from "@/shared/api/tableList";
import type { DepartmentDto } from "@/shared/types/api";
import type { PagedList, TableListParams } from "@/shared/types/tableList";

export async function fetchDepartmentsPage(
  params: TableListParams,
  signal?: AbortSignal,
): Promise<PagedList<DepartmentDto>> {
  const { data } = await apiClient.get<unknown>("/api/v1/departments", {
    params: toTableListQuery(params),
    signal,
  });
  return normalizePagedList<DepartmentDto>(data, params.pageSize);
}

export type DepartmentUpsertBody = {
  siteId?: string;
  code: string;
  name: string;
  allowedRoles?: string[];
};

export async function deactivateDepartment(id: string) {
  const { data } = await apiClient.patch<DepartmentDto>(`/api/v1/departments/${id}/deactivate`);
  return data;
}

export async function createDepartment(body: DepartmentUpsertBody) {
  const { data } = await apiClient.post<DepartmentDto>("/api/v1/departments", body);
  return data;
}

export async function updateDepartment(id: string, body: DepartmentUpsertBody) {
  const { data } = await apiClient.put<DepartmentDto>(`/api/v1/departments/${id}`, body);
  return data;
}
