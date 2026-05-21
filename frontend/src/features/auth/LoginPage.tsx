import { useEffect, useState, type CSSProperties, type FormEvent } from "react";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import CircularProgress from "@mui/material/CircularProgress";
import Stack from "@mui/material/Stack";
import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { agsTokens } from "@/components/theme/tokens";
import { useAuth } from "@/shared/auth/AuthContext";
import { getRoleHomePath } from "@/shared/auth/roles";
import type { AppRole } from "@/shared/auth/types";
import { ActionButton } from "@/shared/webChrome";

const DEV_QUICK_LOGINS: { role: AppRole; code: string }[] = [
  { role: "tbdh", code: "AGS0901" },
  { role: "shift_leader", code: "AGS0185" },
  { role: "sup", code: "AGS0184" },
  { role: "hr", code: "AGS0827" },
  { role: "staff", code: "AGS0138" },
];

const isDevLoginAssist = import.meta.env.DEV;
const devQuickPassword = import.meta.env.VITE_DEV_LOGIN_PASSWORD ?? "";

const loginCard: CSSProperties = {
  width: "min(400px, 92vw)",
  textAlign: "center",
  background: "#fff",
  borderRadius: 10,
  boxShadow: "0 12px 40px rgba(15, 23, 42, 0.22)",
  padding: "28px 32px",
};

const fieldStyle: CSSProperties = {
  width: "100%",
  padding: "10px 12px",
  border: "1px solid #cbd5e1",
  borderRadius: 6,
  fontSize: 14,
  fontFamily: "inherit",
  marginBottom: 12,
};

const labelStyle: CSSProperties = {
  display: "block",
  textAlign: "left",
  fontSize: 12,
  color: "#64748b",
};

export function LoginPage() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const location = useLocation();
  const { login, user, isAuthenticated, isLoading } = useAuth();
  const [loginName, setLoginName] = useState(isDevLoginAssist ? "AGS0901" : "");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const fromState = (location.state as { from?: string } | null)?.from;
  const showDevQuickLogin = isDevLoginAssist && devQuickPassword.length > 0;

  useEffect(() => {
    if (!isLoading && isAuthenticated && user) {
      navigate(fromState ?? getRoleHomePath(user.role, i18n.language), { replace: true });
    }
  }, [fromState, i18n.language, isAuthenticated, isLoading, user, navigate]);

  const submit = async (code: string, pass: string) => {
    setError(null);
    setSubmitting(true);
    try {
      const loggedIn = await login({ login: code, password: pass });
      const target = fromState ?? getRoleHomePath(loggedIn.role, i18n.language);
      navigate(target, { replace: true });
    } catch {
      setError(t("auth.loginFailed"));
    } finally {
      setSubmitting(false);
    }
  };

  const onSubmit = (event: FormEvent) => {
    event.preventDefault();
    void submit(loginName.trim(), password);
  };

  const quickLogin = (code: string) => {
    if (!showDevQuickLogin) {
      return;
    }

    setLoginName(code);
    void submit(code, devQuickPassword);
  };

  return (
    <Box
      sx={{
        minHeight: "100dvh",
        display: "grid",
        placeItems: "center",
        px: 2,
        pt: "max(1rem, env(safe-area-inset-top))",
        pb: "max(1rem, env(safe-area-inset-bottom))",
        background: `linear-gradient(145deg, #022654 0%, ${agsTokens.primary} 55%, #4d8bc9 100%)`,
      }}
    >
      <div style={loginCard}>
        <Box
          component="img"
          src="/AGS_Logo_Login.png"
          alt="AGS"
          sx={{
            display: "block",
            width: "min(280px, 88%)",
            maxWidth: "100%",
            height: "auto",
            mx: "auto",
            mb: 2,
            objectFit: "contain",
          }}
        />

        <form onSubmit={onSubmit}>
          <label style={labelStyle}>{t("auth.loginCode")}</label>
          <input
            value={loginName}
            onChange={(e) => setLoginName(e.target.value.toUpperCase())}
            autoComplete="username"
            disabled={submitting}
            style={fieldStyle}
          />
          <label style={labelStyle}>{t("auth.password")}</label>
          <div style={{ position: "relative", marginBottom: 12 }}>
            <input
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
              disabled={submitting}
              style={{ ...fieldStyle, marginBottom: 0, paddingRight: 40 }}
            />
            <button
              type="button"
              aria-label={showPassword ? t("auth.hidePassword") : t("auth.showPassword")}
              onClick={() => setShowPassword((v) => !v)}
              disabled={submitting}
              style={{
                position: "absolute",
                right: 10,
                top: "50%",
                transform: "translateY(-50%)",
                border: "none",
                background: "transparent",
                cursor: "pointer",
                color: "#64748b",
                padding: 4,
                lineHeight: 0,
              }}
            >
              {showPassword ? (
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
                  <path d="M17.9 17.9A10 10 0 0112 20c-5 0-9.3-3.1-11-7.5a11.6 11.6 0 013.2-4.8M9.9 9.9A3 3 0 0012 9c1.7 0 3 1.3 3 3 0 .4-.1.8-.2 1.1M3 3l18 18" strokeLinecap="round" />
                </svg>
              ) : (
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
                  <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z" strokeLinecap="round" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
              )}
            </button>
          </div>
          {error ? (
            <Alert severity="error" sx={{ mb: 1.5, textAlign: "left" }}>
              {error}
            </Alert>
          ) : null}
          <ActionButton
            type="submit"
            variant="primary"
            disabled={submitting}
            style={{ width: "100%", marginBottom: 16, display: "inline-flex", alignItems: "center", justifyContent: "center", gap: 8 }}
          >
            {submitting ? <CircularProgress size={16} sx={{ color: "inherit" }} /> : null}
            {submitting ? t("auth.loggingIn") : t("auth.login")}
          </ActionButton>
        </form>

        {showDevQuickLogin ? (
          <>
            <p style={{ fontSize: 11, color: "#94a3b8", margin: "0 0 8px" }}>{t("auth.devQuickLogin")}</p>
            <Stack spacing={1}>
              {DEV_QUICK_LOGINS.map(({ role, code }) => (
                <ActionButton
                  key={code}
                  disabled={submitting}
                  style={{ width: "100%" }}
                  onClick={() => quickLogin(code)}
                >
                  {t(
                    `auth.devRole${
                      role === "tbdh"
                        ? "Tbdh"
                        : role === "hr"
                          ? "Hr"
                          : role === "sup"
                            ? "Sup"
                            : "Staff"
                    }`,
                  )}{" "}
                  ({code})
                </ActionButton>
              ))}
            </Stack>
          </>
        ) : null}
      </div>
    </Box>
  );
}
