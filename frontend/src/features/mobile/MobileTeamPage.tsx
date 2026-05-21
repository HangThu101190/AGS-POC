import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/shared/auth/AuthContext";
import { fetchMonitoringSnapshotData } from "@/shared/api/monitoringApi";
import { fetchPhanCongSlot } from "@/shared/api/phanCongSlotApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { formatLocaleTime } from "@/shared/utils/dateLocale";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

type TeamRow = {
  assignmentId: string;
  employeeId: string;
  name: string;
  segments: string[];
  checkedIn: boolean;
  checkInTime?: string;
};

export function MobileTeamPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const { weekId, weekMeta, plan } = useWeekScope();
  const [rows, setRows] = useState<TeamRow[]>([]);
  const [dayLabel, setDayLabel] = useState("");
  const [deptLabel, setDeptLabel] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    const load = async () => {
      if (!plan) return;
      setLoading(true);
      try {
        const dayIdx =
          weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;
        setDayLabel(weekMeta.weekDates[dayIdx] ?? "");
        const board = await fetchPhanCongSlot({ weekId, dayIdx });
        const snap = await fetchMonitoringSnapshotData();
        if (cancelled) return;
        setDeptLabel(departmentLabel(board.departmentCode, t));
        const checkedIds = new Set(snap.checkedIn.map((e) => e.employeeId));
        const markerByEmp = new Map(
          snap.markers.map((m) => [m.employeeId, m.checkInUtc] as const),
        );
        const list: TeamRow[] = [];
        for (const slot of board.slots) {
          for (const a of slot.assignments) {
            list.push({
              assignmentId: a.id,
              employeeId: a.employeeId,
              name: a.employeeName,
              segments: slot.segments,
              checkedIn: checkedIds.has(a.employeeId),
              checkInTime: markerByEmp.get(a.employeeId),
            });
          }
        }
        setRows(list);
      } catch {
        if (!cancelled) setRows([]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [user, weekId, weekMeta.todayIdx, weekMeta.weekDates, plan, t]);

  const checkedCount = useMemo(() => rows.filter((r) => r.checkedIn).length, [rows]);

  return (
    <MobileScreen title={t("mobile.team")} subtitle={t("mobile.teamSubtitle", { day: dayLabel })}>
      <div style={{ fontSize: 18, fontWeight: 700, marginBottom: 12, color: "#0f172a" }}>{deptLabel}</div>

      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 8, marginBottom: 16 }}>
        <div style={mobile.miniMetric}>
          <div style={{ fontSize: 22, fontWeight: 700 }}>{rows.length}</div>
          <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase" }}>
            {t("mobile.teamAssigned")}
          </div>
        </div>
        <div style={mobile.miniMetric}>
          <div style={{ fontSize: 22, fontWeight: 700, color: "#15803d" }}>{checkedCount}</div>
          <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase" }}>
            {t("mobile.teamCheckedIn")}
          </div>
        </div>
      </div>

      {loading ? (
        <p style={{ fontSize: 13, color: "#64748b" }}>{t("common.loading")}</p>
      ) : rows.length === 0 ? (
        <div style={mobile.card}>
          <div style={{ fontSize: 14, fontWeight: 600 }}>{t("mobile.teamEmptyTitle")}</div>
          <p style={{ fontSize: 11, color: "#64748b", margin: "4px 0 0" }}>{t("mobile.teamEmptyP")}</p>
        </div>
      ) : (
        rows.map((row) => (
          <div key={row.assignmentId} style={mobile.teamRow}>
            <div style={mobile.teamAvatar}>{row.name.split(" ").pop()?.[0] ?? "?"}</div>
            <div style={{ flex: 1, minWidth: 0 }}>
              <div
                style={{
                  fontSize: 13,
                  fontWeight: 600,
                  overflow: "hidden",
                  textOverflow: "ellipsis",
                }}
              >
                {row.name}
              </div>
              <div style={{ fontSize: 10, color: "#64748b" }}>{row.segments.join(" · ")}</div>
              {row.checkedIn && row.checkInTime ? (
                <div style={{ fontSize: 10, color: "#15803d", marginTop: 2 }}>
                  {t("mobile.teamInAt", {
                    time: formatLocaleTime(row.checkInTime, i18n.language),
                  })}
                </div>
              ) : null}
            </div>
            {!row.checkedIn ? (
              <button
                type="button"
                style={mobile.btnSm}
                title={t("mobile.teamOverrideSoon")}
                onClick={() => window.alert(t("mobile.teamOverrideSoon"))}
              >
                {t("mobile.teamOverride")}
              </button>
            ) : null}
          </div>
        ))
      )}
    </MobileScreen>
  );
}
