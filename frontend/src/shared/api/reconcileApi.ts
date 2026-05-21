import { apiClient } from "@/shared/api/client";

export type ReconcileSummaryDto = {
  weekId: string;
  departmentCode: string;
  employees: ReconcileEmployeeRowDto[];
};

export type ReconcileEmployeeRowDto = {
  employeeId: string;
  code: string;
  name: string;
  days: ReconcileDayCellDto[];
  hasFlag: boolean;
};

export type ReconcileDayCellDto = {
  dayIdx: number;
  dateLabel: string;
  plannedCode: string;
  actualCode: string;
  codeType?: string;
  mismatch: boolean;
  hasRevision?: boolean;
  hasCheckIn?: boolean;
  plannedTotalHours?: number;
  actualTotalHours?: number;
  plannedSegments: string[];
  actualSegments: string[];
  revisionReason?: string | null;
  revisionBy?: string | null;
};

export async function fetchReconcileSummary(weekId?: string, departmentCode = "PVHK_DI") {
  const { data } = await apiClient.get<ReconcileSummaryDto>("/api/v1/reconcile", {
    params: { weekId, departmentCode },
  });
  return data;
}

export async function exportReconcileWeek(weekId?: string, departmentCode = "PVHK_DI") {
  const response = await apiClient.get<Blob>("/api/v1/reconcile/export/week", {
    params: { weekId, departmentCode },
    responseType: "blob",
  });
  return response.data;
}

export async function exportReconcileMonth(
  weekId?: string,
  departmentCode = "PVHK_DI",
  monthLabel?: string,
) {
  const response = await apiClient.get<Blob>("/api/v1/reconcile/export/month", {
    params: { weekId, departmentCode, monthLabel },
    responseType: "blob",
  });
  return response.data;
}
