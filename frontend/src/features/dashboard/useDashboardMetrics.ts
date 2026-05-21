import { useQuery } from "@tanstack/react-query";
import { fetchFlightsPage } from "@/shared/api/flightsApi";
import { fetchMonitoringSnapshotData } from "@/shared/api/monitoringApi";
import { fetchReconcileSummary } from "@/shared/api/reconcileApi";
import { fetchPhanCongSlot } from "@/shared/api/phanCongSlotApi";
import { useWeekScope } from "@/shared/planning/WeekScopeContext";

function normalizePlanStatus(status: string | undefined): string {
  return (status ?? "draft").toLowerCase().replace(/\s+/g, "_");
}

async function countWeekAssignments(weekId: string, departmentCode?: string) {
  const boards = await Promise.all(
    [0, 1, 2, 3, 4, 5, 6].map((dayIdx) =>
      fetchPhanCongSlot({ weekId, dayIdx, departmentCode }).catch(() => null),
    ),
  );
  const seen = new Set<string>();
  let count = 0;
  for (const board of boards) {
    if (!board) continue;
    for (const slot of board.slots) {
      for (const a of slot.assignments) {
        if (seen.has(a.id)) continue;
        seen.add(a.id);
        count += 1;
      }
    }
  }
  return count;
}

function countRevisionFlags(
  employees: { days: { mismatch: boolean }[] }[] | undefined,
): number {
  if (!employees) return 0;
  let n = 0;
  for (const emp of employees) {
    for (const day of emp.days) {
      if (day.mismatch) n += 1;
    }
  }
  return n;
}

export function useDashboardMetrics(departmentCode?: string) {
  const { weekId, plan: weekPlan, planLoading, planError, refreshPlan } = useWeekScope();

  const planQuery = {
    data: weekPlan,
    isLoading: planLoading,
    isError: planError,
    refetch: refreshPlan,
  };

  const flightsQuery = useQuery({
    queryKey: ["dashboard", "flights", weekId, departmentCode ?? "all"],
    queryFn: ({ signal }) =>
      fetchFlightsPage(
        { page: 0, pageSize: 500, weekId, ...(departmentCode ? { departmentCode } : {}) },
        signal,
      ),
    staleTime: 30_000,
  });

  const monitoringQuery = useQuery({
    queryKey: ["dashboard", "monitoring"],
    queryFn: ({ signal }) => fetchMonitoringSnapshotData(signal),
    staleTime: 15_000,
  });

  const assignmentsQuery = useQuery({
    queryKey: ["dashboard", "assignments", weekId, departmentCode],
    queryFn: () => countWeekAssignments(weekId, departmentCode),
    staleTime: 30_000,
  });

  const reconcileQuery = useQuery({
    queryKey: ["dashboard", "reconcile", weekId, departmentCode ?? "PVHK_DI"],
    queryFn: () => fetchReconcileSummary(weekId, departmentCode ?? "PVHK_DI"),
    staleTime: 60_000,
  });

  const plan = weekPlan;
  const planStatus = normalizePlanStatus(plan?.status);
  const flights = flightsQuery.data?.items ?? [];
  const totalFlights = flights.length;
  const delayedCount = flights.filter((f) => f.isDelayed || (f.delayMinutes ?? 0) > 0).length;
  const totalSlots = plan?.slots?.length ?? 0;
  const totalAssignments = assignmentsQuery.data ?? 0;
  const checkedIn =
    (monitoringQuery.data?.inShiftCount ?? 0) + (monitoringQuery.data?.checkedOutCount ?? 0);
  const revisions = countRevisionFlags(reconcileQuery.data?.employees);

  const isLoading =
    planQuery.isLoading ||
    flightsQuery.isLoading ||
    monitoringQuery.isLoading ||
    assignmentsQuery.isLoading;

  const isError =
    planQuery.isError || flightsQuery.isError || monitoringQuery.isError || assignmentsQuery.isError;

  const refetch = () => {
    void planQuery.refetch();
    void flightsQuery.refetch();
    void monitoringQuery.refetch();
    void assignmentsQuery.refetch();
    void reconcileQuery.refetch();
  };

  return {
    weekId,
    plan,
    planStatus,
    totalFlights,
    delayedCount,
    totalSlots,
    totalAssignments,
    checkedIn,
    revisions,
    isLoading,
    isError,
    refetch,
  };
}
