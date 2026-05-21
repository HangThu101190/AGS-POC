import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";
import { useAuth } from "@/shared/auth/AuthContext";
import { getRoleHomePath } from "@/shared/auth/roles";

export function ForbiddenPage() {
  const { t, i18n } = useTranslation();
  const { user } = useAuth();
  const home = user ? getRoleHomePath(user.role, i18n.language) : "/";

  return (
    <div style={{ padding: "2rem 1rem", maxWidth: 480 }}>
      <h1 style={{ margin: "0 0 8px", fontSize: 20 }}>{t("errors.forbiddenTitle")}</h1>
      <p style={{ margin: "0 0 16px", color: "#64748b", fontSize: 14 }}>
        {t("errors.forbiddenBody")}
      </p>
      <Link to={home} style={{ fontSize: 14, fontWeight: 600 }}>
        {t("errors.backHome")}
      </Link>
    </div>
  );
}
