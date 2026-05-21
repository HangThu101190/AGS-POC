import { createTheme } from "@mui/material/styles";
import { agsBreakpoints } from "./breakpoints";
import { agsTokens } from "./tokens";

export const agsTheme = createTheme({
  breakpoints: {
    values: {
      xs: agsBreakpoints.xs,
      sm: agsBreakpoints.sm,
      md: agsBreakpoints.md,
      lg: agsBreakpoints.lg,
      xl: 1536,
    },
  },
  palette: {
    primary: {
      main: agsTokens.primary,
      dark: agsTokens.primaryHover,
      contrastText: agsTokens.primaryOn,
    },
    secondary: {
      main: agsTokens.yellow,
      contrastText: agsTokens.navy,
    },
    success: { main: agsTokens.success },
    error: { main: agsTokens.danger },
    warning: {
      main: "#ea580c",
      dark: "#9a3412",
      light: "#fff7ed",
      contrastText: "#ffffff",
    },
    text: {
      primary: agsTokens.textBody,
      secondary: agsTokens.muted,
    },
    background: {
      default: agsTokens.bg,
      paper: agsTokens.surface,
    },
    divider: agsTokens.border,
  },
  typography: {
    fontFamily: agsTokens.fontFamily,
    fontSize: 14,
    htmlFontSize: 16,
    h4: {
      fontSize: agsTokens.fontSizePageTitle,
      fontWeight: 600,
      lineHeight: 1.3,
      color: agsTokens.primary,
    },
    h5: {
      fontSize: "1.125rem",
      fontWeight: 600,
      lineHeight: 1.35,
      color: agsTokens.primary,
    },
    h6: {
      fontSize: "1rem",
      fontWeight: 600,
      lineHeight: 1.4,
    },
    subtitle1: {
      fontSize: "0.9375rem",
      fontWeight: 500,
      lineHeight: 1.5,
    },
    subtitle2: {
      fontSize: agsTokens.fontSizeBase,
      fontWeight: 600,
      lineHeight: 1.43,
    },
    body1: {
      fontSize: agsTokens.fontSizeBase,
      lineHeight: 1.5,
    },
    body2: {
      fontSize: "0.8125rem",
      lineHeight: 1.45,
    },
    caption: {
      fontSize: agsTokens.fontSizeCaption,
      lineHeight: 1.4,
      color: agsTokens.muted,
    },
    overline: {
      fontSize: agsTokens.fontSizeCardTitle,
      fontWeight: 600,
      lineHeight: 1.6,
      letterSpacing: "0.06em",
      textTransform: "uppercase",
      color: agsTokens.muted,
    },
    button: {
      fontSize: agsTokens.fontSizeBase,
      fontWeight: 500,
      textTransform: "none",
    },
  },
  shape: {
    borderRadius: agsTokens.radius,
  },
  components: {
    MuiButton: {
      defaultProps: { disableElevation: true, size: "medium" },
      styleOverrides: {
        root: ({ ownerState }) => ({
          textTransform: "none",
          fontWeight: ownerState.variant === "contained" ? 600 : 500,
          borderRadius: agsTokens.radius,
          transition:
            "background-color 0.15s ease, border-color 0.15s ease, color 0.15s ease, box-shadow 0.15s ease",
          "&:focus-visible": {
            outline: `2px solid ${agsTokens.primary}`,
            outlineOffset: 2,
          },
          "&.Mui-disabled": {
            opacity: 1,
            ...(ownerState.variant === "contained" && {
              backgroundColor: "#cbd5e1",
              color: "#f8fafc",
              borderColor: "transparent",
            }),
            ...(ownerState.variant === "outlined" && {
              color: "#94a3b8",
              borderColor: "#e2e8f0",
              backgroundColor: "#f1f5f9",
            }),
          },
          ...(ownerState.variant === "contained" &&
            ownerState.color === "primary" && {
              "&:hover:not(.Mui-disabled)": { backgroundColor: agsTokens.primaryHover },
            }),
          ...(ownerState.variant === "contained" &&
            ownerState.color === "warning" && {
              "&:hover:not(.Mui-disabled)": { backgroundColor: "#c2410c" },
            }),
          ...(ownerState.variant === "outlined" &&
            ownerState.color === "inherit" && {
              color: agsTokens.textBody,
              borderColor: agsTokens.border,
              backgroundColor: agsTokens.surface,
              "&:hover:not(.Mui-disabled)": {
                borderColor: agsTokens.primary,
                backgroundColor: agsTokens.primarySoft,
                color: agsTokens.primary,
              },
            }),
          ...(ownerState.variant === "outlined" &&
            ownerState.color === "primary" && {
              "&:hover:not(.Mui-disabled)": {
                backgroundColor: agsTokens.primarySoft,
                borderColor: agsTokens.primary,
              },
            }),
          ...(ownerState.variant === "outlined" &&
            ownerState.color === "error" && {
              "&:hover:not(.Mui-disabled)": {
                backgroundColor: "#fef2f2",
                borderColor: agsTokens.danger,
              },
            }),
        }),
        sizeMedium: { fontSize: agsTokens.fontSizeBase, padding: "6px 16px" },
        sizeSmall: { fontSize: agsTokens.fontSizeCaption, padding: "3px 10px", minWidth: 0 },
      },
    },
    MuiDialogActions: {
      styleOverrides: {
        root: {
          padding: "12px 16px 16px",
          gap: 8,
        },
      },
    },
    MuiCard: {
      defaultProps: { variant: "outlined" },
      styleOverrides: {
        root: {
          borderRadius: agsTokens.radiusLg,
          borderColor: agsTokens.border,
        },
      },
    },
    MuiAlert: {
      styleOverrides: {
        root: { fontSize: agsTokens.fontSizeBase },
      },
    },
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          backgroundColor: agsTokens.bg,
          fontSize: agsTokens.fontSizeBase,
        },
      },
    },
  },
});
