import Box from "@mui/material/Box";
import Paper from "@mui/material/Paper";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import type { StaffingDay } from "@/shared/api/staffingApi";
import {
  buildQnRosterRows,
  buildQtRosterRows,
  type RosterQnRow,
} from "@/features/dailyStaffing/staffingRosterRows";
const rowStatusSx = {
  empty: { bgcolor: "grey.50" },
  partial: { bgcolor: "#fffbeb" },
  full: { bgcolor: "#f0fdf4" },
} as const;

type Props = {
  day: StaffingDay;
  dayLabel: string;
  canAssign: boolean;
  onLineClick: (lineId: string) => void;
};

function QnTable({
  rows,
  canAssign,
  onLineClick,
}: {
  rows: RosterQnRow[];
  canAssign: boolean;
  onLineClick: (lineId: string) => void;
}) {
  const { t } = useTranslation();

  return (
    <Table size="small" stickyHeader>
      <TableHead>
        <TableRow>
          <TableCell width={36}>{t("staffing.colStt")}</TableCell>
          <TableCell>{t("staffing.colFlight")}</TableCell>
          <TableCell width={56}>{t("staffing.colDest")}</TableCell>
          <TableCell width={52}>{t("staffing.colAircraft")}</TableCell>
          <TableCell width={48}>{t("staffing.colStd")}</TableCell>
          <TableCell>{t("staffing.roles.Counter")}</TableCell>
          <TableCell>{t("staffing.roles.Gate")}</TableCell>
          <TableCell width={52} align="right">
            {t("staffing.colManning")}
          </TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.length === 0 ? (
          <TableRow>
            <TableCell colSpan={8} align="center" sx={{ color: "text.secondary", py: 2 }}>
              {t("staffing.rosterEmptyQn")}
            </TableCell>
          </TableRow>
        ) : (
          rows.map((row) => {
            const clickable = Boolean(row.lineId) && canAssign;
            return (
              <TableRow
                key={row.key}
                hover={clickable}
                selected={false}
                onClick={() => {
                  if (row.lineId && canAssign) onLineClick(row.lineId);
                }}
                sx={{
                  ...(rowStatusSx[row.rowStatus]),
                  cursor: clickable ? "pointer" : "default",
                  "& td": { py: 0.5, fontSize: 13 },
                }}
              >
                <TableCell>{row.stt}</TableCell>
                <TableCell sx={{ fontWeight: 600 }}>{row.flightNo}</TableCell>
                <TableCell>{row.dest}</TableCell>
                <TableCell>{row.aircraft}</TableCell>
                <TableCell>{row.std}</TableCell>
                <TableCell sx={{ color: row.counter ? "text.primary" : "text.disabled" }}>
                  {row.counter || "—"}
                </TableCell>
                <TableCell sx={{ color: row.gate ? "text.primary" : "text.disabled" }}>
                  {row.gate || "—"}
                </TableCell>
                <TableCell align="right" sx={{ fontVariantNumeric: "tabular-nums" }}>
                  {row.manningLabel}
                </TableCell>
              </TableRow>
            );
          })
        )}
      </TableBody>
    </Table>
  );
}

function QtTable({
  rows,
  canAssign,
  onLineClick,
}: {
  rows: ReturnType<typeof buildQtRosterRows>;
  canAssign: boolean;
  onLineClick: (lineId: string) => void;
}) {
  const { t } = useTranslation();

  return (
    <Table size="small" stickyHeader>
      <TableHead>
        <TableRow>
          <TableCell width={36}>{t("staffing.colStt")}</TableCell>
          <TableCell>{t("staffing.colFlight")}</TableCell>
          <TableCell width={56}>{t("staffing.colDest")}</TableCell>
          <TableCell width={48}>{t("staffing.colStd")}</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {rows.length === 0 ? (
          <TableRow>
            <TableCell colSpan={4} align="center" sx={{ color: "text.secondary", py: 2 }}>
              {t("staffing.rosterEmptyQt")}
            </TableCell>
          </TableRow>
        ) : (
          rows.map((row) => (
            <TableRow
              key={row.key}
              hover={canAssign}
              onClick={() => {
                if (canAssign) onLineClick(row.lineId);
              }}
              sx={{
                cursor: canAssign ? "pointer" : "default",
                "& td": { py: 0.5, fontSize: 13 },
              }}
            >
              <TableCell>{row.stt}</TableCell>
              <TableCell sx={{ fontWeight: 600 }}>{row.flightNo}</TableCell>
              <TableCell>{row.dest}</TableCell>
              <TableCell>{row.std}</TableCell>
            </TableRow>
          ))
        )}
      </TableBody>
    </Table>
  );
}

export function StaffingDayRosterPanel({ day, dayLabel, canAssign, onLineClick }: Props) {
  const { t } = useTranslation();
  const qnRows = useMemo(() => buildQnRosterRows(day), [day]);
  const qtRows = useMemo(() => buildQtRosterRows(day), [day]);

  return (
    <Box sx={{ display: "flex", flexDirection: "column", minHeight: 0, height: "100%", gap: 0.75 }}>
      <Box
        sx={{
          display: "flex",
          flexWrap: "wrap",
          alignItems: "baseline",
          gap: 1,
          flexShrink: 0,
          px: 0.5,
        }}
      >
        <Typography variant="subtitle2" sx={{ fontWeight: 700 }}>
          {t("staffing.rosterTitle")} — {dayLabel}
        </Typography>
      </Box>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" },
          gap: 1,
          flex: 1,
          minHeight: 0,
        }}
      >
        <Paper variant="outlined" sx={{ display: "flex", flexDirection: "column", minHeight: 0 }}>
          <Typography
            variant="caption"
            sx={{
              px: 1,
              py: 0.5,
              bgcolor: "#1e3a8a",
              color: "#fff",
              letterSpacing: 0.5,
              fontWeight: 700,
            }}
          >
            {t("staffing.rosterQn")}
          </Typography>
          <TableContainer sx={{ flex: 1, minHeight: 0, maxHeight: "100%" }}>
            <QnTable rows={qnRows} canAssign={canAssign} onLineClick={onLineClick} />
          </TableContainer>
        </Paper>
        <Paper variant="outlined" sx={{ display: "flex", flexDirection: "column", minHeight: 0 }}>
          <Typography
            variant="caption"
            sx={{
              px: 1,
              py: 0.5,
              bgcolor: "#0f766e",
              color: "#fff",
              letterSpacing: 0.5,
              fontWeight: 700,
            }}
          >
            {t("staffing.rosterQt")}
          </Typography>
          <TableContainer sx={{ flex: 1, minHeight: 0, maxHeight: "100%" }}>
            <QtTable rows={qtRows} canAssign={canAssign} onLineClick={onLineClick} />
          </TableContainer>
        </Paper>
      </Box>
    </Box>
  );
}
