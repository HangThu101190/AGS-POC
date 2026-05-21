/** Matches backend `PagedList<T>` — server-driven DataTable responses. */
export interface PagedList<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/** Params sent to list APIs (`TableListRequest` on the server). */
export interface TableListParams {
  page: number;
  pageSize: number;
  sortBy?: string;
  sortDir?: "asc" | "desc";
  /** AG Grid `filterModel` — serialized to `filters` query param. */
  filters?: Record<string, unknown>;
}
