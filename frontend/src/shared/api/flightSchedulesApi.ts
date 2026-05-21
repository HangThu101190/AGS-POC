import { apiClient } from "@/shared/api/client";

export type FlightScheduleDto = {
  weekId: string;
  status: "draft" | "published" | "locked";
  isLocked: boolean;
  publishedAtUtc?: string | null;
};

export async function fetchFlightSchedule(weekId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<FlightScheduleDto>(
    `/api/v1/flight-schedules/${encodeURIComponent(weekId)}`,
    { signal },
  );
  return data;
}

export async function publishFlightSchedule(weekId: string) {
  const { data } = await apiClient.post<FlightScheduleDto>(
    `/api/v1/flight-schedules/${encodeURIComponent(weekId)}/publish`,
  );
  return data;
}
