import { apiClient } from "@/shared/api/client";
import { normalizePagedList, toTableListQuery } from "@/shared/api/tableList";
import type { EmployeeDto } from "@/shared/types/api";
import type { PagedList, TableListParams } from "@/shared/types/tableList";

export async function fetchEmployeesPage(
  params: TableListParams,
  signal?: AbortSignal,
): Promise<PagedList<EmployeeDto>> {
  const { data } = await apiClient.get<unknown>("/api/v1/employees", {
    params: toTableListQuery(params),
    signal,
  });
  return normalizePagedList<EmployeeDto>(data, params.pageSize);
}

export type EmployeeUpsertBody = {
  departmentId: string;
  code: string;
  name: string;
  role: string;
  managerId?: string | null;
  initialPassword?: string;
};

export async function createEmployeeUser(id: string, body: { initialPassword?: string } = {}) {
  const { data } = await apiClient.post<{
    userId: string;
    loginName: string;
    isActive: boolean;
    temporaryPassword?: string;
  }>(`/api/v1/employees/${id}/user`, body);
  return data;
}

export async function resetEmployeePassword(id: string) {
  const { data } = await apiClient.post<{ temporaryPassword: string }>(
    `/api/v1/employees/${id}/reset-password`,
  );
  return data;
}

export async function patchEmployeeUser(id: string, body: { isActive?: boolean }) {
  const { data } = await apiClient.patch<{ userId: string; loginName: string; isActive: boolean }>(
    `/api/v1/employees/${id}/user`,
    body,
  );
  return data;
}

export async function createEmployee(body: EmployeeUpsertBody) {
  const { data } = await apiClient.post<EmployeeDto>("/api/v1/employees", body);
  return data;
}

export async function updateEmployee(id: string, body: EmployeeUpsertBody) {
  const { data } = await apiClient.put<EmployeeDto>(`/api/v1/employees/${id}`, body);
  return data;
}

export async function deactivateEmployee(id: string) {
  const { data } = await apiClient.patch<EmployeeDto>(`/api/v1/employees/${id}/deactivate`);
  return data;
}
