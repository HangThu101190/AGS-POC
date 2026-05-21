import { apiClient } from "@/shared/api/client";

export type AttendanceDto = {
  id: string;
  employeeId: string;
  checkInUtc: string;
  checkOutUtc?: string | null;
  checkInLat?: number | null;
  checkInLng?: number | null;
  currentLat?: number | null;
  currentLng?: number | null;
  inZone: boolean;
  geoNote?: string | null;
  geoSimulated: boolean;
  isActive: boolean;
};

export type StaffTodayShiftDto = {
  planStatus: string;
  segments: string[];
  flightNos: string[];
  shiftChanged?: boolean;
};

export type CheckInRequest = {
  lat: number;
  lng: number;
  inZone: boolean;
  geoNote?: string;
  geoSimulated: boolean;
};

export async function fetchMyTodayShift(weekId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<StaffTodayShiftDto>(
    `/api/v1/attendance/my-shift?weekId=${encodeURIComponent(weekId)}`,
    { signal },
  );
  return data;
}

export async function fetchMyAttendance(employeeId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<AttendanceDto>(
    `/api/v1/attendance/${encodeURIComponent(employeeId)}`,
    { signal },
  );
  return data;
}

export async function postCheckIn(body: CheckInRequest, signal?: AbortSignal) {
  const { data } = await apiClient.post<AttendanceDto>("/api/v1/attendance/check-in", body, {
    signal,
  });
  return data;
}

export async function postCheckOut(earlyNote?: string, signal?: AbortSignal) {
  const { data } = await apiClient.post<AttendanceDto>(
    "/api/v1/attendance/check-out",
    earlyNote ? { earlyNote } : {},
    { signal },
  );
  return data;
}
