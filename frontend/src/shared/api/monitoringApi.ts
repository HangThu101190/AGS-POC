import { apiClient } from "@/shared/api/client";

export type MonitoringGpsPointDto = {
  atUtc: string;
  lat: number;
  lng: number;
};

export type MonitoringWorkforceEntryDto = {
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  departmentCode: string;
};

export type MonitoringMarkerDto = {
  attendanceId: string;
  employeeId: string;
  employeeName: string;
  employeeCode: string;
  departmentCode: string;
  lat: number;
  lng: number;
  inZone: boolean;
  geoNote?: string | null;
  checkedOut: boolean;
  checkInUtc: string;
  checkOutUtc?: string | null;
  shiftSegments: string[];
  flightNos: string[];
  history: MonitoringGpsPointDto[];
};

export type MonitoringSnapshotDto = {
  generatedAtUtc: string;
  inShiftCount: number;
  notCheckedInCount: number;
  checkedOutCount: number;
  outsideZoneCount: number;
  workZone: {
    name: string;
    polygon: { lat: number; lng: number }[];
  };
  markers: MonitoringMarkerDto[];
  checkedIn: MonitoringWorkforceEntryDto[];
  notCheckedIn: MonitoringWorkforceEntryDto[];
  checkedOut: MonitoringWorkforceEntryDto[];
};

export type FetchMonitoringSnapshotResult =
  | { unchanged: true }
  | { unchanged: false; data: MonitoringSnapshotDto };

export async function fetchMonitoringSnapshot(
  sinceUtc?: string | null,
  signal?: AbortSignal,
): Promise<FetchMonitoringSnapshotResult> {
  const { status, data } = await apiClient.get<MonitoringSnapshotDto>(
    "/api/v1/monitoring/snapshot",
    {
      signal,
      params: sinceUtc ? { since: sinceUtc } : undefined,
      validateStatus: (s) => s === 200 || s === 304,
    },
  );

  if (status === 304) {
    return { unchanged: true };
  }

  return { unchanged: false, data };
}

/** Convenience for callers that always need a body (no `since` on first load). */
export async function fetchMonitoringSnapshotData(
  signal?: AbortSignal,
): Promise<MonitoringSnapshotDto> {
  const result = await fetchMonitoringSnapshot(null, signal);
  if (result.unchanged) {
    throw new Error("monitoring_snapshot_unavailable");
  }
  return result.data;
}

export type LocationSampleInput = {
  lat: number;
  lng: number;
  capturedAtUtc?: string;
};

export async function postLocationSamples(samples: LocationSampleInput[]) {
  const { data } = await apiClient.post<{ accepted: number; lastCapturedAtUtc?: string }>(
    "/api/v1/attendance/location-samples",
    { samples },
  );
  return data;
}
