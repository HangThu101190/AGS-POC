import { useEffect, useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import Divider from "@mui/material/Divider";
import FormControlLabel from "@mui/material/FormControlLabel";
import List from "@mui/material/List";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import Switch from "@mui/material/Switch";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { Alert, ButtonOutlined, ButtonPrimary, Loading } from "@/components/ui";
import { staffingApiErrorMessage } from "@/features/dailyStaffing/staffingApiError";
import { rosterOptionsForLine } from "@/features/dailyStaffing/staffingRosterOptions";
import {
  createStaffingAssignment,
  deleteStaffingAssignment,
  fetchStaffingRoster,
  patchStaffingLine,
  updateStaffingAssignment,
  type StaffingDay,
} from "@/shared/api/staffingApi";

const CREW_ROLES = ["General", "Counter", "Gate", "Sup"] as const;

type Props = {
  open: boolean;
  onClose: () => void;
  weekId: string;
  dayIdx: number;
  departmentCode: string;
  lineId: string;
  assignmentId?: string | null;
  day: StaffingDay;
  canEditManning: boolean;
  canAssign: boolean;
  onChanged: () => void;
};

export function StaffingAssignDialog({
  open,
  onClose,
  weekId,
  dayIdx,
  departmentCode,
  lineId,
  assignmentId,
  day,
  canEditManning,
  canAssign,
  onChanged,
}: Props) {
  const { t } = useTranslation();
  const line = day.lines.find((l) => l.id === lineId);
  const flight = line ? day.flights.find((f) => f.id === line.flightId) : undefined;
  const assignment = assignmentId
    ? day.assignments.find((a) => a.id === assignmentId)
    : undefined;
  const [target, setTarget] = useState(line?.targetManning ?? 0);
  const [workStart, setWorkStart] = useState(assignment?.workStart ?? flight?.sta ?? "06:00");
  const [workEnd, setWorkEnd] = useState(assignment?.workEnd ?? flight?.std ?? "14:00");
  const [role, setRole] = useState(assignment?.role ?? "General");
  const [isOvertime, setIsOvertime] = useState(assignment?.isOvertime ?? false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    setTarget(line?.targetManning ?? 0);
    setWorkStart(assignment?.workStart ?? flight?.sta ?? "06:00");
    setWorkEnd(assignment?.workEnd ?? flight?.std ?? "14:00");
    setRole(assignment?.role ?? "General");
    setIsOvertime(assignment?.isOvertime ?? false);
    setErrorMsg(null);
  }, [open, line?.targetManning, assignment, flight?.sta, flight?.std]);

  const roster = useQuery({
    queryKey: ["staffing", "roster", weekId, dayIdx, departmentCode],
    queryFn: () => fetchStaffingRoster(weekId, dayIdx, departmentCode),
    enabled: open && canAssign,
  });

  const patchMut = useMutation({
    mutationFn: () => patchStaffingLine(lineId, target),
    onSuccess: () => {
      onChanged();
      setErrorMsg(null);
    },
    onError: (err) => setErrorMsg(staffingApiErrorMessage(err, t("staffing.actionFailed"))),
  });

  const assignMut = useMutation({
    mutationFn: (employeeId: string) =>
      createStaffingAssignment({
        staffingLineId: lineId,
        employeeId,
        role,
        workStart,
        workEnd,
        isOvertime,
      }),
    onSuccess: () => {
      onChanged();
      setErrorMsg(null);
    },
    onError: (err) => setErrorMsg(staffingApiErrorMessage(err, t("staffing.actionFailed"))),
  });

  const updateMut = useMutation({
    mutationFn: () => {
      if (!assignmentId) throw new Error("missing assignment");
      return updateStaffingAssignment(assignmentId, { role, workStart, workEnd, isOvertime });
    },
    onSuccess: () => {
      onChanged();
      onClose();
    },
    onError: (err) => setErrorMsg(staffingApiErrorMessage(err, t("staffing.actionFailed"))),
  });

  const deleteMut = useMutation({
    mutationFn: () => {
      if (!assignmentId) throw new Error("missing assignment");
      return deleteStaffingAssignment(assignmentId);
    },
    onSuccess: () => {
      onChanged();
      onClose();
    },
    onError: () => setErrorMsg(t("staffing.actionFailed")),
  });

  if (!line) {
    return null;
  }

  const assignedOnLine = day.assignments.filter((a) => a.staffingLineId === lineId);
  const pickOptions = rosterOptionsForLine(roster.data, lineId, {
    assigned: assignedOnLine.length,
    target: line.targetManning,
  });
  const editing = Boolean(assignmentId && assignment && canAssign);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {flight?.departureFlightNo ?? flight?.flightNo} ·{" "}
        {editing ? t("staffing.editAssign") : t("staffing.assignTitle")}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={2}>
          {errorMsg ? (
            <Alert severity="error" onClose={() => setErrorMsg(null)}>
              {errorMsg}
            </Alert>
          ) : null}

          {editing ? (
            <Stack spacing={1.5}>
              <Typography variant="body2" color="text.secondary">
                {assignment!.employeeName} ({assignment!.employeeCode})
              </Typography>
              <Stack direction="row" spacing={1}>
                <TextField
                  size="small"
                  label={t("staffing.workStart")}
                  value={workStart}
                  onChange={(e) => setWorkStart(e.target.value)}
                  placeholder="06:00"
                  sx={{ flex: 1 }}
                />
                <TextField
                  size="small"
                  label={t("staffing.workEnd")}
                  value={workEnd}
                  onChange={(e) => setWorkEnd(e.target.value)}
                  placeholder="14:00"
                  sx={{ flex: 1 }}
                />
              </Stack>
              <TextField
                select
                size="small"
                label={t("staffing.role")}
                value={role}
                onChange={(e) => setRole(e.target.value)}
              >
                {CREW_ROLES.map((r) => (
                  <MenuItem key={r} value={r}>
                    {t(`staffing.roles.${r}`)}
                  </MenuItem>
                ))}
              </TextField>
              <FormControlLabel
                control={
                  <Switch
                    checked={isOvertime}
                    onChange={(e) => setIsOvertime(e.target.checked)}
                  />
                }
                label={t("staffing.overtime")}
              />
            </Stack>
          ) : null}

          {canEditManning ? (
            <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
              <TextField
                size="small"
                type="number"
                label={t("staffing.colTarget")}
                value={target}
                onChange={(e) => setTarget(Number(e.target.value))}
                slotProps={{ htmlInput: { min: 0, max: 99 } }}
              />
              <Typography
                component="button"
                type="button"
                variant="body2"
                sx={{ cursor: "pointer", color: "primary.main", border: 0, background: "none" }}
                onClick={() => patchMut.mutate()}
              >
                {t("common.save")}
              </Typography>
            </Stack>
          ) : null}

          {!editing && assignedOnLine.length > 0 ? (
            <Typography variant="body2" color="text.secondary">
              {assignedOnLine
                .map((a) => `${a.employeeName} (${a.workStart}–${a.workEnd})`)
                .join(" · ")}
            </Typography>
          ) : null}

          {canAssign && !editing ? (
            <>
              <Divider />
              <Typography variant="subtitle2">{t("staffing.addEmployee")}</Typography>
              {roster.isLoading ? (
                <Loading label={t("common.loading")} />
              ) : (
                <List dense>
                  {pickOptions.map((o) => (
                    <ListItemButton
                      key={o.employeeId}
                      disabled={o.disabled || o.assignedOnLine || assignMut.isPending}
                      onClick={() => assignMut.mutate(o.employeeId)}
                    >
                      <ListItemText
                        primary={o.label}
                        secondary={o.assignedOnLine ? t("staffing.alreadyAssigned") : undefined}
                      />
                    </ListItemButton>
                  ))}
                </List>
              )}
            </>
          ) : null}
        </Stack>
      </DialogContent>
      {editing ? (
        <DialogActions sx={{ px: 3, pb: 2, justifyContent: "space-between" }}>
          <ButtonOutlined
            color="error"
            disabled={deleteMut.isPending}
            onClick={() => deleteMut.mutate()}
          >
            {t("staffing.removeAssign")}
          </ButtonOutlined>
          <Stack direction="row" spacing={1}>
            <ButtonOutlined onClick={onClose}>{t("common.cancel")}</ButtonOutlined>
            <ButtonPrimary
              disabled={updateMut.isPending}
              onClick={() => updateMut.mutate()}
            >
              {t("common.save")}
            </ButtonPrimary>
          </Stack>
        </DialogActions>
      ) : null}
    </Dialog>
  );
}
