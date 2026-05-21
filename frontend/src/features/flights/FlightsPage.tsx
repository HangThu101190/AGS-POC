import type {
  ColDef,
  GridApi,
  GridReadyEvent,
  ICellRendererParams,
  ValueFormatterParams,
} from "ag-grid-community";
import LinearProgress from "@mui/material/LinearProgress";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { DataTable } from "@/components/ui";
import { isEmptyCellValue } from "@/lib/agGrid/formatCell";
import { gridCol } from "@/shared/ui/gridColumn";
import { WeekDayChips } from "@/features/planning/WeekDayChips";
import { WeekPicker } from "@/features/planning/WeekPicker";
import styles from "./flightsPage.module.css";

const TIME_COL_CLASS = "flight-grid-time";
import { FlightEtaEtdCell, FlightRemarkCell } from "@/features/flights/FlightGridCells";
import { useAutoDismiss } from "@/shared/hooks/useAutoDismiss";
import { useAuth } from "@/shared/auth/AuthContext";
import { canManageFlights } from "@/shared/auth/roles";
import { FlightFormDialog } from "@/features/flights/FlightFormDialog";
import {
  createFlight,
  deleteFlight,
  fetchFlightsPage,
  fetchFlightImportJob,
  importFlightsExcel,
  setFlightDelay,
  updateFlight,
  type FlightDto,
  type FlightUpsertBody,
} from "@/shared/api/flightsApi";
import {
  fetchFlightSchedule,
  publishFlightSchedule,
  type FlightScheduleDto,
} from "@/shared/api/flightSchedulesApi";
import { formatFlightTimeDisplay } from "@/shared/planning/flightClock";
import { isPastDay } from "@/shared/planning/weekCalendar";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { isAllWeekDay } from "@/shared/planning/weekDayFilter";
import { usePlanningDaySync } from "@/shared/signalr/usePlanningDaySync";
import type { PlanningHubEvent } from "@/shared/signalr/planningHub";
import { StatusBanner, ActionButton, WeekPageShell } from "@/shared/webChrome";

