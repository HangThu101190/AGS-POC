import { apiClient } from "@/shared/api/client";

export type LeaveType = { id: string; code: string; name: string };

export type LeaveRequest = {
  id: string;
  employeeId: string;
  leaveTypeId: string;
  fromDate: string;
  toDate: string;
  status: string;
  note?: string | null;
};

export async function fetchLeaveTypes() {
  const { data } = await apiClient.get<LeaveType[]>("/api/v1/leave/types");
  return data;
}

export async function fetchLeaveRequests() {
  const { data } = await apiClient.get<LeaveRequest[]>("/api/v1/leave/requests");
  return data;
}

export async function approveLeaveRequest(id: string) {
  const { data } = await apiClient.post<LeaveRequest>(`/api/v1/leave/requests/${id}/approve`);
  return data;
}
