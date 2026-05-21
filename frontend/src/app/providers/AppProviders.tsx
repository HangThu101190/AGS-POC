import type { ReactNode } from "react";
import { UiThemeProvider } from "@/components/theme/UiThemeProvider";
import { AuthProvider } from "@/shared/auth/AuthContext";
import { I18nProvider } from "./I18nProvider";
import { QueryProvider } from "./QueryProvider";

export function AppProviders({ children }: { children: ReactNode }) {
  return (
    <UiThemeProvider>
      <I18nProvider>
        <QueryProvider>
          <AuthProvider>{children}</AuthProvider>
        </QueryProvider>
      </I18nProvider>
    </UiThemeProvider>
  );
}
