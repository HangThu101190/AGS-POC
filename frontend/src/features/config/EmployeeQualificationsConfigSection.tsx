import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Checkbox from "@mui/material/Checkbox";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useTranslation } from "react-i18next";
import { ButtonPrimary, Loading } from "@/components/ui";
import {
  fetchEmployeeQualifications,
  upsertEmployeeQualifications,
} from "@/shared/api/staffingApi";

const ROLES = ["Counter", "Gate", "Sup", "General"] as const;
const SEGMENTS = ["Qn", "Qt"] as const;

type CellKey = `${(typeof SEGMENTS)[number]}:${(typeof ROLES)[number]}`;

function cellKey(segment: string, role: string): CellKey {
  return `${segment}:${role}` as CellKey;
}

export function EmployeeQualificationsConfigSection() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const dept = "PVHK_DI";
  const qualKey = ["config", "employee-qualifications", dept] as const;
  const qualQ = useQuery({ queryKey: qualKey, queryFn: () => fetchEmployeeQualifications(dept) });

  const [draft, setDraft] = useState<Record<string, Record<CellKey, boolean>>>({});

  const qualByEmployee = useMemo(() => {
    const map = new Map<string, typeof qualQ.data>();
    for (const q of qualQ.data ?? []) {
      const list = map.get(q.employeeId) ?? [];
      list.push(q);
      map.set(q.employeeId, list);
    }
    return map;
  }, [qualQ.data]);

  const employees = useMemo(() => {
    const seen = new Map<string, { id: string; code: string; name: string }>();
    for (const q of qualQ.data ?? []) {
      if (!seen.has(q.employeeId)) {
        seen.set(q.employeeId, { id: q.employeeId, code: q.employeeCode, name: q.employeeName });
      }
    }
    return [...seen.values()].sort((a, b) => a.code.localeCompare(b.code));
  }, [qualQ.data]);

  const saveMut = useMutation({
    mutationFn: async (employeeId: string) => {
      const cells = draft[employeeId] ?? {};
      const qualifications = SEGMENTS.flatMap((segment) =>
        ROLES.filter((role) => cells[cellKey(segment, role)]).map((crewRole) => ({
          segment,
          crewRole,
          proficiency: 2,
          isActive: true,
        })),
      );
      await upsertEmployeeQualifications(employeeId, qualifications);
    },
    onSuccess: () => void queryClient.invalidateQueries({ queryKey: qualKey }),
  });

  const toggle = (employeeId: string, segment: string, role: string, checked: boolean) => {
    const key = cellKey(segment, role);
    setDraft((prev) => ({
      ...prev,
      [employeeId]: { ...(prev[employeeId] ?? {}), [key]: checked },
    }));
  };

  const isChecked = (employeeId: string, segment: string, role: string) => {
    const key = cellKey(segment, role);
    if (draft[employeeId]?.[key] !== undefined) return draft[employeeId][key];
    return (qualByEmployee.get(employeeId) ?? []).some(
      (q) => q.segment === segment && q.crewRole === role && q.isActive,
    );
  };

  if (qualQ.isLoading) return <Loading label={t("common.loading")} />;

  return (
    <Stack spacing={1.5}>
      <Typography variant="subtitle1">{t("config.qualificationsTitle")}</Typography>
      {employees.length === 0 ? (
        <Typography variant="body2" color="text.secondary">
          {t("config.qualificationsEmpty")}
        </Typography>
      ) : null}
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>{t("config.employee")}</TableCell>
            {SEGMENTS.flatMap((segment) =>
              ROLES.map((role) => (
                <TableCell key={`${segment}-${role}`} align="center">
                  {segment}/{role}
                </TableCell>
              )),
            )}
            <TableCell />
          </TableRow>
        </TableHead>
        <TableBody>
          {employees.map((emp) => (
            <TableRow key={emp.id}>
              <TableCell>
                {emp.code} — {emp.name}
              </TableCell>
              {SEGMENTS.flatMap((segment) =>
                ROLES.map((role) => (
                  <TableCell key={`${emp.id}-${segment}-${role}`} align="center">
                    <Checkbox
                      size="small"
                      checked={isChecked(emp.id, segment, role)}
                      onChange={(e) => toggle(emp.id, segment, role, e.target.checked)}
                    />
                  </TableCell>
                )),
              )}
              <TableCell>
                <ButtonPrimary size="small" onClick={() => saveMut.mutate(emp.id)}>
                  {t("common.save")}
                </ButtonPrimary>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </Stack>
  );
}
