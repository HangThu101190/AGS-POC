import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Typography from "@mui/material/Typography";
import { useCallback, useState } from "react";
import { useTranslation } from "react-i18next";
import { ButtonPrimary, ButtonSecondary } from "@/components/ui";
import { StaffingDayRosterPanel } from "@/features/dailyStaffing/StaffingDayRosterPanel";
import { exportStaffingDay, type StaffingDay } from "@/shared/api/staffingApi";

type Props = {
  open: boolean;
  onClose: () => void;
  weekId: string;
  dayIdx: number;
  dayLabel: string;
  hour: number;
  departmentCode: string;
  day: StaffingDay;
  canExport: boolean;
  canAssign: boolean;
  onLineClick: (lineId: string) => void;
};

export function StaffingDayExportModal({
  open,
  onClose,
  weekId,
  dayIdx,
  dayLabel,
  hour,
  departmentCode,
  day,
  canExport,
  canAssign,
  onLineClick,
}: Props) {
  const { t } = useTranslation();
  const [exporting, setExporting] = useState(false);
  const hourEnd = hour + 1;

  const onDownloadDay = useCallback(async () => {
    setExporting(true);
    try {
      const blob = await exportStaffingDay(weekId, dayIdx, departmentCode);
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `PVHK_Di_${weekId}_${dayLabel.replace(/\//g, ".")}.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setExporting(false);
    }
  }, [weekId, dayIdx, departmentCode, dayLabel]);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth scroll="paper">
      <DialogTitle sx={{ pb: 0.5 }}>
        {t("staffing.dayExportModalTitle", { day: dayLabel })}
        <Typography variant="caption" sx={{ display: "block", color: "text.secondary", mt: 0.5 }}>
          {t("staffing.dayExportModalHour", { hour, hourEnd })}
        </Typography>
      </DialogTitle>
      <DialogContent dividers sx={{ p: { xs: 1, sm: 1.5 }, minHeight: 360 }}>
        <StaffingDayRosterPanel
          day={day}
          dayLabel={dayLabel}
          canAssign={canAssign}
          onLineClick={onLineClick}
        />
      </DialogContent>
      <DialogActions sx={{ px: 2, py: 1.5, gap: 1 }}>
        <ButtonSecondary onClick={onClose}>{t("common.cancel")}</ButtonSecondary>
        {canExport ? (
          <ButtonPrimary disabled={exporting} onClick={() => void onDownloadDay()}>
            {exporting ? t("common.loading") : t("staffing.exportDay")}
          </ButtonPrimary>
        ) : null}
      </DialogActions>
    </Dialog>
  );
}
