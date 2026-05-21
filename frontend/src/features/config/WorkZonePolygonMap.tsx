import { useEffect, useRef } from "react";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import { CAM_RANH_MAP_CENTER, polygonToLatLngTuples } from "@/shared/geofence/checkinZone";
import { attachMapResetControl } from "@/shared/map/mapResetControl";
import type { LatLngPoint } from "@/shared/geofence/polygonText";
import styles from "./workZonePolygonMap.module.css";

type WorkZonePolygonMapProps = {
  savedPolygon: LatLngPoint[];
  draftVerts: [number, number][];
  drawMode: boolean;
  onMapClick?: (lat: number, lng: number) => void;
  fitToken?: number;
  onMapReset?: () => void;
  resetTitle?: string;
  className?: string;
};

function normalizeRing(points: LatLngPoint[]): [number, number][] {
  const ring = polygonToLatLngTuples(
    points.map((p) => ({
      lat: Number((p as LatLngPoint & { Lat?: number }).lat ?? (p as { Lat?: number }).Lat),
      lng: Number((p as LatLngPoint & { Lng?: number }).lng ?? (p as { Lng?: number }).Lng),
    })),
  );
  return ring.filter(([lat, lng]) => Number.isFinite(lat) && Number.isFinite(lng));
}

function containerHasMap(el: HTMLElement): boolean {
  return "_leaflet_id" in el && (el as HTMLElement & { _leaflet_id?: number })._leaflet_id != null;
}

function fitRing(map: L.Map, ring: [number, number][]) {
  if (ring.length >= 3) {
    try {
      const bounds = L.polygon(ring).getBounds();
      if (bounds.isValid()) {
        map.fitBounds(bounds, { padding: [28, 28], maxZoom: 16 });
        return;
      }
    } catch {
      /* invalid ring */
    }
  }
  map.setView(CAM_RANH_MAP_CENTER, 13);
}

function destroyMap(map: L.Map | null) {
  if (!map) return;
  try {
    map.remove();
  } catch {
    /* already removed */
  }
}

export function WorkZonePolygonMap({
  savedPolygon,
  draftVerts,
  drawMode,
  onMapClick,
  fitToken = 0,
  onMapReset,
  resetTitle = "",
  className,
}: WorkZonePolygonMapProps) {
  const elRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<L.Map | null>(null);
  const savedLayerRef = useRef<L.Polygon | null>(null);
  const sketchRef = useRef<L.LayerGroup | null>(null);
  const detachResetRef = useRef<(() => void) | null>(null);
  const onMapResetRef = useRef(onMapReset);
  onMapResetRef.current = onMapReset;

  useEffect(() => {
    const el = elRef.current;
    if (!el) return;

    let disposed = false;
    let map: L.Map | null = null;

    const init = () => {
      if (disposed || !elRef.current || containerHasMap(elRef.current)) return;

      map = L.map(elRef.current, { scrollWheelZoom: true, attributionControl: false }).setView(
        CAM_RANH_MAP_CENTER,
        13,
      );
      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap",
        maxZoom: 19,
      }).addTo(map);

      const sketch = L.layerGroup().addTo(map);
      mapRef.current = map;
      sketchRef.current = sketch;
      if (onMapResetRef.current) {
        detachResetRef.current = attachMapResetControl(map, {
          title: resetTitle,
          onReset: () => onMapResetRef.current?.(),
        });
      }
      map.invalidateSize();
    };

    const t0 = window.setTimeout(init, 0);
    const ro = new ResizeObserver(() => {
      window.requestAnimationFrame(() => mapRef.current?.invalidateSize());
    });
    ro.observe(el);

    return () => {
      disposed = true;
      window.clearTimeout(t0);
      ro.disconnect();
      detachResetRef.current?.();
      detachResetRef.current = null;
      destroyMap(map);
      mapRef.current = null;
      savedLayerRef.current = null;
      sketchRef.current = null;
    };
  }, [resetTitle]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;

    savedLayerRef.current?.remove();
    savedLayerRef.current = null;

    const savedRing = normalizeRing(savedPolygon);
    if (savedRing.length >= 3 && !drawMode) {
      savedLayerRef.current = L.polygon(savedRing, {
        color: "#15803d",
        fillColor: "#22c55e",
        fillOpacity: 0.16,
        weight: 2,
      }).addTo(map);
    }

    const display = drawMode
      ? draftVerts.length >= 3
        ? draftVerts
        : []
      : savedRing.length >= 3
        ? savedRing
        : [];
    fitRing(map, display);
  }, [savedPolygon, drawMode, fitToken]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    const onClick = (e: L.LeafletMouseEvent) => {
      if (drawMode && onMapClick) onMapClick(e.latlng.lat, e.latlng.lng);
    };
    if (drawMode) {
      map.getContainer().style.cursor = "crosshair";
      map.on("click", onClick);
    } else {
      map.getContainer().style.cursor = "";
    }
    return () => {
      map.off("click", onClick);
      map.getContainer().style.cursor = "";
    };
  }, [drawMode, onMapClick]);

  useEffect(() => {
    const sketch = sketchRef.current;
    if (!sketch) return;
    sketch.clearLayers();

    if (!drawMode) return;

    for (const [lat, lng] of draftVerts) {
      L.circleMarker([lat, lng], {
        radius: 7,
        color: "#c2410c",
        fillColor: "#fb923c",
        fillOpacity: 1,
        weight: 2,
      }).addTo(sketch);
    }
    if (draftVerts.length >= 2) {
      L.polyline(draftVerts, { color: "#ea580c", weight: 2, dashArray: "6 4" }).addTo(sketch);
    }
    if (draftVerts.length >= 3) {
      L.polygon(draftVerts, {
        color: "#ea580c",
        fillColor: "#fdba74",
        fillOpacity: 0.3,
        weight: 2,
      }).addTo(sketch);
    }
  }, [draftVerts, drawMode]);

  return (
    <div className={[styles.wrap, className].filter(Boolean).join(" ")}>
      <div ref={elRef} className={styles.map} role="application" aria-label="Work zone map" />
    </div>
  );
}
