import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import type { ColDef, ICellRendererParams } from "ag-grid-community";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { IconEdit, IconKeyReset, IconUserOff, IconUserPlus } from "@/components/icons/ActionIcons";
import {
  Alert,
  ButtonPrimary,
  CancelButton,
  Card,
  DataTable,
  PageActions,
  PageTabs,
  TableIconAction,
} from "@/components/ui";
import styles from "./peoplePage.module.css";
import { roleLabelKey } from "@/features/account/accountUtils";
import { departmentLabelFromDto, departmentLabelWithCode } from "@/shared/i18n/departmentLabel";
import { gridCol } from "@/shared/ui/gridColumn";
import {
  createDepartment,
  deactivateDepartment,
  fetchDepartmentsPage,
  updateDepartment,
  type DepartmentUpsertBody,
} from "@/shared/api/departmentsApi";
import {
  createEmployee,
  createEmployeeUser,
  deactivateEmployee,
  fetchEmployeesPage,
  resetEmployeePassword,
  updateEmployee,
  type EmployeeUpsertBody,
} from "@/shared/api/employeesApi";
import { RolePermissionMatrix } from "@/features/people/RolePermissionMatrix";
import { LeaveRequestsTab } from "@/features/people/LeaveRequestsTab";
import { useAuth } from "@/shared/auth/AuthContext";
import { canManageMasterData } from "@/shared/auth/roles";
import type { DepartmentDto, EmployeeDto } from "@/shared/types/api";
import { useFullPageTableHeight } from "@/shared/hooks/useMediaQuery";

const ALL_ROLES = ["hr", "sup", "staff", "tbdh"] as const;

type PeopleTabId = "employees" | "departments" | "roles" | "leave";

