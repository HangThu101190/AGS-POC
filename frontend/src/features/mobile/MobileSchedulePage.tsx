import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/shared/auth/AuthContext";
import { fetchPhanCongSlot } from "@/shared/api/phanCongSlotApi";
import { dayShortLabel, isPastDay } from "@/shared/planning/weekCalendar";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { agsTokens } from "@/components/theme/tokens";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

type DayShift = {
  assignmentId: string;
  segments: string[];
  flightNos: string[];
};

function formatSegments(segments: string[]): string {
  return segments.join(" · ");
}

/** Staff — lịch ca tuần (prototype ScheduleTab). */
export function MobileSchedulePage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const { weekId, weekMeta, plan } = useWeekScope();
  const [byDay, setByDay] = useState<DayShift[][]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    const load = async () => {
      setError(null);
      try {
        if (!plan) return;
        const boards = await Promise.all(
          Array.from({ length: 7 }, (_, dayIdx) => fetchPhanCongSlot({ weekId, dayIdx })),
        );
        if (cancelled) return;
        const days: DayShift[][] = boards.map((board) => {
          const mine: DayShift[] = [];
          for (const slot of board.slots) {
            for (const a of slot.assignments) {
              if (a.employeeId === user.id) {
                mine.push({
                  assignmentId: a.id,
                  segments: slot.segments,
                  flightNos: a.flightNos.length ? a.flightNos : slot.flightNos,
                });
              }
            }
          }
          return mine;
        });
        setByDay(days);
      } catch {
        if (!cancelled) setError(t("mobile.scheduleLoadFailed"));
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [weekId, plan?.weekId, user, t]);

  const todayIdx = weekMeta.todayIdx;
  const weekDates = weekMeta.weekDates;

  const weekHead = useMemo(
    () => t("mobile.scheduleWeekHead", { wk: weekId }),
    [t, weekId],
  );

  return (
    <MobileScreen title={t("mobile.schedule")} subtitle={weekHead}>
      <div style={{ fontSize: 18, fontWeight: 700, marginBottom: 12, color: agsTokens.textBody }}>
        {t("mobile.scheduleYours")}
      </div>
      {error ? <div style={mobile.bannerWarn}>{error}</div> : null}
      {!plan ? (
        <p style={{ fontSize: 13, color: "#64748b" }}>{t("common.loading")}</p>
      ) : (
        weekDates.map((label, dayIdx) => {
          const shifts = byDay[dayIdx] ?? [];
          const isToday = dayIdx === todayIdx;
          const isPast = isPastDay(dayIdx, todayIdx);
          const [dd, mm] = label.split("/");
          return (
            <div
              key={dayIdx}
              style={{
                display: "flex",
                gap: 12,
                marginBottom: 14,
                opacity: isPast ? 0.55 : 1,
              }}
            >
              <div style={{ width: 44, textAlign: "center", flexShrink: 0 }}>
                <div
                  style={{
                    fontSize: 9,
                    color: isToday ? agsTokens.yellow : "#64748b",
                    textTransform: "uppercase",
                    letterSpacing: 1,
                    fontWeight: 600,
                  }}
                >
                  {dayShortLabel(dayIdx)}
                </div>
                <div
                  style={{
                    fontSize: 16,
                    fontWeight: 700,
                    marginTop: 2,
                    color: isToday ? agsTokens.yellowText : agsTokens.textBody,
                  }}
                >
                  {dd}
                </div>
                <div style={{ fontSize: 9, color: "#94a3b8" }}>{mm}</div>
                {isPast ? (
                  <div
                    style={{
                      fontSize: 8,
                      marginTop: 3,
                      color: "#94a3b8",
                      fontWeight: 700,
                      textTransform: "uppercase",
                    }}
                  >
                    {t("planning.pastBadge")}
                  </div>
                ) : null}
                {isToday ? (
                  <div
                    style={{
                      fontSize: 8,
                      marginTop: 3,
                      color: "#fff",
                      background: agsTokens.yellow,
                      fontWeight: 700,
                      textTransform: "uppercase",
                      padding: "1px 5px",
                      borderRadius: 999,
                    }}
                  >
                    {t("planning.todayBadge")}
                  </div>
                ) : null}
              </div>
              <div style={{ flex: 1 }}>
                {shifts.length === 0 ? (
                  <div style={{ fontSize: 12, color: "#94a3b8", padding: "8px 0", fontStyle: "italic" }}>
                    {t("mobile.scheduleOff")}
                  </div>
                ) : (
                  shifts.map((s) => (
                    <div
                      key={s.assignmentId}
                      style={{
                        ...mobile.card,
                        marginBottom: 8,
                        padding: 12,
                      }}
                    >
                      <div style={{ fontSize: 14, fontWeight: 600 }}>{formatSegments(s.segments)}</div>
                      {s.flightNos.length > 0 ? (
                        <div style={{ display: "flex", gap: 4, marginTop: 6, flexWrap: "wrap" }}>
                          {s.flightNos.map((fn) => (
                            <span
                              key={fn}
                              style={{
                                fontSize: 10,
                                fontWeight: 600,
                                padding: "2px 6px",
                                borderRadius: 4,
                                background: agsTokens.yellowMutedBg,
                                color: "#451a03",
                              }}
                            >
                              {fn}
                            </span>
                          ))}
                        </div>
                      ) : null}
                    </div>
                  ))
                )}
              </div>
            </div>
          );
        })
      )}
    </MobileScreen>
  );
}
