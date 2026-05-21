/** Department codes → display labels (fallback when i18n missing). */
export const DEPT_DISPLAY: Record<string, string> = {
  PVHK_DI: "Phục vụ hành khách — Đi",
  PVHK_DEN: "Phục vụ hành khách — Đến",
  RAMP: "Phục vụ sân đỗ",
  BAGGAGE: "Hành lý",
  HCNS: "Hành chính nhân sự",
};

export const PLANNING_DEPT_CODES = ["PVHK_DI", "PVHK_DEN", "RAMP", "BAGGAGE"] as const;

export const ALL_DEPT_CODES = [...PLANNING_DEPT_CODES, "HCNS"] as const;
