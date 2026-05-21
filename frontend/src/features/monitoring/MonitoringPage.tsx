import { useCallback, useEffect, useMemo, useRef, useState, type CSSProperties } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Loading } from "@/components/ui";
import {
  fetchMonitoringSnapshot,
  type MonitoringSnapshotDto,
} from "@/shared/api/monitoringApi";
import type { MonitoringWorkforceEntryDto } from "@/shared/api/monitoringApi";
import { getAccessToken } from "@/shared/auth/tokenStorage";
import { usePermissions } from "@/shared/auth/usePermissions";
import { Navigate } from "react-router-dom";
import { getRoleHomePath } from "@/shared/auth/roles";
import { MONITORING_REST_POLL_MS } from "@/shared/config/monitoringConfig";
import { DEPT_MARKER_HEX } from "@/shared/geofence/checkinZone";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { connectMonitoringHub, isSignalREnabled } from "@/shared/signalr/monitoringHub";
import { formatLocaleDate } from "@/shared/utils/dateLocale";
import { opsNow } from "@/shared/utils/dayjs";
import { MonitoringMap } from "./MonitoringMap";
import { MonitoringTreeGrid } from "./MonitoringTreeGrid";
import styles from "./monitoringPage.module.css";

type StatKey = "checkedIn" | "notCheckedIn" | "checkedOut" | null;

const DEPT_KEYS = Object.keys(DEPT_MARKER_HEX);

function StatDropdown({
  list,
  onPick,
}: {
  list: MonitoringWorkforceEntryDto[];
  onPick: (id: string) => void;
}) {
  const { t } = useTranslation();
  if (list.length === 0) {
    return (
      <p style={{ margin: 0, padding: 10, fontSize: 12, color: "#64748b" }}>
        {t("monitoring.statEmpty")}
      </p>
    );
  }
  return (
    <>
      {list.map((e) => (
        <button
          key={e.employeeId}
          type="button"
          className={styles.statDropdownItem}
          onClick={() => onPick(e.employeeId)}
        >
          <strong>{e.employeeName}</strong>
          <span style={{ color: "#64748b", marginLeft: 6 }}>{e.employeeCode}</span>
        </button>
      ))}
    </>
  );
}

function formatWorkingDay(lang: string): string {
  return formatLocaleDate(opsNow(), lang);
}

