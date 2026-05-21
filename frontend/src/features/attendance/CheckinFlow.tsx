import { useCallback, useEffect, useRef, useState, type CSSProperties, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { postCheckIn } from "@/shared/api/attendanceApi";
import { nearOutsideCheckinCoords, pointInCheckinZone } from "@/shared/geofence/checkinZone";
import { ensureCheckinPolygonLoaded } from "@/shared/geofence/workZoneCache";
import { mobile } from "@/features/mobile/mobileStyles";

type Phase = "idle" | "loading" | "ok" | "denied" | "unavailable" | "outside_confirm";

type Props = {
  disabled?: boolean;
  modal?: boolean;
  onCancel?: () => void;
  onCheckedIn?: (meta: { inZone: boolean; geoNote?: string }) => void;
};

/** Geofence check-in with GPS or manual zone confirmation. */
export function CheckinFlow({ disabled, modal, onCancel, onCheckedIn }: Props) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(modal);
  const [phase, setPhase] = useState<Phase>(modal ? "loading" : "idle");
  const [coords, setCoords] = useState<{ lat: number; lng: number } | null>(null);
  const [outsideCoords, setOutsideCoords] = useState<{ lat: number; lng: number } | null>(null);
  const [outsideNote, setOutsideNote] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [geoRetryKey, setGeoRetryKey] = useState(0);
  const skipGeoRef = useRef(false);
  const phaseBeforeOutsideRef = useRef<Phase>("loading");
  const outsideRealGpsRef = useRef(false);

  useEffect(() => {
    void ensureCheckinPolygonLoaded();
  }, []);

  const finish = useCallback(
    async (lat: number, lng: number, inZone: boolean, note?: string, geoSimulated = false) => {
      setSubmitting(true);
      try {
        await postCheckIn({ lat, lng, inZone, geoNote: note, geoSimulated });
        setMessage(inZone ? t("checkinFlow.geoOkTitle") : t("mobile.checkinOutsideOk"));
        onCheckedIn?.({ inZone, geoNote: note });
        setPhase("idle");
        setOpen(false);
        onCancel?.();
      } catch {
        setMessage(t("reconcile.actionFailed"));
      } finally {
        setSubmitting(false);
      }
    },
    [onCancel, onCheckedIn, t],
  );

  const startGeo = useCallback(() => {
    if (disabled || submitting) return;
    setMessage(null);
    setOpen(true);
    if (!navigator.geolocation) {
      setPhase("unavailable");
      return;
    }
    setPhase("loading");
    skipGeoRef.current = false;
  }, [disabled, submitting]);

  useEffect(() => {
    if (phase !== "loading" || !open) return;
    if (!navigator.geolocation) return;
    let ignore = false;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        if (ignore || skipGeoRef.current) return;
        const lat = pos.coords.latitude;
        const lng = pos.coords.longitude;
        const c = { lat, lng };
        setCoords(c);
        setPhase(pointInCheckinZone(lat, lng) ? "ok" : "denied");
      },
      () => {
        if (!ignore && !skipGeoRef.current) setPhase("unavailable");
      },
      { enableHighAccuracy: true, timeout: 14_000, maximumAge: 0 },
    );
    return () => {
      ignore = true;
    };
  }, [geoRetryKey, open, phase]);

  const manualInZone = () => {
    skipGeoRef.current = true;
    const lat = Number(
      import.meta.env.VITE_FALLBACK_GPS_LAT ?? import.meta.env.VITE_DEMO_GPS_LAT ?? "11.9982",
    );
    const lng = Number(
      import.meta.env.VITE_FALLBACK_GPS_LNG ?? import.meta.env.VITE_DEMO_GPS_LNG ?? "109.2192",
    );
    void finish(lat, lng, true, undefined, true);
  };

  const openOutsideConfirm = (useRealGps: boolean) => {
    outsideRealGpsRef.current = useRealGps;
    phaseBeforeOutsideRef.current = phase;
    skipGeoRef.current = true;
    const [oLat, oLng] = nearOutsideCheckinCoords();
    const latLng = useRealGps && coords ? coords : { lat: oLat, lng: oLng };
    setOutsideNote("");
    setOutsideCoords(latLng);
    setPhase("outside_confirm");
  };

  const cancelOutside = () => {
    const back = phaseBeforeOutsideRef.current;
    setOutsideCoords(null);
    setOutsideNote("");
    setPhase(back);
    if (back === "loading") {
      skipGeoRef.current = false;
      setGeoRetryKey((k) => k + 1);
    }
  };

  const confirmOutside = () => {
    if (!outsideCoords) return;
    void finish(
      outsideCoords.lat,
      outsideCoords.lng,
      false,
      outsideNote.trim() || undefined,
      !outsideRealGpsRef.current,
    );
  };

  const confirmInZone = () => {
    if (!coords) return;
    void finish(coords.lat, coords.lng, true, undefined, false);
  };

  const zonePick = (
    <ZonePick
      t={t}
      onInZone={manualInZone}
      onOutside={() => openOutsideConfirm(false)}
      onCurrentGps={coords ? () => openOutsideConfirm(true) : undefined}
    />
  );

  const panel = (
    <>
      {message ? <Banner text={message} ok /> : null}

      {phase === "outside_confirm" && outsideCoords ? (
        <div style={mobile.card}>
          <p style={s.outsideTitle}>{t("checkinFlow.outsideSimTitle")}</p>
          <p style={s.coord}>
            {t("checkinFlow.outsideSimCoordLine", {
              lat: outsideCoords.lat.toFixed(5),
              lng: outsideCoords.lng.toFixed(5),
            })}
          </p>
          <label style={s.noteLbl}>
            {t("checkinFlow.outsideSimNoteLbl")}
            <textarea
              value={outsideNote}
              onChange={(e) => setOutsideNote(e.target.value)}
              placeholder={t("checkinFlow.outsideSimNotePh")}
              rows={3}
              style={s.textarea}
            />
          </label>
          <div style={mobile.btnRow}>
            <button type="button" style={s.outsideOk} onClick={confirmOutside} disabled={submitting}>
              {t("checkinFlow.outsideSimOk")}
            </button>
            <button type="button" style={mobile.btnSecondary} onClick={cancelOutside}>
              {t("checkinFlow.outsideSimCancel")}
            </button>
          </div>
        </div>
      ) : null}

      {phase === "loading" ? (
        <div style={mobile.card}>
          <p style={s.title}>{t("checkinFlow.locating")}</p>
          <p style={s.hint}>{t("checkinFlow.zonePickHint")}</p>
          {zonePick}
        </div>
      ) : null}

      {phase === "ok" && coords ? (
        <div style={mobile.card}>
          <p style={{ ...s.title, color: "#15803d" }}>{t("checkinFlow.geoOkTitle")}</p>
          <p style={s.coord}>
            {t("checkinFlow.confirmCoords", { lat: coords.lat.toFixed(5), lng: coords.lng.toFixed(5) })}
          </p>
          <button type="button" style={mobile.btnPrimary} onClick={confirmInZone} disabled={submitting}>
            {t("checkinFlow.confirmCheckin")}
          </button>
        </div>
      ) : null}

      {(phase === "denied" || phase === "unavailable") && (
        <div style={mobile.card}>
          <p style={{ ...s.title, color: "#991b1b" }}>
            {phase === "denied" ? t("checkinFlow.geoDeniedTitle") : t("checkinFlow.geoUnavailableTitle")}
          </p>
          <p style={s.hint}>
            {phase === "denied" ? t("checkinFlow.geoDeniedBody") : t("checkinFlow.geoUnavailableBody")}
          </p>
          {coords ? (
            <p style={s.coord}>
              {t("checkinFlow.confirmCoords", { lat: coords.lat.toFixed(5), lng: coords.lng.toFixed(5) })}
            </p>
          ) : null}
          {zonePick}
        </div>
      )}
    </>
  );

  return (
    <div>
      {!modal ? (
        <button
          type="button"
          style={mobile.btnPrimary}
          disabled={disabled || submitting}
          onClick={startGeo}
        >
          {t("checkinFlow.startCheckin")}
        </button>
      ) : null}

      {open || modal ? (
        <ModalShell
          onBackdrop={phase === "outside_confirm" ? cancelOutside : () => { setOpen(false); onCancel?.(); }}
        >
          {panel}
        </ModalShell>
      ) : null}
    </div>
  );
}

