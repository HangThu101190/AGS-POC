import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { getAccessToken } from "@/shared/auth/tokenStorage";

const HUB_PATH = "/hubs/planning";

export type PlanningHubEvent =
  | { type: "flightsDayUpdated"; weekId: string; dayIdx: number; change: string }
  | { type: "staffingDayUpdated"; weekId: string; dayIdx: number; change: string; planStatus: string }
  | { type: "attendanceUpdated"; weekId: string; dayIdx: number }
  | { type: "importProgress"; weekId: string; dayIdx: number; progressPercent: number; status: string }
  | { type: "flightSchedulePublished"; weekId: string; staleDayIndices: number[] };

export function getPlanningHubUrl(): string {
  const base = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";
  if (base) return `${base}${HUB_PATH}`;
  return HUB_PATH;
}

export async function connectPlanningHub(
  weekId: string,
  dayIdx: number,
  onEvent: (event: PlanningHubEvent) => void,
  accessToken?: string,
): Promise<() => void> {
  const token = accessToken ?? getAccessToken();
  if (!token) {
    return () => undefined;
  }

  const connection: HubConnection = new HubConnectionBuilder()
    .withUrl(getPlanningHubUrl(), { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();

  connection.on("flightsDayUpdated", (payload: { weekId: string; dayIdx: number; change: string }) => {
    onEvent({ type: "flightsDayUpdated", ...payload });
  });
  connection.on(
    "staffingDayUpdated",
    (payload: { weekId: string; dayIdx: number; change: string; planStatus: string }) => {
      onEvent({ type: "staffingDayUpdated", ...payload });
    },
  );
  connection.on("attendanceUpdated", (payload: { weekId: string; dayIdx: number }) => {
    onEvent({ type: "attendanceUpdated", ...payload });
  });
  connection.on(
    "importProgress",
    (payload: { weekId: string; dayIdx: number; progressPercent: number; status: string }) => {
      onEvent({ type: "importProgress", ...payload });
    },
  );
  connection.on(
    "flightSchedulePublished",
    (payload: { weekId: string; staleDayIndices: number[] }) => {
      onEvent({ type: "flightSchedulePublished", ...payload });
    },
  );

  try {
    await connection.start();
    if (connection.state === HubConnectionState.Connected) {
      await connection.invoke("SubscribeDay", weekId, dayIdx);
    }
  } catch (err) {
    console.warn("[SignalR] Planning hub connect failed.", err);
    return () => undefined;
  }

  return () => {
    void connection.invoke("UnsubscribeDay", weekId, dayIdx).catch(() => undefined);
    void connection.stop().catch(() => undefined);
  };
}
