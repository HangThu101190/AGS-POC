import { useState } from "react";
import { useTranslation } from "react-i18next";
import Popover from "@mui/material/Popover";
import Tooltip from "@mui/material/Tooltip";
import { useIsMdUp } from "@/shared/hooks/useMediaQuery";
import styles from "./sidebarQuickGuide.module.css";

function HelpIcon() {
  return (
    <svg
      className={styles.icon}
      width={20}
      height={20}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.75}
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <circle cx="12" cy="12" r="10" />
      <path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3" />
      <line x1="12" y1="17" x2="12.01" y2="17" />
    </svg>
  );
}

export function SidebarQuickGuide() {
  const { t } = useTranslation();
  const isMdUp = useIsMdUp();
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const [hoverTip, setHoverTip] = useState(false);
  const open = Boolean(anchor);

  const steps = [
    t("dashboard.d1"),
    t("dashboard.d2"),
    t("dashboard.d3"),
    t("dashboard.d4"),
    t("dashboard.d5"),
  ];

  const close = () => setAnchor(null);

  return (
    <div className={styles.footer}>
      <Tooltip
        title={t("nav.quickGuideHint")}
        placement={isMdUp ? "right" : "top"}
        arrow
        open={hoverTip && !open}
        onOpen={() => setHoverTip(true)}
        onClose={() => setHoverTip(false)}
        disableHoverListener={open}
        disableFocusListener
        disableTouchListener={open}
        enterDelay={400}
        slotProps={{ popper: { sx: { zIndex: 1200 } } }}
      >
        <button
          type="button"
          className={[styles.btn, open ? styles.btnOpen : ""].filter(Boolean).join(" ")}
          aria-expanded={open}
          aria-haspopup="dialog"
          aria-label={t("dashboard.quickGuide")}
          onClick={(e) => {
            setHoverTip(false);
            setAnchor(open ? null : e.currentTarget);
          }}
        >
          <HelpIcon />
          <span className={styles.label}>{t("dashboard.quickGuide")}</span>
        </button>
      </Tooltip>

      <Popover
        open={open}
        anchorEl={anchor}
        onClose={close}
        anchorOrigin={{
          vertical: isMdUp ? "center" : "top",
          horizontal: isMdUp ? "right" : "center",
        }}
        transformOrigin={{
          vertical: isMdUp ? "center" : "bottom",
          horizontal: isMdUp ? "left" : "center",
        }}
        slotProps={{
          root: { sx: { zIndex: 1600 } },
          paper: {
            sx: {
              bgcolor: "transparent",
              boxShadow: "none",
              overflow: "visible",
              ml: isMdUp ? 1 : 0,
              zIndex: 1600,
            },
          },
        }}
      >
        <div className={styles.panel} role="dialog" aria-label={t("dashboard.quickGuide")}>
          <div className={styles.panelHeader}>
            <p className={styles.panelTitle}>{t("dashboard.quickGuide")}</p>
            <button type="button" className={styles.closeBtn} onClick={close} aria-label={t("common.cancel")}>
              ×
            </button>
          </div>
          <ol className={styles.list}>
            {steps.map((step) => (
              <li key={step}>{step}</li>
            ))}
          </ol>
        </div>
      </Popover>
    </div>
  );
}
