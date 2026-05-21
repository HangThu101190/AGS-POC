import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { connectPlanningHub, type PlanningHubEvent } from "@/shared/signalr/planningHub";

type Options = {
  onEvent?: (event: PlanningHubEvent) => void;
  /** Refresh all staffing days in the week when hub reports staffing/attendance changes. */
  invalidateStaffingWeek?: boolean;
};

export function usePlanningDaySync(
  weekId: string | undefined,
  dayIdx: number | undefined,
  options?: Options,
) {
  const queryClient = useQueryClient();
  const onEvent = options?.onEvent;
  const invalidateStaffingWeek = options?.invalidateStaffingWeek ?? false;

  useEffect(() => {
    if (!weekId || dayIdx === undefined) {
      return;
    }

    let dispose: (() => void) | undefined;
    void connectPlanningHub(weekId, dayIdx, (event) => {
      onEvent?.(event);
      const refreshFlights =
        event.type === "flightsDayUpdated" ||
        event.type === "staffingDayUpdated" ||
        event.type === "attendanceUpdated" ||
        (event.type === "importProgress" &&
          (event.status === "completed" || event.status === "failed"));

      if (refreshFlights) {
        void queryClient.invalidateQueries({ queryKey: ["flights"] });
      }

      if (event.type === "flightSchedulePublished") {
        void queryClient.invalidateQueries({ queryKey: ["staffing", weekId] });
        void queryClient.invalidateQueries({ queryKey: ["flights"] });
      }

      if (
        event.type === "staffingDayUpdated" ||
        event.type === "attendanceUpdated" ||
        (invalidateStaffingWeek && event.type === "flightsDayUpdated")
      ) {
        if (invalidateStaffingWeek) {
          void queryClient.invalidateQueries({ queryKey: ["staffing", weekId] });
        } else {
          void queryClient.invalidateQueries({ queryKey: ["staffing", weekId, dayIdx] });
        }
      }
    }).then((fn) => {
      dispose = fn;
    });

    return () => {
      dispose?.();
    };
  }, [weekId, dayIdx, queryClient, onEvent, invalidateStaffingWeek]);
}
