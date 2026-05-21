import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import type { ColDef } from "ag-grid-community";
import { useCallback, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert, ButtonToolbar, Card, DataTable } from "@/components/ui";
import { fetchAuditEventsPage, type AuditEventDto } from "@/shared/api/auditApi";
import { useAuth } from "@/shared/auth/AuthContext";
import { canAccessAudit } from "@/shared/auth/roles";
import { useFullPageTableHeight } from "@/shared/hooks/useMediaQuery";
import { gridCol } from "@/shared/ui/gridColumn";
import {
  datetimeLocalToUtcIso,
  formatLocaleDateTime,
  startOfTodayUtcIso,
  toDatetimeLocalValue,
} from "@/shared/utils/dateLocale";
import { auditActionLabel } from "./auditActionLabel";
import { auditPathLabel } from "./auditPathLabel";

export function AuditPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const [fromUtc, setFromUtc] = useState(() => startOfTodayUtcIso());
  const [refreshKey, setRefreshKey] = useState(0);
  const [loadError, setLoadError] = useState<string | null>(null);
  const tableHeight = useFullPageTableHeight(72, loadError ? 1 : 0);

  const lang = i18n.language;

  const columns = useMemo<ColDef<AuditEventDto>[]>(
    () => [
      gridCol(t, "audit.colTime", undefined, {
        colId: "occurredAtUtc",
        field: "occurredAtUtc",
        width: 168,
        flex: 0,
        sort: "desc",
        valueFormatter: (p) =>
          p.value ? formatLocaleDateTime(String(p.value), lang) : "—",
      }),
      gridCol(t, "audit.colActor", undefined, {
        colId: "employeeCode",
        flex: 1,
        minWidth: 140,
        valueGetter: (p) => {
          const row = p.data;
          if (!row) return "";
          if (row.actorName && row.employeeCode) {
            return `${row.actorName} (${row.employeeCode})`;
          }
          return row.actorName ?? row.employeeCode ?? "—";
        },
      }),
      gridCol(t, "audit.colAction", undefined, {
        colId: "action",
        flex: 1,
        minWidth: 180,
        sortable: false,
        filter: false,
        valueGetter: (p) =>
          p.data ? auditActionLabel(p.data.httpMethod, p.data.path, t) : "",
      }),
      gridCol(t, "audit.colPath", "audit.colPathTip", {
        colId: "path",
        flex: 1,
        minWidth: 200,
        valueGetter: (p) => (p.data?.path ? auditPathLabel(p.data.path, t) : ""),
        tooltipValueGetter: (p) => p.data?.path ?? "",
      }),
      gridCol(t, "audit.colMethod", undefined, {
        colId: "httpMethod",
        field: "httpMethod",
        width: 88,
        flex: 0,
      }),
      gridCol(t, "audit.colStatus", undefined, {
        colId: "statusCode",
        field: "statusCode",
        width: 88,
        flex: 0,
        cellStyle: (p) => {
          const code = Number(p.value ?? 0);
          if (code >= 200 && code < 300) return { color: "#15803d", fontWeight: 600 };
          if (code >= 400) return { color: "#b91c1c", fontWeight: 600 };
          return undefined;
        },
      }),
    ],
    [lang, t],
  );

  const fetchRows = useCallback(
    async (params: Parameters<typeof fetchAuditEventsPage>[0], signal?: AbortSignal) => {
      void refreshKey;
      try {
        const page = await fetchAuditEventsPage(
          {
            ...params,
            fromUtc: fromUtc || undefined,
            sortBy: params.sortBy ?? "occurredAtUtc",
            sortDir: params.sortDir ?? "desc",
          },
          signal,
        );
        setLoadError(null);
        return page;
      } catch {
        setLoadError(t("audit.loadFailed"));
        return { items: [], page: 0, pageSize: params.pageSize, totalCount: 0 };
      }
    },
    [fromUtc, refreshKey, t],
  );

  if (!user || !canAccessAudit(user.role)) {
    return (
      <Stack spacing={2}>
        <Alert severity="info">{t("errors.forbiddenBody")}</Alert>
      </Stack>
    );
  }

  return (
    <Stack
      spacing={{ xs: 1.5, md: 2 }}
      sx={{ minWidth: 0, height: "calc(100dvh - var(--ags-shell-header-height) - 2 * var(--ags-content-py, 16px))" }}
    >
      <Stack direction={{ xs: "column", sm: "row" }} spacing={1} sx={{ alignItems: { sm: "center" } }}>
        <TextField
          label={t("audit.fromDate")}
          type="datetime-local"
          size="small"
          value={fromUtc ? toDatetimeLocalValue(fromUtc) : ""}
          onChange={(e) => {
            const next = e.target.value ? datetimeLocalToUtcIso(e.target.value) : "";
            setFromUtc(next);
            setRefreshKey((k) => k + 1);
          }}
          slotProps={{ inputLabel: { shrink: true } }}
          sx={{ minWidth: 220 }}
        />
        <ButtonToolbar
          onClick={() => {
            setFromUtc(startOfTodayUtcIso());
            setRefreshKey((k) => k + 1);
          }}
        >
          {t("audit.todayOnly")}
        </ButtonToolbar>
      </Stack>

      {loadError ? <Alert severity="error">{loadError}</Alert> : null}

      <Card>
        <DataTable<AuditEventDto>
          columnDefs={columns}
          fetchRows={fetchRows}
          height={tableHeight}
          showRowCount
          gridOptions={{
            getRowId: (p) =>
              p.data?.id ?? `${p.data?.occurredAtUtc ?? "row"}-${p.data?.path ?? ""}`,
          }}
        />
      </Card>
    </Stack>
  );
}