function ZonePick({
  t,
  onInZone,
  onOutside,
  onCurrentGps,
}: {
  t: (k: string) => string;
  onInZone: () => void;
  onOutside: () => void;
  onCurrentGps?: () => void;
}) {
  return (
    <div style={{ marginTop: 12, display: "flex", flexDirection: "column", gap: 8 }}>
      <button type="button" style={s.simIn} onClick={onInZone}>
        {t("checkinFlow.manualInZoneBtn")}
      </button>
      <button type="button" style={s.simOut} onClick={onOutside}>
        {t("checkinFlow.manualOutsideZoneBtn")}
      </button>
      {onCurrentGps ? (
        <button type="button" style={s.currentGps} onClick={onCurrentGps}>
          {t("checkinFlow.useCurrentGpsBtn")}
        </button>
      ) : null}
    </div>
  );
}

function Banner({ text, ok }: { text: string; ok?: boolean }) {
  return (
    <div
      style={{
        ...mobile.bannerInfo,
        ...(ok ? { background: "#f0fdf4", color: "#166534", border: "1px solid #bbf7d0" } : {}),
        marginBottom: 10,
      }}
    >
      {text}
    </div>
  );
}

function ModalShell({ children, onBackdrop }: { children: ReactNode; onBackdrop: () => void }) {
  return (
    <div style={s.modal} onClick={onBackdrop} role="presentation">
      <div style={s.modalInner} onClick={(e) => e.stopPropagation()} role="dialog" aria-modal>
        {children}
      </div>
    </div>
  );
}

