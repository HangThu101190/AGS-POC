/** CXR work zone — mirrors `src/checkinZoneEnv.js` / prototype geofence. */

export const CHECKIN_ZONE_DEFAULT_POLYGON: [number, number][] = [
  [12.0148, 109.2175],
  [11.9953, 109.2112],
  [11.9918, 109.2137],
  [11.9856, 109.2122],
  [11.9837, 109.2142],
  [11.9801, 109.2161],
  [11.9798, 109.2189],
  [12.0091, 109.2291],
  [12.0148, 109.2176],
];

export const CAM_RANH_MAP_CENTER: [number, number] = [11.995, 109.2167];

export const DEPT_MARKER_HEX: Record<string, string> = {
  PVHK_DI: "#2563eb",
  PVHK_DEN: "#0d9488",
  RAMP: "#d97706",
  BAGGAGE: "#9333ea",
  HCNS: "#475569",
};

function parseLatLngPair(raw: string | undefined): [number, number] | null {
  if (!raw?.trim()) return null;
  const parts = raw.trim().split(/[,;\s]+/).filter(Boolean);
  if (parts.length < 2) return null;
  const lat = Number(parts[0]);
  const lng = Number(parts[1]);
  if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;
  return [lat, lng];
}

function parsePolygonEnv(raw: string | undefined): [number, number][] | null {
  if (!raw?.trim()) return null;
  const t = raw.trim();
  if (t.startsWith("[")) {
    try {
      const parsed = JSON.parse(t) as unknown;
      if (!Array.isArray(parsed) || parsed.length < 3) return null;
      const ring: [number, number][] = [];
      for (const p of parsed) {
        if (!Array.isArray(p) || p.length < 2) return null;
        const lat = Number(p[0]);
        const lng = Number(p[1]);
        if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;
        ring.push([lat, lng]);
      }
      return ring;
    } catch {
      return null;
    }
  }
  const verts = t.split(";").map((s) => parseLatLngPair(s)).filter((p): p is [number, number] => p !== null);
  return verts.length >= 3 ? verts : null;
}

import { getCachedCheckinPolygon } from "@/shared/geofence/workZoneCache";

export function getCheckinPolygon(): [number, number][] {
  const fromEnv = parsePolygonEnv(import.meta.env.VITE_CAM_RANH_CHECKIN_POLYGON);
  if (fromEnv) return fromEnv;
  return getCachedCheckinPolygon();
}

export function polygonToLatLngTuples(
  polygon: { lat: number; lng: number }[],
): [number, number][] {
  return polygon.map((p) => [p.lat, p.lng]);
}

export function pointInPolygon(lat: number, lng: number, ring: [number, number][]): boolean {
  let inside = false;
  for (let i = 0, j = ring.length - 1; i < ring.length; j = i++) {
    const [yi, xi] = ring[i];
    const [yj, xj] = ring[j];
    const intersect =
      yi > lat !== yj > lat && lng < ((xj - xi) * (lat - yi)) / (yj - yi + 0.0) + xi;
    if (intersect) inside = !inside;
  }
  return inside;
}

export function pointInCheckinZone(lat: number, lng: number): boolean {
  return pointInPolygon(lat, lng, getCheckinPolygon());
}

export function nearOutsideCheckinCoords(): [number, number] {
  const ring = getCheckinPolygon();
  let minLat = ring[0][0];
  let maxLat = ring[0][0];
  let minLng = ring[0][1];
  let maxLng = ring[0][1];
  for (const [lat, lng] of ring) {
    minLat = Math.min(minLat, lat);
    maxLat = Math.max(maxLat, lat);
    minLng = Math.min(minLng, lng);
    maxLng = Math.max(maxLng, lng);
  }
  return [minLat - 0.0012, (minLng + maxLng) / 2];
}
