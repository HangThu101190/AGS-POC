import type { ReactNode } from "react";
import CssBaseline from "@mui/material/CssBaseline";
import GlobalStyles from "@mui/material/GlobalStyles";
import { ThemeProvider } from "@mui/material/styles";
import { agsTheme } from "./agsTheme";
import { agsCssVars, agsRadiusCss } from "./tokens";

export function UiThemeProvider({ children }: { children: ReactNode }) {
  return (
    <ThemeProvider theme={agsTheme}>
      <CssBaseline />
      <GlobalStyles styles={{ ":root": { ...agsCssVars, ...agsRadiusCss } }} />
      {children}
    </ThemeProvider>
  );
}
