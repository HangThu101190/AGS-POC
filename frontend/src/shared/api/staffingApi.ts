import { apiClient } from "@/shared/api/client";

export type StaffingDay = {
  plan: {
    id: string;
    weekId: string;
    dayIdx: number;
    departmentCode: string;
    status: string;
    headerJson?: string | null;
    confirmedAt?: string | null;
    publishedAt?: string | null;
    publishedByEmployeeId?: string | null;
    lockReason?: string | null;
    isLocked?: boolean;
  };
  lines: Array<{
    id: string;
    flightId: string;
    segment: "Qn" | "Qt" | string;
    sortOrder: number;
    targetManning: number;
    proposedManning: number;
  }>;
  flights: Array<{
    id: string;
    flightNo: string;
    departureFlightNo?: string | null;
    route: string;
    sta: string;
    std: string;
    eta?: string | null;
    etd?: string | null;
    etaDelayMinutes?: number;
    etdDelayMinutes?: number;
    delayMinutes?: number;
    isDelayed?: boolean;
    aircraft?: string | null;
    isVip: boolean;
    manning: number;
    belt?: string | null;
  }>;
  assignments: Array<{
    id: string;
    staffingLineId: string;
    employeeId: string;
    employeeCode: string;
    employeeName: string;
    employeeDepartmentCode?: string;
    role: string;
    workStart: string;
    workEnd: string;
    isOvertime: boolean;
    attendance?: { status: string; checkInAt?: string | null } | null;
  }>;
};

export async function fetchStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<StaffingDay> {
  const { data } = await apiClient.get<StaffingDay>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}`,
    { params: { departmentCode } },
  );
  return data;
}

export async function proposeStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<StaffingDay> {
  const { data } = await apiClient.post<StaffingDay>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/propose`,
    null,
    { params: { departmentCode } },
  );
  return data;
}

export type AutoAssignStaffingResult = {
  proposals: StaffingCrewProposal[];
  warnings: string[];
  summary: {
    totalEmployees: number;
    totalOvertime: number;
    coveragePercent: number;
    unfilledSlots: number;
  };
};

export type StaffingCrewProposal = {
  id: string;
  staffingLineId: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  shiftTemplateId?: string | null;
  shiftTemplateCode?: string | null;
  crewRole: string;
  workStart: string;
  workEnd: string;
  isOvertime: boolean;
  isAutoAssigned: boolean;
  note?: string | null;
};

export type StaffingBioHeader = {
  morningSupEmployeeId?: string | null;
  morningSupName?: string | null;
  eveningSupEmployeeId?: string | null;
  eveningSupName?: string | null;
  radioNote?: string | null;
  assignerEmployeeId?: string | null;
  assignerName?: string | null;
};

export type ShiftTemplate = {
  id: string;
  departmentCode: string;
  code: string;
  name: string;
  startTime: string;
  endTime: string;
  isOvernight: boolean;
  maxHours: number;
  segment?: "Qn" | "Qt" | null;
  sortOrder: number;
  isActive: boolean;
};

export type EmployeeQualification = {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  segment: "Qn" | "Qt" | string;
  crewRole: string;
  proficiency: number;
  isActive: boolean;
};

export async function autoAssignStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<AutoAssignStaffingResult> {
  const { data } = await apiClient.post<AutoAssignStaffingResult>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/auto-assign`,
    null,
    { params: { departmentCode } },
  );
  return data;
}

export async function publishStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
  body?: { lockReason?: string },
): Promise<{ status: string; publishedAt: string; notificationsSent: number }> {
  const { data } = await apiClient.post<{
    status: string;
    publishedAt: string;
    notificationsSent: number;
  }>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/publish`,
    body ?? {},
    { params: { departmentCode } },
  );
  return data;
}

