import { useEffect, useRef } from "react";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import type { MonitoringSnapshotDto } from "@/shared/api/monitoringApi";
import { buildMonitoringMarkerPopup } from "./monitoringPopup";
import {
  CAM_RANH_MAP_CENTER,
  DEPT_MARKER_HEX,
  getCheckinPolygon,
  polygonToLatLngTuples,
} from "@/shared/geofence/checkinZone";
import { attachMapResetControl } from "@/shared/map/mapResetControl";
import styles from "./monitoringPage.module.css";

type MonitoringMapProps = {
  snapshot: MonitoringSnapshotDto;
  selectedEmployeeId: string | null;
  onSelectEmployee: (employeeId: string | null) => void;
  highlight?: { lat: number; lng: number } | null;
  zoneDrawMode?: boolean;
  zoneDrawVerts?: [number, number][];
  onMapClick?: (lat: number, lng: number) => void;
  mapResetToken?: number;
  onMapReset?: () => void;
  resetTitle?: string;
};

function fitZone(map: L.Map, ring: [number, number][]) {
  if (ring.length >= 3) {
    map.fitBounds(L.polygon(ring).getBounds(), { padding: [24, 24] });
    return;
  }
  map.setView(CAM_RANH_MAP_CENTER, 13);
}

export function MonitoringMap({
  snapshot,
  selectedEmployeeId,
  onSelectEmployee,
  highlight,
  zoneDrawMode = false,
  zoneDrawVerts = [],
  onMapClick,
  mapResetToken = 0,
  onMapReset,
  resetTitle = "",
}: MonitoringMapProps) {
  const elRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<L.Map | null>(null);
  const layerRef = useRef<L.LayerGroup | null>(null);
  const sketchRef = useRef<L.LayerGroup | null>(null);
  const highlightRef = useRef<L.CircleMarker | null>(null);
  const detachResetRef = useRef<(() => void) | null>(null);
  const onMapResetRef = useRef(onMapReset);
  onMapResetRef.current = onMapReset;

  useEffect(() => {
    const el = elRef.current;
    if (!el) return;

    const polygon =
      snapshot.workZone.polygon.length >= 3
        ? polygonToLatLngTuples(snapshot.workZone.polygon)
        : getCheckinPolygon();

    const map = L.map(el, { scrollWheelZoom: true, attributionControl: false }).setView(
      CAM_RANH_MAP_CENTER,
      13,
    );
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: "",
    }).addTo(map);
    L.polygon(polygon, {
      color: "#15803d",
      fillColor: "#22c55e",
      fillOpacity: 0.16,
      weight: 2,
    }).addTo(map);
    fitZone(map, polygon);

    const g = L.layerGroup().addTo(map);
    const sketch = L.layerGroup().addTo(map);
    mapRef.current = map;
    layerRef.current = g;
    sketchRef.current = sketch;
    if (onMapResetRef.current) {
      detachResetRef.current = attachMapResetControl(map, {
        title: resetTitle,
        onReset: () => onMapResetRef.current?.(),
      });
    }

    const t0 = window.setTimeout(() => map.invalidateSize(), 80);
    const ro = new ResizeObserver(() => map.invalidateSize());
    ro.observe(el);

    return () => {
      window.clearTimeout(t0);
      ro.disconnect();
      detachResetRef.current?.();
      detachResetRef.current = null;
      map.remove();
      mapRef.current = null;
      layerRef.current = null;
      highlightRef.current = null;
      sketchRef.current = null;
    };
  }, [snapshot.workZone.polygon, resetTitle]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    const polygon =
      snapshot.workZone.polygon.length >= 3
        ? polygonToLatLngTuples(snapshot.workZone.polygon)
        : getCheckinPolygon();
    fitZone(map, polygon);
  }, [mapResetToken, snapshot.workZone.polygon]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    const onClick = (e: L.LeafletMouseEvent) => {
      if (zoneDrawMode && onMapClick) onMapClick(e.latlng.lat, e.latlng.lng);
    };
    if (zoneDrawMode) {
      map.getContainer().style.cursor = "crosshair";
      map.on("click", onClick);
    } else {
      map.getContainer().style.cursor = "";
    }
    return () => {
      map.off("click", onClick);
      map.getContainer().style.cursor = "";
    };
  }, [zoneDrawMode, onMapClick]);

  useEffect(() => {
    const sketch = sketchRef.current;
    if (!sketch) return;
    sketch.clearLayers();
    for (const [lat, lng] of zoneDrawVerts) {
      L.circleMarker([lat, lng], {
        radius: 6,
        color: "#c2410c",
        fillColor: "#fb923c",
        fillOpacity: 1,
        weight: 2,
      }).addTo(sketch);
    }
    if (zoneDrawVerts.length >= 2) {
      L.polyline(zoneDrawVerts, { color: "#ea580c", weight: 2, dashArray: "6 4" }).addTo(sketch);
    }
    if (zoneDrawVerts.length >= 3) {
      L.polygon(zoneDrawVerts, {
        color: "#ea580c",
        fillColor: "#fdba74",
        fillOpacity: 0.28,
        weight: 2,
      }).addTo(sketch);
    }
  }, [zoneDrawVerts]);

  useEffect(() => {
    const map = mapRef.current;
    const g = layerRef.current;
    if (!map || !g) return;

    g.clearLayers();
    for (const m of snapshot.markers) {
      const hex = DEPT_MARKER_HEX[m.departmentCode] ?? "#64748b";
      const isSel = selectedEmployeeId === m.employeeId;
      const marker = L.circleMarker([m.lat, m.lng], {
        radius: m.checkedOut ? (isSel ? 7 : 5) : isSel ? 9 : 7,
        color: m.checkedOut ? "#64748b" : m.inZone ? "#15803d" : "#b91c1c",
        fillColor: hex,
        fillOpacity: m.checkedOut ? 0.45 : 0.92,
        weight: m.checkedOut ? 1 : 2,
        dashArray: m.checkedOut ? "4 3" : undefined,
      });
      marker.bindPopup(buildMonitoringMarkerPopup(m));
      marker.on("click", () => {
        onSelectEmployee(selectedEmployeeId === m.employeeId ? null : m.employeeId);
      });
      marker.addTo(g);
    }
  }, [snapshot.markers, selectedEmployeeId, onSelectEmployee]);

  useEffect(() => {
    const map = mapRef.current;
    if (!map) return;
    if (highlightRef.current) {
      highlightRef.current.remove();
      highlightRef.current = null;
    }
    if (!highlight) return;
    highlightRef.current = L.circleMarker([highlight.lat, highlight.lng], {
      radius: 11,
      color: "#033b7d",
      fillColor: "#60a5fa",
      fillOpacity: 0.35,
      weight: 3,
    }).addTo(map);
    map.panTo([highlight.lat, highlight.lng], { animate: true });
  }, [highlight]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onSelectEmployee(null);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [onSelectEmployee]);

  return <div ref={elRef} className={styles.map} role="application" aria-label="Monitoring map" />;
}
