import { apiClient } from "@/shared/api/client";

/** DTO khớp OpenAPI `GET /api/v1/supboard` (tên API giữ nguyên). */
export type PhanCongSlotDto = {
  weekId: string;
  dayIdx: number;
  departmentCode: string;
  planStatus: string;
  slots: PhanCongSlotSlotDto[];
  flights: PhanCongSlotFlightDto[];
};

export type PhanCongSlotSlotDto = {
  id: string;
  dayIdx: number;
  departmentCode: string;
  segments: string[];
  headcount: number;
  flightNos: string[];
  assignments: PhanCongSlotAssignmentDto[];
};

export type PhanCongSlotAssignmentDto = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  flightNos: string[];
};

export type PhanCongSlotFlightDto = {
  id: string;
  flightNo: string;
  route: string;
  sta: string;
  std: string;
  isDelayed: boolean;
  delayMinutes: number;
};

export async function fetchPhanCongSlot(params: {
  weekId?: string;
  dayIdx?: number;
  departmentCode?: string;
}) {
  const { data } = await apiClient.get<PhanCongSlotDto>("/api/v1/supboard", { params });
  return data;
}

export async function assignToSlot(body: {
  weekId?: string;
  departmentId: string;
  slotId: string;
  employeeId: string;
}) {
  const { data } = await apiClient.post("/api/v1/assignments", body);
  return data;
}

export async function removeAssignment(assignmentId: string, weekId: string) {
  await apiClient.delete(`/api/v1/assignments/${assignmentId}`, { params: { weekId } });
}

export async function linkAssignmentFlights(
  assignmentId: string,
  body: { weekId?: string; flightNos: string[] },
) {
  const { data } = await apiClient.put<{ flightNos: string[] }>(
    `/api/v1/assignments/${assignmentId}/flights`,
    body,
  );
  return data;
}