export async function fetchStaffingBioHeader(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<StaffingBioHeader> {
  const { data } = await apiClient.get<StaffingBioHeader>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/bio-header`,
    { params: { departmentCode } },
  );
  return data;
}

export async function putStaffingBioHeader(
  weekId: string,
  dayIdx: number,
  body: {
    morningSupEmployeeId?: string | null;
    eveningSupEmployeeId?: string | null;
    radioNote?: string | null;
    assignerEmployeeId?: string | null;
  },
  departmentCode?: string,
): Promise<StaffingBioHeader> {
  const { data } = await apiClient.put<StaffingBioHeader>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/bio-header`,
    body,
    { params: { departmentCode } },
  );
  return data;
}

export async function fetchShiftTemplates(departmentCode = "PVHK_DI") {
  const { data } = await apiClient.get<ShiftTemplate[]>(
    "/api/v1/staffing/config/shift-templates",
    { params: { departmentCode } },
  );
  return data;
}

export async function upsertShiftTemplate(body: {
  id?: string;
  departmentCode?: string;
  code: string;
  name: string;
  startTime: string;
  endTime: string;
  isOvernight?: boolean;
  maxHours?: number;
  segment?: "Qn" | "Qt" | null;
  sortOrder?: number;
}) {
  if (body.id) {
    const { id, ...rest } = body;
    const { data } = await apiClient.put<ShiftTemplate>(
      `/api/v1/staffing/config/shift-templates/${id}`,
      rest,
    );
    return data;
  }
  const { data } = await apiClient.post<ShiftTemplate>(
    "/api/v1/staffing/config/shift-templates",
    body,
  );
  return data;
}

export async function deleteShiftTemplate(id: string) {
  await apiClient.delete(`/api/v1/staffing/config/shift-templates/${id}`);
}

export async function fetchEmployeeQualifications(departmentCode = "PVHK_DI") {
  const { data } = await apiClient.get<EmployeeQualification[]>(
    "/api/v1/staffing/config/employee-qualifications",
    { params: { departmentCode } },
  );
  return data;
}

export async function upsertEmployeeQualifications(
  employeeId: string,
  qualifications: Array<{
    segment: "Qn" | "Qt";
    crewRole: string;
    proficiency: number;
    isActive?: boolean;
  }>,
) {
  await apiClient.put(`/api/v1/employees/${employeeId}/qualifications`, { qualifications });
}

export async function confirmStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<{ slotsUpdated: number; assignmentsCreated: number }> {
  const { data } = await apiClient.post<{ slotsUpdated: number; assignmentsCreated: number }>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/confirm`,
    null,
    { params: { departmentCode } },
  );
  return data;
}

export async function exportStaffingDay(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<Blob> {
  const { data } = await apiClient.get<Blob>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/export`,
    { params: { departmentCode }, responseType: "blob" },
  );
  return data;
}

export async function exportStaffingWeek(
  weekId: string,
  departmentCode?: string,
): Promise<Blob> {
  const { data } = await apiClient.get<Blob>(
    `/api/v1/staffing/weeks/${encodeURIComponent(weekId)}/export`,
    { params: { departmentCode }, responseType: "blob" },
  );
  return data;
}

export type StaffingRosterEntry = {
  employeeId: string;
  code: string;
  name: string;
  departmentCode?: string;
  eligible: boolean;
  eligibleAsOvertime: boolean;
  availabilityStatus?: string | null;
  leaveTypeCode?: string | null;
  assignedLineIds: string[];
};

export async function fetchStaffingRoster(
  weekId: string,
  dayIdx: number,
  departmentCode?: string,
): Promise<StaffingRosterEntry[]> {
  const { data } = await apiClient.get<StaffingRosterEntry[]>(
    `/api/v1/staffing/days/${encodeURIComponent(weekId)}/${dayIdx}/roster`,
    { params: { departmentCode } },
  );
  return data;
}

export async function patchStaffingLine(lineId: string, targetManning: number) {
  const { data } = await apiClient.patch<{ id: string; targetManning: number }>(
    `/api/v1/staffing/lines/${lineId}`,
    { targetManning },
  );
  return data;
}

