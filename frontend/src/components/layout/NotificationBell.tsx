import { useCallback, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  fetchMyNotifications,
  markNotificationRead,
  type NotificationDto,
} from "@/shared/api/notificationsApi";
import styles from "./notificationBell.module.css";

export function NotificationBell() {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  const unread = items.filter((n) => !n.isRead).length;

  const load = useCallback(async () => {
    setLoading(true);
    try {
      const page = await fetchMyNotifications({ page: 0, pageSize: 30 });
      setItems(page.items);
    } catch {
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
    const id = window.setInterval(() => void load(), 60_000);
    return () => window.clearInterval(id);
  }, [load]);

  useEffect(() => {
    if (!open) return;
    void load();
    const onDoc = (e: MouseEvent) => {
      if (!rootRef.current?.contains(e.target as Node)) setOpen(false);
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
  }, [load, open]);

  const onOpenItem = async (n: NotificationDto) => {
    if (!n.isRead) {
      try {
        await markNotificationRead(n.id);
        setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
      } catch {
        /* ignore */
      }
    }
  };

  return (
    <div className={styles.wrap} ref={rootRef}>
      <button
        type="button"
        className={styles.bellBtn}
        aria-expanded={open}
        aria-haspopup="menu"
        aria-label={t("notifications.title")}
        onClick={() => setOpen((v) => !v)}
      >
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
          <path d="M18 8a6 6 0 10-12 0c0 7-3 9-3 9h18s-3-2-3-9" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M13.7 21a2 2 0 01-3.4 0" strokeLinecap="round" />
        </svg>
        {unread > 0 ? (
          <span className={styles.badge} aria-label={t("notifications.unreadCount", { count: unread })}>
            {unread > 9 ? "9+" : unread}
          </span>
        ) : null}
      </button>
      {open ? (
        <div className={styles.panel} role="menu">
          <div className={styles.panelHeader}>{t("notifications.title")}</div>
          {loading ? <p className={styles.empty}>{t("common.loading")}</p> : null}
          {!loading && items.length === 0 ? <p className={styles.empty}>{t("inbox.empty")}</p> : null}
          {!loading
            ? items.map((n) => (
                <button
                  key={n.id}
                  type="button"
                  role="menuitem"
                  className={[styles.item, !n.isRead ? styles.itemUnread : ""].filter(Boolean).join(" ")}
                  onClick={() => void onOpenItem(n)}
                >
                  <strong>{n.title}</strong>
                  <span>{n.body}</span>
                </button>
              ))
            : null}
        </div>
      ) : null}
    </div>
  );
}
