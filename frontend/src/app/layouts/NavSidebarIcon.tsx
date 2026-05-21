import type { NavIconKey } from "./navConfig";

type NavSidebarIconProps = {
  name: NavIconKey;
  className?: string;
};

const common = {
  width: 20,
  height: 20,
  viewBox: "0 0 24 24",
  fill: "none",
  stroke: "currentColor",
  strokeWidth: 1.75,
  strokeLinecap: "round" as const,
  strokeLinejoin: "round" as const,
  "aria-hidden": true,
};

export function NavSidebarIcon({ name, className }: NavSidebarIconProps) {
  switch (name) {
    case "dashboard":
      return (
        <svg className={className} {...common}>
          <rect x="3" y="3" width="8" height="8" rx="1" />
          <rect x="13" y="3" width="8" height="8" rx="1" />
          <rect x="3" y="13" width="8" height="8" rx="1" />
          <rect x="13" y="13" width="8" height="8" rx="1" />
        </svg>
      );
    case "flights":
      return (
        <svg className={className} {...common}>
          <path d="M2 16l20-6-8 4 2 8-4-4-10 4z" />
        </svg>
      );
    case "dailyStaffing":
      return (
        <svg className={className} {...common}>
          <rect x="3" y="4" width="18" height="18" rx="2" />
          <path d="M16 2v4M8 2v4M3 10h18" />
        </svg>
      );
    case "phanCongSlot":
      return (
        <svg className={className} {...common}>
          <circle cx="9" cy="7" r="3" />
          <circle cx="17" cy="9" r="2.5" />
          <path d="M3 20c0-3.3 2.7-6 6-6s6 2.7 6 6" />
          <path d="M14 14c2.2 0 4 1.8 4 4" />
        </svg>
      );
    case "monitoring":
      return (
        <svg className={className} {...common}>
          <path d="M12 21s7-4.5 7-11a7 7 0 10-14 0c0 6.5 7 11 7 11z" />
          <circle cx="12" cy="10" r="2.5" />
        </svg>
      );
    case "reconcile":
      return (
        <svg className={className} {...common}>
          <path d="M9 11l3 3 8-8" />
          <path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11" />
        </svg>
      );
    case "people":
      return (
        <svg className={className} {...common}>
          <circle cx="9" cy="7" r="3.5" />
          <path d="M3 20c0-3.9 2.7-7 6-7s6 3.1 6 7" />
          <path d="M16 11h5M18.5 8.5v5" />
        </svg>
      );
    case "config":
      return (
        <svg className={className} {...common}>
          <circle cx="12" cy="12" r="3" />
          <path d="M12 1v2M12 21v2M4.2 4.2l1.4 1.4M18.4 18.4l1.4 1.4M1 12h2M21 12h2M4.2 19.8l1.4-1.4M18.4 5.6l1.4-1.4" />
        </svg>
      );
    case "audit":
      return (
        <svg className={className} {...common}>
          <circle cx="12" cy="12" r="9" />
          <path d="M12 7v5l3 2" />
        </svg>
      );
    default:
      return null;
  }
}