export function MonitoringPage() {
  const { t, i18n } = useTranslation();
  const permissions = usePermissions();
  const queryClient = useQueryClient();
  const [selectedEmployeeId, setSelectedEmployeeId] = useState<string | null>(null);
  const [selectedRowKey, setSelectedRowKey] = useState<string | null>(null);
  const [highlight, setHighlight] = useState<{ lat: number; lng: number } | null>(null);
  const [openStat, setOpenStat] = useState<StatKey>(null);
  const [deptOn, setDeptOn] = useState<Record<string, boolean>>(() =>
    Object.fromEntries(DEPT_KEYS.map((k) => [k, true])),
  );
  const [searchQuery, setSearchQuery] = useState("");
  const [mapResetToken, setMapResetToken] = useState(0);
  const sinceUtcRef = useRef<string | null>(null);

  const loadSnapshot = useCallback(
    async (signal?: AbortSignal): Promise<MonitoringSnapshotDto> => {
      const result = await fetchMonitoringSnapshot(sinceUtcRef.current, signal);
      if (result.unchanged) {
        const cached = queryClient.getQueryData<MonitoringSnapshotDto>([
          "monitoring",
          "snapshot",
        ]);
        if (cached) return cached;
      } else {
        sinceUtcRef.current = result.data.generatedAtUtc;
        return result.data;
      }
      const fresh = await fetchMonitoringSnapshot(null, signal);
      if (fresh.unchanged) {
        throw new Error("monitoring_snapshot_unavailable");
      }
      sinceUtcRef.current = fresh.data.generatedAtUtc;
      return fresh.data;
    },
    [queryClient],
  );

  const snapQuery = useQuery({
    queryKey: ["monitoring", "snapshot"],
    queryFn: ({ signal }) => loadSnapshot(signal),
    refetchInterval: isSignalREnabled() ? false : MONITORING_REST_POLL_MS,
    staleTime: 10_000,
  });

  const refetchSnapshot = useCallback(() => {
    void snapQuery.refetch();
  }, [snapQuery]);

  useEffect(() => {
    if (!isSignalREnabled()) return;
    let dispose: (() => void) | undefined;
    void connectMonitoringHub(refetchSnapshot, getAccessToken() ?? undefined).then((fn) => {
      dispose = fn;
    });
    return () => {
      dispose?.();
    };
  }, [refetchSnapshot]);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setSelectedEmployeeId(null);
        setSelectedRowKey(null);
        setHighlight(null);
        setOpenStat(null);
      }
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  const snap = snapQuery.data;

  const filteredSnap = useMemo(() => {
    if (!snap) return null;
    const q = searchQuery.trim().toLowerCase();
    const markers = snap.markers.filter((m) => {
      if (deptOn[m.departmentCode] === false) return false;
      if (!q) return true;
      return (
        m.employeeName.toLowerCase().includes(q) ||
        m.employeeCode.toLowerCase().includes(q)
      );
    });
    return { ...snap, markers };
  }, [snap, deptOn, searchQuery]);

  const statList = useMemo(() => {
    if (!snap || !openStat) return [];
    if (openStat === "checkedIn") return snap.checkedIn ?? [];
    if (openStat === "notCheckedIn") return snap.notCheckedIn ?? [];
    return snap.checkedOut ?? [];
  }, [snap, openStat]);

  const workingDay = formatWorkingDay(i18n.language);

  if (permissions && !permissions.canAccessMonitoring) {
    return <Navigate to={getRoleHomePath(permissions.role, i18n.language)} replace />;
  }

  return (
    <div className={styles.page}>
      {snapQuery.isLoading ? <Loading label={t("common.loading")} /> : null}
      {snapQuery.isError ? (
        <div className={`${styles.banner} ${styles.bannerWarn}`}>{t("monitoring.loadFailed")}</div>
      ) : null}

      {snap && filteredSnap ? (
        <>
          <div className={styles.deptFilter}>
            <span className={styles.deptFilterLabel}>{t("monitoring.filterDept")}</span>
            {DEPT_KEYS.map((code) => {
              const on = deptOn[code] !== false;
              const color = DEPT_MARKER_HEX[code] ?? "#94a3b8";
              return (
                <button
                  key={code}
                  type="button"
                  className={[styles.deptPill, on ? styles.deptPillOn : ""].filter(Boolean).join(" ")}
                  style={
                    on
                      ? ({ "--pill-color": color } as CSSProperties)
                      : undefined
                  }
                  onClick={() => setDeptOn((prev) => ({ ...prev, [code]: !prev[code] }))}
                >
                  {departmentLabel(code, t)}
                </button>
              );
            })}
            <input
              type="search"
              className={styles.searchInput}
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              placeholder={t("monitoring.searchPh")}
            />
          </div>

          {snap.outsideZoneCount > 0 ? (
            <div className={`${styles.banner} ${styles.bannerWarn}`}>
              {t("monitoring.outsideBanner", { n: snap.outsideZoneCount })}
            </div>
          ) : null}

          <div className={styles.layout}>
            <div className={styles.mapPanel}>
              <MonitoringMap
                snapshot={filteredSnap}
                selectedEmployeeId={selectedEmployeeId}
                onSelectEmployee={(id) => {
                  setSelectedEmployeeId(id);
                  setSelectedRowKey(null);
                  setHighlight(null);
                }}
                highlight={highlight}
                mapResetToken={mapResetToken}
                onMapReset={() => setMapResetToken((n) => n + 1)}
                resetTitle={t("monitoring.mapReset")}
              />
            </div>

            <div className={styles.rightCol}>
              <div className={styles.statsBox}>
                <div className={styles.statsGrid}>
                  <div className={styles.statCell}>
                    <span className={styles.statLabel}>{t("monitoring.workingDayLabel")}</span>
                    <span className={styles.statValue} style={{ color: "#166534", fontSize: 16 }}>
                      {workingDay}
                    </span>
                  </div>
                  {(
                    [
                      ["checkedIn", t("monitoring.inShift"), snap.inShiftCount, "#15803d"],
                      ["notCheckedIn", t("monitoring.notIn"), snap.notCheckedInCount, "#64748b"],
                      ["checkedOut", t("monitoring.checkedOut"), snap.checkedOutCount, "#0369a1"],
                    ] as const
                  ).map(([key, label, value, color]) => (
                    <div key={key} className={styles.statCell} style={{ position: "relative" }}>
                      <span className={styles.statLabel}>{label}</span>
                      <button
                        type="button"
                        className={`${styles.statValue} ${styles.statValueBtn}`}
                        style={{ color }}
                        onClick={() => setOpenStat((cur) => (cur === key ? null : key))}
                      >
                        {value}
                      </button>
                      {openStat === key ? (
                        <div className={styles.statDropdown}>
                          <StatDropdown
                            list={statList}
                            onPick={(id) => {
                              setSelectedEmployeeId(id);
                              setOpenStat(null);
                            }}
                          />
                        </div>
                      ) : null}
                    </div>
                  ))}
                </div>
                {snap.outsideZoneCount > 0 ? (
                  <p className={styles.outsideHint}>
                    {t("monitoring.outsidePolygonHint", { n: snap.outsideZoneCount })}
                  </p>
                ) : null}
              </div>

              <div className={styles.listPanel}>
                <div className={styles.listHeader}>
                  {t("monitoring.tableTitle")}
                  {filteredSnap.markers.length > 0 ? ` · ${filteredSnap.markers.length}` : ""}
                </div>
                <div className={styles.gridWrap}>
                  <MonitoringTreeGrid
                    snapshot={filteredSnap}
                    searchQuery={searchQuery}
                    selectedEmployeeId={selectedEmployeeId}
                    selectedRowKey={selectedRowKey}
                    onSelectEmployee={setSelectedEmployeeId}
                    onSelectPoint={(p) => {
                      if (!p) {
                        setSelectedRowKey(null);
                        setHighlight(null);
                        return;
                      }
                      setSelectedRowKey(p.rowKey);
                      setHighlight({ lat: p.lat, lng: p.lng });
                    }}
                  />
                </div>
              </div>
            </div>
          </div>
        </>
      ) : null}
    </div>
  );
}
