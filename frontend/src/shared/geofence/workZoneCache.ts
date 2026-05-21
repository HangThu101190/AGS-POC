import { fetchActiveWorkZone, type LatLng } from "@/shared/api/workZoneApi";
import { CHECKIN_ZONE_DEFAULT_POLYGON } from "@/shared/geofence/checkinZone";

let cachedRing: [number, number][] | null = null;
let loadPromise: Promise<[number, number][]> | null = null;

export function ringFromDto(polygon: LatLng[]): [number, number][] {
  return polygon.map((p) => [p.lat, p.lng] as [number, number]);
}

export function getCachedCheckinPolygon(): [number, number][] {
  return cachedRing ?? CHECKIN_ZONE_DEFAULT_POLYGON;
}

export async function ensureCheckinPolygonLoaded(signal?: AbortSignal): Promise<[number, number][]> {
  if (cachedRing) return cachedRing;
  if (!loadPromise) {
    loadPromise = fetchActiveWorkZone(signal)
      .then((zone) => {
        const ring =
          zone.polygon.length >= 3 ? ringFromDto(zone.polygon) : CHECKIN_ZONE_DEFAULT_POLYGON;
        cachedRing = ring;
        return ring;
      })
      .catch((err) => {
        console.warn("[workZone] API polygon load failed — using default CXR ring", err);
        cachedRing = CHECKIN_ZONE_DEFAULT_POLYGON;
        return cachedRing;
      })
      .finally(() => {
        loadPromise = null;
      });
  }
  return loadPromise;
}

export function invalidateWorkZoneCache() {
  cachedRing = null;
}
