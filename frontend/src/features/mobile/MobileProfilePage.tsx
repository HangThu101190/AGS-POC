import { useTranslation } from "react-i18next";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { isSupRole } from "@/shared/auth/roles";
import { getUserInitials, roleLabelKey } from "@/features/account/accountUtils";
import { departmentLabel } from "@/shared/i18n/departmentLabel";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

export function MobileProfilePage() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const onLogout = async () => {
    await logout();
    navigate("/login", { replace: true });
  };

  if (!user) return null;

  const initials = getUserInitials(user.name, user.code);
  const roleLabel = isSupRole(user.role) ? t("mobile.profileRoleSup") : t(roleLabelKey(user.role));
  const deptLabel = departmentLabel(user.departmentCode, t);

  return (
    <MobileScreen title={t("mobile.profile")} subtitle={user.code}>
      <div style={{ textAlign: "center", padding: "12px 0 20px" }}>
        <div
          style={{
            ...mobile.avatarLg,
            margin: "0 auto",
            background: "var(--ags-primary)",
            color: "var(--ags-yellow)",
            fontSize: 22,
            fontWeight: 700,
          }}
        >
          {initials}
        </div>
        <div style={{ fontSize: 16, fontWeight: 700, marginTop: 12 }}>{user.name}</div>
        <div style={{ fontSize: 12, color: "#64748b", marginTop: 2 }}>{user.loginName}</div>
        <div
          style={{
            display: "inline-block",
            marginTop: 10,
            padding: "4px 10px",
            borderRadius: 999,
            fontSize: 11,
            fontWeight: 600,
            background: "var(--ags-primary-soft)",
            color: "var(--ags-primary)",
          }}
        >
          {roleLabel}
        </div>
      </div>

      <div style={mobile.profileSection}>
        <div style={mobile.profileRow}>
          <span>{t("mobile.profileDept")}</span>
          <span style={{ fontWeight: 600 }}>{deptLabel}</span>
        </div>
        <div style={mobile.profileRow}>
          <span>{t("account.fieldCode")}</span>
          <span style={{ fontWeight: 600, fontFamily: "ui-monospace, monospace", fontSize: 12 }}>{user.code}</span>
        </div>
      </div>

      <Link
        to="/mobile/settings"
        style={{ ...mobile.btnPrimary, marginTop: 12, display: "block", textAlign: "center", textDecoration: "none" }}
      >
        {t("account.settingsTitle")}
      </Link>

      <button
        type="button"
        style={{ ...mobile.btnPrimary, marginTop: 8, background: "#64748b" }}
        onClick={() => void onLogout()}
      >
        {t("mobile.profileLogout")}
      </button>
    </MobileScreen>
  );
}
