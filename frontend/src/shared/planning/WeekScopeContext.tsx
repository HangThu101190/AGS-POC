import {
  createContext,
  startTransition,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { fetchWeeklyPlan, type WeeklyPlanDto } from "@/shared/api/plansApi";
import {
  buildWeekMeta,
  currentWeekId,
  parseWeekId,
  type WeekMeta,
} from "@/shared/planning/weekCalendar";

type WeekScopeValue = {
  weekId: string;
  weekMeta: WeekMeta;
  plan: WeeklyPlanDto | null;
  planLoading: boolean;
  planError: boolean;
  setWeekId: (weekId: string) => void;
  refreshPlan: () => void;
};

const WeekScopeContext = createContext<WeekScopeValue | null>(null);

export function WeekScopeProvider({ children }: { children: ReactNode }) {
  const [weekId, setWeekIdState] = useState(currentWeekId);
  const [plan, setPlan] = useState<WeeklyPlanDto | null>(null);
  const [planWeekId, setPlanWeekId] = useState<string | null>(null);
  const [planFetching, setPlanFetching] = useState(true);
  const [planError, setPlanError] = useState(false);
  const [refreshToken, setRefreshToken] = useState(0);

  const weekMeta = useMemo(() => buildWeekMeta(weekId)!, [weekId]);

  const setWeekId = useCallback((next: string) => {
    const normalized = parseWeekId(next) ? next : currentWeekId();
    startTransition(() => {
      setWeekIdState(normalized);
    });
  }, []);

  const refreshPlan = useCallback(() => setRefreshToken((n) => n + 1), []);

  useEffect(() => {
    let cancelled = false;
    setPlanFetching(true);
    setPlanError(false);
    void fetchWeeklyPlan(weekId)
      .then((p) => {
        if (!cancelled) {
          setPlan(p);
          setPlanWeekId(weekId);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setPlan(null);
          setPlanWeekId(weekId);
          setPlanError(true);
        }
      })
      .finally(() => {
        if (!cancelled) setPlanFetching(false);
      });
    return () => {
      cancelled = true;
    };
  }, [weekId, refreshToken]);

  const planInSync = planWeekId === weekId;
  const planLoading = planFetching && !planInSync;

  const value = useMemo<WeekScopeValue>(
    () => ({
      weekId,
      weekMeta,
      plan: planInSync ? plan : null,
      planLoading,
      planError: planInSync ? planError : false,
      setWeekId,
      refreshPlan,
    }),
    [
      weekId,
      weekMeta,
      plan,
      planInSync,
      planLoading,
      planError,
      setWeekId,
      refreshPlan,
    ],
  );

  return <WeekScopeContext.Provider value={value}>{children}</WeekScopeContext.Provider>;
}

export function useWeekScope(): WeekScopeValue {
  const ctx = useContext(WeekScopeContext);
  if (!ctx) {
    throw new Error("useWeekScope must be used within WeekScopeProvider");
  }
  return ctx;
}

export function useOptionalWeekScope(): WeekScopeValue | null {
  return useContext(WeekScopeContext);
}
