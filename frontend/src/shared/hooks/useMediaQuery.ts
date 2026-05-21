import { useEffect, useState } from "react";

/** Match CSS breakpoints in `components/theme/breakpoints.ts`. */
export const mediaQueries = {
  sm: "(min-width: 600px)",
  md: "(min-width: 768px)",
  lg: "(min-width: 1024px)",
} as const;

export function useMediaQuery(query: string): boolean {
  const [matches, setMatches] = useState(() => {
    if (typeof window === "undefined") return false;
    return window.matchMedia(query).matches;
  });

  useEffect(() => {
    const mq = window.matchMedia(query);
    const onChange = () => setMatches(mq.matches);
    mq.addEventListener("change", onChange);
    onChange();
    return () => mq.removeEventListener("change", onChange);
  }, [query]);

  return matches;
}

export function useIsMdUp() {
  return useMediaQuery(mediaQueries.md);
}

export function useResponsiveTableHeight(mobile: number, desktop: number) {
  const isMd = useIsMdUp();
  return isMd ? desktop : mobile;
}

/** Viewport minus app chrome (topbar + content padding). Use on DataTable `height`. */
export const TABLE_VIEWPORT_HEIGHT =
  "calc(100dvh - var(--ags-shell-header-height) - 2 * var(--ags-content-py, 16px))";

/** Full-page table area below a toolbar (~72px) and optional banners (~48px each). */
export function useFullPageTableHeight(toolbarOffsetPx = 88, bannerCount = 0) {
  const bannerOffset = bannerCount * 48;
  return `calc(100dvh - var(--ags-shell-header-height) - 2 * var(--ags-content-py, 16px) - ${toolbarOffsetPx + bannerOffset}px)`;
}
