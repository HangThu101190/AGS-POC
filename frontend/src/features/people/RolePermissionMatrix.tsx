import Checkbox from "@mui/material/Checkbox";
import Stack from "@mui/material/Stack";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Alert, ButtonPrimary, CancelButton, Card } from "@/components/ui";
import {
  fetchRoleCatalog,
  updateRoleCatalogPermissions,
  type RoleCatalog,
} from "@/shared/api/rolesApi";
import { useAuth } from "@/shared/auth/AuthContext";
import styles from "./rolePermissionMatrix.module.css";

function grantsFromCatalog(catalog: RoleCatalog): Record<string, Set<string>> {
  const map: Record<string, Set<string>> = {};
  for (const role of catalog.roles) {
    map[role.code] = new Set(role.permissions);
  }
  return map;
}

function setsEqual(a: Set<string>, b: Set<string>): boolean {
  if (a.size !== b.size) return false;
  for (const x of a) if (!b.has(x)) return false;
  return true;
}

export function RolePermissionMatrix() {
  const { t, i18n } = useTranslation();
  const { refreshUser } = useAuth();
  const isEn = i18n.language.startsWith("en");
  const [catalog, setCatalog] = useState<RoleCatalog | null>(null);
  const [draft, setDraft] = useState<Record<string, Set<string>> | null>(null);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      const c = await fetchRoleCatalog();
      setCatalog(c);
      setDraft(grantsFromCatalog(c));
    } catch {
      setError(t("people.rolesCatalogLoadFailed"));
    }
  }, [t]);

  useEffect(() => {
    void load();
  }, [load]);

  const saved = useMemo(() => {
    if (!catalog || !draft) return null;
    return grantsFromCatalog(catalog);
  }, [catalog, draft]);

  const dirty = useMemo(() => {
    if (!saved || !draft) return false;
    return catalog!.roles.some((r) => !setsEqual(saved[r.code] ?? new Set(), draft[r.code] ?? new Set()));
  }, [saved, draft, catalog]);

  const toggle = (roleCode: string, permCode: string) => {
    setDraft((prev) => {
      if (!prev) return prev;
      const next = { ...prev, [roleCode]: new Set(prev[roleCode]) };
      if (next[roleCode].has(permCode)) next[roleCode].delete(permCode);
      else next[roleCode].add(permCode);
      return next;
    });
  };

  const reset = () => {
    if (catalog) setDraft(grantsFromCatalog(catalog));
  };

  const save = async () => {
    if (!draft || !catalog) return;
    setSaving(true);
    setError(null);
    setMessage(null);
    try {
      const roles: Record<string, string[]> = {};
      for (const role of catalog.roles) {
        roles[role.code] = [...(draft[role.code] ?? [])];
      }
      const updated = await updateRoleCatalogPermissions({ roles });
      setCatalog(updated);
      setDraft(grantsFromCatalog(updated));
      await refreshUser();
      setMessage(t("people.rolesCatalogSaved"));
    } catch {
      setError(t("people.rolesCatalogSaveFailed"));
    } finally {
      setSaving(false);
    }
  };

  if (!catalog || !draft) {
    return error ? <Alert severity="error">{error}</Alert> : null;
  }

  return (
    <Stack spacing={1.5}>
      {message ? <Alert severity="success" onClose={() => setMessage(null)}>{message}</Alert> : null}
      {error ? <Alert severity="error" onClose={() => setError(null)}>{error}</Alert> : null}

      <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
        <ButtonPrimary disabled={!dirty || saving} onClick={() => void save()}>
          {t("common.save", { defaultValue: "Lưu" })}
        </ButtonPrimary>
        <CancelButton disabled={!dirty || saving} onClick={reset}>
          {t("common.cancel", { defaultValue: "Hủy" })}
        </CancelButton>
      </Stack>

      <Card sx={{ p: 0, overflow: "auto" }}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th className={styles.permCol}>{t("people.permission")}</th>
              {catalog.roles.map((r) => (
                <th key={r.code} className={styles.roleCol}>
                  <span className={styles.roleName}>{isEn ? r.nameEn : r.nameVi}</span>
                  <span className={styles.roleCode}>{r.code}</span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {catalog.permissions.map((perm) => (
              <tr key={perm.code}>
                <td className={styles.permCell}>
                  <span className={styles.permLabel}>{isEn ? perm.nameEn : perm.nameVi}</span>
                  <span className={styles.permCode}>{perm.code}</span>
                </td>
                {catalog.roles.map((role) => {
                  const checked = draft[role.code]?.has(perm.code) ?? false;
                  return (
                    <td key={role.code} className={styles.checkCell}>
                      <Checkbox
                        size="small"
                        checked={checked}
                        onChange={() => toggle(role.code, perm.code)}
                        slotProps={{
                          input: { "aria-label": `${role.code} — ${perm.code}` },
                        }}
                        sx={{ p: 0.5 }}
                      />
                    </td>
                  );
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </Card>
    </Stack>
  );
}
