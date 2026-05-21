import {
  useCallback,
  useEffect,
  useLayoutEffect,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { createPortal } from "react-dom";

const MENU_Z = 10_050;

type MenuPos = { top: number; left: number; minWidth: number };

type Props = {
  wrapClassName?: string;
  triggerClassName: string;
  triggerLabel: ReactNode;
  menuClassName?: string;
  menuMaxHeight?: number;
  disabled?: boolean;
  title?: string;
  ariaLabel?: string;
  children: ReactNode;
};

function stopBubble(e: React.SyntheticEvent) {
  e.stopPropagation();
}

function placeMenu(anchor: HTMLElement, menuMaxH: number): MenuPos {
  const r = anchor.getBoundingClientRect();
  const gap = 4;
  const minWidth = Math.max(r.width, 168);
  let top = r.bottom + gap;
  if (top + menuMaxH > window.innerHeight - 8) {
    top = Math.max(8, r.top - gap - menuMaxH);
  }
  let left = r.left;
  if (left + minWidth > window.innerWidth - 8) {
    left = Math.max(8, window.innerWidth - minWidth - 8);
  }
  return { top, left, minWidth };
}

export function StaffingCardMenu({
  wrapClassName = "staffing-slot-card__menu-wrap",
  triggerClassName,
  triggerLabel,
  menuClassName = "staffing-slot-card__menu",
  menuMaxHeight = 220,
  disabled,
  title,
  ariaLabel,
  children,
}: Props) {
  const [open, setOpen] = useState(false);
  const [pos, setPos] = useState<MenuPos>({ top: 0, left: 0, minWidth: 168 });
  const anchorRef = useRef<HTMLButtonElement>(null);

  const updatePos = useCallback(() => {
    if (anchorRef.current) setPos(placeMenu(anchorRef.current, menuMaxHeight));
  }, [menuMaxHeight]);

  useLayoutEffect(() => {
    if (!open) return;
    updatePos();
  }, [open, updatePos]);

  useEffect(() => {
    if (!open) return;
    const onScroll = () => updatePos();
    window.addEventListener("resize", updatePos);
    window.addEventListener("scroll", onScroll, true);
    return () => {
      window.removeEventListener("resize", updatePos);
      window.removeEventListener("scroll", onScroll, true);
    };
  }, [open, updatePos]);

  useEffect(() => {
    if (!open) return;
    const onDown = (e: MouseEvent) => {
      const t = e.target as Node;
      if (anchorRef.current?.contains(t)) return;
      const portal = document.getElementById("staffing-card-menu-portal");
      if (portal?.contains(t)) return;
      setOpen(false);
    };
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    window.addEventListener("mousedown", onDown);
    window.addEventListener("keydown", onKey);
    return () => {
      window.removeEventListener("mousedown", onDown);
      window.removeEventListener("keydown", onKey);
    };
  }, [open]);

  const menuPortal =
    open && !disabled
      ? createPortal(
          <div
            id="staffing-card-menu-portal"
            className={menuClassName}
            role="listbox"
            style={{
              position: "fixed",
              top: pos.top,
              left: pos.left,
              minWidth: pos.minWidth,
              maxHeight: menuMaxHeight,
              zIndex: MENU_Z,
            }}
          >
            {children}
          </div>,
          document.body,
        )
      : null;

  return (
    <div className={wrapClassName} onClick={stopBubble} onMouseDown={stopBubble}>
      <button
        ref={anchorRef}
        type="button"
        className={triggerClassName}
        disabled={disabled}
        title={title}
        aria-label={ariaLabel}
        aria-expanded={open}
        onClick={() => setOpen((v) => !v)}
      >
        {triggerLabel}
      </button>
      {menuPortal}
    </div>
  );
}
