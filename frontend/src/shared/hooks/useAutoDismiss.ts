import { useEffect } from "react";

/** Clears a string state after `ms` (toast-style banners). */
export function useAutoDismiss(
  value: string | null | undefined,
  onClear: () => void,
  ms = 5000,
) {
  useEffect(() => {
    if (!value) return;
    const timer = window.setTimeout(onClear, ms);
    return () => window.clearTimeout(timer);
  }, [value, onClear, ms]);
}