export function PeoplePage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const canManage = user ? canManageMasterData(user.role) : false;
  const [tab, setTab] = useState<PeopleTabId>("employees");
  const [depts, setDepts] = useState<DepartmentDto[]>([]);
  const [empDialog, setEmpDialog] = useState<"create" | "edit" | null>(null);
  const [deptDialog, setDeptDialog] = useState<"create" | "edit" | null>(null);
  const [editingEmp, setEditingEmp] = useState<EmployeeDto | null>(null);
  const [editingDept, setEditingDept] = useState<DepartmentDto | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);
  const [deptRefreshKey, setDeptRefreshKey] = useState(0);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [tempPassword, setTempPassword] = useState<string | null>(null);
  const bannerCount = (message ? 1 : 0) + (error ? 1 : 0) + (tempPassword ? 1 : 0);
  const tableHeight = useFullPageTableHeight(120, bannerCount);

  const [empForm, setEmpForm] = useState<EmployeeUpsertBody>({
    departmentId: "",
    code: "",
    name: "",
    role: "staff",
    initialPassword: "",
  });
  const [deptForm, setDeptForm] = useState<DepartmentUpsertBody>({
    code: "",
    name: "",
    allowedRoles: ["staff"],
  });

  const loadDepts = useCallback(async () => {
    const p = await fetchDepartmentsPage({ page: 0, pageSize: 100 });
    setDepts(p.items);
    if (p.items[0] && !empForm.departmentId) {
      setEmpForm((f) => ({ ...f, departmentId: p.items[0].id }));
    }
  }, [empForm.departmentId]);

  useEffect(() => {
    void loadDepts();
  }, [loadDepts]);

  const rolesForDept = (deptId: string) => {
    const dept = depts.find((d) => d.id === deptId);
    return dept?.allowedRoles?.length ? dept.allowedRoles : [...ALL_ROLES];
  };

  const fetchEmployees = useCallback(
    (params: Parameters<typeof fetchEmployeesPage>[0], signal?: AbortSignal) =>
      fetchEmployeesPage(params, signal),
    [refreshKey],
  );

  const fetchDepts = useCallback(
    (params: Parameters<typeof fetchDepartmentsPage>[0], signal?: AbortSignal) =>
      fetchDepartmentsPage(params, signal),
    [deptRefreshKey],
  );

  const openEditEmp = (row: EmployeeDto) => {
    setEditingEmp(row);
    setEmpForm({
      departmentId: row.departmentId,
      code: row.code,
      name: row.name,
      role: row.role,
    });
    setEmpDialog("edit");
  };

  const openEditDept = (row: DepartmentDto) => {
    setEditingDept(row);
    setDeptForm({
      code: row.code,
      name: row.name,
      allowedRoles: row.allowedRoles ?? ["staff"],
    });
    setDeptDialog("edit");
  };

  const empCols = useMemo<ColDef<EmployeeDto>[]>(
    () => [
      gridCol(t, "people.colCode", undefined, { field: "code", width: 108, maxWidth: 120 }),
      gridCol(t, "people.colName", undefined, { field: "name", flex: 1, minWidth: 160 }),
      gridCol(t, "people.colRole", undefined, {
        field: "role",
        width: 148,
        valueFormatter: (p) => (p.value ? t(roleLabelKey(String(p.value))) : "—"),
      }),
      gridCol(t, "people.colUser", "people.colUserTip", {
        field: "hasUser",
        width: 72,
        maxWidth: 80,
        cellStyle: { textAlign: "center" },
        headerClass: "ag-center-header",
        valueFormatter: (p) => (p.value ? "✓" : "—"),
      }),
      gridCol(t, "people.colActive", "people.colActiveTip", {
        field: "isActive",
        width: 72,
        maxWidth: 80,
        cellStyle: { textAlign: "center" },
        headerClass: "ag-center-header",
        valueFormatter: (p) => (p.value ? "✓" : "—"),
      }),
      {
        headerName: t("people.colActions"),
        width: 132,
        maxWidth: 140,
        sortable: false,
        filter: false,
        suppressSizeToFit: true,
        cellRenderer: (p: ICellRendererParams<EmployeeDto>) => {
          const row = p.data;
          if (!row) return null;
          return (
            <div className={styles.actionCell}>
              <TableIconAction title={t("common.edit")} onClick={() => openEditEmp(row)}>
                <IconEdit />
              </TableIconAction>
              {!row.hasUser ? (
                <TableIconAction
                  title={t("people.createUser")}
                  onClick={() => void handleCreateUser(row.id)}
                >
                  <IconUserPlus />
                </TableIconAction>
              ) : (
                <TableIconAction
                  title={t("people.resetPassword")}
                  onClick={() => void handleResetPassword(row.id)}
                >
                  <IconKeyReset />
                </TableIconAction>
              )}
              {row.isActive ? (
                <TableIconAction
                  title={t("people.deactivate")}
                  tone="danger"
                  onClick={() => void handleDeactivateEmp(row.id)}
                >
                  <IconUserOff />
                </TableIconAction>
              ) : null}
            </div>
          );
        },
      },
    ],
    [t, depts],
  );

  const deptCols = useMemo<ColDef<DepartmentDto>[]>(
    () => [
      gridCol(t, "people.colDeptCode", undefined, { field: "code", width: 108, maxWidth: 120 }),
      gridCol(t, "people.colName", undefined, { field: "name", flex: 1, minWidth: 140 }),
      gridCol(t, "people.colAllowedRoles", undefined, {
        field: "allowedRoles",
        flex: 2,
        minWidth: 200,
        valueFormatter: (p) =>
          Array.isArray(p.value) ? p.value.map((r) => t(roleLabelKey(String(r)))).join(", ") : "",
      }),
      gridCol(t, "people.colActive", "people.colActiveTip", {
        field: "isActive",
        width: 72,
        maxWidth: 80,
        cellStyle: { textAlign: "center" },
        headerClass: "ag-center-header",
        valueFormatter: (p) => (p.value ? "✓" : "—"),
      }),
      {
        headerName: t("people.colActions"),
        width: 96,
        maxWidth: 104,
        sortable: false,
        filter: false,
        suppressSizeToFit: true,
        cellRenderer: (p: ICellRendererParams<DepartmentDto>) => {
          const row = p.data;
          if (!row) return null;
          return (
            <div className={styles.actionCell}>
              <TableIconAction title={t("common.edit")} onClick={() => openEditDept(row)}>
                <IconEdit />
              </TableIconAction>
              {row.isActive ? (
                <TableIconAction
                  title={t("people.deactivate")}
                  tone="danger"
                  onClick={() => void handleDeactivateDept(row.id)}
                >
                  <IconUserOff />
                </TableIconAction>
              ) : null}
            </div>
          );
        },
      },
    ],
    [t],
  );

  const handleCreateUser = async (id: string) => {
    setError(null);
    try {
      const res = await createEmployeeUser(id, {});
      setTempPassword((res as { temporaryPassword?: string }).temporaryPassword ?? null);
      setMessage(t("people.userCreated"));
      setRefreshKey((k) => k + 1);
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  const handleResetPassword = async (id: string) => {
    setError(null);
    try {
      const res = await resetEmployeePassword(id);
      setTempPassword(res.temporaryPassword);
      setMessage(t("people.passwordReset"));
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  const handleDeactivateEmp = async (id: string) => {
    setError(null);
    try {
      await deactivateEmployee(id);
      setMessage(t("people.employeeDeactivated"));
      setRefreshKey((k) => k + 1);
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  const handleDeactivateDept = async (id: string) => {
    setError(null);
    try {
      await deactivateDepartment(id);
      setMessage(t("people.deptDeactivated"));
      setDeptRefreshKey((k) => k + 1);
      await loadDepts();
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  const saveEmployee = async () => {
    setError(null);
    try {
      if (empDialog === "edit" && editingEmp) {
        await updateEmployee(editingEmp.id, empForm);
        setMessage(t("people.employeeUpdated"));
      } else {
        await createEmployee(empForm);
        setMessage(t("people.employeeCreated"));
      }
      setEmpDialog(null);
      setEditingEmp(null);
      setRefreshKey((k) => k + 1);
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  const saveDepartment = async () => {
    setError(null);
    try {
      if (deptDialog === "edit" && editingDept) {
        await updateDepartment(editingDept.id, deptForm);
        setMessage(t("people.deptUpdated"));
      } else {
        await createDepartment(deptForm);
        setMessage(t("people.deptCreated"));
      }
      setDeptDialog(null);
      setEditingDept(null);
      setDeptRefreshKey((k) => k + 1);
      await loadDepts();
    } catch {
      setError(t("people.actionFailed"));
    }
  };

  if (!canManage) {
    return (
      <Stack spacing={2}>
        <Alert severity="info">{t("errors.forbiddenBody")}</Alert>
      </Stack>
    );
  }

  return (
    <Stack
      spacing={{ xs: 1.5, md: 2 }}
      sx={{
        minWidth: 0,
        height: "calc(100dvh - var(--ags-shell-header-height) - 2 * var(--ags-content-py, 16px))",
        "& > [data-page-tabs-grow]": { flex: 1, minHeight: 0, display: "flex", flexDirection: "column" },
      }}
    >
      {message ? <Alert severity="success" onClose={() => setMessage(null)}>{message}</Alert> : null}
      {tempPassword ? (
        <Alert severity="info" onClose={() => setTempPassword(null)}>
          {t("people.tempPassword", { password: tempPassword })}
        </Alert>
      ) : null}
      {error ? <Alert severity="error" onClose={() => setError(null)}>{error}</Alert> : null}

      <PageTabs<PeopleTabId>
        value={tab}
        onChange={setTab}
        grow
        ariaLabel={t("people.tabsAria")}
        items={[
          { value: "employees", label: t("people.tabEmployees") },
          { value: "departments", label: t("people.tabDepartments") },
          { value: "roles", label: t("people.tabRoles") },
          { value: "leave", label: t("people.tabLeave") },
        ]}
      >
        {tab === "employees" ? (
          <Stack spacing={{ xs: 1.5, md: 2 }} sx={{ minHeight: 0, flex: 1 }}>
            <PageActions>
              <ButtonPrimary
                onClick={() => {
                  setEmpDialog("create");
                  setEditingEmp(null);
                  setEmpForm({
                    departmentId: depts[0]?.id ?? "",
                    code: "",
                    name: "",
                    role: "staff",
                    initialPassword: "",
                  });
                }}
              >
                {t("people.addEmployee")}
              </ButtonPrimary>
            </PageActions>
            <Card
              sx={{
                flex: 1,
                minHeight: 0,
                display: "flex",
                flexDirection: "column",
                border: "none",
                boxShadow: "none",
                borderRadius: 0,
              }}
            >
              <DataTable<EmployeeDto> columnDefs={empCols} fetchRows={fetchEmployees} height={tableHeight} />
            </Card>
          </Stack>
        ) : null}

        {tab === "departments" ? (
          <Stack spacing={{ xs: 1.5, md: 2 }} sx={{ minHeight: 0, flex: 1 }}>
            <PageActions>
              <ButtonPrimary
                onClick={() => {
                  setDeptDialog("create");
                  setDeptForm({ code: "", name: "", allowedRoles: ["staff"] });
                }}
              >
                {t("people.addDept")}
              </ButtonPrimary>
            </PageActions>
            <Card
              sx={{
                flex: 1,
                minHeight: 0,
                display: "flex",
                flexDirection: "column",
                border: "none",
                boxShadow: "none",
                borderRadius: 0,
              }}
            >
              <DataTable<DepartmentDto> columnDefs={deptCols} fetchRows={fetchDepts} height={tableHeight} />
            </Card>
          </Stack>
        ) : null}

        {tab === "roles" && canManage ? <RolePermissionMatrix /> : null}

        {tab === "leave" && canManage ? <LeaveRequestsTab /> : null}
      </PageTabs>

      <Dialog open={empDialog !== null} onClose={() => setEmpDialog(null)} fullWidth maxWidth="sm">
        <DialogTitle>
          {empDialog === "edit" ? t("people.editEmployee") : t("people.addEmployee")}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              label={t("people.colCode")}
              value={empForm.code}
              onChange={(e) => setEmpForm({ ...empForm, code: e.target.value })}
              size="small"
              disabled={empDialog === "edit"}
            />
            <TextField
              label={t("people.colName")}
              value={empForm.name}
              onChange={(e) => setEmpForm({ ...empForm, name: e.target.value })}
              size="small"
            />
            <TextField
              select
              label={t("people.department")}
              value={empForm.departmentId}
              onChange={(e) => setEmpForm({ ...empForm, departmentId: e.target.value, role: "staff" })}
              size="small"
            >
              {depts.map((d) => (
                <MenuItem key={d.id} value={d.id} title={departmentLabelWithCode(d.code, t)}>
                  {departmentLabelFromDto(d, t)}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              select
              label={t("people.colRole")}
              value={empForm.role}
              onChange={(e) => setEmpForm({ ...empForm, role: e.target.value })}
              size="small"
            >
              {rolesForDept(empForm.departmentId).map((r) => (
                <MenuItem key={r} value={r}>
                  {t(roleLabelKey(r))}
                </MenuItem>
              ))}
            </TextField>
            {empDialog === "create" ? (
              <TextField
                label={t("people.initialPassword")}
                type="password"
                value={empForm.initialPassword ?? ""}
                onChange={(e) => setEmpForm({ ...empForm, initialPassword: e.target.value })}
                size="small"
                helperText={t("people.initialPasswordHint")}
              />
            ) : null}
          </Stack>
        </DialogContent>
        <DialogActions>
          <CancelButton onClick={() => setEmpDialog(null)}>
            {t("common.cancel", { defaultValue: "Hủy" })}
          </CancelButton>
          <ButtonPrimary onClick={() => void saveEmployee()}>{t("common.save", { defaultValue: "Lưu" })}</ButtonPrimary>
        </DialogActions>
      </Dialog>

      <Dialog open={deptDialog !== null} onClose={() => setDeptDialog(null)} fullWidth maxWidth="sm">
        <DialogTitle>{deptDialog === "edit" ? t("people.editDept") : t("people.addDept")}</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            <TextField
              label={t("people.colCode")}
              value={deptForm.code}
              onChange={(e) => setDeptForm({ ...deptForm, code: e.target.value })}
              size="small"
              disabled={deptDialog === "edit"}
            />
            <TextField
              label={t("people.colName")}
              value={deptForm.name}
              onChange={(e) => setDeptForm({ ...deptForm, name: e.target.value })}
              size="small"
            />
            <TextField
              select
              label={t("people.colAllowedRoles")}
              value={deptForm.allowedRoles ?? []}
              onChange={(e) => {
                const v = e.target.value;
                setDeptForm({
                  ...deptForm,
                  allowedRoles: typeof v === "string" ? v.split(",") : (v as string[]),
                });
              }}
              size="small"
              slotProps={{ select: { multiple: true } }}
            >
              {ALL_ROLES.map((r) => (
                <MenuItem key={r} value={r}>
                  {t(roleLabelKey(r))}
                </MenuItem>
              ))}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <CancelButton onClick={() => setDeptDialog(null)}>
            {t("common.cancel", { defaultValue: "Hủy" })}
          </CancelButton>
          <ButtonPrimary onClick={() => void saveDepartment()}>{t("common.save", { defaultValue: "Lưu" })}</ButtonPrimary>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
