import Tab from "@mui/material/Tab";
import Tabs from "@mui/material/Tabs";
import type { ReactNode, SyntheticEvent } from "react";
import styles from "./pageTabs.module.css";

export type PageTabItem<T extends string | number = string> = {
  value: T;
  label: ReactNode;
  disabled?: boolean;
  /** Optional count or short label shown beside the tab text */
  badge?: ReactNode;
};

export type PageTabsProps<T extends string | number = string> = {
  value: T;
  onChange: (value: T) => void;
  items: readonly PageTabItem<T>[];
  /** Accessible name for the tab list */
  ariaLabel?: string;
  /** Evenly stretch tabs across the row (2–4 short labels) */
  fullWidth?: boolean;
  /** Horizontal scroll when labels overflow (default true unless fullWidth) */
  scrollable?: boolean;
  className?: string;
  /**
   * Panel content — renders below tabs with folder-style border (active tab connects to panel).
   * Omit for tab bar only (legacy).
   */
  children?: ReactNode;
  /** Panel inner padding: `flush` (16px) for tables/cards, `padded` (20px) for forms */
  panelPadding?: "flush" | "padded";
  /** Let layout + panel fill remaining vertical space (full-height pages) */
  grow?: boolean;
  /** `scroll` = panel scrolls; `fit` = panel clips and child fills (maps) */
  panelMode?: "scroll" | "fit";
};

function tabLabelContent(label: ReactNode, badge?: ReactNode) {
  if (badge == null) return label;
  return (
    <span className={styles.tabLabel}>
      <span>{label}</span>
      <span className={styles.badge}>{badge}</span>
    </span>
  );
}

function PageTabsBar<T extends string | number>({
  value,
  onChange,
  items,
  ariaLabel,
  fullWidth,
  scrollable,
  className,
}: Omit<PageTabsProps<T>, "children" | "panelPadding" | "grow" | "panelMode">) {
  const allowScroll = scrollable ?? !fullWidth;

  const handleChange = (_: SyntheticEvent, next: string | number) => {
    onChange(next as T);
  };

  const rootClass = [styles.root, fullWidth ? styles.rootFullWidth : "", className]
    .filter(Boolean)
    .join(" ");

  const tabsClass = [styles.tabs, fullWidth ? styles.tabsFullWidth : ""].filter(Boolean).join(" ");

  return (
    <div className={styles.tabRow}>
      <div className={rootClass}>
        <Tabs
          value={value}
          onChange={handleChange}
          variant={fullWidth ? "fullWidth" : "standard"}
          scrollButtons={allowScroll ? "auto" : false}
          allowScrollButtonsMobile={allowScroll}
          className={tabsClass}
          aria-label={ariaLabel}
          sx={{
            minHeight: 0,
            overflow: "visible",
            "& .MuiTabs-scroller": { overflow: "visible !important" },
            "& .MuiTabs-list": { minHeight: 0, alignItems: "flex-end" },
            "& .MuiTabs-indicator": { display: "none" },
          }}
        >
          {items.map((item) => (
            <Tab
              key={String(item.value)}
              value={item.value}
              label={tabLabelContent(item.label, item.badge)}
              disabled={item.disabled}
              disableRipple
            />
          ))}
        </Tabs>
      </div>
    </div>
  );
}

/** Folder-style page tabs — active tab connects to the content panel (AGS palette) */
export function PageTabs<T extends string | number = string>({
  children,
  panelPadding = "flush",
  grow = false,
  panelMode = "scroll",
  ...barProps
}: PageTabsProps<T>) {
  const bar = <PageTabsBar {...barProps} />;

  if (children == null) {
    return bar;
  }

  const layoutClass = [styles.layout, grow ? styles.layoutGrow : ""].filter(Boolean).join(" ");

  const panelClass = [
    styles.panel,
    panelPadding === "padded" ? styles.panelPadded : styles.panelFlush,
    grow ? styles.panelGrow : "",
  ]
    .filter(Boolean)
    .join(" ");

  const bodyClass = [
    styles.panelBody,
    panelMode === "fit" ? styles.panelBodyFit : styles.panelBodyScroll,
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={layoutClass} {...(grow ? { "data-page-tabs-grow": true } : {})}>
      {bar}
      <div className={panelClass} role="tabpanel">
        <div className={bodyClass}>{children}</div>
      </div>
    </div>
  );
}
