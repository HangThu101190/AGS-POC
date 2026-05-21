import { useTranslation } from "react-i18next";
import { MobileScreen } from "./MobileScreen";
import { mobile } from "./mobileStyles";

const REQUEST_ROWS = [
  { code: "F", lk: "reqF", dk: "reqFDesc" },
  { code: "F4", lk: "reqF4", dk: "reqF4Desc" },
  { code: "OM", lk: "reqOM", dk: "reqOMDesc" },
  { code: "NB", lk: "reqNB", dk: "reqNBDesc" },
  { code: "RQ", lk: "reqRQ", dk: "reqRQDesc" },
  { code: "CT", lk: "reqCT", dk: "reqCTDesc" },
] as const;

export function MobileRequestsPage() {
  const { t } = useTranslation();

  return (
    <MobileScreen title={t("mobile.requests")} subtitle={t("mobile.requestsSubtitle")}>
      <div style={mobile.rqGrid}>
        {REQUEST_ROWS.map((row) => (
          <button key={row.code} type="button" style={mobile.rqBtn}>
            <div style={{ fontWeight: 700, fontSize: 14 }}>{row.code}</div>
            <div style={{ fontSize: 12, fontWeight: 500, marginTop: 2 }}>{t(`mobile.${row.lk}`)}</div>
            <div style={{ fontSize: 10, color: "#64748b", marginTop: 1 }}>{t(`mobile.${row.dk}`)}</div>
          </button>
        ))}
      </div>
      <div style={{ ...mobile.bannerInfo, marginTop: 16 }}>{t("mobile.requestsStub")}</div>
    </MobileScreen>
  );
}
