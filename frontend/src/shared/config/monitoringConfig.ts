/** Monitoring intervals — default GPS jitter 60×30s = 30 minutes. */

const parseSec = (raw: string | undefined, fallback: number) => {
  const n = Number(raw);
  return Number.isFinite(n) && n > 0 ? n : fallback;
};

/** REST poll when SignalR off (seconds). */
export const MONITORING_REST_POLL_MS =
  parseSec(import.meta.env.VITE_MONITORING_POLL_SEC, 30) * 1000;

/** Client hint for GPS jitter (server uses appsettings Monitoring:GpsJitterIntervalSeconds). */
export const MONITORING_GPS_TICK_MS =
  parseSec(import.meta.env.VITE_MONITORING_GPS_TICK_SEC, 30) * 1000;

export const MONITORING_SIGNALR_POLL_HINT_SEC = parseSec(
  import.meta.env.VITE_MONITORING_SIGNALR_POLL_SEC,
  30,
);
