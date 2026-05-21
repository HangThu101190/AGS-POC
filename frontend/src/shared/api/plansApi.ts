import { apiClient } from "@/shared/api/client";

export type ShiftSlotDto = {
  id: string;
  departmentCode: string;
  dayIdx: number;
  segments: string[];
  headcount: number;
  flightNos: string[];
};

export type WeeklyPlanDto = {
  id: string;
  weekId: string;
  todayIdx: number;
  weekDates: string[];
  status: string;
  publishedAtUtc?: string | null;
  slots: ShiftSlotDto[];
};

export async function fetchWeeklyPlan(weekId: string, signal?: AbortSignal) {
  const { data } = await apiClient.get<WeeklyPlanDto>(
    `/api/v1/plans/${encodeURIComponent(weekId)}`,
    { signal },
  );
  return data;
}

export async function generatePlanSlots(weekId: string) {
  const { data } = await apiClient.post<WeeklyPlanDto>(
    `/api/v1/plans/${encodeURIComponent(weekId)}/slots/generate`,
  );
  return data;
}

export async function publishWeeklyPlan(weekId: string) {
  const { data } = await apiClient.post<WeeklyPlanDto>(
    `/api/v1/plans/${encodeURIComponent(weekId)}/publish`,
  );
  return data;
}

export async function resetWeeklyPlan(weekId: string) {
  const { data } = await apiClient.post<WeeklyPlanDto>(
    `/api/v1/plans/${encodeURIComponent(weekId)}/reset`,
  );
  return data;
}
