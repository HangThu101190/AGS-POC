import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  fetchPhanCongSlot,
  removeAssignment,
  type PhanCongSlotSlotDto,
} from "@/shared/api/phanCongSlotApi";
import { isPastDay } from "@/shared/planning/weekCalendar";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

/** Sup — phân công slot mobile (một ngày). */
export function MobileAssignPage() {
  const { t } = useTranslation();
  const { weekId, weekMeta } = useWeekScope();
  const [dayIdx, setDayIdx] = useState(0);
  const [slots, setSlots] = useState<PhanCongSlotSlotDto[]>([]);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = async (d: number) => {
    setError(null);
    try {
      const board = await fetchPhanCongSlot({ weekId, dayIdx: d });
      setSlots(board.slots);
    } catch {
      setError(t("phanCongSlot.loadFailed"));
    }
  };

  useEffect(() => {
    const idx = weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;
    setDayIdx(idx);
    void load(idx);
  }, [weekId, weekMeta.todayIdx]);

  useEffect(() => {
    void load(dayIdx);
  }, [dayIdx, weekId]);

  const todayIdx = weekMeta.todayIdx;
  const isPast = isPastDay(dayIdx, todayIdx);

  return (
    <MobileScreen
      title={t("mobile.assign")}
      subtitle={t("phanCongSlot.subtitle", {
        weekId,
        day: weekMeta.weekDates[dayIdx] ?? "",
      })}
    >
      <div style={{ display: "flex", gap: 6, flexWrap: "wrap", marginBottom: 10 }}>
          {weekMeta.weekDates.map((label, i) => (
            <button
              key={label}
              type="button"
              onClick={() => {
                setDayIdx(i);
                void load(i);
              }}
              style={{
                padding: "6px 10px",
                borderRadius: 999,
                border: i === dayIdx ? "2px solid #033b7d" : "1px solid #cbd5e1",
                background: i === dayIdx ? "#eff6ff" : "#fff",
                fontSize: 11,
                fontWeight: 600,
                opacity: isPastDay(i, todayIdx) ? 0.55 : 1,
                cursor: "pointer",
                fontFamily: "inherit",
              }}
            >
              {label}
              {i === todayIdx ? ` · ${t("planning.todayBadge")}` : ""}
            </button>
          ))}
      </div>

      {message ? (
        <div style={{ ...mobile.bannerInfo, background: "#f0fdf4", color: "#166534" }}>{message}</div>
      ) : null}
      {error ? <div style={mobile.bannerWarn}>{error}</div> : null}

      {slots.map((slot) => (
        <div key={slot.id} style={mobile.card}>
          <div style={{ fontSize: 14, fontWeight: 700 }}>
            {slot.segments.join(" · ")} · {slot.headcount} NV
          </div>
          {slot.assignments.map((a) => (
            <div
              key={a.id}
              style={{
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                marginTop: 8,
                fontSize: 13,
              }}
            >
              <span>
                {a.employeeName} ({a.employeeCode})
              </span>
              {!isPast ? (
                <button
                  type="button"
                  style={{ ...mobile.btnSecondary, flex: "none", padding: "4px 8px" }}
                  onClick={async () => {
                    await removeAssignment(a.id, weekId);
                    setMessage(t("phanCongSlot.removed"));
                    await load(dayIdx);
                  }}
                >
                  ✕
                </button>
              ) : null}
            </div>
          ))}
        </div>
      ))}
    </MobileScreen>
  );
}
