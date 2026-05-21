import { useMutation } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router-dom";
import { Alert } from "@/components/ui";
import { ActionButton } from "@/shared/webChrome";
import styles from "@/features/dailyStaffing/dailyStaffingPage.module.css";
import { WeekPicker } from "@/features/planning/WeekPicker";
import { exportStaffingWeek } from "@/shared/api/staffingApi";
import { StaffingDayDetailView } from "@/features/dailyStaffing/StaffingDayDetailView";
import { StaffingWeekOverview } from "@/features/dailyStaffing/StaffingWeekOverview";
import { DailyStaffingWeekBoard } from "@/features/dailyStaffing/DailyStaffingWeekBoard";
import { useAuth } from "@/shared/auth/AuthContext";
import {
  canEditStaffingManning,
  canManageDailyStaffing,
  canViewStaffingWeek,
} from "@/shared/auth/roles";
import { resolveStaffingDepartmentCode } from "@/shared/planning/staffingDepartment";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { WeekPageShell } from "@/shared/webChrome/WeekPageShell";

export function DailyStaffingPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [params, setParams] = useSearchParams();
  const { weekId, weekMeta } = useWeekScope();
  const canViewStaffing = user ? canViewStaffingWeek(user.role) : false;
  const canEditManning = user ? canEditStaffingManning(user.role) : false;
  const canAssignStaffing = user ? canManageDailyStaffing(user.role) : false;
  const canExportStaffing = canViewStaffing;
  const dept = resolveStaffingDepartmentCode(user?.departmentCode);

  const view = params.get("view");
  const ngayParam = params.get("ngay");
  const dayIdx =
    ngayParam != null && ngayParam !== ""
      ? Number(ngayParam)
      : weekMeta.todayIdx >= 0
        ? weekMeta.todayIdx
        : 0;
  const showDayDetail = view !== "timeline" && ngayParam != null && ngayParam !== "";
  const showTimeline = view === "timeline";

  const openDay = (idx: number) => {
    const next = new URLSearchParams(params);
    next.set("ngay", String(idx));
    next.delete("view");
    setParams(next);
  };

  const backToWeek = () => {
    const next = new URLSearchParams(params);
    next.delete("ngay");
    next.delete("view");
    setParams(next);
  };

  const openWeekOverview = () => {
    const next = new URLSearchParams(params);
    next.delete("view");
    next.delete("ngay");
    setParams(next);
  };

  const openTimeline = () => {
    const next = new URLSearchParams(params);
    next.set("view", "timeline");
    next.delete("ngay");
    setParams(next);
  };

  const exportMut = useMutation({
    mutationFn: () => exportStaffingWeek(weekId, dept),
    onSuccess: (blob) => {
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `PVHK_Di_${weekId}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    },
  });

  if (!canViewStaffing || !user) {
    return (
      <WeekPageShell>
        <Alert severity="info">{t("dailyStaffing.roleDenied")}</Alert>
      </WeekPageShell>
    );
  }

  return (
    <WeekPageShell weekInHeader={false} fill>
      <div className={styles.page}>
        {!showDayDetail ? (
          <div className={styles.toolbarRow}>
            <div className={styles.toolbarWeek}>
              <WeekPicker inline inRow />
            </div>
            <div className={styles.toolbarActions}>
              <ActionButton
                className={styles.toolbarBtn}
                variant={!showTimeline ? "primary" : "secondary"}
                onClick={openWeekOverview}
              >
                {t("dailyStaffing.viewWeek")}
              </ActionButton>
              <ActionButton
                className={styles.toolbarBtn}
                variant={showTimeline ? "primary" : "secondary"}
                onClick={openTimeline}
              >
                {t("dailyStaffing.viewTimeline")}
              </ActionButton>
              {canExportStaffing ? (
                <ActionButton
                  className={styles.toolbarBtn}
                  variant="secondary"
                  onClick={() => exportMut.mutate()}
                  disabled={exportMut.isPending}
                >
                  {t("dailyStaffing.exportWeek")}
                </ActionButton>
              ) : null}
            </div>
          </div>
        ) : null}
        {showTimeline ? (
          <DailyStaffingWeekBoard
            canEditManning={canEditManning}
            canAssign={canAssignStaffing}
          />
        ) : showDayDetail ? (
          <StaffingDayDetailView
            dayIdx={dayIdx}
            departmentCode={dept}
            canAssign={canAssignStaffing}
            canExport={canExportStaffing}
            onBack={backToWeek}
          />
        ) : (
          <StaffingWeekOverview departmentCode={dept} onOpenDay={openDay} />
        )}
      </div>
    </WeekPageShell>
  );
}
