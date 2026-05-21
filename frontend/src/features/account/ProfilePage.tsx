import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { PageShell } from "@/shared/webChrome/PageShell";
import { useAuth } from "@/shared/auth/AuthContext";
import { appLangFromI18n, routePathFor } from "@/shared/routing/routePaths";
import { AccountSubnav } from "./AccountSubnav";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { getUserInitials, roleBadgeClass, roleLabelKey } from "./accountUtils";
import styles from "./accountPage.module.css";

function InfoRow({ label, value, mono }: { label: string; value: string; mono?: boolean }) {
  return (
    <div className={styles.row}>
      <span className={styles.rowLabel}>{label}</span>
      <span className={mono ? styles.rowValueMono : styles.rowValue}>{value}</span>
    </div>
  );
}

export function ProfilePage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const lang = appLangFromI18n(i18n.language);
  const settingsPath = routePathFor("settings", lang);

  if (!user) return null;

  const initials = getUserInitials(user.name, user.code);
  const deptLabel = departmentLabel(user.departmentCode, t);
  const roleClass = styles[roleBadgeClass(user.role) as keyof typeof styles];

  return (
    <PageShell>
      <div className={styles.wrap}>
        <AccountSubnav active="profile" />

        <header className={styles.hero}>
          <div className={styles.heroAvatar} aria-hidden>
            {initials}
          </div>
          <div className={styles.heroBody}>
            <h2 className={styles.heroName}>{user.name}</h2>
            <p className={styles.heroMeta}>{user.loginName}</p>
            <div className={styles.heroBadges}>
              <span className={roleClass}>{t(roleLabelKey(user.role))}</span>
              <span className={styles.codeBadge}>{user.code}</span>
            </div>
          </div>
        </header>

        <div className={styles.grid}>
          <section className={styles.card} aria-labelledby="account-work-heading">
            <h3 id="account-work-heading" className={styles.cardTitle}>
              {t("account.sectionWork")}
            </h3>
            <div className={styles.cardBody}>
              <InfoRow label={t("account.fieldDepartment")} value={deptLabel} />
              <InfoRow label={t("account.fieldRole")} value={t(roleLabelKey(user.role))} />
            </div>
          </section>

          <section className={styles.card} aria-labelledby="account-login-heading">
            <h3 id="account-login-heading" className={styles.cardTitle}>
              {t("account.sectionAccount")}
            </h3>
            <div className={styles.cardBody}>
              <InfoRow label={t("account.fieldCode")} value={user.code} mono />
              <InfoRow label={t("account.fieldLogin")} value={user.loginName} mono />
              <InfoRow label={t("account.fieldName")} value={user.name} />
            </div>
          </section>
        </div>

        <div className={styles.ctaRow}>
          <Link to={settingsPath} className={styles.cta}>
            {t("account.goToSettings")} →
          </Link>
        </div>
      </div>
    </PageShell>
  );
}