const s = {
  modal: {
    position: "fixed",
    inset: 0,
    background: "rgba(15,23,42,0.45)",
    zIndex: 5000,
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    padding: 16,
  } satisfies CSSProperties,
  modalInner: {
    background: "#fff",
    borderRadius: 12,
    padding: 20,
    maxWidth: 400,
    width: "100%",
    maxHeight: "90vh",
    overflow: "auto",
  } satisfies CSSProperties,
  title: { fontSize: 14, fontWeight: 600, margin: "0 0 6px" } satisfies CSSProperties,
  hint: { fontSize: 12, color: "#64748b", margin: "0 0 8px", lineHeight: 1.45 } satisfies CSSProperties,
  coord: {
    fontSize: 11,
    color: "#475569",
    fontFamily: "ui-monospace, monospace",
    margin: "0 0 10px",
  } satisfies CSSProperties,
  outsideTitle: { fontSize: 15, fontWeight: 700, color: "#9a3412", margin: "0 0 8px" } satisfies CSSProperties,
  noteLbl: {
    display: "block",
    textAlign: "left",
    fontSize: 11,
    fontWeight: 600,
    color: "#475569",
  } satisfies CSSProperties,
  textarea: {
    display: "block",
    width: "100%",
    marginTop: 6,
    padding: "8px 10px",
    fontSize: 12,
    borderRadius: 8,
    border: "1px solid #e2e8f0",
    resize: "vertical",
    fontFamily: "inherit",
    boxSizing: "border-box",
  } satisfies CSSProperties,
  simIn: {
    width: "100%",
    padding: 10,
    border: "1px dashed #f1c662",
    background: "#faf6eb",
    borderRadius: 8,
    fontSize: 12,
    fontWeight: 700,
    cursor: "pointer",
    color: "#451a03",
    fontFamily: "inherit",
  } satisfies CSSProperties,
  simOut: {
    width: "100%",
    padding: 10,
    border: "1px dashed #dc2626",
    background: "#fef2f2",
    borderRadius: 8,
    fontSize: 12,
    fontWeight: 700,
    cursor: "pointer",
    color: "#991b1b",
    fontFamily: "inherit",
  } satisfies CSSProperties,
  currentGps: {
    width: "100%",
    padding: 10,
    border: "1px solid #0f172a",
    background: "#f8fafc",
    borderRadius: 8,
    fontSize: 12,
    fontWeight: 700,
    cursor: "pointer",
    color: "#0f172a",
    fontFamily: "inherit",
  } satisfies CSSProperties,
  outsideOk: {
    flex: 1,
    padding: "10px 12px",
    border: "none",
    borderRadius: 8,
    background: "#c2410c",
    color: "#fff",
    fontSize: 13,
    fontWeight: 700,
    cursor: "pointer",
    fontFamily: "inherit",
  } satisfies CSSProperties,
};
