import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Loading } from "@/components/ui";
import {
  exportReconcileMonth,
  exportReconcileWeek,
  fetchReconcileSummary,
} from "@/shared/api/reconcileApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import {
  StatusBanner,
  ActionButton,
  WeekPageShell,
  chrome,
} from "@/shared/webChrome";
import { ALL_DEPT_CODES } from "@/shared/constants/departments";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { ReconcileWeekPreviewModal } from "./ReconcileWeekPreviewModal";

export function ReconcilePage() {
  const { t } = useTranslation();
  const { weekId } = useWeekScope();
  const [dept, setDept] = useState<string>("PVHK_DI");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [previewOpen, setPreviewOpen] = useState(false);

  const summaryQuery = useQuery({
    queryKey: ["reconcile", weekId, dept],
    queryFn: () => fetchReconcileSummary(weekId, dept),
    staleTime: 30_000,
  });

  const downloadBlob = (blob: Blob, name: string) => {
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = name;
    a.click();
    URL.revokeObjectURL(url);
  };

  const onExportWeek = async () => {
    setError(null);
    try {
      const blob = await exportReconcileWeek(weekId, dept);
      downloadBlob(blob, `TH_CONG_${dept}_${weekId}.xlsx`);
      setMessage(t("reconcile.exported"));
    } catch {
      setError(t("reconcile.actionFailed"));
    }
  };

  const onExportMonth = async () => {
    setError(null);
    try {
      const blob = await exportReconcileMonth(weekId, dept);
      downloadBlob(blob, `TH_CONG_THANG_${dept}.xlsx`);
      setMessage(t("reconcile.exportedMonth"));
    } catch {
      setError(t("reconcile.actionFailed"));
    }
  };

  const summary = summaryQuery.data;

  return (
    <WeekPageShell>
      <div
        style={{
          ...chrome.toolbar,
          marginBottom: 8,
          flexWrap: "wrap",
          gap: 8,
          alignItems: "center",
        }}
      >
        <label style={{ fontSize: 12, color: "#64748b" }}>
          {t("reconcile.dept")}
          <select
            value={dept}
            onChange={(e) => setDept(e.target.value)}
            style={{
              marginLeft: 8,
              padding: "6px 10px",
              borderRadius: 6,
              border: "1px solid #e2e8f0",
              fontFamily: "inherit",
            }}
          >
            {ALL_DEPT_CODES.map((d) => (
              <option key={d} value={d}>
                {departmentLabel(d, t)}
              </option>
            ))}
          </select>
        </label>
        <ActionButton
          variant="secondary"
          disabled={!summary}
          onClick={() => setPreviewOpen(true)}
        >
          {t("reconcile.previewWeek")}
        </ActionButton>
        <ActionButton variant="primary" onClick={() => void onExportWeek()}>
          {t("reconcile.exportWeek")}
        </ActionButton>
        <ActionButton variant="secondary" onClick={() => void onExportMonth()}>
          {t("reconcile.exportMonth")}
        </ActionButton>
      </div>

      {message ? <StatusBanner variant="success">{message}</StatusBanner> : null}
      {error ? <StatusBanner variant="warn">{error}</StatusBanner> : null}
      {summaryQuery.isLoading ? <Loading label={t("common.loading")} /> : null}
      {summaryQuery.isError ? (
        <StatusBanner variant="warn">{t("reconcile.loadFailed")}</StatusBanner>
      ) : null}

      {summary?.employees.map((emp) => {
        const open = expandedId === emp.employeeId;
        const revCount = emp.days.filter((d) => d.hasRevision || d.mismatch).length;
        return (
          <div
            key={emp.employeeId}
            style={{ ...chrome.tableWrap, marginBottom: 10, overflow: "hidden" }}
          >
            <button
              type="button"
              onClick={() => setExpandedId(open ? null : emp.employeeId)}
              style={{
                width: "100%",
                textAlign: "left",
                padding: "14px 16px",
                border: "none",
                background: open ? "#f8fafc" : "#fff",
                cursor: "pointer",
                fontFamily: "inherit",
              }}
            >
              <div style={{ fontSize: 14, fontWeight: 600, color: "#0f172a" }}>
                {emp.name} ({emp.code})
                {revCount > 0 ? (
                  <span style={{ color: "#b45309", fontWeight: 500, marginLeft: 8 }}>
                    · {t("reconcile.revCount", { n: revCount })}
                  </span>
                ) : null}
              </div>
            </button>
            {open ? (
              <div style={{ padding: "0 16px 14px", display: "flex", flexDirection: "column", gap: 8 }}>
                {emp.days.map((d) => (
                  <div
                    key={d.dayIdx}
                    style={{
                      fontSize: 12,
                      padding: "8px 10px",
                      borderRadius: 6,
                      background: d.mismatch || d.hasRevision ? "#fef3c7" : "#f8fafc",
                      border: `1px solid ${d.hasRevision ? "#fcd34d" : "#e2e8f0"}`,
                    }}
                  >
                    <div style={{ fontWeight: 600, marginBottom: 4 }}>
                      {d.dateLabel}
                      {d.hasRevision ? (
                        <span style={{ marginLeft: 6, color: "#b45309", fontSize: 11 }}>
                          {t("reconcile.revisionFlag")}
                        </span>
                      ) : null}
                    </div>
                    <div style={{ color: "#475569" }}>
                      {t("reconcile.plannedVsActual", {
                        planned: d.plannedCode || "—",
                        actual: d.actualCode || "—",
                      })}
                    </div>
                    {d.revisionReason ? (
                      <div style={{ fontSize: 11, color: "#64748b", marginTop: 4 }}>
                        {d.revisionReason}
                        {d.revisionBy ? ` · ${d.revisionBy}` : ""}
                      </div>
                    ) : null}
                  </div>
                ))}
              </div>
            ) : null}
          </div>
        );
      })}

      {previewOpen && summary ? (
        <ReconcileWeekPreviewModal summary={summary} onClose={() => setPreviewOpen(false)} />
      ) : null}
    </WeekPageShell>
  );
}
