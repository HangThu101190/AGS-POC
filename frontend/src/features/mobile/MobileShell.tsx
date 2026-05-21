import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { roleLabelKey } from "@/features/account/accountUtils";
import { WeekPicker } from "@/features/planning/WeekPicker";
import { useAuth } from "@/shared/auth/AuthContext";
import { isSupRole } from "@/shared/auth/roles";
import { WeekScopeProvider } from "@/shared/planning/WeekScopeContext";
import styles from "./mobileShell.module.css";

const MOBILE_WEEK_PATHS = new Set([
  "/mobile/plan",
  "/mobile/assign",
  "/mobile/team",
  "/mobile/flights",
  "/mobile/today",
  "/mobile/schedule",
]);

const SUP_TABS = [
  { to: "/mobile/plan", key: "mobile.plan", icon: "▤" },
  { to: "/mobile/assign", key: "mobile.assign", icon: "◐" },
  { to: "/mobile/team", key: "mobile.team", icon: "○○" },
  { to: "/mobile/flights", key: "mobile.flights", icon: "✈" },
  { to: "/mobile/profile", key: "mobile.profile", icon: "◯" },
] as const;

const STAFF_TABS = [
  { to: "/mobile/today", key: "mobile.today", icon: "◉" },
  { to: "/mobile/schedule", key: "mobile.schedule", icon: "▤" },
  { to: "/mobile/inbox", key: "mobile.inbox", icon: "✉" },
  { to: "/mobile/requests", key: "mobile.requests", icon: "↺" },
  { to: "/mobile/profile", key: "mobile.profile", icon: "◯" },
] as const;

export function MobileShell() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const location = useLocation();
  const tabs = user && isSupRole(user.role) ? SUP_TABS : STAFF_TABS;
  const initial = (user?.name || user?.code || "?").trim().slice(-1).toUpperCase();
  const roleLabel = user ? t(roleLabelKey(user.role)) : "";
  const roleTip = user
    ? t(`roles.${user.role}Tip`, { defaultValue: "" }) || undefined
    : undefined;

  return (
    <div className={styles.wrap}>
      {user ? (
        <header className={styles.roleBanner}>
          <span className={styles.avatar} aria-hidden>
            {initial}
          </span>
          <div className={styles.roleMeta}>
            <div className={styles.roleName}>{user.name}</div>
            <div className={styles.roleSub}>{user.code}</div>
          </div>
          <span className={styles.rolePill} title={roleTip}>
            {roleLabel}
          </span>
        </header>
      ) : null}

      <main className={styles.main}>
        {MOBILE_WEEK_PATHS.has(location.pathname) ? (
          <WeekScopeProvider>
            <div style={{ padding: "8px 12px 0" }}>
              <WeekPicker compact />
            </div>
            <Outlet key={location.pathname} />
          </WeekScopeProvider>
        ) : (
          <Outlet key={location.pathname} />
        )}
      </main>

      <nav className={styles.nav} aria-label={t("mobile.nav")}>
        {tabs.map((tab) => (
          <NavLink
            key={tab.to}
            to={tab.to}
            className={({ isActive }) =>
              [styles.tab, isActive ? styles.tabActive : ""].filter(Boolean).join(" ")
            }
          >
            <span className={styles.tabIcon} aria-hidden>
              {tab.icon}
            </span>
            {t(tab.key)}
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
