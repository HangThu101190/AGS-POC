import type { AppRouteKey } from "@/shared/auth/roles";

export type AppLang = "vi" | "en";

/** Route keys = camelCase của slug tiếng Việt; vi/en dùng chung slug (không path tiếng Anh riêng). */
export const ROUTE_PATH_BY_KEY: Record<AppRouteKey, Record<AppLang, string>> = {
  dashboard: { vi: "/", en: "/" },
  flights: { vi: "/lich-bay", en: "/flights" },
  dailyStaffing: { vi: "/bang-phan-ca", en: "/shift-board" },
  phanCongSlot: { vi: "/phan-cong-slot", en: "/phan-cong-slot" },
  reconcile: { vi: "/doi-soat", en: "/reconcile" },
  monitoring: { vi: "/giam-sat", en: "/monitoring" },
  people: { vi: "/nhan-su", en: "/people" },
  config: { vi: "/cau-hinh", en: "/config" },
  audit: { vi: "/nhat-ky-he-thong", en: "/audit" },
  profile: { vi: "/ho-so", en: "/profile" },
  settings: { vi: "/cai-dat", en: "/settings" },
};

const PATH_TO_KEY = new Map<string, AppRouteKey>();
for (const [key, paths] of Object.entries(ROUTE_PATH_BY_KEY) as [
  AppRouteKey,
  Record<AppLang, string>,
][]) {
  PATH_TO_KEY.set(paths.vi, key);
  PATH_TO_KEY.set(paths.en, key);
  const seg = paths.vi.replace(/^\//, "");
  if (seg) PATH_TO_KEY.set(seg, key);
  const segEn = paths.en.replace(/^\//, "");
  if (segEn && segEn !== seg) PATH_TO_KEY.set(segEn, key);
}

const LEGACY_PATH_ALIASES: Record<string, AppRouteKey> = {
  "/lap-ke-hoach-ca": "dailyStaffing",
  "lap-ke-hoach-ca": "dailyStaffing",
  "/planner": "dailyStaffing",
  planner: "dailyStaffing",
  "/bang-phan-ca": "dailyStaffing",
  "bang-phan-ca": "dailyStaffing",
  "/supboard": "phanCongSlot",
  supboard: "phanCongSlot",
};

for (const [path, key] of Object.entries(LEGACY_PATH_ALIASES)) {
  PATH_TO_KEY.set(path, key);
}

export function appLangFromI18n(language: string | undefined): AppLang {
  return language?.startsWith("vi") ? "vi" : "en";
}

export function routePathFor(key: AppRouteKey, lang: AppLang | string): string {
  const l = typeof lang === "string" ? appLangFromI18n(lang) : lang;
  return ROUTE_PATH_BY_KEY[key][l];
}

export function routeKeyFromPathname(pathname: string): AppRouteKey | null {
  if (pathname === "/mobile" || pathname.startsWith("/mobile/")) {
    const segment = pathname.replace(/^\/mobile\/?/, "").split("/")[0] || "today";
    const mobileMap: Record<string, AppRouteKey> = {
      plan: "dailyStaffing",
      assign: "phanCongSlot",
      flights: "phanCongSlot",
      team: "phanCongSlot",
    };
    return mobileMap[segment] ?? null;
  }

  const normalized = pathname.replace(/\/$/, "") || "/";
  const direct = PATH_TO_KEY.get(normalized);
  if (direct) return direct;

  const segment = normalized.replace(/^\//, "").split("/")[0] ?? "";
  return PATH_TO_KEY.get(segment) ?? null;
}
