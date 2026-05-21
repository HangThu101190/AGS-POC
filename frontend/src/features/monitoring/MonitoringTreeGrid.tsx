import { useCallback, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { AgGridReact } from "ag-grid-react";
import type { ColDef, ICellRendererParams } from "ag-grid-community";
import type { MonitoringSnapshotDto } from "@/shared/api/monitoringApi";
import { agsGridTheme } from "@/lib/agGrid/agsTheme";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { formatLocaleDateTime } from "@/shared/utils/dateLocale";
import { dayjs } from "@/shared/utils/dayjs";
import styles from "./monitoringPage.module.css";

type GroupRow = {
  rowType: "group";
  rowKey: string;
  employeeId: string;
  empName: string;
  empCode: string;
  departmentCode: string;
  lat: number;
  lng: number;
  inZone: boolean;
  checkedOut: boolean;
  checkInUtc: string;
  childCount: number;
};

type LeafRow = {
  rowType: "leaf";
  rowKey: string;
  employeeId: string;
  empName: string;
  empCode: string;
  timeLabel: string;
  lat: number;
  lng: number;
  isCurrent: boolean;
};

type GridRow = GroupRow | LeafRow;

function EmpCell(props: ICellRendererParams<GridRow>) {
  const ctx = props.context as {
    expandedIds: Set<string>;
    onToggleExpand: (id: string) => void;
    tt: (k: string) => string;
  };
  const data = props.data;
  if (!data) return null;

  if (data.rowType === "group") {
    const expanded = ctx.expandedIds.has(data.employeeId);
    return (
      <div style={{ padding: "4px 0", lineHeight: 1.35 }}>
        <button
          type="button"
          onClick={(e) => {
            e.stopPropagation();
            ctx.onToggleExpand(data.employeeId);
          }}
          style={{
            border: "none",
            background: "transparent",
            cursor: "pointer",
            marginRight: 6,
            fontFamily: "inherit",
          }}
          aria-expanded={expanded}
        >
          {expanded ? "▼" : "▶"}
        </button>
        <strong>{data.empName}</strong>
        <div style={{ fontSize: 11, color: "#64748b" }}>
          {data.empCode} · {departmentLabel(data.departmentCode, ctx.tt)}
          {data.checkedOut ? (
            <span style={{ color: "#64748b", marginLeft: 6 }}>Đã ra ca</span>
          ) : !data.inZone ? (
            <span style={{ color: "#b91c1c", fontWeight: 600, marginLeft: 6 }}>Ngoài vùng</span>
          ) : null}
        </div>
      </div>
    );
  }

  return (
    <div style={{ paddingLeft: 22, fontSize: 12, color: data.isCurrent ? "#033b7d" : "#64748b" }}>
      {ctx.tt("monitoring.timePoint")}: {data.timeLabel}
    </div>
  );
}

type MonitoringTreeGridProps = {
  snapshot: MonitoringSnapshotDto;
  searchQuery?: string;
  selectedEmployeeId: string | null;
  selectedRowKey: string | null;
  onSelectEmployee: (employeeId: string | null) => void;
  onSelectPoint: (
    point: null | { employeeId: string; lat: number; lng: number; rowKey: string },
  ) => void;
};

function matchesSearch(m: MonitoringSnapshotDto["markers"][0], q: string) {
  const needle = q.trim().toLowerCase();
  if (!needle) return true;
  return (
    m.employeeName.toLowerCase().includes(needle) ||
    m.employeeCode.toLowerCase().includes(needle)
  );
}

export function MonitoringTreeGrid({
  snapshot,
  searchQuery = "",
  selectedEmployeeId,
  selectedRowKey,
  onSelectEmployee,
  onSelectPoint,
}: MonitoringTreeGridProps) {
  const { t, i18n } = useTranslation();
  const [expandedIds, setExpandedIds] = useState<Set<string>>(() => new Set());

  const gridRows = useMemo(() => {
    const lang = i18n.language;
    const byEmp = new Map<string, Omit<LeafRow, "rowType">[]>();

    const markers = snapshot.markers.filter((m) => matchesSearch(m, searchQuery));

    for (const m of markers) {
      const hist = [...(m.history ?? [])].sort(
        (a, b) => dayjs(b.atUtc).valueOf() - dayjs(a.atUtc).valueOf(),
      );
      const leaves: Omit<LeafRow, "rowType">[] = hist.map((h, idx) => ({
        rowKey: `${m.employeeId}#${h.atUtc}`,
        employeeId: m.employeeId,
        empName: m.employeeName,
        empCode: m.employeeCode,
        timeLabel: formatLocaleDateTime(h.atUtc, lang),
        lat: h.lat,
        lng: h.lng,
        isCurrent: idx === 0,
      }));
      if (leaves.length === 0) {
        leaves.push({
          rowKey: `${m.employeeId}#live`,
          employeeId: m.employeeId,
          empName: m.employeeName,
          empCode: m.employeeCode,
          timeLabel: formatLocaleDateTime(m.checkInUtc, lang),
          lat: m.lat,
          lng: m.lng,
          isCurrent: true,
        });
      }
      byEmp.set(m.employeeId, leaves);
    }

    const visible: GridRow[] = [];
    for (const m of markers) {
      const childLeaves = byEmp.get(m.employeeId) ?? [];
      visible.push({
        rowType: "group",
        rowKey: `g-${m.employeeId}`,
        employeeId: m.employeeId,
        empName: m.employeeName,
        empCode: m.employeeCode,
        departmentCode: m.departmentCode,
        lat: m.lat,
        lng: m.lng,
        inZone: m.inZone,
        checkedOut: m.checkedOut,
        checkInUtc: m.checkInUtc,
        childCount: childLeaves.length,
      });
      if (expandedIds.has(m.employeeId)) {
        for (const leaf of childLeaves) {
          visible.push({ rowType: "leaf", ...leaf });
        }
      }
    }
    return visible;
  }, [snapshot.markers, searchQuery, expandedIds, i18n.language]);

  const onToggleExpand = useCallback((employeeId: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(employeeId)) next.delete(employeeId);
      else next.add(employeeId);
      return next;
    });
  }, []);

  const columnDefs = useMemo<ColDef<GridRow>[]>(
    () => [
      {
        headerName: t("monitoring.colEmp"),
        flex: 1,
        minWidth: 160,
        cellRenderer: EmpCell,
      },
      {
        headerName: t("monitoring.colLat"),
        field: "lat",
        width: 72,
        valueFormatter: (p) => (typeof p.value === "number" ? p.value.toFixed(5) : ""),
      },
      {
        headerName: t("monitoring.colLng"),
        field: "lng",
        width: 76,
        valueFormatter: (p) => (typeof p.value === "number" ? p.value.toFixed(5) : ""),
      },
    ],
    [t],
  );

  const context = useMemo(
    () => ({
      expandedIds,
      onToggleExpand,
      tt: t,
    }),
    [expandedIds, onToggleExpand, t],
  );

  if (snapshot.markers.length === 0) {
    return (
      <p style={{ padding: 14, margin: 0, fontSize: 12, color: "#64748b" }}>
        {t("monitoring.empty")}
      </p>
    );
  }

  return (
    <div className={`${styles.gridWrap} ag-theme-quartz`}>
      <AgGridReact<GridRow>
        theme={agsGridTheme}
        rowData={gridRows}
        columnDefs={columnDefs}
        context={context}
        getRowId={(p) => p.data?.rowKey ?? ""}
        headerHeight={32}
        rowHeight={48}
        domLayout="normal"
        suppressCellFocus
        onRowClicked={(e) => {
          const data = e.data;
          if (!data) return;
          if (data.rowType === "group") {
            onSelectEmployee(
              selectedEmployeeId === data.employeeId && !selectedRowKey ? null : data.employeeId,
            );
            onSelectPoint(null);
            return;
          }
          if (selectedRowKey === data.rowKey) {
            onSelectPoint(null);
            onSelectEmployee(null);
            return;
          }
          onSelectEmployee(data.employeeId);
          onSelectPoint({
            employeeId: data.employeeId,
            lat: data.lat,
            lng: data.lng,
            rowKey: data.rowKey,
          });
        }}
        getRowClass={(p) => {
          const d = p.data;
          if (!d) return "";
          if (d.rowType === "group" && selectedEmployeeId === d.employeeId && !selectedRowKey) {
            return !d.inZone ? "ags-mon-row-risk" : "ags-mon-row-emp-sel";
          }
          if (d.rowType === "leaf" && selectedRowKey === d.rowKey) return "ags-mon-row-hist-sel";
          if (d.rowType === "leaf" && d.isCurrent) return "ags-mon-row-hist-current";
          return "";
        }}
        getContextMenuItems={(params) => {
          const d = params.node?.data;
          if (!d || d.rowType !== "leaf") return [];
          const lat = d.lat;
          const lng = d.lng;
          return [
            {
              name: t("monitoring.ctxShowOnMap"),
              action: () =>
                onSelectPoint({
                  employeeId: d.employeeId,
                  lat,
                  lng,
                  rowKey: d.rowKey,
                }),
            },
            {
              name: t("monitoring.ctxCopyCoords"),
              action: () => {
                void navigator.clipboard?.writeText(`${lat.toFixed(5)}, ${lng.toFixed(5)}`);
              },
            },
          ];
        }}
      />
      <style>{`
        .ags-mon-row-emp-sel { background: #eff6ff !important; }
        .ags-mon-row-risk { background: #fef2f2 !important; border-left: 3px solid #dc2626; }
        .ags-mon-row-hist-sel { background: #dbeafe !important; }
        .ags-mon-row-hist-current { font-weight: 600; }
      `}</style>
    </div>
  );
}
