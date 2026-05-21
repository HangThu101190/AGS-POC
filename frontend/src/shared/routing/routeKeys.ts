/**
 * Khóa route (AppRouteKey) = camelCase của slug URL tiếng Việt.
 * Ví dụ: `/bang-phan-ca` → `dailyStaffing`, `/shift-board` (EN) → `dailyStaffing`.
 * UI i18n: `nav.dailyStaffing`, `dailyStaffing.*`.
 */

export const ROUTE_KEYS = {
  dailyStaffing: "dailyStaffing",
  phanCongSlot: "phanCongSlot",
} as const;
