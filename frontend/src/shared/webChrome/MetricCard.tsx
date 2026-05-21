import type { ReactNode } from "react";
import { agsTokens } from "@/components/theme/tokens";
import { chrome } from "./styles";

type MetricCardProps = {
  label: string;
  value: number | string;
  sub: string;
  title?: string;
  icon?: ReactNode;
  accentColor?: string;
};

export function MetricCard({ label, value, sub, title, icon, accentColor }: MetricCardProps) {
  return (
    <div
      style={{
        ...chrome.metric,
        borderLeft: `3px solid ${accentColor ?? agsTokens.primary}`,
      }}
      title={title}
    >
      {icon ? <span style={{ display: "block", marginBottom: 8, fontSize: 18, lineHeight: 1 }}>{icon}</span> : null}
      <div
        style={{
          fontSize: 11,
          color: "#64748b",
          textTransform: "uppercase",
          letterSpacing: 1,
          marginBottom: 4,
        }}
      >
        {label}
      </div>
      <div style={{ fontSize: 28, fontWeight: 600, color: "#0f172a", lineHeight: 1 }}>{value}</div>
      <div style={{ fontSize: 11, color: "#94a3b8", marginTop: 4 }}>{sub}</div>
    </div>
  );
}
