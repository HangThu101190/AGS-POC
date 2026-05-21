import { useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import type { PhanCongSlotFlightDto } from "@/shared/api/phanCongSlotApi";
import { ActionButton } from "@/shared/webChrome";

type Props = {
  flightNos: string[];
  flights: PhanCongSlotFlightDto[];
  disabled?: boolean;
  onToggle: (flightNo: string) => void;
};

export function FlightAssignDropdown({ flightNos, flights, disabled, onToggle }: Props) {
  const { t } = useTranslation();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!open) return;
    const onDown = (e: MouseEvent) => {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
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

  return (
    <div ref={rootRef} style={{ position: "relative" }}>
      <ActionButton
        disabled={disabled}
        onClick={() => setOpen((v) => !v)}
        style={{ padding: "4px 10px", fontSize: 12 }}
        title={t("phanCongSlot.assignFltTitle")}
      >
        {t("phanCongSlot.assignFlt")}
      </ActionButton>
      {open ? (
        <div
          style={{
            position: "absolute",
            right: 0,
            top: "100%",
            marginTop: 4,
            background: "#fff",
            border: "1px solid #e2e8f0",
            borderRadius: 6,
            minWidth: 180,
            zIndex: 10,
            boxShadow: "0 4px 12px rgba(0,0,0,0.08)",
            maxHeight: 240,
            overflow: "auto",
          }}
        >
          {flights.length === 0 ? (
            <p style={{ margin: 0, padding: 10, fontSize: 11, color: "#64748b", lineHeight: 1.4 }}>
              {t("phanCongSlot.noFlightsOverlap")}
            </p>
          ) : (
            flights.map((f) => {
              const selected = flightNos.includes(f.flightNo);
              return (
                <button
                  key={f.id}
                  type="button"
                  onClick={() => onToggle(f.flightNo)}
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: 8,
                    width: "100%",
                    padding: "8px 10px",
                    border: "none",
                    borderBottom: "1px solid #f1f5f9",
                    background: selected ? "#f0fdf4" : "#fff",
                    cursor: "pointer",
                    textAlign: "left",
                  }}
                >
                  <span style={{ fontSize: 11, fontWeight: 600, minWidth: 60 }}>{f.flightNo}</span>
                  <span style={{ fontSize: 10, color: "#64748b", flex: 1 }}>{f.std}</span>
                  {selected ? <span style={{ color: "#15803d", fontSize: 10 }}>✓</span> : null}
                </button>
              );
            })
          )}
        </div>
      ) : null}
    </div>
  );
}
