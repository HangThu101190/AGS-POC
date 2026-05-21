import { useEffect, useState } from "react";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import { WeekScopedOutlet } from "@/app/layouts/WeekScopedOutlet";
import { useTranslation } from "react-i18next";
import { SidebarQuickGuide } from "@/components/layout/SidebarQuickGuide";
import { UserMenu } from "@/components/layout/UserMenu";
import { useAuth } from "@/shared/auth/AuthContext";
import { canAccessRoute } from "@/shared/auth/roles";
import { useIsMdUp } from "@/shared/hooks/useMediaQuery";
import { routePathFor } from "@/shared/routing/routePaths";
import { NavMenuIcon } from "./NavMenuIcon";
import { NavSidebarIcon } from "./NavSidebarIcon";
import { NAV_ITEMS } from "./navConfig";
import styles from "./appShell.module.css";

function canSeeNav(
  role: Parameters<typeof canAccessRoute>[0],
  permissions: string[] | undefined,
  key: (typeof NAV_ITEMS)[number]["key"],
) {
  return canAccessRoute(role, key, permissions);
}

export function AppShell() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { user, logout } = useAuth();
  const isMdUp = useIsMdUp();
  const [navOpen, setNavOpen] = useState(false);

  useEffect(() => {
    setNavOpen(false);
  }, [location.pathname]);

  useEffect(() => {
    if (isMdUp || !navOpen) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = prev;
    };
  }, [isMdUp, navOpen]);

  useEffect(() => {
    if (isMdUp || !navOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setNavOpen(false);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [isMdUp, navOpen]);

  const handleLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  const sidebarClass = [styles.sidebar, !isMdUp && navOpen ? styles.sidebarOpen : ""]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={styles.shell}>
      {!isMdUp && navOpen ? (
        <button
          type="button"
          className={styles.backdrop}
          aria-label={t("nav.closeMenu")}
          onClick={() => setNavOpen(false)}
        />
      ) : null}

      <aside
        id="app-shell-nav"
        className={sidebarClass}
        aria-hidden={!isMdUp && !navOpen ? true : undefined}
      >
        <div className={styles.brand}>
          <img src="/AGS_Logo.png" alt="AGS" />
        </div>

        <nav className={styles.nav} aria-label={t("nav.main")}>
          {NAV_ITEMS.filter((item) => user && canSeeNav(user.role, user.permissions, item.key)).map((item) => (
            <NavLink
              key={item.key}
              to={routePathFor(item.key, i18n.language)}
              end={item.end}
              className={({ isActive }) =>
                [styles.navLink, isActive ? styles.navLinkActive : ""].filter(Boolean).join(" ")
              }
              onClick={() => !isMdUp && setNavOpen(false)}
            >
              <span className={styles.navIcon} aria-hidden>
                <NavSidebarIcon name={item.icon} />
              </span>
              <span className={styles.navLabel}>{t(`nav.${item.key}`)}</span>
              {item.star ? (
                <span className={styles.navStar} aria-hidden>
                  ★
                </span>
              ) : null}
            </NavLink>
          ))}
        </nav>

        <SidebarQuickGuide />
      </aside>

      <div className={styles.main}>
        <header className={styles.topbar}>
          {!isMdUp ? (
            <div className={styles.topbarStart}>
              <button
                type="button"
                className={styles.menuBtn}
                aria-expanded={navOpen}
                aria-controls="app-shell-nav"
                aria-label={navOpen ? t("nav.closeMenu") : t("nav.openMenu")}
                onClick={() => setNavOpen((o) => !o)}
              >
                <NavMenuIcon open={navOpen} />
              </button>
            </div>
          ) : null}
          <UserMenu onLogout={handleLogout} />
        </header>
        <main className={styles.content}>
          <WeekScopedOutlet />
        </main>
      </div>
    </div>
  );
}