export function FlightsPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const canManage = user ? canManageFlights(user.role) : false;
  const { weekId, weekMeta, planError } = useWeekScope();
  const fileRef = useRef<HTMLInputElement>(null);
  const [dayIdx, setDayIdx] = useState<number | null>(null);
  const [weekFlightTotal, setWeekFlightTotal] = useState(0);
  const [message, setMessage] = useState<string | null>(null);
  const [warnings, setWarnings] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [importProgress, setImportProgress] = useState<number | null>(null);
  const gridApiRef = useRef<GridApi<FlightDto> | null>(null);

  const refreshFlightsGrid = useCallback(() => {
    gridApiRef.current?.refreshInfiniteCache();
  }, []);

  const patchFlightInGrid = useCallback((flight: FlightDto) => {
    const api = gridApiRef.current;
    if (!api) return;
    api.forEachNode((node) => {
      if (node.data?.id === flight.id) {
        node.setData({ ...node.data, ...flight });
      }
    });
    api.refreshCells({ columns: ["eta", "etd"], force: true });
  }, []);
  const [formOpen, setFormOpen] = useState(false);
  const [editFlight, setEditFlight] = useState<FlightDto | null>(null);
  const [schedule, setSchedule] = useState<FlightScheduleDto | null>(null);
  useAutoDismiss(message, () => setMessage(null));
  useAutoDismiss(error, () => setError(null));

  const chipWeekDates = weekMeta.weekDates;
  const chipTodayIdx = weekMeta.todayIdx;

  useEffect(() => {
    const raw = weekMeta.todayIdx;
    const idx = raw >= 0 && raw <= 6 ? raw : 0;
    setDayIdx(idx);
  }, [weekId, weekMeta.todayIdx]);

  useEffect(() => {
    let cancelled = false;
    const reqWeek = weekId;
    setLoadError(planError ? t("flights.loadFailed") : null);
    void fetchFlightSchedule(reqWeek)
      .then((s) => {
        if (!cancelled) setSchedule(s);
      })
      .catch((err) => {
        console.warn("[FlightsPage] flight schedule load failed", err);
        if (!cancelled) setSchedule(null);
      });
    void fetchFlightsPage({ page: 0, pageSize: 1, weekId: reqWeek })
      .then((r) => {
        if (!cancelled) setWeekFlightTotal(r.totalCount);
      })
      .catch((err) => {
        console.warn("[FlightsPage] flight count load failed", err);
      });
    return () => {
      cancelled = true;
    };
  }, [weekId, planError, t]);

  const todayIdx = chipTodayIdx >= 0 && chipTodayIdx <= 6 ? chipTodayIdx : 0;
  const selectedDay = dayIdx ?? todayIdx;
  const allWeek = isAllWeekDay(selectedDay);
  const isPastFilter = !allWeek && isPastDay(selectedDay, todayIdx);
  const scheduleLockBlocks =
    Boolean(schedule?.isLocked) && weekMeta.kind === "past";
  const canMutateFlights = canManage && !isPastFilter && !scheduleLockBlocks;
  const showPublishSchedule = canManage && !isPastFilter && weekMeta.kind !== "past";
  const syncDayIdx = allWeek ? todayIdx : selectedDay;

  const finishImportUi = useCallback(
    (result?: { importedCount: number; dayLabel?: string | null; warnings?: string[] }, dayIdxHint?: number) => {
      setImportProgress(null);
      if (result) {
        const idx = dayIdxHint ?? selectedDay;
        const day = result.dayLabel ?? chipWeekDates[idx] ?? "";
        setMessage(t("flights.imported", { count: result.importedCount, day }));
        setWarnings(result.warnings ?? []);
      }
      refreshFlightsGrid();
      void fetchFlightsPage({ page: 0, pageSize: 1, weekId }).then((week) => {
        setWeekFlightTotal(week.totalCount);
      });
    },
    [chipWeekDates, refreshFlightsGrid, selectedDay, t, weekId],
  );

  const handleHubEvent = useCallback(
    (event: PlanningHubEvent) => {
      if (event.type !== "importProgress") return;
      setImportProgress(event.progressPercent);
      if (event.status === "completed" && event.weekId === weekId) {
        finishImportUi();
      }
      if (event.status === "failed") {
        setImportProgress(null);
        setError(t("flights.actionFailed"));
      }
    },
    [finishImportUi, t, weekId],
  );

  usePlanningDaySync(weekId, syncDayIdx, { onEvent: handleHubEvent });

  const fetchFlights = useCallback(
    async (params: Parameters<typeof fetchFlightsPage>[0], signal?: AbortSignal) => {
      return fetchFlightsPage(
        {
          ...params,
          weekId,
          ...(allWeek ? {} : { dayIdx: selectedDay }),
        },
        signal,
      );
    },
    [allWeek, selectedDay, weekId],
  );

  const canEditDelay = canManage && !isPastFilter;

  const handleDelaySave = useCallback(
    async (
      flightId: string,
      delays: { etaDelayMinutes: number; etdDelayMinutes: number },
    ): Promise<FlightDto> => {
      setError(null);
      try {
        const updated = await setFlightDelay(flightId, delays);
        patchFlightInGrid(updated);
        const { etaDelayMinutes: eta, etdDelayMinutes: etd } = delays;
        setMessage(
          eta > 0 || etd > 0
            ? t("flights.delaySetBoth", { eta, etd })
            : t("flights.delayCleared"),
        );
        refreshFlightsGrid();
        return updated;
      } catch {
        setError(t("flights.actionFailed"));
        throw new Error("flight_delay_failed");
      }
    },
    [patchFlightInGrid, refreshFlightsGrid, t],
  );

  const etaRenderer = useCallback(
    (props: ICellRendererParams<FlightDto>) => (
      <FlightEtaEtdCell
        {...props}
        kind="eta"
        todayIdx={todayIdx}
        canEdit={canEditDelay}
        onSave={handleDelaySave}
      />
    ),
    [canEditDelay, handleDelaySave, todayIdx],
  );

  const etdRenderer = useCallback(
    (props: ICellRendererParams<FlightDto>) => (
      <FlightEtaEtdCell
        {...props}
        kind="etd"
        todayIdx={todayIdx}
        canEdit={canEditDelay}
        onSave={handleDelaySave}
      />
    ),
    [canEditDelay, handleDelaySave, todayIdx],
  );

  const weekDates = weekMeta.weekDates;

  const columns = useMemo<ColDef<FlightDto>[]>(() => {
    const textFmt = (p: ValueFormatterParams<FlightDto>) => {
      if (isEmptyCellValue(p.value)) return "";
      return String(p.value).trim();
    };

    const timeFmt = (p: ValueFormatterParams<FlightDto>) =>
      formatFlightTimeDisplay(
        isEmptyCellValue(p.value) ? "" : String(p.value),
      );

    const fixedCol = (
      headerKey: string,
      tipKey: string | undefined,
      def: ColDef<FlightDto>,
    ): ColDef<FlightDto> =>
      gridCol(t, headerKey, tipKey, {
        flex: 0,
        suppressSizeToFit: true,
        wrapHeaderText: false,
        ...def,
      });

    const dateCol: ColDef<FlightDto> | undefined = allWeek
      ? fixedCol("flights.colDate", undefined, {
          colId: "dayIdx",
          field: "dayIdx",
          width: 108,
          minWidth: 96,
          valueFormatter: (p) => weekDates[p.value as number] ?? "",
        })
      : undefined;

    const rowNoCol: ColDef<FlightDto> = fixedCol("flights.colNo", "flights.colNoTip", {
      colId: "rowNo",
      width: 56,
      minWidth: 52,
      valueGetter: (p) => {
        const idx = p.node?.rowIndex;
        return idx != null && idx >= 0 ? idx + 1 : "";
      },
      sortable: false,
      filter: false,
    });

    const excelCols: ColDef<FlightDto>[] = [
      rowNoCol,
      fixedCol("flights.colArr", "flights.colArrTip", {
        field: "flightNo",
        width: 118,
        minWidth: 110,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colDep", "flights.colDepTip", {
        field: "departureFlightNo",
        width: 118,
        minWidth: 110,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colRegs", "flights.colRegsTip", {
        field: "registration",
        width: 112,
        minWidth: 100,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colRoute", "flights.colRouteTip", {
        field: "route",
        width: 172,
        minWidth: 148,
        valueFormatter: textFmt,
        tooltipValueGetter: (p) => {
          const v = p.value;
          return isEmptyCellValue(v) ? "" : String(v).trim();
        },
      }),
      fixedCol("flights.colSta", "flights.colStaTip", {
        field: "sta",
        width: 84,
        minWidth: 80,
        cellClass: TIME_COL_CLASS,
        valueFormatter: timeFmt,
      }),
      fixedCol("flights.colStd", "flights.colStdTip", {
        field: "std",
        width: 84,
        minWidth: 80,
        cellClass: TIME_COL_CLASS,
        valueFormatter: timeFmt,
      }),
      fixedCol("flights.colEta", "flights.colEtaTip", {
        colId: "eta",
        field: "eta",
        width: 108,
        minWidth: 100,
        cellClass: TIME_COL_CLASS,
        cellRenderer: etaRenderer,
        sortable: false,
        filter: false,
      }),
      fixedCol("flights.colEtd", "flights.colEtdTip", {
        colId: "etd",
        field: "etd",
        width: 108,
        minWidth: 100,
        cellClass: TIME_COL_CLASS,
        cellRenderer: etdRenderer,
        sortable: false,
        filter: false,
      }),
      fixedCol("flights.colBelt", "flights.colBeltTip", {
        field: "belt",
        width: 100,
        minWidth: 92,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colPrk", "flights.colPrkTip", {
        field: "parking",
        width: 72,
        minWidth: 64,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colGate", "flights.colGateTip", {
        field: "gate",
        width: 84,
        minWidth: 72,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colRemark", "flights.colRemarkTip", {
        field: "remark",
        minWidth: 260,
        width: 400,
        cellRenderer: FlightRemarkCell,
        sortable: false,
        filter: false,
      }),
      fixedCol("flights.colAc", "flights.colAcTip", {
        field: "aircraft",
        width: 92,
        minWidth: 84,
        valueFormatter: textFmt,
      }),
      fixedCol("flights.colCarry", "flights.colCarryTip", {
        field: "carry",
        width: 100,
        minWidth: 88,
        valueFormatter: textFmt,
      }),
    ];

    return [...(dateCol ? [dateCol] : []), ...excelCols];
  }, [allWeek, etaRenderer, etdRenderer, t, weekDates]);

  const onImport = async (file: File) => {
    setError(null);
    setWarnings([]);
    const importDayIdx = allWeek ? todayIdx : selectedDay;
    try {
      const outcome = await importFlightsExcel(file, importDayIdx, weekId, { preferAsync: true });
      if (outcome.mode === "sync") {
        finishImportUi(outcome.result, importDayIdx);
        return;
      }
      setImportProgress(outcome.job.progressPercent);
      const poll = async () => {
        const job = await fetchFlightImportJob(outcome.job.id);
        setImportProgress(job.progressPercent);
        const status = String(job.status).toLowerCase();
        if (status === "completed" && job.result) {
          finishImportUi(job.result, importDayIdx);
          return;
        }
        if (status === "failed") {
          setImportProgress(null);
          setError(job.errorMessage ?? t("flights.actionFailed"));
          return;
        }
        window.setTimeout(() => void poll(), 800);
      };
      void poll();
    } catch {
      setImportProgress(null);
      setError(t("flights.actionFailed"));
    }
  };

  const formDayIdx = allWeek ? todayIdx : selectedDay;
  const hasBanners = Boolean(message || warnings.length > 0 || loadError || error);

  return (
    <WeekPageShell weekInHeader={false} fill>
      <div className={styles.page}>
        <div className={styles.toolbarOneRow}>
          <div className={styles.toolbarWeek}>
            <WeekPicker inline inRow />
          </div>
          <div className={styles.chipsWrap}>
            <WeekDayChips
              embedded
              scrollable
              compact
              weekDates={chipWeekDates}
              todayIdx={chipTodayIdx}
              value={selectedDay}
              onChange={setDayIdx}
              allWeekCount={weekFlightTotal}
            />
          </div>
          {canMutateFlights || showPublishSchedule ? (
            <div className={styles.toolbarActions}>
              {showPublishSchedule ? (
                <ActionButton
                  className={`${styles.actionBtn} ${styles.actionBtnPrimary}`}
                  variant="primary"
                  onClick={async () => {
                    setError(null);
                    try {
                      const next = await publishFlightSchedule(weekId);
                      setSchedule(next);
                      setMessage(
                        `${t("flights.schedulePublished")} ${t("flights.staffingRefreshHint")}`,
                      );
                    } catch {
                      setError(t("flights.actionFailed"));
                    }
                  }}
                >
                  {t("flights.publishSchedule")}
                </ActionButton>
              ) : null}
              {canMutateFlights ? (
                <>
                  <ActionButton
                    className={`${styles.actionBtn} ${styles.actionBtnPrimary}`}
                    variant="primary"
                    onClick={() => {
                      setEditFlight(null);
                      setFormOpen(true);
                    }}
                  >
                    {t("flights.addFlight")}
                  </ActionButton>
                  <ActionButton
                    className={styles.actionBtn}
                    onClick={() => fileRef.current?.click()}
                  >
                    {t("flights.importExcel")}
                  </ActionButton>
                  <input
                    ref={fileRef}
                    type="file"
                    accept=".xlsx,.xls"
                    hidden
                    onChange={(e) => {
                      const file = e.target.files?.[0];
                      if (file) void onImport(file);
                      e.target.value = "";
                    }}
                  />
                </>
              ) : null}
            </div>
          ) : null}
        </div>

        {importProgress !== null ? (
          <LinearProgress variant="determinate" value={importProgress} sx={{ mb: 1 }} />
        ) : null}
        {hasBanners ? (
          <div className={styles.banners}>
            {message ? <StatusBanner variant="success">{message}</StatusBanner> : null}
            {warnings.length > 0 ? (
              <StatusBanner variant="warn">
                <div>
                  <strong>{t("flights.importWarnings", { count: warnings.length })}</strong>
                  <ul style={{ margin: "6px 0 0", paddingLeft: 18 }}>
                    {warnings.map((w) => (
                      <li key={w}>{w}</li>
                    ))}
                  </ul>
                </div>
              </StatusBanner>
            ) : null}
            {loadError ? <StatusBanner variant="warn">{loadError}</StatusBanner> : null}
            {error ? <StatusBanner variant="warn">{error}</StatusBanner> : null}
          </div>
        ) : null}

        <div className={styles.tableSection}>
          <DataTable<FlightDto>
            columnDefs={columns}
            fetchRows={fetchFlights}
            fillHeight
            headerHeight={44}
            defaultColDef={{
              flex: 0,
              minWidth: 48,
              suppressSizeToFit: true,
            }}
            rowHeight={54}
            onGridReady={(e: GridReadyEvent<FlightDto>) => {
              gridApiRef.current = e.api;
            }}
            gridOptions={{
              getRowId: (p) => p.data?.id ?? "",
              suppressHorizontalScroll: false,
              rowClassRules: {
                "ag-row-vip": (p) => p.data?.isVip === true,
              },
              getRowStyle: (p) =>
                p.data?.isDelayed && !p.data?.isVip ? { background: "#fef2f2" } : undefined,
              onRowDoubleClicked: canMutateFlights
                  ? (e) => {
                      if (e.data) {
                        setEditFlight(e.data);
                        setFormOpen(true);
                      }
                    }
                  : undefined,
            }}
          />
        </div>
      </div>

      <FlightFormDialog
        open={formOpen}
        dayIdx={formDayIdx}
        flight={editFlight}
        onClose={() => setFormOpen(false)}
        onSave={async (body: FlightUpsertBody) => {
          if (editFlight) {
            await updateFlight(editFlight.id, body);
            setMessage(t("flights.updated"));
          } else {
            await createFlight({ ...body, weekId });
            setMessage(t("flights.created"));
          }
          refreshFlightsGrid();
        }}
        onDelete={
          editFlight
            ? async () => {
                if (!window.confirm(t("flights.deleteConfirm"))) return;
                await deleteFlight(editFlight.id);
                setMessage(t("flights.deleted"));
                setFormOpen(false);
                setEditFlight(null);
                refreshFlightsGrid();
              }
            : undefined
        }
      />
    </WeekPageShell>
  );
}
