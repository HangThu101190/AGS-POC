import { apiClient } from "@/shared/api/client";
import { normalizePagedList, toTableListQuery } from "@/shared/api/tableList";
import type { PagedList, TableListParams } from "@/shared/types/tableList";

export type NotificationDto = {
  id: string;
  type: string;
  title: string;
  body: string;
  isRead: boolean;
  createdAt: string;
};

export async function fetchMyNotifications(
  params: TableListParams,
  signal?: AbortSignal,
): Promise<PagedList<NotificationDto>> {
  const { data } = await apiClient.get<unknown>("/api/v1/notifications/me", {
    params: toTableListQuery(params),
    signal,
  });
  return normalizePagedList<NotificationDto>(data, params.pageSize);
}

export async function markNotificationRead(id: string) {
  const { data } = await apiClient.patch<NotificationDto>(`/api/v1/notifications/${id}/read`);
  return data;
}
