import { themeQuartz } from "ag-grid-community";
import { agsTokens } from "@/components/theme/tokens";

/** AGS-branded Quartz theme (Theming API — use with `theme={agsGridTheme}`). */
export const agsGridTheme = themeQuartz.withParams({
  accentColor: agsTokens.primary,
  borderColor: agsTokens.border,
  columnBorder: { width: 1, color: agsTokens.border },
  headerColumnBorder: { width: 1, color: agsTokens.border },
  headerColumnBorderHeight: "100%",
  headerColumnResizeHandleColor: "transparent",
  headerColumnResizeHandleWidth: 6,
  headerColumnResizeHandleHeight: "100%",
  backgroundColor: agsTokens.surface,
  headerBackgroundColor: "#f8fafc",
  headerTextColor: agsTokens.muted,
  foregroundColor: agsTokens.textBody,
  fontFamily: ["Segoe UI", "system-ui", "-apple-system", "sans-serif"],
  fontSize: 14,
  headerFontSize: 12,
  headerFontWeight: 700,
  rowHeight: 42,
  headerHeight: 44,
  borderRadius: agsTokens.radius,
  wrapperBorderRadius: agsTokens.radius,
  oddRowBackgroundColor: "#fafbfc",
  rowHoverColor: "rgba(3, 59, 125, 0.06)",
  selectedRowBackgroundColor: agsTokens.primarySoft,
});
