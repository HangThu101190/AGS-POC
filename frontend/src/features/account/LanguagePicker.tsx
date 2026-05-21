import { useState } from "react";
import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { GbFlag, VietnamFlag } from "@/shared/ui/flags/FlagIcons";
import { routeKeyFromPathname, routePathFor } from "@/shared/routing/routePaths";
import styles from "./accountPage.module.css";

export function LanguagePicker() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { user, updatePreferredLanguage } = useAuth();
  const current = i18n.language.startsWith("vi") ? "vi" : "en";
  const [pending, setPending] = useState(false);

  const select = async (lang: "vi" | "en") => {
    if (lang === current || pending) return;
    setPending(true);
    try {
      if (user) {
        await updatePreferredLanguage(lang);
      } else {
        await i18n.changeLanguage(lang);
        localStorage.setItem("ags.lang", lang);
      }
      const key = routeKeyFromPathname(location.pathname);
      if (key) {
        const target = routePathFor(key, lang);
        const normalized = location.pathname.replace(/\/$/, "") || "/";
        if (target !== normalized) {
          navigate({ pathname: target, search: location.search, hash: location.hash }, { replace: true });
        }
      }
    } catch {
      await i18n.changeLanguage(lang);
      localStorage.setItem("ags.lang", lang);
    } finally {
      setPending(false);
    }
  };

  return (
    <div className={styles.langGrid} role="group" aria-label={t("account.languageSection")}>
      <button
        type="button"
        className={current === "vi" ? styles.langOptionActive : styles.langOption}
        onClick={() => void select("vi")}
        disabled={pending}
        aria-pressed={current === "vi"}
      >
        <VietnamFlag className={styles.langFlag} title={t("account.languageVi")} />
        <span>{t("account.languageVi")}</span>
        {current === "vi" ? <span className={styles.langCheck}>{t("account.languageActive")}</span> : null}
      </button>
      <button
        type="button"
        className={current === "en" ? styles.langOptionActive : styles.langOption}
        onClick={() => void select("en")}
        disabled={pending}
        aria-pressed={current === "en"}
      >
        <GbFlag className={styles.langFlag} title={t("account.languageEn")} />
        <span>{t("account.languageEn")}</span>
        {current === "en" ? <span className={styles.langCheck}>{t("account.languageActive")}</span> : null}
      </button>
    </div>
  );
}
