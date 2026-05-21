import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { createRoot, type Root } from "react-dom/client";
import { useMutation, useQueries, useQueryClient } from "@tanstack/react-query";
import Box from "@mui/material/Box";
import Tooltip from "@mui/material/Tooltip";
import FullCalendar from "@fullcalendar/react";
import timeGridPlugin from "@fullcalendar/timegrid";
import interactionPlugin from "@fullcalendar/interaction";
import type { EventContentArg, SlotLabelContentArg, SlotLaneContentArg } from "@fullcalendar/core";
import { useTranslation } from "react-i18next";
import { Alert, Loading } from "@/components/ui";
import weekPickerStyles from "@/features/planning/weekPicker.module.css";
import { WeekPicker } from "@/features/planning/WeekPicker";
import {
  computeHourSlotStats,
  defaultWorkWindowForLineHour,
  lineForFlight,
  type HourSlotStats,
} from "@/features/dailyStaffing/staffingHeadcount";
import { StaffingHourSlotCard } from "@/features/dailyStaffing/StaffingHourSlotCard";
import { calendarDateIso, isCurrentHourSlot } from "@/features/dailyStaffing/staffingCalendar";
import { scrollTimeForShiftGrid, shiftSlotTimeLabel } from "@/features/dailyStaffing/staffingShiftSlots";
import { buildWeekStaffingEvents } from "@/features/dailyStaffing/staffingWeekEvents";
import calendarStyles from "@/features/dailyStaffing/staffingWeekCalendar.module.css";
import { resolveStaffingDepartmentCode } from "@/shared/planning/staffingDepartment";
import {
  createStaffingAssignment,
  deleteStaffingAssignment,
  exportStaffingWeek,
  fetchStaffingDay,
  fetchStaffingRoster,
  type StaffingDay,
} from "@/shared/api/staffingApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { usePlanningDaySync } from "@/shared/signalr/usePlanningDaySync";
import { useAuth } from "@/shared/auth/AuthContext";

type Props = {
  canEditManning: boolean;
  canAssign: boolean;
  canExport: boolean;
};

const DAY_INDICES = [0, 1, 2, 3, 4, 5, 6] as const;

type HourSlotMount = { root: Root; el: HTMLElement; dayIdx: number; hour: number };

function scheduleRootUnmount(root: Root) {
  queueMicrotask(() => root.unmount());
}

