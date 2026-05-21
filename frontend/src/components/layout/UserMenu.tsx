import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { NotificationBell } from "@/components/layout/NotificationBell";
import { useAuth } from "@/shared/auth/AuthContext";
import { LangFlagToggle } from "@/shared/ui/LangFlagToggle";
import { fetchDepartmentsPage } from "@/shared/api/departmentsApi";
import { departmentLabel, departmentLabelFromDto } from "@/shared/i18n/departmentLabel";
import { routePathFor, appLangFromI18n } from "@/shared/routing/routePaths";
import styles from "./userMenu.module.css";

type UserMenuProps = {
  onLogout: () => void | Promise<void>;
};

function roleLabelKey(role: string): string {
  switch (role) {
    case "hr":
      return "roles.hr";
    case "sup":
      return "roles.sup";
    case "staff":
      return "roles.staff";
    case "tbdh":
      return "roles.tbdh";
    default:
      return "roles.staff";
  }
}

function looksLikeUuid(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
}

export function UserMenu({ onLogout }: UserMenuProps) {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const [deptName, setDeptName] = useState("—");
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onDoc = (e: MouseEvent) => {
      if (!rootRef.current?.contains(e.target as Node)) setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("mousedown", onDoc);
    window.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDoc);
      window.removeEventListener("keydown", onKey);
    };
  }, [open]);

  useEffect(() => {
    if (!user) return;
    if (user.departmentCode) {
      setDeptName(departmentLabel(user.departmentCode, t));
      return;
    }
    let cancelled = false;
    if (!looksLikeUuid(user.departmentId)) {
      setDeptName(departmentLabel(user.departmentId, t));
      return;
    }
    void fetchDepartmentsPage({ page: 0, pageSize: 100 }).then((page) => {
      if (cancelled) return;
      const dept = page.items.find((d) => d.id === user.departmentId);
      setDeptName(departmentLabelFromDto(dept, t));
    });
    return () => {
      cancelled = true;
    };
  }, [user, t]);

  if (!user) {
    return (
      <div className={styles.actions}>
        <LangFlagToggle />
      </div>
    );
  }

  const initial = (user.name || user.code).trim().slice(-1).toUpperCase();
  const lang = appLangFromI18n(i18n.language);
  const profilePath = routePathFor("profile", lang);
  const settingsPath = routePathFor("settings", lang);

  return (
    <div className={styles.actions} ref={rootRef}>
      <LangFlagToggle />
      <NotificationBell />
      <button
        type="button"
        className={styles.chip}
        aria-expanded={open}
        aria-haspopup="menu"
        onClick={() => setOpen((v) => !v)}
      >
        <span className={styles.avatar} aria-hidden>
          {initial}
        </span>
        <span className={styles.meta}>
          <span className={styles.name}>{user.name}</span>
          <span className={styles.detail}>
            {t(roleLabelKey(user.role))} · {deptName}
          </span>
        </span>
        <span className={styles.chevron} aria-hidden>
          ▾
        </span>
      </button>
      {open ? (
        <div className={styles.menu} role="menu">
          <div className={styles.menuHeader}>
            <strong>{user.name}</strong>
            <span>
              {user.code} · {t(roleLabelKey(user.role))}
            </span>
          </div>
          <Link to={profilePath} role="menuitem" className={styles.menuItem} onClick={() => setOpen(false)}>
            {t("account.profileTitle")}
          </Link>
          <Link to={settingsPath} role="menuitem" className={styles.menuItem} onClick={() => setOpen(false)}>
            {t("account.settingsTitle")}
          </Link>
          <button
            type="button"
            role="menuitem"
            className={styles.menuItemDanger}
            onClick={() => {
              setOpen(false);
              void onLogout();
            }}
          >
            {t("auth.logout")}
          </button>
        </div>
      ) : null}
    </div>
  );
}
