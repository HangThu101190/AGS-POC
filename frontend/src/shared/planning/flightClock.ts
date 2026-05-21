/** Excel / API time → HH:mm for display (0840 → 08:40). */
export function formatFlightTimeDisplay(raw: string | null | undefined): string {
  const s = raw?.trim() ?? "";
  if (!s) return "";
  if (/^\d{4}$/.test(s)) return `${s.slice(0, 2)}:${s.slice(2, 4)}`;
  return s;
}

function normalizeClockInput(raw: string): string {
  const s = raw.trim();
  if (!s) return "";
  const hasPlus = s.includes("+");
  const core = hasPlus ? s.replace(/\+/g, "").trim() : s;
  if (/^\d{4}$/.test(core)) {
    return `${core.slice(0, 2)}:${core.slice(2, 4)}${hasPlus ? "+" : ""}`;
  }
  return s;
}

/** STA/STD + delay minutes → HH:mm (matches backend Flight.AddMinutesToClock). */
export function addMinutesToClock(hhmm: string, minutes: number): string {
  const raw = normalizeClockInput(hhmm);
  if (!raw) return "";
  const parts = raw.replace("+", "").split(":");
  if (parts.length < 2) return raw;
  const h = Number.parseInt(parts[0] ?? "", 10);
  const m = Number.parseInt(parts[1] ?? "", 10);
  if (Number.isNaN(h) || Number.isNaN(m)) return raw;

  const total = h * 60 + m + minutes;
  const nh = Math.floor(total / 60) % 24;
  const nm = total % 60;
  const suffix = raw.includes("+") || total >= 24 * 60 ? "+" : "";
  return `${String(nh).padStart(2, "0")}:${String(nm).padStart(2, "0")}${suffix}`;
}

export function flightDelayedClock(
  scheduled: string | null | undefined,
  delayMinutes: number | undefined,
): string {
  const sched = formatFlightTimeDisplay(scheduled);
  const delay = delayMinutes ?? 0;
  if (!sched) return "";
  if (delay > 0) return addMinutesToClock(sched, delay);
  return sched;
}

/** Fields used to resolve ETA/ETD delay display (list + PATCH). */
export type FlightDelayFields = {
  sta?: string;
  std?: string;
  eta?: string | null;
  etd?: string | null;
  etaDelayMinutes?: number;
  etdDelayMinutes?: number;
  delayMinutes?: number;
  isDelayed?: boolean;
};

/** Split ETA/ETD delay; falls back to legacy `delayMinutes` when API omits split fields. */
export function resolveFlightDelayMinutes(
  f: FlightDelayFields,
  kind: "eta" | "etd",
): number {
  if (kind === "eta") {
    const eta = f.etaDelayMinutes;
    if (eta != null && eta > 0) return eta;
    const etd = f.etdDelayMinutes ?? 0;
    if (etd > 0) return eta ?? 0;
    const legacy = f.delayMinutes ?? 0;
    if (legacy > 0 && (f.isDelayed || etd === 0)) return legacy;
    return eta ?? 0;
  }
  const etd = f.etdDelayMinutes;
  if (etd != null && etd > 0) return etd;
  return 0;
}

/** Display time for ETA/ETD column: API `eta`/`etd` first, else STA/STD + delay minutes. */
export function flightEtaEtdDisplayTime(
  f: FlightDelayFields,
  kind: "eta" | "etd",
): string {
  const stored = kind === "eta" ? f.eta : f.etd;
  if (stored?.trim()) return formatFlightTimeDisplay(stored);
  const sched = kind === "eta" ? f.sta : f.std;
  const delay = resolveFlightDelayMinutes(f, kind);
  if (delay <= 0 || !sched?.trim()) return "";
  return flightDelayedClock(sched, delay);
}
