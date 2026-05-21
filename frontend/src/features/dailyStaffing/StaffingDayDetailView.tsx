import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { Alert, Loading } from "@/components/ui";
import { StaffingDayExcelGrid } from "@/features/dailyStaffing/StaffingDayExcelGrid";
import { StaffingDayExportModal } from "@/features/dailyStaffing/StaffingDayExportModal";
import styles from "@/features/dailyStaffing/dailyStaffingPage.module.css";
import { fetchStaffingDay, publishStaffingDay } from "@/shared/api/staffingApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { usePlanningDaySync } from "@/shared/signalr/usePlanningDaySync";
import { ActionButton } from "@/shared/webChrome";

type Props = {
  dayIdx: number;
  departmentCode: string;
  canAssign: boolean;
  canExport: boolean;
  onBack: () => void;
};

export function StaffingDayDetailView({
  dayIdx,
  departmentCode,
  canAssign,
  canExport,
  onBack,
}: Props) {
  const { t } = useTranslation();
  const { weekId, weekMeta } = useWeekScope();
  const queryClient = useQueryClient();
  const [showExport, setShowExport] = useState(false);
  const [actionMsg, setActionMsg] = useState<{ severity: "success" | "error"; text: string } | null>(
    null,
  );

  usePlanningDaySync(weekId, dayIdx);

  const dayKey = ["staffing", weekId, dayIdx, departmentCode] as const;
  const dayQ = useQuery({
    queryKey: dayKey,
    queryFn: () => fetchStaffingDay(weekId, dayIdx, departmentCode),
  });

  const invalidate = () => void queryClient.invalidateQueries({ queryKey: dayKey });

  const publishMut = useMutation({
    mutationFn: () => publishStaffingDay(weekId, dayIdx, departmentCode),
    onSuccess: () => {
      invalidate();
      setActionMsg({ severity: "success", text: t("dailyStaffing.publishOk") });
    },
    onError: () => setActionMsg({ severity: "error", text: t("staffing.actionFailed") }),
  });

  if (dayQ.isLoading) return <Loading label={t("common.loading")} />;
  if (dayQ.error || !dayQ.data) return <Alert severity="error">{t("common.error")}</Alert>;

  const day = dayQ.data;
  const locked = !!day.plan.isLocked;
  const dayLabel = weekMeta.weekDates[dayIdx] ?? `Day ${dayIdx + 1}`;

  return (
    <Stack spacing={1.5} sx={{ minHeight: 0, flex: 1 }}>
      <div className={styles.toolbarRow}>
        <div className={styles.toolbarWeek}>
          <ActionButton className={styles.toolbarBtn} variant="secondary" onClick={onBack}>
            {t("dailyStaffing.backToWeek")}
          </ActionButton>
          <Typography component="span" variant="subtitle1" sx={{ fontWeight: 700, ml: 0.5 }}>
            {dayLabel}
          </Typography>
        </div>
        <div className={styles.toolbarActions}>
          {canAssign && !locked ? (
            <ActionButton
              className={`${styles.toolbarBtn} ${styles.toolbarBtnPrimary}`}
              variant="primary"
              onClick={() => publishMut.mutate()}
              disabled={publishMut.isPending}
            >
              {t("dailyStaffing.publishLock")}
            </ActionButton>
          ) : null}
          {canExport ? (
            <ActionButton
              className={styles.toolbarBtn}
              variant="secondary"
              onClick={() => setShowExport(true)}
            >
              {t("dailyStaffing.exportPreview")}
            </ActionButton>
          ) : null}
        </div>
      </div>

      {actionMsg ? (
        <Alert severity={actionMsg.severity} onClose={() => setActionMsg(null)}>
          {actionMsg.text}
        </Alert>
      ) : null}
      {locked ? <Alert severity="info">{t("dailyStaffing.dayLocked")}</Alert> : null}

      <StaffingDayExcelGrid
        weekId={weekId}
        dayIdx={dayIdx}
        departmentCode={departmentCode}
        day={day}
        locked={locked}
        canAssign={canAssign}
        onChanged={invalidate}
      />

      <StaffingDayExportModal
        open={showExport}
        onClose={() => setShowExport(false)}
        weekId={weekId}
        dayIdx={dayIdx}
        dayLabel={dayLabel}
        hour={0}
        departmentCode={departmentCode}
        day={day}
        canExport={canExport}
        canAssign={canAssign && !locked}
        onLineClick={() => setShowExport(false)}
      />
    </Stack>
  );
}
