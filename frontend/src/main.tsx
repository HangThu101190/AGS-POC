import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { App } from "@/app/App";
import { ErrorBoundary } from "@/shared/ui/ErrorBoundary";
import "@/app/styles/global.css";
import "@/lib/agGrid/setup";
import "@/shared/i18n";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ErrorBoundary>
      <App />
    </ErrorBoundary>
  </StrictMode>,
);
