import { useCallback } from "react";
import { fetchDepartmentsPage } from "@/shared/api/departmentsApi";
import type { TableListParams } from "@/shared/types/tableList";

/** Stable `fetchRows` callback for DataTable (server-side paging). */
export function useDepartmentsFetchRows() {
  return useCallback(
    (params: TableListParams, signal?: AbortSignal) => fetchDepartmentsPage(params, signal),
    [],
  );
}
