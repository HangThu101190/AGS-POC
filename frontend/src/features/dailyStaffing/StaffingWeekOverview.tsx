import { useMemo } from "react";
import { useQueries } from "@tanstack/react-query";
import Box from "@mui/material/Box";
import Card from "@mui/material/Card";
import CardActionArea from "@mui/material/CardActionArea";
import CardContent from "@mui/material/CardContent";
import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { Alert, Loading } from "@/components/ui";
import { isQnSegment, isQtSegment } from "@/features/dailyStaffing/staffingSegment";
import { fetchStaffingDay, type StaffingDay } from "@/shared/api/staffingApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { usePlanningDaySync } from "@/shared/signalr/usePlanningDaySync";

const DAY_INDICES = [0, 1, 2, 3, 4, 5, 6] as const;

type Props = {
  departmentCode: string;
  onOpenDay: (dayIdx: number) => void;
};

function statusLabel(status: string, t: (k: string) => string) {
  switch (status) {
    case "Published":
      return t("dailyStaffing.statusPublished");
    case "Confirmed":
      return t("dailyStaffing.statusConfirmed");
    case "CrewDraft":
      return t("dailyStaffing.statusCrewDraft");
    case "Proposed":
      return t("dailyStaffing.statusProposed");
    default:
      return t("dailyStaffing.statusDraft");
  }
}

function dayStats(day: StaffingDay | undefined) {
  if (!day) return { flights: 0, qn: 0, qt: 0, assigned: 0, gaps: 0 };
  const qnLines = day.lines.filter((l) => isQnSegment(l.segment));
  const qtLines = day.lines.filter((l) => isQtSegment(l.segment));
  const assigned = day.assignments.length;
  const gaps = day.lines.reduce((sum, l) => {
    const onLine = day.assignments.filter((a) => a.staffingLineId === l.id).length;
    return sum + Math.max(0, l.targetManning - onLine);
  }, 0);
  return { flights: day.flights.length, qn: qnLines.length, qt: qtLines.length, assigned, gaps };
}

export function StaffingWeekOverview({ departmentCode, onOpenDay }: Props) {
  const { t } = useTranslation();
  const { weekId, weekMeta } = useWeekScope();
  const hubDay = weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;
  usePlanningDaySync(weekId, hubDay, { invalidateStaffingWeek: true });

  const dayQueries = useQueries({
    queries: DAY_INDICES.map((dayIdx) => ({
      queryKey: ["staffing", weekId, dayIdx, departmentCode] as const,
      queryFn: () => fetchStaffingDay(weekId, dayIdx, departmentCode),
    })),
  });

  const weekDates = weekMeta.weekDates;
  const isLoading = dayQueries.some((q) => q.isLoading);
  const loadError = dayQueries.find((q) => q.error)?.error;

  const cards = useMemo(
    () =>
      DAY_INDICES.map((dayIdx, i) => ({
        dayIdx,
        label: weekDates[dayIdx] ?? `Day ${dayIdx + 1}`,
        day: dayQueries[i]?.data,
      })),
    [dayQueries, weekDates],
  );

  if (isLoading) return <Loading label={t("common.loading")} />;
  if (loadError) return <Alert severity="error">{String(loadError)}</Alert>;

  return (
    <Stack spacing={2} sx={{ minHeight: 0, flex: 1 }}>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(160px, 1fr))",
          gap: 1.5,
        }}
      >
        {cards.map(({ dayIdx, label, day }) => {
          const stats = dayStats(day);
          const status = day?.plan.status ?? "Draft";
          const locked = day?.plan.isLocked;
          return (
            <Card key={dayIdx} variant="outlined" sx={{ opacity: locked ? 0.85 : 1 }}>
              <CardActionArea onClick={() => onOpenDay(dayIdx)}>
                <CardContent>
                  <Typography variant="subtitle2">{label}</Typography>
                  <Chip size="small" label={statusLabel(status, t)} sx={{ mt: 0.5, mb: 1 }} />
                  <Typography variant="caption" sx={{ display: "block" }}>
                    {t("dailyStaffing.cardFlights", { count: stats.flights })}
                    {stats.flights > 0
                      ? ` · ${t("dailyStaffing.cardSegments", { qn: stats.qn, qt: stats.qt })}`
                      : ""}
                  </Typography>
                  <Typography variant="caption" sx={{ display: "block" }}>
                    {t("dailyStaffing.assignedCount", { count: stats.assigned })}
                  </Typography>
                  {stats.gaps > 0 ? (
                    <Typography variant="caption" color="error.main" sx={{ display: "block" }}>
                      {t("dailyStaffing.gapCount", { count: stats.gaps })}
                    </Typography>
                  ) : null}
                </CardContent>
              </CardActionArea>
            </Card>
          );
        })}
      </Box>
    </Stack>
  );
}
