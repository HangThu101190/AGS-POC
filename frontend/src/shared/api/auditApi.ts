import { apiClient } from "@/shared/api/client";
import { normalizePagedList, toTableListQuery } from "@/shared/api/tableList";
import type { PagedList, TableListParams } from "@/shared/types/tableList";

export type AuditEventDto = {
  id: string;
  occurredAtUtc: string;
  employeeCode?: string | null;
  actorName?: string | null;
  httpMethod: string;
  path: string;
  statusCode: number;
};

export type AuditListParams = TableListParams & {
  fromUtc?: string;
  toUtc?: string;
};

function normalizeAuditEvent(raw: unknown): AuditEventDto {
  const row = (raw ?? {}) as Record<string, unknown>;
  const occurred =
    row.occurredAtUtc ?? row.occurred_at ?? row.OccurredAtUtc ?? row.occurredAt ?? "";
  return {
    id: String(row.id ?? row.Id ?? ""),
    occurredAtUtc: typeof occurred === "string" ? occurred : String(occurred),
    employeeCode: (row.employeeCode ?? row.employee_code ?? row.EmployeeCode ?? null) as
      | string
      | null,
    actorName: (row.actorName ?? row.actor_name ?? row.ActorName ?? null) as string | null,
    httpMethod: String(row.httpMethod ?? row.http_method ?? row.HttpMethod ?? ""),
    path: String(row.path ?? row.Path ?? ""),
    statusCode: Number(row.statusCode ?? row.status_code ?? row.StatusCode ?? 0),
  };
}

export async function fetchAuditEventsPage(
  params: AuditListParams,
  signal?: AbortSignal,
): Promise<PagedList<AuditEventDto>> {
  const { fromUtc, toUtc, ...table } = params;
  const { data } = await apiClient.get<unknown>("/api/v1/audit-events", {
    params: {
      ...toTableListQuery(table),
      ...(fromUtc ? { fromUtc } : {}),
      ...(toUtc ? { toUtc } : {}),
    },
    signal,
  });
  const page = normalizePagedList<unknown>(data, params.pageSize);
  return {
    ...page,
    items: page.items.map(normalizeAuditEvent),
  };
}
