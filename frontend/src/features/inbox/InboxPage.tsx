import Box from "@mui/material/Box";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useCallback, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert, Card, Loading } from "@/components/ui";
import { fetchMyNotifications, markNotificationRead, type NotificationDto } from "@/shared/api/notificationsApi";
import { formatLocaleDateTime } from "@/shared/utils/dateLocale";

export function InboxPage() {
  const { t, i18n } = useTranslation();
  const [items, setItems] = useState<NotificationDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const page = await fetchMyNotifications({ page: 0, pageSize: 50, sortBy: "createdAt", sortDir: "desc" });
      setItems(page.items);
    } catch {
      setError(t("inbox.loadFailed"));
    } finally {
      setLoading(false);
    }
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  const onOpen = async (n: NotificationDto) => {
    if (n.isRead) return;
    try {
      await markNotificationRead(n.id);
      setItems((prev) => prev.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)));
    } catch {
      setError(t("inbox.actionFailed"));
    }
  };

  return (
    <Stack spacing={{ xs: 1.5, md: 2 }} sx={{ minWidth: 0 }}>
      {error ? <Alert severity="error">{error}</Alert> : null}
      <Card>
        {loading ? (
          <Loading />
        ) : items.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            {t("inbox.empty")}
          </Typography>
        ) : (
          <Stack spacing={1}>
            {items.map((n) => (
              <Box
                key={n.id}
                role="button"
                tabIndex={0}
                onClick={() => void onOpen(n)}
                onKeyDown={(e) => {
                  if (e.key === "Enter" || e.key === " ") void onOpen(n);
                }}
                sx={{
                  p: 1.25,
                  borderRadius: 1,
                  border: 1,
                  borderColor: n.isRead ? "divider" : "primary.main",
                  bgcolor: n.isRead ? "transparent" : "action.hover",
                  cursor: "pointer",
                }}
              >
                <Typography variant="subtitle2" sx={{ fontWeight: n.isRead ? 500 : 700 }}>
                  {n.title}
                </Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  {n.body}
                </Typography>
                <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: "block" }}>
                  {formatLocaleDateTime(n.createdAt, i18n.language)}
                </Typography>
              </Box>
            ))}
          </Stack>
        )}
      </Card>
    </Stack>
  );
}
