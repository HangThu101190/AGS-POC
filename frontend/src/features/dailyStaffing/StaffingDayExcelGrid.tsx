import { useCallback, useMemo, useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { AgGridReact } from "ag-grid-react";
import type {
  CellValueChangedEvent,
  ColDef,
  ColGroupDef,
  ICellEditorParams,
  ValueFormatterParams,
} from "ag-grid-community";
import { useTranslation } from "react-i18next";
import { agsGridTheme } from "@/lib/agGrid/agsTheme";
import { staffingApiErrorMessage } from "@/features/dailyStaffing/staffingApiError";
import {
  buildCombinedExcelRows,
  type StaffingExcelCombinedRow,
} from "@/features/dailyStaffing/staffingRosterRows";
import { rosterOptionsForLine } from "@/features/dailyStaffing/staffingRosterOptions";
import styles from "@/features/dailyStaffing/staffingDayExcelGrid.module.css";
import {
  createStaffingAssignment,
  deleteStaffingAssignment,
  fetchStaffingRoster,
  type StaffingDay,
} from "@/shared/api/staffingApi";

const EMPTY_EMPLOYEE = "";

type Props = {
  weekId: string;
  dayIdx: number;
  departmentCode: string;
  day: StaffingDay;
  locked: boolean;
  canAssign: boolean;
  onChanged: () => void;
};

function shortDisplayName(fullName: string): string {
  const parts = fullName.split(/\s+/).filter(Boolean);
  return parts.length === 0 ? fullName : parts[parts.length - 1]!.toUpperCase();
}

function employeeEditorValues(
  roster: ReturnType<typeof rosterOptionsForLine>,
): string[] {
  const ids = roster.filter((o) => !o.disabled).map((o) => o.employeeId);
  return [EMPTY_EMPLOYEE, ...ids];
}

function formatEmployeeCell(
  employeeId: string | null | undefined,
  roster: ReturnType<typeof rosterOptionsForLine>,
  day: StaffingDay,
  lineId: string | null,
  role: "Counter" | "Gate" | "Sup",
): string {
  if (!employeeId) return "";
  const fromRoster = roster.find((o) => o.employeeId === employeeId);
  if (fromRoster) {
    const name = fromRoster.label.split("—").pop()?.trim() ?? fromRoster.label;
    return shortDisplayName(name);
  }
  if (lineId) {
    const a = day.assignments.find(
      (x) => x.staffingLineId === lineId && x.role === role && x.employeeId === employeeId,
    );
    if (a) return shortDisplayName(a.employeeName);
  }
  return "";
}

export function StaffingDayExcelGrid({
  weekId,
  dayIdx,
  departmentCode,
  day,
  locked,
  canAssign,
  onChanged,
}: Props) {
  const { t } = useTranslation();
  const [gridError, setGridError] = useState<string | null>(null);
  const savingRef = useRef(false);

  const rowData = useMemo(() => buildCombinedExcelRows(day), [day]);

  const rosterQ = useQuery({
    queryKey: ["staffing", "roster", weekId, dayIdx, departmentCode],
    queryFn: () => fetchStaffingRoster(weekId, dayIdx, departmentCode),
    enabled: canAssign && !locked,
  });

  const gioRows = useMemo(() => {
    const map = new Map<string, { segment: string; names: string[] }>();
    for (const a of day.assignments) {
      const line = day.lines.find((l) => l.id === a.staffingLineId);
      const segment =
        line?.segment === "Qt" || line?.segment === "qt" ? "QT" : "QN";
      const key = `${a.workStart}-${a.workEnd}|${segment}`;
      const entry = map.get(key) ?? { segment, names: [] };
      entry.names.push(shortDisplayName(a.employeeName));
      map.set(key, entry);
    }
    return [...map.entries()].map(([window, v]) => ({
      window: window.split("|")[0]!,
      segment: v.segment,
      names: [...new Set(v.names)].join(", "),
    }));
  }, [day]);

  const workWindowForLine = useCallback(
    (lineId: string) => {
      const line = day.lines.find((l) => l.id === lineId);
      const flight = line ? day.flights.find((f) => f.id === line.flightId) : undefined;
      return {
        workStart: flight?.sta ?? "06:00",
        workEnd: flight?.std ?? "14:00",
      };
    },
    [day],
  );

  const onCellValueChanged = useCallback(
    async (e: CellValueChangedEvent<StaffingExcelCombinedRow>) => {
      if (!canAssign || locked || savingRef.current) return;
      const field = e.colDef.field;
      const row = e.data;
      if (!field || !row) return;

      const applyRole = async (
        lineId: string | null,
        role: "Counter" | "Gate" | "Sup",
        employeeId: string,
        assignmentId: string | null,
        employeeField: keyof StaffingExcelCombinedRow,
      ) => {
        if (!lineId) return;
        const prevId = (row[employeeField] as string | null) ?? null;
        const nextId = employeeId.trim();
        if (prevId === nextId || (prevId === null && nextId === "")) return;
        savingRef.current = true;
        try {
          const { workStart, workEnd } = workWindowForLine(lineId);
          if (assignmentId && (!nextId || nextId !== prevId)) {
            await deleteStaffingAssignment(assignmentId);
          }
          if (nextId) {
            await createStaffingAssignment({
              staffingLineId: lineId,
              employeeId: nextId,
              role,
              workStart,
              workEnd,
            });
          }
          setGridError(null);
          onChanged();
        } catch (err) {
          setGridError(
            staffingApiErrorMessage(err, t("staffing.actionFailed")),
          );
          e.api.refreshCells({ rowNodes: [e.node], force: true });
        } finally {
          savingRef.current = false;
        }
      };

      if (field === "qnCounterEmployeeId" && row.qnLineId) {
        await applyRole(
          row.qnLineId,
          "Counter",
          String(e.newValue ?? ""),
          row.qnCounterAssignmentId,
          "qnCounterEmployeeId",
        );
      } else if (field === "qnGateEmployeeId" && row.qnLineId) {
        await applyRole(
          row.qnLineId,
          "Gate",
          String(e.newValue ?? ""),
          row.qnGateAssignmentId,
          "qnGateEmployeeId",
        );
      } else if (field === "qtSupEmployeeId" && row.qtLineId) {
        await applyRole(
          row.qtLineId,
          "Sup",
          String(e.newValue ?? ""),
          row.qtSupAssignmentId,
          "qtSupEmployeeId",
        );
      }
    },
    [canAssign, locked, onChanged, t, workWindowForLine],
  );

  const columnDefs = useMemo((): (ColDef<StaffingExcelCombinedRow> | ColGroupDef<StaffingExcelCombinedRow>)[] => {
    const roster = rosterQ.data ?? [];
    const editable = canAssign && !locked;

    const empFormatter =
      (
        lineField: "qnLineId" | "qtLineId",
        empField: "qnCounterEmployeeId" | "qnGateEmployeeId" | "qtSupEmployeeId",
        role: "Counter" | "Gate" | "Sup",
      ) =>
      (p: ValueFormatterParams<StaffingExcelCombinedRow>) => {
        const lineId = p.data?.[lineField] ?? null;
        const empId = p.data?.[empField] as string | null;
        if (!lineId) return "";
        const opts = rosterOptionsForLine(roster, lineId);
        return formatEmployeeCell(empId, opts, day, lineId, role);
      };

    const empCol = (
      field: "qnCounterEmployeeId" | "qnGateEmployeeId" | "qtSupEmployeeId",
      headerName: string,
      lineField: "qnLineId" | "qtLineId",
      role: "Counter" | "Gate" | "Sup",
      width: number,
    ): ColDef<StaffingExcelCombinedRow> => ({
      field,
      headerName,
      width,
      editable: (p) =>
        editable && !!(p.data?.[lineField] as string | null | undefined),
      cellEditor: "agSelectCellEditor",
      cellEditorParams: (p: ICellEditorParams<StaffingExcelCombinedRow>) => {
        const lineId = p.data?.[lineField];
        const opts = lineId ? rosterOptionsForLine(roster, lineId) : [];
        return { values: employeeEditorValues(opts) };
      },
      valueFormatter: empFormatter(lineField, field, role),
      singleClickEdit: true,
    });

    return [
      {
        headerName: t("dailyStaffing.excelQn"),
        children: [
          { field: "qnStt", headerName: "STT", width: 48, editable: false },
          { field: "qnFlightNo", headerName: "SHCB", width: 88, editable: false },
          { field: "qnDest", headerName: "DEST", width: 56, editable: false },
          { field: "qnAircraft", headerName: "A/C", width: 56, editable: false },
          { field: "qnEtd", headerName: "ETD", width: 88, editable: false },
          empCol("qnCounterEmployeeId", "S. Quầy // KS", "qnLineId", "Counter", 110),
          empCol("qnGateEmployeeId", "S.Gate", "qnLineId", "Gate", 88),
        ],
      },
      {
        headerName: t("dailyStaffing.excelQt"),
        children: [
          { field: "qtStt", headerName: "STT", width: 48, editable: false },
          { field: "qtFlightNo", headerName: "SHCB", width: 88, editable: false },
          { field: "qtDest", headerName: "Dest", width: 56, editable: false },
          { field: "qtEtd", headerName: "ETD", width: 88, editable: false },
          empCol("qtSupEmployeeId", "SUP", "qtLineId", "Sup", 100),
          {
            field: "qtStaffCount",
            headerName: t("dailyStaffing.colManning"),
            width: 72,
            editable: false,
          },
        ],
      },
    ];
  }, [canAssign, locked, rosterQ.data, day, t]);

  const defaultColDef = useMemo(
    (): ColDef => ({
      sortable: false,
      filter: false,
      resizable: true,
      suppressMovable: true,
    }),
    [],
  );

  return (
    <div className={styles.sheet}>
      {gridError ? (
        <div role="alert" style={{ color: "#b91c1c", fontSize: 13 }}>
          {gridError}
        </div>
      ) : null}

      <div className={styles.gridWrap}>
        <AgGridReact<StaffingExcelCombinedRow>
          theme={agsGridTheme}
          rowData={rowData}
          columnDefs={columnDefs}
          defaultColDef={defaultColDef}
          domLayout="autoHeight"
          getRowId={(p) => p.data.key}
          stopEditingWhenCellsLoseFocus
          onCellValueChanged={onCellValueChanged}
          overlayNoRowsTemplate={t("dailyStaffing.noFlightsDay")}
        />
      </div>

      <div className={styles.gioSection}>
        <div className={styles.gioTitle}>{t("dailyStaffing.gioLenCa")}</div>
        <table className={styles.gioTable}>
          <thead>
            <tr>
              <th>{t("dailyStaffing.colShift")}</th>
              <th>{t("dailyStaffing.colSegment")}</th>
              <th>{t("dailyStaffing.colStaff")}</th>
            </tr>
          </thead>
          <tbody>
            {gioRows.length === 0 ? (
              <tr>
                <td colSpan={3} style={{ color: "#64748b" }}>
                  —
                </td>
              </tr>
            ) : (
              gioRows.map((g) => (
                <tr key={`${g.window}-${g.segment}`}>
                  <td>{g.window}</td>
                  <td>{g.segment}</td>
                  <td>{g.names}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
