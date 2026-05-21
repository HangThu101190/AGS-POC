import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { Button, ButtonDanger, CancelButton } from "@/components/ui";
import type { FlightDto, FlightUpsertBody } from "@/shared/api/flightsApi";
import { ALL_DEPT_CODES } from "@/shared/constants/departments";
import { departmentLabel, departmentLabelWithCode } from "@/shared/i18n/departmentLabel";

type FlightFormDialogProps = {
  open: boolean;
  dayIdx: number;
  flight?: FlightDto | null;
  onClose: () => void;
  onSave: (body: FlightUpsertBody) => Promise<void>;
  onDelete?: () => Promise<void>;
};

export function FlightFormDialog({ open, dayIdx, flight, onClose, onSave, onDelete }: FlightFormDialogProps) {
  const { t } = useTranslation();
  const [flightNo, setFlightNo] = useState("");
  const [route, setRoute] = useState("");
  const [sta, setSta] = useState("");
  const [std, setStd] = useState("");
  const [dept, setDept] = useState("PVHK_DI");
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!open) return;
    setFlightNo(flight?.flightNo ?? "");
    setRoute(flight?.route ?? "");
    setSta(flight?.sta ?? "");
    setStd(flight?.std ?? "");
    setDept(flight?.departmentCode ?? "PVHK_DI");
  }, [open, flight]);

  const submit = async () => {
    setSaving(true);
    try {
      await onSave({
        dayIdx,
        flightNo,
        route,
        sta,
        std,
        departmentCode: dept,
        manning: 2,
      });
      onClose();
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} fullWidth maxWidth="sm">
      <DialogTitle>{flight ? t("flights.editFlight") : t("flights.addFlight")}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField
            label={t("flights.colArr")}
            helperText={t("flights.colArrTip")}
            value={flightNo}
            onChange={(e) => setFlightNo(e.target.value)}
            size="small"
          />
          <TextField
            label={t("flights.colRoute")}
            value={route}
            onChange={(e) => setRoute(e.target.value)}
            size="small"
          />
          <Stack direction="row" spacing={1}>
            <TextField
              label={t("flights.fieldSta")}
              value={sta}
              onChange={(e) => setSta(e.target.value)}
              size="small"
              fullWidth
            />
            <TextField
              label={t("flights.fieldStd")}
              value={std}
              onChange={(e) => setStd(e.target.value)}
              size="small"
              fullWidth
            />
          </Stack>
          <TextField
            select
            label={t("flights.deptCol")}
            value={dept}
            onChange={(e) => setDept(e.target.value)}
            size="small"
          >
            {ALL_DEPT_CODES.map((d) => (
              <MenuItem key={d} value={d} title={departmentLabelWithCode(d, t)}>
                {departmentLabel(d, t)}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ justifyContent: flight && onDelete ? "space-between" : "flex-end" }}>
        {flight && onDelete ? (
          <ButtonDanger onClick={() => void onDelete()}>{t("flights.deleteFlight")}</ButtonDanger>
        ) : (
          <span />
        )}
        <Stack direction="row" spacing={1}>
          <CancelButton onClick={onClose}>{t("common.cancel")}</CancelButton>
          <Button disabled={saving || !flightNo || !route} onClick={() => void submit()}>
            {t("common.save")}
          </Button>
        </Stack>
      </DialogActions>
    </Dialog>
  );
}
