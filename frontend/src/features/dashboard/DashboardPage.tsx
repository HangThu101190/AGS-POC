import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useNavigate } from "react-router-dom";
import { Alert, Loading } from "@/components/ui";
import { agsTokens } from "@/components/theme/tokens";
import { MetricCard } from "@/shared/webChrome/MetricCard";
import { WeekPageShell } from "@/shared/webChrome/WeekPageShell";
import {
  WorkflowStages,
  type WorkflowStage,
} from "@/shared/webChrome/WorkflowStages";
import { chrome } from "@/shared/webChrome/styles";
import { routePathFor } from "@/shared/routing/routePaths";
import { useDashboardMetrics } from "./useDashboardMetrics";

export function DashboardPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const metrics = useDashboardMetrics();
  const slotsSub =
    metrics.planStatus === "draft"
      ? t("dashboard.metricSlotsTipDraft")
      : t("dashboard.metricSlotsTipPub");

  const hasStaleData =
    metrics.isError &&
    (metrics.totalFlights > 0 ||
      metrics.totalSlots > 0 ||
      metrics.totalAssignments > 0 ||
      metrics.checkedIn > 0);

  const stages = useMemo<WorkflowStage[]>(
    () => [
      {
        n: 1,
        label: t("dashboard.s1l"),
        actor: t("dashboard.s1a"),
        action: t("dashboard.s1goto"),
        path: "/flights",
        done: metrics.totalFlights > 0,
        status:
          metrics.totalFlights > 0
            ? t("dashboard.s1stFlights", { n: metrics.totalFlights })
            : t("dashboard.s1stNone"),
      },
      {
        n: 2,
        label: t("dashboard.s2l"),
        actor: t("dashboard.s2a"),
        action: t("dashboard.s2goto"),
        path: routePathFor("dailyStaffing", i18n.language),
        done: metrics.planStatus !== "draft",
        status:
          metrics.planStatus === "draft"
            ? t("dashboard.s2stDraft")
            : t("dashboard.s2stPub", { n: metrics.totalSlots }),
      },
      {
        n: 3,
        label: t("dashboard.s3l"),
        actor: t("dashboard.s3a"),
        action: t("dashboard.s3goto"),
        path: routePathFor("phanCongSlot", i18n.language),
        done: metrics.totalAssignments > 0,
        status: t("dashboard.s3st", { n: metrics.totalAssignments }),
      },
      {
        n: 4,
        label: t("dashboard.s4l"),
        actor: t("dashboard.s4a"),
        action: t("dashboard.s4goto"),
        path: "/monitoring",
        done: metrics.checkedIn > 0,
        status: t("dashboard.s4st", { n: metrics.checkedIn, r: metrics.revisions }),
      },
      {
        n: 5,
        label: t("dashboard.s5l"),
        actor: t("dashboard.s5a"),
        action: t("dashboard.s5goto"),
        path: "/reconcile",
        done: metrics.planStatus === "closed",
        status:
          metrics.planStatus === "closed"
            ? t("dashboard.s5stDone")
            : t("dashboard.s5stPending"),
      },
    ],
    [metrics, t],
  );

  return (
    <WeekPageShell>
      {metrics.isLoading ? <Loading label={t("common.loading")} /> : null}
      {metrics.isError && !hasStaleData ? (
        <Alert severity="error" sx={{ mb: 2 }}>
          {t("dashboard.loadFailed")}{" "}
          <button type="button" onClick={() => metrics.refetch()} style={chrome.linkBtn}>
            {t("common.retry")}
          </button>
        </Alert>
      ) : null}
      {hasStaleData ? (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {t("dashboard.staleWarning")}{" "}
          <button type="button" onClick={() => metrics.refetch()} style={chrome.linkBtn}>
            {t("common.retry")}
          </button>
        </Alert>
      ) : null}

      <div style={chrome.metricGrid}>
        <MetricCard
          label={t("dashboard.flights")}
          value={metrics.totalFlights}
          sub={t("dashboard.delays", { n: metrics.delayedCount })}
          title={t("dashboard.metricFlightsTip")}
          icon="✈"
          accentColor={agsTokens.primary}
        />
        <MetricCard
          label={t("dashboard.slotsWeek")}
          value={metrics.totalSlots}
          sub={slotsSub}
          title={t("dashboard.metricSlotsTip")}
          icon="📅"
          accentColor="#0ea5e9"
        />
        <MetricCard
          label={t("dashboard.assigned")}
          value={metrics.totalAssignments}
          sub={t("dashboard.assignedStaff")}
          icon="👥"
          accentColor="#8b5cf6"
        />
        <MetricCard
          label={t("dashboard.chk")}
          value={metrics.checkedIn}
          sub={t("dashboard.chkRev", { n: metrics.revisions })}
          icon="✓"
          accentColor={agsTokens.success}
        />
      </div>

      <h2 style={chrome.h2}>{t("dashboard.workflowTitle")}</h2>
      <WorkflowStages stages={stages} onNavigate={(path) => navigate(path)} />

    </WeekPageShell>
  );
}