export function DailyStaffingWeekBoard({ canAssign, canExport }: Props) {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const { weekId, weekMeta } = useWeekScope();
  const queryClient = useQueryClient();
  const dept = resolveStaffingDepartmentCode(user?.departmentCode);

  const hubDay =
    weekMeta.todayIdx >= 0 && weekMeta.todayIdx <= 6 ? weekMeta.todayIdx : 0;

  usePlanningDaySync(weekId, hubDay, { invalidateStaffingWeek: true });

  const dayQueries = useQueries({
    queries: DAY_INDICES.map((dayIdx) => ({
      queryKey: ["staffing", weekId, dayIdx, dept] as const,
      queryFn: () => fetchStaffingDay(weekId, dayIdx, dept),
    })),
  });

  const rosterQueries = useQueries({
    queries: DAY_INDICES.map((dayIdx) => ({
      queryKey: ["staffing", "roster", weekId, dayIdx, dept] as const,
      queryFn: () => fetchStaffingRoster(weekId, dayIdx, dept),
      enabled: canAssign,
      staleTime: 30_000,
    })),
  });

  const rosterByDayIdx = useMemo(() => {
    const map = new Map<number, NonNullable<(typeof rosterQueries)[number]["data"]>>();
    DAY_INDICES.forEach((dayIdx, i) => {
      const data = rosterQueries[i]?.data;
      if (data) map.set(dayIdx, data);
    });
    return map;
  }, [rosterQueries]);

  const daysByIdx = useMemo(() => {
    const map = new Map<number, StaffingDay>();
    DAY_INDICES.forEach((dayIdx, i) => {
      const data = dayQueries[i]?.data;
      if (data) map.set(dayIdx, data);
    });
    return map;
  }, [dayQueries]);

  const isLoading = dayQueries.some((q) => q.isLoading);
  const loadError = dayQueries.find((q) => q.error)?.error;

  const [saveError, setSaveError] = useState<string | null>(null);
  const hourSlotMountsRef = useRef(new Map<string, HourSlotMount>());

  const invalidateWeek = useCallback(() => {
    void queryClient.invalidateQueries({ queryKey: ["staffing", weekId] });
    void queryClient.invalidateQueries({ queryKey: ["staffing", "roster", weekId] });
  }, [queryClient, weekId]);

  const weekEvents = useMemo(
    () => buildWeekStaffingEvents(daysByIdx, weekMeta),
    [daysByIdx, weekMeta],
  );

  const assignMut = useMutation({
    mutationFn: async ({
      day,
      lineId,
      hour,
      employeeId,
    }: {
      day: StaffingDay;
      lineId: string;
      hour: number;
      employeeId: string;
    }) => {
      const { workStart, workEnd } = defaultWorkWindowForLineHour(day, lineId, hour);
      return createStaffingAssignment({
        staffingLineId: lineId,
        employeeId,
        role: "General",
        workStart,
        workEnd,
        isOvertime: false,
      });
    },
    onSuccess: () => invalidateWeek(),
    onError: () => setSaveError(t("staffing.actionFailed")),
  });

  const removeMut = useMutation({
    mutationFn: (assignmentId: string) => deleteStaffingAssignment(assignmentId),
    onSuccess: () => invalidateWeek(),
    onError: () => setSaveError(t("staffing.actionFailed")),
  });

  const isSlotSaving = assignMut.isPending || removeMut.isPending;

  useEffect(() => {
    return () => {
      hourSlotMountsRef.current.forEach(({ root }) => scheduleRootUnmount(root));
      hourSlotMountsRef.current.clear();
    };
  }, []);

  useEffect(() => {
    hourSlotMountsRef.current.forEach(({ root }) => scheduleRootUnmount(root));
    hourSlotMountsRef.current.clear();
  }, [weekId, dept]);

  /** Gỡ root khi event biến mất — không unmount đồng bộ trong eventContent (race với React). */
  useEffect(() => {
    const activeIds = new Set(
      weekEvents
        .filter((e) => (e.extendedProps as { kind?: string } | undefined)?.kind === "hour-slot")
        .map((e) => e.id)
        .filter((id): id is string => typeof id === "string"),
    );
    hourSlotMountsRef.current.forEach((mount, eventId) => {
      if (!activeIds.has(eventId)) {
        scheduleRootUnmount(mount.root);
        hourSlotMountsRef.current.delete(eventId);
      }
    });
  }, [weekEvents]);

  const handleSlotAssign = useCallback(
    (day: StaffingDay, hour: number, lineId: string, employeeId: string) => {
      setSaveError(null);
      assignMut.mutate({ day, lineId, hour, employeeId });
    },
    [assignMut],
  );

  const handleSlotRemove = useCallback(
    (assignmentId: string) => {
      setSaveError(null);
      removeMut.mutate(assignmentId);
    },
    [removeMut],
  );

  const handleToggleFlight = useCallback(
    (day: StaffingDay, hour: number, employeeId: string, flightId: string) => {
      const line = lineForFlight(day, flightId);
      if (!line) return;
      const existing = day.assignments.find(
        (a) => a.employeeId === employeeId && a.staffingLineId === line.id,
      );
      setSaveError(null);
      if (existing) {
        removeMut.mutate(existing.id);
        return;
      }
      assignMut.mutate({ day, lineId: line.id, hour, employeeId });
    },
    [assignMut, removeMut],
  );

  const renderHourSlotCard = useCallback(
    (mount: HourSlotMount) => {
      const day = daysByIdx.get(mount.dayIdx);
      if (!day) return;
      const stats = computeHourSlotStats(day, mount.dayIdx, mount.hour);
      mount.root.render(
        <StaffingHourSlotCard
          day={day}
          hour={mount.hour}
          stats={stats}
          roster={rosterByDayIdx.get(mount.dayIdx)}
          rosterLoading={rosterQueries[mount.dayIdx]?.isLoading}
          canAssign={canAssign}
          isSaving={isSlotSaving}
          onRemove={handleSlotRemove}
          onToggleFlight={(employeeId, flightId) =>
            handleToggleFlight(day, mount.hour, employeeId, flightId)
          }
        />,
      );
    },
    [
      daysByIdx,
      rosterByDayIdx,
      rosterQueries,
      canAssign,
      isSlotSaving,
      handleSlotAssign,
      handleSlotRemove,
      handleToggleFlight,
    ],
  );

  /** FullCalendar không gọi lại eventContent khi refetch — cập nhật mọi card đã mount. */
  useEffect(() => {
    hourSlotMountsRef.current.forEach((mount) => renderHourSlotCard(mount));
  }, [renderHourSlotCard]);

  const onExportWeek = useCallback(async () => {
    const blob = await exportStaffingWeek(weekId, dept);
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `PVHK_Di_${weekId}.xlsx`;
    a.click();
    URL.revokeObjectURL(url);
  }, [weekId, dept]);

  const renderEventContent = useCallback(
    (arg: EventContentArg) => {
      const kind = arg.event.extendedProps.kind as string | undefined;
      if (kind !== "hour-slot") return true;

      const dayIdx = arg.event.extendedProps.dayIdx as number;
      const hour = arg.event.extendedProps.hour as number;
      const stats = arg.event.extendedProps.stats as HourSlotStats | undefined;
      const day = daysByIdx.get(dayIdx);
      if (!day || !stats || !Number.isInteger(hour)) return true;

      const el = document.createElement("div");
      el.className = "staffing-slot-card-mount";

      const eventId = arg.event.id;
      const existing = hourSlotMountsRef.current.get(eventId);
      if (existing) {
        existing.dayIdx = dayIdx;
        existing.hour = hour;
        queueMicrotask(() => renderHourSlotCard(existing));
        return { domNodes: [existing.el] };
      }

      const root = createRoot(el);
      const mount: HourSlotMount = { root, el, dayIdx, hour };
      hourSlotMountsRef.current.set(eventId, mount);
      queueMicrotask(() => renderHourSlotCard(mount));

      return { domNodes: [el] };
    },
    [daysByIdx, renderHourSlotCard],
  );

  const weekStart = calendarDateIso(weekMeta, 0);
  const calendarRef = useRef<FullCalendar>(null);
  const scrollTime = useMemo(() => scrollTimeForShiftGrid(), [weekId]);

  const [nowTick, setNowTick] = useState(() => Date.now());
  useEffect(() => {
    const id = window.setInterval(() => setNowTick(Date.now()), 60_000);
    return () => window.clearInterval(id);
  }, []);

  const slotNowClassNames = useCallback(
    (arg: SlotLaneContentArg) => {
      void nowTick;
      if (!arg.date) return [];
      return isCurrentHourSlot(arg.date) ? ["staffing-slot-now"] : [];
    },
    [nowTick],
  );

  const slotLabelContent = useCallback((arg: SlotLabelContentArg) => {
    const h = arg.date.getHours();
    if (h === 0 || h === 8 || h === 16) {
      return { html: `<span class="staffing-shift-label">${shiftSlotTimeLabel(h)}</span>` };
    }
    return true;
  }, []);

  const scrollToCurrentTime = useCallback(() => {
    calendarRef.current?.getApi()?.scrollToTime(scrollTime);
  }, [scrollTime]);

  useEffect(() => {
    if (isLoading) return;
    const id = requestAnimationFrame(() => scrollToCurrentTime());
    return () => cancelAnimationFrame(id);
  }, [isLoading, weekId, weekEvents, scrollToCurrentTime]);

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        flex: 1,
        minHeight: 0,
        height: "100%",
        gap: 1,
      }}
    >
      <Box sx={{ display: "inline-flex", alignItems: "center", gap: "6px", flexShrink: 0 }}>
        <WeekPicker compact inRow />
        {canExport ? (
          <Tooltip title={t("staffing.exportWeek")} arrow placement="bottom">
            <span>
              <button
                type="button"
                disabled={isLoading}
                onClick={() => void onExportWeek()}
                aria-label={t("staffing.exportWeek")}
                className={weekPickerStyles.navBtn}
              >
                <svg width={18} height={18} viewBox="0 0 24 24" fill="currentColor" aria-hidden>
                  <path d="M19 9h-4V3H9v6H5l7 7 7-7zM5 18v2h14v-2H5z" />
                </svg>
              </button>
            </span>
          </Tooltip>
        ) : null}
      </Box>

      {saveError ? (
        <Alert severity="warning" onClose={() => setSaveError(null)}>
          {saveError}
        </Alert>
      ) : null}

      {isLoading ? <Loading label={t("common.loading")} /> : null}
      {loadError ? (
        <Alert severity="error" onClose={invalidateWeek}>
          {t("common.error")}
        </Alert>
      ) : null}

      <Box
        className={calendarStyles.shell}
        sx={{ flex: 1, minHeight: 0, display: "flex", flexDirection: "column" }}
      >
        <FullCalendar
          ref={calendarRef}
          key={weekId}
          plugins={[timeGridPlugin, interactionPlugin]}
          initialView="timeGridWeek"
          headerToolbar={false}
          initialDate={weekStart}
          height="100%"
          expandRows
          slotMinTime="00:00:00"
          slotMaxTime="24:00:00"
          slotDuration="08:00:00"
          slotLabelInterval="08:00:00"
          slotLabelContent={slotLabelContent}
          snapDuration="08:00:00"
          eventOverlap={false}
          scrollTime={scrollTime}
          scrollTimeReset={false}
          slotLaneClassNames={slotNowClassNames}
          slotLabelClassNames={slotNowClassNames}
          allDaySlot={false}
          firstDay={1}
          events={weekEvents}
          eventContent={renderEventContent}
          locale={i18n.language === "vi" ? "vi" : "en"}
          datesSet={scrollToCurrentTime}
          editable={false}
          selectable={false}
          eventMaxStack={2}
        />
      </Box>

    </Box>
  );
}
