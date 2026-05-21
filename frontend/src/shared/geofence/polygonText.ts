export type LatLngPoint = { lat: number; lng: number };

export function polygonToText(ring: LatLngPoint[]): string {
  return ring.map((p) => `${p.lat},${p.lng}`).join(";");
}

export function vertsToPoints(verts: [number, number][]): LatLngPoint[] {
  return verts.map(([lat, lng]) => ({ lat, lng }));
}

export function pointsToVerts(points: LatLngPoint[]): [number, number][] {
  return points.map((p) => [p.lat, p.lng]);
}

export function parsePolygonText(raw: string): LatLngPoint[] {
  const verts = raw
    .split(";")
    .map((s) => s.trim())
    .filter(Boolean)
    .map((pair) => {
      const [a, b] = pair.split(/[,;\s]+/).filter(Boolean);
      return { lat: Number(a), lng: Number(b) };
    });
  if (verts.some((p) => !Number.isFinite(p.lat) || !Number.isFinite(p.lng))) {
    throw new Error("invalid");
  }
  if (verts.length < 3) throw new Error("invalid");
  return verts;
}
