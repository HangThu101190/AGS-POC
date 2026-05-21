import { useCallback } from "react";
import { fetchEmployeesPage } from "@/shared/api/employeesApi";
import type { TableListParams } from "@/shared/types/tableList";

/** Stable `fetchRows` callback for DataTable (server-side paging). */
export function useEmployeesFetchRows() {
  return useCallback(
    (params: TableListParams, signal?: AbortSignal) => fetchEmployeesPage(params, signal),
    [],
  );
}
