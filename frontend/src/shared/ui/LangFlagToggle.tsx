import Box from "@mui/material/Box";
import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { GbFlag, VietnamFlag } from "@/shared/ui/flags/FlagIcons";
import { routeKeyFromPathname, routePathFor } from "@/shared/routing/routePaths";
import styles from "./langFlagToggle.module.css";

/** Shows flag of the **current** UI language; click to switch and remap URL slug. */
export function LangFlagToggle() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { user, updatePreferredLanguage } = useAuth();
  const isVi = i18n.language.startsWith("vi");

  const toggleLang = async () => {
    const next = isVi ? "en" : "vi";
    if (user) {
      try {
        await updatePreferredLanguage(next);
      } catch {
        void i18n.changeLanguage(next);
        localStorage.setItem("ags.lang", next);
      }
    } else {
      void i18n.changeLanguage(next);
      localStorage.setItem("ags.lang", next);
    }

    const key = routeKeyFromPathname(location.pathname);
    if (!key) return;
    const target = routePathFor(key, next);
    const normalizedCurrent = location.pathname.replace(/\/$/, "") || "/";
    if (target !== normalizedCurrent) {
      navigate({ pathname: target, search: location.search, hash: location.hash }, { replace: true });
    }
  };

  const label = isVi ? t("common.switchToEnglish") : t("common.switchToVietnamese");

  return (
    <Box
      component="button"
      type="button"
      className={styles.flagBtn}
      onClick={() => void toggleLang()}
      aria-label={label}
      title={label}
    >
      {isVi ? <VietnamFlag className={styles.flag} /> : <GbFlag className={styles.flag} />}
    </Box>
  );
}
