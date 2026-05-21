import type { AppRouteKey } from "@/shared/auth/roles";

export type NavIconKey =
  | "dashboard"
  | "flights"
  | "dailyStaffing"
  | "phanCongSlot"
  | "monitoring"
  | "reconcile"
  | "people"
  | "config"
  | "audit";

export type NavItemConfig = {
  key: AppRouteKey;
  end?: boolean;
  icon: NavIconKey;
  star?: boolean;
};

/** Sidebar items — SVG icons via `NavSidebarIcon`. */
export const NAV_ITEMS: NavItemConfig[] = [
  { key: "dashboard", end: true, icon: "dashboard" },
  { key: "flights", icon: "flights" },
  { key: "dailyStaffing", icon: "dailyStaffing", star: true },
  { key: "phanCongSlot", icon: "phanCongSlot", star: true },
  { key: "reconcile", icon: "reconcile", star: true },
  { key: "monitoring", icon: "monitoring", star: true },
  { key: "people", icon: "people" },
  { key: "config", icon: "config" },
  { key: "audit", icon: "audit" },
];
