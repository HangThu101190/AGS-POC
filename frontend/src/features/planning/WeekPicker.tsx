import {
  useCallback,
  useEffect,
  useId,
  useLayoutEffect,
  useMemo,
  useRef,
  useState,
  type CSSProperties,
  type MouseEvent as ReactMouseEvent,
} from "react";
import { createPortal } from "react-dom";
import { useTranslation } from "react-i18next";
import {
  addWeeks,
  buildWeekMeta,
  currentWeekId,
  listWeekOptions,
  type WeekKind,
  type WeekMeta,
} from "@/shared/planning/weekCalendar";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import styles from "./weekPicker.module.css";

type WeekPickerProps = {
  /** Mobile shell — slightly smaller controls. */
  compact?: boolean;
  /** Inline in page header — no full-width bar. */
  inline?: boolean;
  /** Same row as day chips — no bottom margin. */
  inRow?: boolean;
  className?: string;
};

export function WeekPicker({ compact = false, inline = false, inRow = false, className }: WeekPickerProps) {
  const { t } = useTranslation();
  const { weekId, weekMeta, setWeekId } = useWeekScope();
  const [open, setOpen] = useState(false);
  const [panelStyle, setPanelStyle] = useState<CSSProperties | null>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const listId = useId();

  const PANEL_MIN_WIDTH = 300;
  const VIEWPORT_PAD = 8;

  const updatePanelPosition = useCallback(() => {
    const trigger = triggerRef.current;
    if (!trigger) return;
    const rect = trigger.getBoundingClientRect();
    const width = Math.max(PANEL_MIN_WIDTH, rect.width);
    let left = rect.left;
    if (left + width > window.innerWidth - VIEWPORT_PAD) {
      left = window.innerWidth - width - VIEWPORT_PAD;
    }
    left = Math.max(VIEWPORT_PAD, left);
    setPanelStyle({
      position: "fixed",
      top: rect.bottom + 6,
      left,
      minWidth: width,
      zIndex: 1300,
    });
  }, []);

  const groups = useMemo(() => listWeekOptions(), [weekId]);
  const currentId = currentWeekId();

  useLayoutEffect(() => {
    if (!open) {
      setPanelStyle(null);
      return;
    }
    updatePanelPosition();
    window.addEventListener("resize", updatePanelPosition);
    window.addEventListener("scroll", updatePanelPosition, true);
    return () => {
      window.removeEventListener("resize", updatePanelPosition);
      window.removeEventListener("scroll", updatePanelPosition, true);
    };
  }, [open, updatePanelPosition]);

  useEffect(() => {
    if (!open) return;
    const onDoc = (e: MouseEvent) => {
      const target = e.target as Node;
      if (rootRef.current?.contains(target) || panelRef.current?.contains(target)) return;
      setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("mousedown", onDoc);
    window.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("mousedown", onDoc);
      window.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const kindLabel = (kind: WeekKind) => {
    switch (kind) {
      case "past":
        return t("weekPicker.past");
      case "future":
        return t("weekPicker.future");
      default:
        return t("weekPicker.current");
    }
  };

  const weekBadge = (meta: WeekMeta) => `W${meta.isoWeek}`;

  const rangeClass =
    weekMeta.kind === "past"
      ? styles.rangePast
      : weekMeta.kind === "future"
        ? styles.rangeFuture
        : styles.rangeCurrent;

  const badgeClass =
    weekMeta.kind === "past"
      ? styles.badgePast
      : weekMeta.kind === "future"
        ? styles.badgeFuture
        : styles.badgeCurrent;

  const onPick = (meta: WeekMeta) => {
    setWeekId(meta.weekId);
    setOpen(false);
  };

  const shiftWeek = (delta: number) => {
    setOpen(false);
    setWeekId(addWeeks(weekId, delta));
  };

  const onNavClick = (delta: number) => (e: ReactMouseEvent<HTMLButtonElement>) => {
    e.stopPropagation();
    e.preventDefault();
    shiftWeek(delta);
  };

  const renderOption = (meta: WeekMeta) => {
    const selected = meta.weekId === weekId;
    const optBadge =
      meta.kind === "past"
        ? styles.badgePast
        : meta.kind === "future"
          ? styles.badgeFuture
          : styles.badgeCurrent;
    return (
      <button
        key={meta.weekId}
        type="button"
        role="option"
        aria-selected={selected}
        className={[styles.option, selected ? styles.optionSelected : ""].filter(Boolean).join(" ")}
        onClick={() => onPick(meta)}
      >
        <span className={styles.optionText}>
          <div
            className={[
              styles.optionRange,
              meta.kind === "past"
                ? styles.rangePast
                : meta.kind === "future"
                  ? styles.rangeFuture
                  : styles.rangeCurrent,
            ]
              .filter(Boolean)
              .join(" ")}
          >
            {meta.dateFrom} – {meta.dateTo}/{meta.calendarYear}
          </div>
          <div className={styles.optionMeta}>{kindLabel(meta.kind)}</div>
        </span>
        <span className={[styles.badge, optBadge].join(" ")}>{weekBadge(meta)}</span>
      </button>
    );
  };

  const rootClass = [
    styles.bar,
    inline ? styles.inline : "",
    compact ? styles.compact : "",
    inRow ? styles.inRow : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  const panel =
    open && panelStyle ? (
      <div
        ref={panelRef}
        className={styles.panel}
        id={listId}
        role="listbox"
        aria-label={t("weekPicker.chooseWeek")}
        style={panelStyle}
      >
        {weekMeta.kind !== "current" ? (
          <button
            type="button"
            className={styles.todayBtn}
            style={{ marginBottom: 8, width: "100%" }}
            onClick={() => onPick(buildWeekMeta(currentId)!)}
          >
            {t("weekPicker.jumpCurrent")}
          </button>
        ) : null}

        {groups.past.length > 0 ? (
          <>
            <div className={styles.sectionTitle}>{t("weekPicker.sectionPast")}</div>
            {[...groups.past].reverse().map(renderOption)}
          </>
        ) : null}

        <div className={styles.sectionTitle}>{t("weekPicker.sectionCurrent")}</div>
        {renderOption(groups.current)}

        {groups.future.length > 0 ? (
          <>
            <div className={styles.sectionTitle}>{t("weekPicker.sectionFuture")}</div>
            {groups.future.map(renderOption)}
          </>
        ) : null}
      </div>
    ) : null;

  return (
    <div className={rootClass} ref={rootRef}>
      <button
        type="button"
        className={styles.navBtn}
        aria-label={t("weekPicker.prevWeek")}
        onClick={onNavClick(-1)}
        onMouseDown={(e) => e.stopPropagation()}
      >
        ‹
      </button>

      <div className={styles.main}>
        <button
          ref={triggerRef}
          type="button"
          className={[styles.trigger, open ? styles.triggerOpen : ""].filter(Boolean).join(" ")}
          aria-expanded={open}
          aria-haspopup="listbox"
          aria-controls={listId}
          onClick={() => setOpen((v) => !v)}
        >
          <span style={{ flex: 1, minWidth: 0 }}>
            <div className={[styles.range, rangeClass].filter(Boolean).join(" ")}>
              {weekMeta.dateFrom} – {weekMeta.dateTo}/{weekMeta.calendarYear}
            </div>
          </span>
          <span className={[styles.badge, badgeClass].join(" ")}>{weekBadge(weekMeta)}</span>
          <span className={styles.chevron} aria-hidden>
            {open ? "▴" : "▾"}
          </span>
        </button>

      </div>

      {panel ? createPortal(panel, document.body) : null}

      <button
        type="button"
        className={styles.navBtn}
        aria-label={t("weekPicker.nextWeek")}
        onClick={onNavClick(1)}
        onMouseDown={(e) => e.stopPropagation()}
      >
        ›
      </button>
    </div>
  );
}
