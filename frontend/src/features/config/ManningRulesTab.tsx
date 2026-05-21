import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Box from "@mui/material/Box";
import IconButton from "@mui/material/IconButton";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { Alert, ButtonOutlined, ButtonPrimary, Loading } from "@/components/ui";
import { EmployeeQualificationsConfigSection } from "@/features/config/EmployeeQualificationsConfigSection";
import { ShiftTemplatesConfigSection } from "@/features/config/ShiftTemplatesConfigSection";
import {
  createAircraftManningRule,
  createAirlineManningRule,
  deleteAircraftManningRule,
  deleteAirlineManningRule,
  fetchAircraftManningRules,
  fetchAirlineManningRules,
  updateAircraftManningRule,
  updateAirlineManningRule,
  type AircraftManningRule,
  type AirlineManningRule,
} from "@/shared/api/staffingApi";

type AircraftDraft = { aircraftPattern: string; baseManning: string };
type AirlineDraft = { airlinePrefix: string; multiplier: string };

export function ManningRulesTab() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const aircraftKey = ["config", "aircraft-rules"] as const;
  const airlineKey = ["config", "airline-rules"] as const;

  const aircraft = useQuery({ queryKey: aircraftKey, queryFn: fetchAircraftManningRules });
  const airline = useQuery({ queryKey: airlineKey, queryFn: fetchAirlineManningRules });

  const [editAircraftId, setEditAircraftId] = useState<string | null>(null);
  const [aircraftDraft, setAircraftDraft] = useState<AircraftDraft>({ aircraftPattern: "", baseManning: "4" });
  const [editAirlineId, setEditAirlineId] = useState<string | null>(null);
  const [airlineDraft, setAirlineDraft] = useState<AirlineDraft>({ airlinePrefix: "", multiplier: "1" });
  const [msg, setMsg] = useState<string | null>(null);

  const invalidate = () => {
    void queryClient.invalidateQueries({ queryKey: aircraftKey });
    void queryClient.invalidateQueries({ queryKey: airlineKey });
  };

  const saveAircraftMut = useMutation({
    mutationFn: async () => {
      const body = {
        aircraftPattern: aircraftDraft.aircraftPattern.trim(),
        baseManning: Number(aircraftDraft.baseManning),
      };
      if (editAircraftId) return updateAircraftManningRule(editAircraftId, body);
      return createAircraftManningRule(body);
    },
    onSuccess: () => {
      invalidate();
      setEditAircraftId(null);
      setAircraftDraft({ aircraftPattern: "", baseManning: "4" });
      setMsg(t("config.manningSaved"));
    },
  });

  const saveAirlineMut = useMutation({
    mutationFn: async () => {
      const body = {
        airlinePrefix: airlineDraft.airlinePrefix.trim(),
        multiplier: Number(airlineDraft.multiplier),
      };
      if (editAirlineId) return updateAirlineManningRule(editAirlineId, body);
      return createAirlineManningRule(body);
    },
    onSuccess: () => {
      invalidate();
      setEditAirlineId(null);
      setAirlineDraft({ airlinePrefix: "", multiplier: "1" });
      setMsg(t("config.manningSaved"));
    },
  });

  const deleteAircraftMut = useMutation({
    mutationFn: (id: string) => deleteAircraftManningRule(id),
    onSuccess: invalidate,
  });

  const deleteAirlineMut = useMutation({
    mutationFn: (id: string) => deleteAirlineManningRule(id),
    onSuccess: invalidate,
  });

  const startEditAircraft = (r: AircraftManningRule) => {
    setEditAircraftId(r.id);
    setAircraftDraft({ aircraftPattern: r.aircraftPattern, baseManning: String(r.baseManning) });
  };

  const startEditAirline = (r: AirlineManningRule) => {
    setEditAirlineId(r.id);
    setAirlineDraft({ airlinePrefix: r.airlinePrefix, multiplier: String(r.multiplier) });
  };

  if (aircraft.isLoading || airline.isLoading) {
    return <Loading label={t("common.loading")} />;
  }

  return (
    <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
      <Typography variant="body2" sx={{ color: "text.secondary" }}>
        {t("config.manningRulesHint")}
      </Typography>
      {msg ? (
        <Alert severity="success" onClose={() => setMsg(null)}>
          {msg}
        </Alert>
      ) : null}

      <Typography variant="subtitle2">{t("config.manningAircraftTitle")}</Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", alignItems: "center" }}>
        <TextField
          size="small"
          label={t("config.manningColPattern")}
          value={aircraftDraft.aircraftPattern}
          onChange={(e) => setAircraftDraft((d) => ({ ...d, aircraftPattern: e.target.value }))}
          sx={{ width: 120 }}
        />
        <TextField
          size="small"
          type="number"
          label={t("config.manningColBase")}
          value={aircraftDraft.baseManning}
          onChange={(e) => setAircraftDraft((d) => ({ ...d, baseManning: e.target.value }))}
          sx={{ width: 100 }}
          slotProps={{ htmlInput: { min: 1, max: 99 } }}
        />
        <ButtonPrimary
          size="small"
          disabled={saveAircraftMut.isPending || !aircraftDraft.aircraftPattern.trim()}
          onClick={() => saveAircraftMut.mutate()}
        >
          {editAircraftId ? t("common.save") : t("config.manningAdd")}
        </ButtonPrimary>
        {editAircraftId ? (
          <ButtonOutlined size="small" onClick={() => {
              setEditAircraftId(null);
              setAircraftDraft({ aircraftPattern: "", baseManning: "4" });
            }}>
            {t("common.cancel")}
          </ButtonOutlined>
        ) : null}
      </Stack>
      <Box component="table" sx={{ width: "100%", borderCollapse: "collapse", fontSize: 14 }}>
        <thead>
          <tr>
            <th align="left">{t("config.manningColPattern")}</th>
            <th align="right">{t("config.manningColBase")}</th>
            <th align="right">{t("config.manningColActions")}</th>
          </tr>
        </thead>
        <tbody>
          {(aircraft.data ?? []).map((r) => (
            <tr key={r.id}>
              <td>{r.aircraftPattern}</td>
              <td align="right">{r.baseManning}</td>
              <td align="right">
                <IconButton size="small" aria-label={t("common.edit")} onClick={() => startEditAircraft(r)}>
                  ✎
                </IconButton>
                <IconButton
                  size="small"
                  aria-label={t("common.delete")}
                  onClick={() => deleteAircraftMut.mutate(r.id)}
                >
                  ×
                </IconButton>
              </td>
            </tr>
          ))}
        </tbody>
      </Box>

      <Typography variant="subtitle2" sx={{ mt: 1 }}>
        {t("config.manningAirlineTitle")}
      </Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", alignItems: "center" }}>
        <TextField
          size="small"
          label={t("config.manningColAirline")}
          value={airlineDraft.airlinePrefix}
          onChange={(e) => setAirlineDraft((d) => ({ ...d, airlinePrefix: e.target.value }))}
          sx={{ width: 100 }}
        />
        <TextField
          size="small"
          type="number"
          label={t("config.manningColMultiplier")}
          value={airlineDraft.multiplier}
          onChange={(e) => setAirlineDraft((d) => ({ ...d, multiplier: e.target.value }))}
          sx={{ width: 100 }}
          slotProps={{ htmlInput: { min: 0.1, max: 5, step: 0.1 } }}
        />
        <ButtonPrimary
          size="small"
          disabled={saveAirlineMut.isPending || !airlineDraft.airlinePrefix.trim()}
          onClick={() => saveAirlineMut.mutate()}
        >
          {editAirlineId ? t("common.save") : t("config.manningAdd")}
        </ButtonPrimary>
        {editAirlineId ? (
          <ButtonOutlined size="small" onClick={() => {
              setEditAirlineId(null);
              setAirlineDraft({ airlinePrefix: "", multiplier: "1" });
            }}>
            {t("common.cancel")}
          </ButtonOutlined>
        ) : null}
      </Stack>
      <Box component="table" sx={{ width: "100%", borderCollapse: "collapse", fontSize: 14 }}>
        <thead>
          <tr>
            <th align="left">{t("config.manningColAirline")}</th>
            <th align="right">{t("config.manningColMultiplier")}</th>
            <th align="right">{t("config.manningColActions")}</th>
          </tr>
        </thead>
        <tbody>
          {(airline.data ?? []).map((r) => (
            <tr key={r.id}>
              <td>{r.airlinePrefix}</td>
              <td align="right">{r.multiplier}</td>
              <td align="right">
                <IconButton size="small" aria-label={t("common.edit")} onClick={() => startEditAirline(r)}>
                  ✎
                </IconButton>
                <IconButton
                  size="small"
                  aria-label={t("common.delete")}
                  onClick={() => deleteAirlineMut.mutate(r.id)}
                >
                  ×
                </IconButton>
              </td>
            </tr>
          ))}
        </tbody>
      </Box>

      <ShiftTemplatesConfigSection />
      <EmployeeQualificationsConfigSection />
    </Box>
  );
}
