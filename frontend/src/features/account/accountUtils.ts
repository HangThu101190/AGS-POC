import type { AppRole } from "@/shared/auth/types";

export function roleLabelKey(role: string): string {
  switch (role) {
    case "hr":
      return "roles.hr";
    case "sup":
      return "roles.sup";
    case "staff":
      return "roles.staff";
    case "tbdh":
      return "roles.tbdh";
    case "shift_leader":
      return "roles.shiftLeader";
    default:
      return "roles.staff";
  }
}

export function getUserInitials(name: string, code: string): string {
  const trimmed = name.trim();
  if (trimmed.includes(" ")) {
    const parts = trimmed.split(/\s+/).filter(Boolean);
    const first = parts[0]?.[0] ?? "";
    const last = parts[parts.length - 1]?.[0] ?? "";
    return `${first}${last}`.toUpperCase() || code.slice(-2).toUpperCase();
  }
  return (trimmed.slice(-2) || code.slice(-2)).toUpperCase();
}

export function roleBadgeClass(role: AppRole): string {
  switch (role) {
    case "hr":
      return "roleHr";
    case "sup":
      return "roleSup";
    case "tbdh":
      return "roleTbdh";
    case "shift_leader":
      return "roleSup";
    default:
      return "roleStaff";
  }
}
