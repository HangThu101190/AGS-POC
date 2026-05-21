import { useCallback, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Navigate } from "react-router-dom";
import {
  Alert,
  ButtonPrimary,
  CancelButton,
  Loading,
} from "@/components/ui";
import { useAuth } from "@/shared/auth/AuthContext";
import { getRoleHomePath } from "@/shared/auth/roles";
import { usePermissions } from "@/shared/auth/usePermissions";
import {
  fetchActiveWorkZone,
  updateWorkZonePolygon,
  type WorkZoneDto,
} from "@/shared/api/workZoneApi";
import { CHECKIN_ZONE_DEFAULT_POLYGON } from "@/shared/geofence/checkinZone";
import { invalidateWorkZoneCache } from "@/shared/geofence/workZoneCache";
import { pointsToVerts, vertsToPoints, type LatLngPoint } from "@/shared/geofence/polygonText";
import { PageShell } from "@/shared/webChrome";
import { WorkZonePolygonMap } from "./WorkZonePolygonMap";
import styles from "./workZoneConfig.module.css";

const BTN_SIZE = "medium" as const;

function defaultPoly(): LatLngPoint[] {
  return CHECKIN_ZONE_DEFAULT_POLYGON.map(([lat, lng]) => ({ lat, lng }));
}

function normalizePoint(p: { lat?: number; lng?: number; Lat?: number; Lng?: number }): LatLngPoint | null {
  const lat = Number(p.lat ?? p.Lat);
  const lng = Number(p.lng ?? p.Lng);
  if (!Number.isFinite(lat) || !Number.isFinite(lng)) return null;
  return { lat, lng };
}

function ringFromZone(z: WorkZoneDto | null): LatLngPoint[] {
  if (z && z.polygon.length >= 3) {
    const ring = z.polygon.map(normalizePoint).filter((p): p is LatLngPoint => p !== null);
    if (ring.length >= 3) return ring;
  }
  return defaultPoly();
}

type WorkZoneConfigPageProps = {
  /** Render inside Config hub without duplicate chrome / HR-only gate. */
  embedded?: boolean;
};

/** HCNS — khu vực làm việc (geofence polygon) với vẽ trên bản đồ. */
export function WorkZoneConfigPage({ embedded = false }: WorkZoneConfigPageProps = {}) {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const permissions = usePermissions();
  const initialRing = defaultPoly();
  const [savedRing, setSavedRing] = useState<LatLngPoint[]>(initialRing);
  const [draftVerts, setDraftVerts] = useState<[number, number][]>(() => pointsToVerts(initialRing));
  const [drawMode, setDrawMode] = useState(false);
  const [fitToken, setFitToken] = useState(0);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const syncDraftFromRing = useCallback((ring: LatLngPoint[]) => {
    setDraftVerts(pointsToVerts(ring));
  }, []);

  useEffect(() => {
    let cancelled = false;
    const load = async () => {
      setLoading(true);
      try {
        const z = await fetchActiveWorkZone();
        if (cancelled) return;
        const ring = ringFromZone(z);
        setSavedRing(ring);
        syncDraftFromRing(ring);
        setFitToken((n) => n + 1);
      } catch {
        if (!cancelled) {
          const ring = defaultPoly();
          setSavedRing(ring);
          syncDraftFromRing(ring);
          setFitToken((n) => n + 1);
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [syncDraftFromRing]);

  const onMapClick = useCallback((lat: number, lng: number) => {
    setDraftVerts((prev) => [...prev, [lat, lng] as [number, number]]);
    setError(null);
  }, []);

  const resetFromSaved = useCallback(() => {
    syncDraftFromRing(savedRing);
    setFitToken((n) => n + 1);
    setError(null);
  }, [savedRing, syncDraftFromRing]);

  const startDraw = () => {
    setDraftVerts([]);
    setDrawMode(true);
    setFitToken((n) => n + 1);
    setError(null);
  };

  const cancelDraw = () => {
    resetFromSaved();
    setDrawMode(false);
  };

  const save = async () => {
    if (draftVerts.length < 3) {
      setError(t("config.workZoneInvalid"));
      return;
    }
    setError(null);
    setMessage(null);
    try {
      setSaving(true);
      const updated = await updateWorkZonePolygon(vertsToPoints(draftVerts));
      invalidateWorkZoneCache();
      const ring = ringFromZone(updated);
      setSavedRing(ring);
      syncDraftFromRing(ring);
      setDrawMode(false);
      setFitToken((n) => n + 1);
      setMessage(t("config.workZoneSaved"));
    } catch {
      setError(t("config.workZoneInvalid"));
    } finally {
      setSaving(false);
    }
  };

  if (!embedded && permissions && !permissions.canAccessConfig) {
    return <Navigate to={getRoleHomePath(user?.role ?? "staff", i18n.language)} replace />;
  }

  if (!embedded && user && user.role !== "hr") {
    return (
      <PageShell>
        <Alert severity="info">{t("errors.forbiddenBody")}</Alert>
      </PageShell>
    );
  }

  const polygonReady = draftVerts.length >= 3;

  const body = (
      <div className={styles.page}>
        {loading ? <Loading label={t("common.loading")} /> : null}

        <div className={styles.toolbar}>
          {!drawMode ? (
            <ButtonPrimary size={BTN_SIZE} onClick={startDraw} disabled={loading}>
              {t("config.drawOn")}
            </ButtonPrimary>
          ) : (
            <>
              <ButtonPrimary
                size={BTN_SIZE}
                onClick={() => void save()}
                disabled={saving || loading || !polygonReady}
              >
                {saving ? t("common.loading") : t("config.drawSave")}
              </ButtonPrimary>
              <CancelButton size={BTN_SIZE} onClick={cancelDraw} disabled={saving || loading}>
                {t("config.drawCancel")}
              </CancelButton>
            </>
          )}
        </div>

        <section className={styles.mapSection} aria-label={t("config.workZoneTitle")}>
          {drawMode ? (
            <div className={styles.mapHeader}>
              <span className={[styles.statusChip, polygonReady ? styles.statusReady : ""].filter(Boolean).join(" ")}>
                {t("config.vertexCount", { n: draftVerts.length })}
                {polygonReady ? ` · ${t("config.polygonReady")}` : ""}
              </span>
            </div>
          ) : null}

          <div className={styles.mapWrap}>
            <WorkZonePolygonMap
              className={styles.mapEmbed}
              savedPolygon={savedRing}
              draftVerts={draftVerts}
              drawMode={drawMode}
              onMapClick={onMapClick}
              fitToken={fitToken}
              onMapReset={() => setFitToken((n) => n + 1)}
              resetTitle={t("monitoring.mapReset")}
            />
          </div>

          {drawMode ? <p className={styles.mapHint}>{t("config.drawHintActive")}</p> : null}
        </section>

        {error ? (
          <Alert severity="error" onClose={() => setError(null)}>
            {error}
          </Alert>
        ) : null}
        {message ? (
          <Alert severity="success" onClose={() => setMessage(null)}>
            {message}
          </Alert>
        ) : null}
      </div>
  );

  if (embedded) {
    return body;
  }

  return <PageShell fill>{body}</PageShell>;
}
