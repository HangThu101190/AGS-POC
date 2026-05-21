import { useTranslation } from "react-i18next";
import { NavLink } from "react-router-dom";
import { appLangFromI18n, routePathFor } from "@/shared/routing/routePaths";
import styles from "./accountPage.module.css";

type AccountSubnavProps = {
  active: "profile" | "settings";
};

export function AccountSubnav({ active }: AccountSubnavProps) {
  const { t, i18n } = useTranslation();
  const lang = appLangFromI18n(i18n.language);
  const profilePath = routePathFor("profile", lang);
  const settingsPath = routePathFor("settings", lang);

  return (
    <nav className={styles.subnav} aria-label={t("account.navLabel")}>
      <NavLink
        to={profilePath}
        className={active === "profile" ? styles.subnavActive : styles.subnavLink}
        end
      >
        {t("account.profileTitle")}
      </NavLink>
      <NavLink
        to={settingsPath}
        className={active === "settings" ? styles.subnavActive : styles.subnavLink}
        end
      >
        {t("account.settingsTitle")}
      </NavLink>
    </nav>
  );
}
