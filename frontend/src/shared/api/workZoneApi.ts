import { apiClient } from "@/shared/api/client";

export type LatLng = { lat: number; lng: number };

export type WorkZoneDto = {
  id: string;
  siteId: string;
  name: string;
  version: number;
  polygon: LatLng[];
};

export async function fetchActiveWorkZone(signal?: AbortSignal) {
  const { data } = await apiClient.get<WorkZoneDto>("/api/v1/work-zones/active", { signal });
  return data;
}

export async function updateWorkZonePolygon(polygon: LatLng[], signal?: AbortSignal) {
  const { data } = await apiClient.put<WorkZoneDto>(
    "/api/v1/work-zones/active/polygon",
    { polygon },
    { signal },
  );
  return data;
}
