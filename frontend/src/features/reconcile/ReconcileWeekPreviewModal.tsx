import { Fragment } from "react";
import { useTranslation } from "react-i18next";
import type { ReconcileSummaryDto } from "@/shared/api/reconcileApi";
import {
  formatSegCell,
  weekExportSegTextColor,
  weekExportShiftFillHex,
  WEEK_EXPORT_THEME,
  type ReconcilePreviewDay,
} from "@/shared/planning/weekExportTheme";
import { ActionButton, chrome } from "@/shared/webChrome";

function toPreviewDay(d: {
  actualCode: string;
  plannedSegments: string[];
  actualSegments: string[];
  mismatch: boolean;
  hasRevision?: boolean;
}): ReconcilePreviewDay {
  let actualType = "work";
  if (d.actualCode === "T") actualType = "weekoff";
  else if (d.actualCode === "L") actualType = "holiday";
  return {
    actualCode: d.actualCode,
    actualType,
    plannedSegments: d.plannedSegments ?? [],
    actualSegments: d.actualSegments ?? [],
    hasRevisionFlag: Boolean(d.hasRevision || d.mismatch),
  };
}

export function ReconcileWeekPreviewModal({
  summary,
  onClose,
}: {
  summary: ReconcileSummaryDto;
  onClose: () => void;
}) {
  const { t } = useTranslation();
  const weekDates =
    summary.employees[0]?.days.map((d) => d.dateLabel) ??
    Array.from({ length: 7 }, (_, i) => `D${i}`);

  return (
    <div
      role="dialog"
      aria-modal
      style={{
        position: "fixed",
        inset: 0,
        zIndex: 1200,
        background: "rgba(15, 23, 42, 0.45)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: 24,
      }}
      onClick={onClose}
    >
      <div
        style={{
          background: "#fff",
          borderRadius: 12,
          maxWidth: 960,
          width: "100%",
          maxHeight: "85vh",
          overflow: "auto",
          padding: 20,
          boxShadow: "0 20px 50px rgba(0,0,0,0.2)",
        }}
        onClick={(e) => e.stopPropagation()}
      >
        <h2 style={{ ...chrome.h2, marginTop: 0, color: `#${WEEK_EXPORT_THEME.titleRed}` }}>
          {t("reconcile.previewTitle")} · {summary.weekId}
        </h2>
        <p style={{ fontSize: 12, color: "#64748b" }}>{t("reconcile.previewHint")}</p>

        <table style={{ width: "100%", borderCollapse: "collapse", fontSize: 11 }}>
          <thead>
            <tr style={{ background: "#fff", color: `#${WEEK_EXPORT_THEME.titleRed}` }}>
              <th style={{ textAlign: "left", padding: 8, border: "1px solid #fecaca" }}>
                {t("reconcile.colEmployee")}
              </th>
              {weekDates.map((d) => (
                <th key={d} style={{ padding: 8, border: "1px solid #fecaca", minWidth: 72 }}>
                  {d}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {summary.employees.map((emp) => {
              const days = emp.days.map(toPreviewDay);
              return (
                <Fragment key={emp.employeeId}>
                  <tr>
                    <td
                      style={{
                        padding: 8,
                        border: "1px solid #e2e8f0",
                        fontWeight: 600,
                        verticalAlign: "top",
                      }}
                    >
                      {emp.name}
                      <div style={{ fontSize: 10, color: "#64748b" }}>{emp.code}</div>
                    </td>
                    {emp.days.map((d, i) => {
                      const pd = days[i];
                      const bg = weekExportShiftFillHex(pd);
                      return (
                        <td
                          key={d.dayIdx}
                          style={{
                            padding: 6,
                            border: "1px solid #e2e8f0",
                            background: bg,
                            color: weekExportSegTextColor(bg),
                            textAlign: "center",
                            fontSize: 10,
                            fontWeight: pd?.hasRevisionFlag ? 700 : 500,
                          }}
                        >
                          {formatSegCell(pd)}
                        </td>
                      );
                    })}
                  </tr>
                  <tr>
                    <td style={{ padding: 8, border: "1px solid #e2e8f0", color: "#64748b" }}>
                      h
                    </td>
                    {emp.days.map((d) => (
                      <td
                        key={`h-${d.dayIdx}`}
                        style={{
                          padding: 6,
                          border: "1px solid #e2e8f0",
                          textAlign: "center",
                          background: `#${WEEK_EXPORT_THEME.white}`,
                        }}
                      >
                        {d.actualCode || "—"}
                      </td>
                    ))}
                  </tr>
                </Fragment>
              );
            })}
          </tbody>
        </table>

        <div style={{ marginTop: 16, textAlign: "right" }}>
          <ActionButton variant="secondary" onClick={onClose}>
            {t("common.cancel")}
          </ActionButton>
        </div>
      </div>
    </div>
  );
}
