import type { PagedList, TableListParams } from "@/shared/types/tableList";

/** Normalize API body — supports `PagedList` or legacy bare array. */
export function normalizePagedList<T>(data: unknown, fallbackPageSize: number): PagedList<T> {
  if (Array.isArray(data)) {
    return {
      items: data as T[],
      page: 0,
      pageSize: data.length,
      totalCount: data.length,
    };
  }
  const page = data as Record<string, unknown> | null | undefined;
  const rawItems = page?.items ?? page?.Items;
  const items = Array.isArray(rawItems) ? (rawItems as T[]) : [];
  const totalCount = Number(page?.totalCount ?? page?.TotalCount ?? items.length);
  return {
    items,
    page: Number(page?.page ?? page?.Page ?? 0),
    pageSize: Number(page?.pageSize ?? page?.PageSize ?? fallbackPageSize),
    totalCount,
  };
}

/** Query string for `GET` list endpoints backing DataTable. */
export function toTableListQuery(params: TableListParams): Record<string, string | number> {
  const q: Record<string, string | number> = {
    page: params.page,
    pageSize: params.pageSize,
  };
  if (params.sortBy) {
    q.sortBy = params.sortBy;
  }
  if (params.sortDir) {
    q.sortDir = params.sortDir;
  }
  if (params.filters && Object.keys(params.filters).length > 0) {
    q.filters = JSON.stringify(params.filters);
  }
  return q;
}
