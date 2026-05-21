/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_API_URL?: string;
  readonly VITE_API_BASE_URL?: string;
  readonly VITE_HUB_URL?: string;
  readonly VITE_SIGNALR_ENABLED?: string;
  /** Local dev only — enables quick-login buttons (never set in production builds). */
  readonly VITE_DEV_LOGIN_PASSWORD?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
