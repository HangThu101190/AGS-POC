import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { ButtonOutlined, ButtonPrimary, Loading } from "@/components/ui";
import {
  deleteShiftTemplate,
  fetchShiftTemplates,
  upsertShiftTemplate,
  type ShiftTemplate,
} from "@/shared/api/staffingApi";

export function ShiftTemplatesConfigSection() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const key = ["config", "shift-templates"] as const;
  const q = useQuery({ queryKey: key, queryFn: () => fetchShiftTemplates("PVHK_DI") });
  const [editId, setEditId] = useState<string | null>(null);
  const [draft, setDraft] = useState({
    code: "",
    name: "",
    startTime: "06:00",
    endTime: "14:00",
    maxHours: "8",
  });

  const saveMut = useMutation({
    mutationFn: () =>
      upsertShiftTemplate({
        id: editId ?? undefined,
        departmentCode: "PVHK_DI",
        code: draft.code,
        name: draft.name,
        startTime: draft.startTime,
        endTime: draft.endTime,
        maxHours: Number(draft.maxHours),
      }),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: key });
      setEditId(null);
      setDraft({ code: "", name: "", startTime: "06:00", endTime: "14:00", maxHours: "8" });
    },
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => deleteShiftTemplate(id),
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: key }),
  });

  const startEdit = (row: ShiftTemplate) => {
    setEditId(row.id);
    setDraft({
      code: row.code,
      name: row.name,
      startTime: row.startTime,
      endTime: row.endTime,
      maxHours: String(row.maxHours),
    });
  };

  if (q.isLoading) return <Loading label={t("common.loading")} />;

  return (
    <Stack spacing={1.5}>
      <Typography variant="subtitle1">{t("config.shiftTemplatesTitle")}</Typography>
      <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
        <TextField size="small" label={t("config.shiftCode")} value={draft.code} onChange={(e) => setDraft((d) => ({ ...d, code: e.target.value }))} />
        <TextField size="small" label={t("config.shiftName")} value={draft.name} onChange={(e) => setDraft((d) => ({ ...d, name: e.target.value }))} />
        <TextField size="small" label={t("config.shiftStart")} value={draft.startTime} onChange={(e) => setDraft((d) => ({ ...d, startTime: e.target.value }))} />
        <TextField size="small" label={t("config.shiftEnd")} value={draft.endTime} onChange={(e) => setDraft((d) => ({ ...d, endTime: e.target.value }))} />
        <TextField size="small" label={t("config.shiftMaxHours")} value={draft.maxHours} onChange={(e) => setDraft((d) => ({ ...d, maxHours: e.target.value }))} />
        <ButtonPrimary onClick={() => saveMut.mutate()} disabled={saveMut.isPending}>
          {editId ? t("common.save") : t("config.manningAdd")}
        </ButtonPrimary>
        {editId ? (
          <ButtonOutlined onClick={() => { setEditId(null); setDraft({ code: "", name: "", startTime: "06:00", endTime: "14:00", maxHours: "8" }); }}>
            {t("common.cancel")}
          </ButtonOutlined>
        ) : null}
      </Stack>
      {q.data?.map((row) => (
        <Stack key={row.id} direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <Typography variant="body2">
            {row.code} · {row.name} · {row.startTime}–{row.endTime}
          </Typography>
          <ButtonOutlined size="small" onClick={() => startEdit(row)}>{t("common.edit")}</ButtonOutlined>
          <ButtonOutlined size="small" onClick={() => deleteMut.mutate(row.id)}>{t("common.delete")}</ButtonOutlined>
        </Stack>
      ))}
    </Stack>
  );
}
