import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import { useTranslation } from "react-i18next";
import { Alert, Loading } from "@/components/ui";
import { approveLeaveRequest, fetchLeaveRequests } from "@/shared/api/leaveApi";

export function LeaveRequestsTab() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["leave", "requests"],
    queryFn: fetchLeaveRequests,
  });

  const approveMut = useMutation({
    mutationFn: approveLeaveRequest,
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["leave", "requests"] }),
  });

  if (isLoading) {
    return <Loading label={t("common.loading")} />;
  }

  if (error) {
    return (
      <Alert severity="error" onClose={() => void refetch()}>
        {t("common.error")}
      </Alert>
    );
  }

  return (
    <Box component="table" sx={{ width: "100%", borderCollapse: "collapse", fontSize: 14 }}>
      <thead>
        <tr>
          <th align="left">{t("leave.colEmployee")}</th>
          <th align="left">{t("leave.colFrom")}</th>
          <th align="left">{t("leave.colTo")}</th>
          <th align="left">{t("leave.colStatus")}</th>
          <th align="right">{t("leave.colActions")}</th>
        </tr>
      </thead>
      <tbody>
        {(data ?? []).map((r) => (
          <tr key={r.id}>
            <td>{r.employeeId.slice(0, 8)}…</td>
            <td>{r.fromDate}</td>
            <td>{r.toDate}</td>
            <td>
              <Chip size="small" label={r.status} />
            </td>
            <td align="right">
              {r.status.toLowerCase() === "pending" ? (
                <Button
                  size="small"
                  variant="outlined"
                  disabled={approveMut.isPending}
                  onClick={() => approveMut.mutate(r.id)}
                >
                  {t("leave.approve")}
                </Button>
              ) : null}
            </td>
          </tr>
        ))}
      </tbody>
    </Box>
  );
}