export async function createStaffingAssignment(body: {
  staffingLineId: string;
  employeeId: string;
  role?: string;
  workStart?: string;
  workEnd?: string;
  isOvertime?: boolean;
}) {
  const { data } = await apiClient.post<StaffingDay["assignments"][number]>(
    "/api/v1/staffing/assignments",
    {
      role: "General",
      workStart: "06:00",
      workEnd: "14:00",
      isOvertime: false,
      ...body,
    },
  );
  return data;
}

export async function deleteStaffingAssignment(id: string) {
  await apiClient.delete(`/api/v1/staffing/assignments/${id}`);
}

export async function updateStaffingAssignment(
  id: string,
  body: {
    role?: string;
    workStart: string;
    workEnd: string;
    isOvertime?: boolean;
  },
) {
  const { data } = await apiClient.patch<StaffingDay["assignments"][number]>(
    `/api/v1/staffing/assignments/${id}`,
    {
      role: body.role ?? "General",
      workStart: body.workStart,
      workEnd: body.workEnd,
      isOvertime: body.isOvertime ?? false,
    },
  );
  return data;
}

export async function createAircraftManningRule(body: { aircraftPattern: string; baseManning: number }) {
  const { data } = await apiClient.post<AircraftManningRule>(
    "/api/v1/staffing/config/aircraft-manning-rules",
    body,
  );
  return data;
}

export async function updateAircraftManningRule(
  id: string,
  body: { aircraftPattern: string; baseManning: number },
) {
  const { data } = await apiClient.put<AircraftManningRule>(
    `/api/v1/staffing/config/aircraft-manning-rules/${id}`,
    body,
  );
  return data;
}

export async function deleteAircraftManningRule(id: string) {
  await apiClient.delete(`/api/v1/staffing/config/aircraft-manning-rules/${id}`);
}

export async function createAirlineManningRule(body: { airlinePrefix: string; multiplier: number }) {
  const { data } = await apiClient.post<AirlineManningRule>(
    "/api/v1/staffing/config/airline-manning-rules",
    body,
  );
  return data;
}

export async function updateAirlineManningRule(
  id: string,
  body: { airlinePrefix: string; multiplier: number },
) {
  const { data } = await apiClient.put<AirlineManningRule>(
    `/api/v1/staffing/config/airline-manning-rules/${id}`,
    body,
  );
  return data;
}

export async function deleteAirlineManningRule(id: string) {
  await apiClient.delete(`/api/v1/staffing/config/airline-manning-rules/${id}`);
}

export type AircraftManningRule = {
  id: string;
  aircraftPattern: string;
  baseManning: number;
  isActive: boolean;
};

export type AirlineManningRule = {
  id: string;
  airlinePrefix: string;
  multiplier: number;
  isActive: boolean;
};

export type ShiftCheckInPolicy = {
  id: string;
  departmentCode?: string | null;
  bucketKey: string;
  segmentStart: string;
  segmentEnd: string;
  checkInEarliestMinutesBefore: number;
  checkInLatestMinutesAfterStart: number;
  checkOutEarliestMinutesBeforeEnd: number;
  checkOutLatestMinutesAfterEnd: number;
  requireNoteWhenLate: boolean;
  isActive: boolean;
};

export async function fetchAircraftManningRules() {
  const { data } = await apiClient.get<AircraftManningRule[]>(
    "/api/v1/staffing/config/aircraft-manning-rules",
  );
  return data;
}

export async function fetchAirlineManningRules() {
  const { data } = await apiClient.get<AirlineManningRule[]>(
    "/api/v1/staffing/config/airline-manning-rules",
  );
  return data;
}

export async function fetchShiftCheckInPolicies(departmentCode?: string) {
  const { data } = await apiClient.get<ShiftCheckInPolicy[]>(
    "/api/v1/staffing/config/shift-check-in-policies",
    { params: { departmentCode } },
  );
  return data;
}
