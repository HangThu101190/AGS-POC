import { useState } from "react";
import { useTranslation } from "react-i18next";
import { postCheckOut } from "@/shared/api/attendanceApi";
import { mobile } from "@/features/mobile/mobileStyles";

type Props = {
  onCheckedOut?: () => void;
};

export function CheckoutFlow({ onCheckedOut }: Props) {
  const { t } = useTranslation();
  const [earlyNote, setEarlyNote] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  const submit = async () => {
    setSubmitting(true);
    try {
      await postCheckOut(earlyNote.trim() || undefined);
      setMessage(t("checkinFlow.checkoutOk"));
      onCheckedOut?.();
    } catch {
      setMessage(t("reconcile.actionFailed"));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div style={{ marginTop: 10 }}>
      {message ? (
        <div style={{ ...mobile.bannerInfo, background: "#f0fdf4", color: "#166534" }}>{message}</div>
      ) : null}
      <label style={{ display: "block", fontSize: 12, fontWeight: 600, color: "#475569" }}>
        {t("checkinFlow.checkoutNoteLbl")}
        <textarea
          value={earlyNote}
          onChange={(e) => setEarlyNote(e.target.value)}
          placeholder={t("checkinFlow.checkoutNotePh")}
          rows={2}
          style={{
            display: "block",
            width: "100%",
            marginTop: 6,
            padding: 8,
            borderRadius: 8,
            border: "1px solid #cbd5e1",
            fontSize: 13,
            fontFamily: "inherit",
            boxSizing: "border-box",
          }}
        />
      </label>
      <button type="button" style={mobile.btnPrimary} onClick={() => void submit()} disabled={submitting}>
        {t("checkinFlow.checkoutBtn")}
      </button>
    </div>
  );
}
