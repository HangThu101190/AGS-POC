import Box from "@mui/material/Box";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { Navigate, useSearchParams } from "react-router-dom";
import { routePathFor } from "@/shared/routing/routePaths";
import { WorkZoneConfigPage } from "@/features/config/WorkZoneConfigPage";
import { ManningRulesTab } from "@/features/config/ManningRulesTab";
import { ShiftAttendancePolicyTab } from "@/features/config/ShiftAttendancePolicyTab";
import { PageTabs } from "@/components/ui";
import { useAuth } from "@/shared/auth/AuthContext";
import { isHrRole } from "@/shared/auth/roles";

type ConfigTab = "workzone" | "attendance-policy" | "manning-rules";

export function ConfigHubPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const [params, setParams] = useSearchParams();
  const tab = (params.get("tab") as ConfigTab | null) ?? "manning-rules";
  const isHr = isHrRole(user?.role ?? "staff");

  const tabs = useMemo(() => {
    const items: { id: ConfigTab; label: string; show: boolean }[] = [
      { id: "manning-rules", label: t("config.tabs.manningRules"), show: isHr },
      {
        id: "attendance-policy",
        label: t("config.tabs.attendancePolicy"),
        show: isHr,
      },
      { id: "workzone", label: t("config.tabs.workzone"), show: isHr },
    ];
    return items.filter((x) => x.show);
  }, [t, isHr]);

  const active = tabs.some((x) => x.id === tab) ? tab : tabs[0]?.id ?? "manning-rules";

  if (params.get("tab") === "daily-staffing") {
    const ngay = params.get("ngay");
    const suffix = ngay ? `?view=staffing&ngay=${ngay}` : "?view=staffing";
    return <Navigate to={`${routePathFor("dailyStaffing", i18n.language)}${suffix}`} replace />;
  }

  const isWorkzone = active === "workzone";

  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        minHeight: 0,
        height: "calc(100dvh - var(--ags-shell-header-height) - 2 * var(--ags-content-py, 16px))",
        overflow: "hidden",
        "& > [data-page-tabs-grow]": {
          flex: 1,
          minHeight: 0,
          display: "flex",
          flexDirection: "column",
        },
      }}
    >
      <PageTabs<ConfigTab>
        value={active}
        grow
        panelPadding={isWorkzone ? "flush" : "padded"}
        panelMode={isWorkzone ? "fit" : "scroll"}
        ariaLabel={t("config.tabsAria")}
        onChange={(value) => {
          const next = new URLSearchParams(params);
          next.set("tab", value);
          setParams(next);
        }}
        items={tabs.map((item) => ({ value: item.id, label: item.label }))}
      >
        {active === "workzone" ? <WorkZoneConfigPage embedded /> : null}
        {active === "manning-rules" ? <ManningRulesTab /> : null}
        {active === "attendance-policy" ? <ShiftAttendancePolicyTab /> : null}
      </PageTabs>
    </Box>
  );
}
