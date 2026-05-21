import { useQuery } from "@tanstack/react-query";
import Box from "@mui/material/Box";
import { useTranslation } from "react-i18next";
import { Loading } from "@/components/ui";
import { fetchShiftCheckInPolicies } from "@/shared/api/staffingApi";
import { useAuth } from "@/shared/auth/AuthContext";

export function ShiftAttendancePolicyTab() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const dept = user?.departmentCode ?? "PVHK_DI";
  const { data, isLoading } = useQuery({
    queryKey: ["config", "shift-policies", dept],
    queryFn: () => fetchShiftCheckInPolicies(dept),
  });

  if (isLoading) {
    return <Loading label={t("common.loading")} />;
  }

  return (
    <Box component="table" sx={{ width: "100%", borderCollapse: "collapse", fontSize: 14 }}>
      <thead>
        <tr>
          <th align="left">{t("config.policyColBucket")}</th>
          <th align="left">{t("config.policyColSegment")}</th>
          <th align="right">{t("config.policyColCheckIn")}</th>
          <th align="right">{t("config.policyColCheckOut")}</th>
        </tr>
      </thead>
      <tbody>
        {(data ?? []).map((p) => (
          <tr key={p.id}>
            <td>{p.bucketKey}</td>
            <td>
              {p.segmentStart} – {p.segmentEnd}
            </td>
            <td align="right">
              −{p.checkInEarliestMinutesBefore}′ / +{p.checkInLatestMinutesAfterStart}′
            </td>
            <td align="right">
              −{p.checkOutEarliestMinutesBeforeEnd}′ / +{p.checkOutLatestMinutesAfterEnd}′
            </td>
          </tr>
        ))}
      </tbody>
    </Box>
  );
}
