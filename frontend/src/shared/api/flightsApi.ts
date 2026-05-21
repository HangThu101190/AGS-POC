import { apiClient } from "@/shared/api/client";
import { normalizePagedList, toTableListQuery } from "@/shared/api/tableList";
import type { PagedList, TableListParams } from "@/shared/types/tableList";

export type FlightDto = {
  id: string;
  weekId: string;
  dayIdx: number;
  excelRowNo?: number;
  sortOrder?: number;
  flightNo: string;
  departureFlightNo?: string | null;
  route: string;
  sta: string;
  std: string;
  eta?: string | null;
  etd?: string | null;
  etaDelayMinutes?: number;
  etdDelayMinutes?: number;
  delayMinutes: number;
  isDelayed: boolean;
  registration?: string | null;
  aircraft?: string | null;
  carry?: string | null;
  departmentCode: string;
  manning: number;
  manningExplain?: string | null;
  isVip?: boolean;
  gate?: string | null;
  belt?: string | null;
  parking?: string | null;
  remark?: string | null;
};

export type FlightScheduleDayDto = {
  weekId: string;
  dayIdx: number;
  sourceDayLabel?: string | null;
  sheetRemark?: string | null;
};

export async function fetchFlightScheduleDay(
  weekId: string,
  dayIdx: number,
  signal?: AbortSignal,
): Promise<FlightScheduleDayDto | null> {
  try {
    const { data } = await apiClient.get<FlightScheduleDayDto>("/api/v1/flights/day-meta", {
      params: { weekId, dayIdx },
      signal,
    });
    return data;
  } catch (err: unknown) {
    const status = (err as { response?: { status?: number } })?.response?.status;
    if (status === 404) return null;
    throw err;
  }
}

export async function fetchFlightsPage(
  params: TableListParams & { weekId?: string; dayIdx?: number; departmentCode?: string },
  signal?: AbortSignal,
): Promise<PagedList<FlightDto>> {
  const { data } = await apiClient.get<unknown>("/api/v1/flights", {
    params: {
      ...toTableListQuery({
        ...params,
        sortBy: params.sortBy ?? "sortOrder",
        sortDir: params.sortDir ?? "asc",
      }),
      weekId: params.weekId,
      dayIdx: params.dayIdx,
      departmentCode: params.departmentCode,
    },
    signal,
  });
  return normalizePagedList<FlightDto>(data, params.pageSize);
}

export async function setFlightDelay(
  flightId: string,
  delays: { etaDelayMinutes: number; etdDelayMinutes: number },
) {
  const { data } = await apiClient.patch<FlightDto>(`/api/v1/flights/${flightId}/delay`, {
    etaDelayMinutes: delays.etaDelayMinutes,
    etdDelayMinutes: delays.etdDelayMinutes,
  });
  return data;
}

export type FlightUpsertBody = {
  weekId?: string;
  dayIdx: number;
  flightNo: string;
  departureFlightNo?: string | null;
  route: string;
  sta: string;
  std: string;
  departmentCode: string;
  manning?: number;
  aircraft?: string | null;
  isVip?: boolean;
};

export async function createFlight(body: FlightUpsertBody) {
  const { data } = await apiClient.post<FlightDto>("/api/v1/flights", body);
  return data;
}

export async function updateFlight(id: string, body: FlightUpsertBody) {
  const { data } = await apiClient.put<FlightDto>(`/api/v1/flights/${id}`, body);
  return data;
}

export async function deleteFlight(id: string) {
  await apiClient.delete(`/api/v1/flights/${id}`);
}

export type FlightImportSyncResult = {
  importedCount: number;
  dayIdx?: number;
  dayLabel?: string | null;
  sheetRemark?: string | null;
  warnings: string[];
};

export type FlightImportJob = {
  id: string;
  weekId: string;
  dayIdx: number;
  status: string;
  progressPercent: number;
  errorMessage?: string | null;
  result?: FlightImportSyncResult | null;
};

export type FlightImportOutcome =
  | { mode: "sync"; result: FlightImportSyncResult }
  | { mode: "async"; job: FlightImportJob };

export async function importFlightsExcel(
  file: File,
  dayIdx: number,
  weekId?: string,
  options?: { preferAsync?: boolean },
): Promise<FlightImportOutcome> {
  const form = new FormData();
  form.append("file", file);
  const preferAsync = options?.preferAsync ?? file.size > 2 * 1024 * 1024;
  const headers: Record<string, string> = { "Content-Type": "multipart/form-data" };
  if (preferAsync) {
    headers.Prefer = "respond-async";
  }
  const response = await apiClient.post<FlightImportSyncResult | FlightImportJob>(
    "/api/v1/flights/import",
    form,
    {
      params: { dayIdx, ...(weekId ? { weekId } : {}) },
      headers,
      validateStatus: (status) => status === 200 || status === 202,
    },
  );
  if (response.status === 202) {
    return { mode: "async", job: response.data as FlightImportJob };
  }
  return { mode: "sync", result: response.data as FlightImportSyncResult };
}

export async function fetchFlightImportJob(jobId: string) {
  const { data } = await apiClient.get<FlightImportJob>(`/api/v1/flights/import/jobs/${jobId}`);
  return data;
}
