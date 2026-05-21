import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/shared/auth/AuthContext";
import { fetchMyAttendance, fetchMyTodayShift } from "@/shared/api/attendanceApi";
import { getAccessToken } from "@/shared/auth/tokenStorage";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { connectStaffHub } from "@/shared/signalr/staffHub";
import { MobileCheckinFlow } from "./MobileCheckinFlow";
import { CheckoutFlow } from "@/features/attendance/CheckoutFlow";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

/** Staff — ca hôm nay (prototype TodayTab). */
export function MobileTodayPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { weekId } = useWeekScope();
  const [planStatus, setPlanStatus] = useState<string>("draft");
  const [segments, setSegments] = useState<string[]>([]);
  const [flights, setFlights] = useState<string[]>([]);
  const [checkedIn, setCheckedIn] = useState(false);
  const [hasRevision, setHasRevision] = useState(false);

  const applyShift = (shift: Awaited<ReturnType<typeof fetchMyTodayShift>>) => {
    setPlanStatus(shift.planStatus);
    setSegments(shift.segments ?? []);
    setFlights(shift.flightNos ?? []);
    setHasRevision(Boolean(shift.shiftChanged));
  };

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    const load = async () => {
      try {
        const shift = await fetchMyTodayShift(weekId);
        if (cancelled) return;
        applyShift(shift);
        try {
          const att = await fetchMyAttendance(user.id);
          if (!cancelled) setCheckedIn(att.isActive);
        } catch {
          if (!cancelled) setCheckedIn(false);
        }
      } catch {
        if (!cancelled) {
          setPlanStatus("draft");
          setSegments([]);
          setHasRevision(false);
        }
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [user, weekId]);

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    const reload = async () => {
      try {
        const shift = await fetchMyTodayShift(weekId);
        if (!cancelled) applyShift(shift);
      } catch {
        /* ignore */
      }
    };
    const disconnect = connectStaffHub((event) => {
      if (event.type === "shiftChanged") {
        void reload();
        setHasRevision(true);
      }
    }, getAccessToken() ?? undefined);
    return () => {
      cancelled = true;
      void disconnect.then((fn) => fn());
    };
  }, [user, weekId]);

  const planNorm = planStatus.toLowerCase();
  const canShowShift =
    planNorm === "published" || planNorm === "in_progress" || planNorm === "inprogress";
  const showShift = canShowShift && segments.length > 0;

  const shiftLabel = useMemo(() => segments.join(" · "), [segments]);

  return (
    <MobileScreen title={t("mobile.today")} subtitle={t("mobile.todaySubtitle")}>
      {!canShowShift ? (
        <div style={mobile.bannerInfo}>{t("mobile.todayHint")}</div>
      ) : null}

      {hasRevision ? (
        <div style={{ ...mobile.bannerWarn, marginBottom: 10 }}>
          {t("mobile.shiftChanged")}
        </div>
      ) : null}

      {showShift ? (
        <div style={mobile.card}>
          <div style={{ fontSize: 14, fontWeight: 600 }}>{user?.name ?? user?.code}</div>
          <p style={{ fontSize: 15, fontWeight: 700, margin: "8px 0", color: "#033b7d" }}>{shiftLabel}</p>
          {flights.length > 0 ? (
            <div style={{ display: "flex", gap: 4, flexWrap: "wrap", marginBottom: 8 }}>
              {flights.map((fn) => (
                <span
                  key={fn}
                  style={{
                    fontSize: 10,
                    fontWeight: 600,
                    padding: "2px 6px",
                    borderRadius: 4,
                    background: "#faf6eb",
                    color: "#451a03",
                  }}
                >
                  {fn}
                </span>
              ))}
            </div>
          ) : null}
          {checkedIn ? (
            <>
              <p style={{ fontSize: 13, color: "#15803d", margin: "0 0 8px" }}>
                {t("mobile.alreadyCheckedIn")}
              </p>
              <CheckoutFlow onCheckedOut={() => setCheckedIn(false)} />
            </>
          ) : (
            <MobileCheckinFlow
              onCheckedIn={() => {
                setCheckedIn(true);
                setHasRevision(false);
              }}
            />
          )}
        </div>
      ) : (
        <div style={mobile.card}>
          <p style={{ fontSize: 13, color: "#64748b", margin: 0 }}>{t("mobile.shiftPending")}</p>
        </div>
      )}
    </MobileScreen>
  );
}
