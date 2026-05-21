import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import {
  confirmSyncProposal,
  dismissSyncProposal,
  fetchSyncProposals,
  type SyncProposalDto,
} from "@/shared/api/syncProposalsApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

/** Sup — đề xuất sync ca sau delay (prototype FlightsTab + SyncProposal). */
export function MobileFlightsPage() {
  const { t } = useTranslation();
  const { weekId } = useWeekScope();
  const [items, setItems] = useState<SyncProposalDto[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const load = async () => {
    setError(null);
    try {
      const data = await fetchSyncProposals(weekId, true);
      setItems(data);
    } catch {
      setError(t("sync.actionFailed"));
    }
  };

  useEffect(() => {
    void load();
  }, [weekId]);

  return (
    <MobileScreen title={t("mobile.flights")} subtitle={t("mobile.flightsSubtitle")}>
      <div style={mobile.bannerInfo}>{t("sync.hint")}</div>
      {message ? <div style={{ ...mobile.bannerInfo, background: "#f0fdf4", color: "#166534" }}>{message}</div> : null}
      {error ? <div style={mobile.bannerWarn}>{error}</div> : null}
      {items.length === 0 ? (
        <p style={{ fontSize: 13, color: "#64748b" }}>{t("sync.empty")}</p>
      ) : null}
      {items.map((p) => (
        <div key={p.id} style={mobile.card}>
          <div style={{ fontSize: 15, fontWeight: 700, color: "#0f172a" }}>
            {p.flightNo} · +{p.delayMinutes}&apos;
          </div>
          <p style={{ fontSize: 13, color: "#475569", margin: "8px 0" }}>{p.summary}</p>
          {p.affected.map((a) => (
            <p key={a.slotId} style={{ fontSize: 11, color: "#64748b", margin: "2px 0" }}>
              {a.employeeName}: {a.currentSegments.join("+")} → {a.proposedSegments.join("+")}
            </p>
          ))}
          <div style={mobile.btnRow}>
            <button
              type="button"
              style={mobile.btnPrimary}
              onClick={async () => {
                await confirmSyncProposal(p.id);
                setMessage(t("sync.confirmed"));
                await load();
              }}
            >
              {t("sync.confirm")}
            </button>
            <button
              type="button"
              style={mobile.btnSecondary}
              onClick={async () => {
                await dismissSyncProposal(p.id);
                setMessage(t("sync.dismissed"));
                await load();
              }}
            >
              {t("sync.dismiss")}
            </button>
          </div>
        </div>
      ))}
    </MobileScreen>
  );
}
