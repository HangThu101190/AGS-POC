import Alert from "@mui/material/Alert";
import TextField from "@mui/material/TextField";
import { useState } from "react";
import { useTranslation } from "react-i18next";
import { Link, useLocation } from "react-router-dom";
import { MobileScreen } from "@/features/mobile/MobileScreen";
import { Alert as UiAlert, Button } from "@/components/ui";
import { changePassword } from "@/shared/api/authApi";
import { useAuth } from "@/shared/auth/AuthContext";
import type { AuthUser } from "@/shared/auth/types";
import { appLangFromI18n, routePathFor } from "@/shared/routing/routePaths";
import { PageShell } from "@/shared/webChrome/PageShell";
import { AccountSubnav } from "./AccountSubnav";
import { LanguagePicker } from "./LanguagePicker";
import styles from "./accountPage.module.css";

type SettingsContentProps = {
  user: AuthUser;
  profilePath: string;
  submitting: boolean;
  message: string | null;
  error: string | null;
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
  setCurrentPassword: (v: string) => void;
  setNewPassword: (v: string) => void;
  setConfirmPassword: (v: string) => void;
  submitPassword: () => void;
  showSubnav: boolean;
};

function SettingsContent({
  user,
  profilePath,
  submitting,
  message,
  error,
  currentPassword,
  newPassword,
  confirmPassword,
  setCurrentPassword,
  setNewPassword,
  setConfirmPassword,
  submitPassword,
  showSubnav,
}: SettingsContentProps) {
  const { t } = useTranslation();

  return (
    <div className={styles.wrap}>
      {showSubnav ? <AccountSubnav active="settings" /> : null}

      {user.mustChangePassword ? (
        <div className={styles.mustChangeBanner}>
          <UiAlert severity="warning">{t("account.mustChangePassword")}</UiAlert>
        </div>
      ) : null}

      <div className={styles.settingsLayout}>
        <section className={styles.sectionCard} aria-labelledby="account-lang-heading">
          <h2 id="account-lang-heading" className={styles.cardTitle}>
            {t("account.languageSection")}
          </h2>
          <p className={styles.sectionHint}>{t("account.languageHint")}</p>
          <LanguagePicker />
        </section>

        <section className={styles.sectionCard} aria-labelledby="account-pw-heading">
          <h2 id="account-pw-heading" className={styles.cardTitle}>
            {t("account.passwordSection")}
          </h2>
          <p className={styles.sectionHint}>{t("account.passwordHint")}</p>

          {(message || error) && (
            <div className={styles.alertStack}>
              {message ? <Alert severity="success">{message}</Alert> : null}
              {error ? <Alert severity="error">{error}</Alert> : null}
            </div>
          )}

          <form
            className={styles.passwordForm}
            onSubmit={(e) => {
              e.preventDefault();
              submitPassword();
            }}
          >
            <TextField
              type="password"
              label={t("account.currentPassword")}
              value={currentPassword}
              onChange={(e) => setCurrentPassword(e.target.value)}
              size="small"
              fullWidth
              required
              autoComplete="current-password"
            />
            <TextField
              type="password"
              label={t("account.newPassword")}
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              size="small"
              fullWidth
              required
              autoComplete="new-password"
            />
            <TextField
              type="password"
              label={t("account.confirmPassword")}
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              size="small"
              fullWidth
              required
              autoComplete="new-password"
            />
            <Button type="submit" disabled={submitting} sx={{ alignSelf: "flex-start", mt: 0.5 }}>
              {submitting ? t("account.saving") : t("account.changePassword")}
            </Button>
          </form>
        </section>
      </div>

      {showSubnav ? (
        <div className={styles.ctaRow} style={{ marginTop: "1.25rem" }}>
          <Link to={profilePath} className={styles.ctaSecondary}>
            ← {t("account.goToProfile")}
          </Link>
        </div>
      ) : null}
    </div>
  );
}

export function SettingsPage() {
  const { t, i18n } = useTranslation();
  const location = useLocation();
  const { user, refreshUser } = useAuth();
  const lang = appLangFromI18n(i18n.language);
  const profilePath = routePathFor("profile", lang);
  const isMobile = location.pathname.startsWith("/mobile");

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const submitPassword = async () => {
    setError(null);
    setMessage(null);
    if (newPassword !== confirmPassword) {
      setError(t("account.passwordMismatch"));
      return;
    }
    setSubmitting(true);
    try {
      await changePassword(currentPassword, newPassword);
      setMessage(t("account.passwordChanged"));
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      await refreshUser();
    } catch {
      setError(t("account.passwordChangeFailed"));
    } finally {
      setSubmitting(false);
    }
  };

  if (!user) return null;

  const contentProps: SettingsContentProps = {
    user,
    profilePath,
    submitting,
    message,
    error,
    currentPassword,
    newPassword,
    confirmPassword,
    setCurrentPassword,
    setNewPassword,
    setConfirmPassword,
    submitPassword: () => void submitPassword(),
    showSubnav: !isMobile,
  };

  if (isMobile) {
    return (
      <MobileScreen title={t("account.settingsTitle")} subtitle={t("account.settingsSubtitle")}>
        <SettingsContent {...contentProps} />
      </MobileScreen>
    );
  }

  return (
    <PageShell>
      <SettingsContent {...contentProps} />
    </PageShell>
  );
}
