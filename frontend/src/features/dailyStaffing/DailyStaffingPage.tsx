import Stack from "@mui/material/Stack";
import { useEffect } from "react";
import { useTranslation } from "react-i18next";
import { useSearchParams } from "react-router-dom";
import { Alert } from "@/components/ui";
import { WeekPageShell } from "@/shared/webChrome/WeekPageShell";
import { DailyStaffingWeekBoard } from "@/features/dailyStaffing/DailyStaffingWeekBoard";
import { useAuth } from "@/shared/auth/AuthContext";
import {
  canEditStaffingManning,
  canManageDailyStaffing,
  canViewStaffingWeek,
} from "@/shared/auth/roles";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";

export function DailyStaffingPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [params, setParams] = useSearchParams();
  const canViewStaffing = user ? canViewStaffingWeek(user.role) : false;
  const canEditManning = user ? canEditStaffingManning(user.role) : false;
  const canAssignStaffing = user ? canManageDailyStaffing(user.role) : false;
  const canExportStaffing = canViewStaffing;

  useWeekScope();

  useEffect(() => {
    if (params.get("view") === "weekly") {
      const next = new URLSearchParams(params);
      next.delete("view");
      setParams(next, { replace: true });
    }
  }, [params, setParams]);

  if (!canViewStaffing) {
    return (
      <WeekPageShell>
        <Alert severity="info">{t("dailyStaffing.roleDenied")}</Alert>
      </WeekPageShell>
    );
  }

  if (!user) {
    return (
      <WeekPageShell>
        <Alert severity="info">{t("dailyStaffing.roleDenied")}</Alert>
      </WeekPageShell>
    );
  }

  return (
    <WeekPageShell weekInHeader={false} fill>
      <Stack spacing={1} sx={{ minWidth: 0, flex: 1, minHeight: 0, height: "100%" }}>
        <DailyStaffingWeekBoard
          canEditManning={canEditManning}
          canAssign={canAssignStaffing}
          canExport={canExportStaffing}
        />
      </Stack>
    </WeekPageShell>
  );
}
