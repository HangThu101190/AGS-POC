import { useTranslation } from "react-i18next";

const STYLES: Record<string, { bg: string; color: string; key: string }> = {
  draft: { bg: "#f1f5f9", color: "#475569", key: "planStatus.draft" },
  published: { bg: "#fef3c7", color: "#854d0e", key: "planStatus.published" },
  in_progress: { bg: "#dbeafe", color: "#1e40af", key: "planStatus.inProgress" },
  inprogress: { bg: "#dbeafe", color: "#1e40af", key: "planStatus.inProgress" },
  closed: { bg: "#dcfce7", color: "#15803d", key: "planStatus.closed" },
};

export function PlanStatusBadge({ status }: { status?: string | null }) {
  const { t } = useTranslation();
  const norm = (status ?? "draft").replace(/([A-Z])/g, "_$1").toLowerCase().replace(/^_/, "");
  const v = STYLES[norm] ?? STYLES.draft;

  return (
    <span
      style={{
        padding: "4px 10px",
        borderRadius: 100,
        fontSize: 10,
        fontWeight: 600,
        background: v.bg,
        color: v.color,
        letterSpacing: 0.5,
        textTransform: "uppercase",
        whiteSpace: "nowrap",
      }}
    >
      {t(v.key)}
    </span>
  );
}
