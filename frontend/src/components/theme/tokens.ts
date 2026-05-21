import { agsBreakpointCss } from "./breakpoints";

/**
 * AGS brand tokens — aligned with prototype `AGS_BRAND` in `source-code/combined/AGS_Demo.jsx`.
 */
export const agsTokens = {
  navy: "#031b4e",
  primary: "#033b7d",
  primaryHover: "#024a9a",
  primarySoft: "rgba(3, 59, 125, 0.1)",
  primaryOn: "#ffffff",

  yellow: "#f1c662",
  yellowMutedBg: "#faf6eb",
  yellowMutedBorder: "#f1c66299",
  yellowText: "#5c4a14",

  bg: "#f8fafc",
  surface: "#ffffff",
  border: "#e2e8f0",
  grayLight: "#e9e9e9",
  textBody: "#0f172a",
  textMuted: "#64748b",
  muted: "#64748b",

  success: "#15803d",
  danger: "#dc2626",

  fontFamily: '"Segoe UI", ui-sans-serif, system-ui, -apple-system, sans-serif',

  fontSizeBase: "0.875rem",
  fontSizeNav: "0.8125rem",
  fontSizeBody: "0.875rem",
  fontSizeCaption: "0.75rem",
  fontSizePageTitle: "1.625rem",
  fontSizeCardTitle: "0.75rem",

  radius: 8,
  radiusLg: 12,
} as const;

export const agsRadiusCss = {
  "--ags-radius": `${agsTokens.radius}px`,
} as const;

export const agsCssVars = {
  ...agsBreakpointCss,
  "--ags-navy": agsTokens.navy,
  "--ags-primary": agsTokens.primary,
  "--ags-primary-hover": agsTokens.primaryHover,
  "--ags-primary-soft": agsTokens.primarySoft,
  "--ags-primary-on": agsTokens.primaryOn,
  "--ags-yellow": agsTokens.yellow,
  "--ags-yellow-muted-bg": agsTokens.yellowMutedBg,
  "--ags-yellow-text": agsTokens.yellowText,
  "--ags-bg": agsTokens.bg,
  "--ags-sidebar": agsTokens.primary,
  "--ags-surface": agsTokens.surface,
  "--ags-tab-inactive": "#eef2f6",
  "--ags-border": agsTokens.border,
  "--ags-text": agsTokens.navy,
  "--ags-text-body": agsTokens.textBody,
  "--ags-muted": agsTokens.muted,
  "--ags-accent": agsTokens.yellow,
  "--ags-success": agsTokens.success,
  "--ags-danger": agsTokens.danger,
  "--ags-font-family": agsTokens.fontFamily,
  "--ags-font-size-nav": agsTokens.fontSizeNav,
  "--ags-font-size-body": agsTokens.fontSizeBody,
  "--ags-font-size-caption": agsTokens.fontSizeCaption,
  "--ags-font-size-page-title": agsTokens.fontSizePageTitle,
} as const;
