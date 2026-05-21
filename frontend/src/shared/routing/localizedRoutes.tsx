import type { ReactNode } from "react";
import type { RouteObject } from "react-router-dom";
import type { AppRouteKey } from "@/shared/auth/roles";
import { ROUTE_PATH_BY_KEY } from "@/shared/routing/routePaths";

/** Register both EN and VI URL segments for the same page component. */
export function localizedChildRoutes(
  key: AppRouteKey,
  element: ReactNode,
): RouteObject[] {
  if (key === "dashboard") {
    return [{ index: true, element }];
  }

  const paths = ROUTE_PATH_BY_KEY[key];
  const segments = new Set<string>();
  for (const p of [paths.en, paths.vi]) {
    const seg = p.replace(/^\//, "");
    if (seg) segments.add(seg);
  }

  return [...segments].map((path) => ({ path, element }));
}
