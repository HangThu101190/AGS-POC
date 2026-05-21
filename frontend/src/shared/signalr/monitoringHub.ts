import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { getAccessToken } from "@/shared/auth/tokenStorage";

const HUB_PATH = "/hubs/monitoring";

export type MonitoringHubEvent = {
  type: "snapshotUpdated";
};

export function isSignalREnabled(): boolean {
  return import.meta.env.VITE_SIGNALR_ENABLED === "true";
}

export function getMonitoringHubUrl(): string {
  const base = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";
  if (base) return `${base}${HUB_PATH}`;
  return HUB_PATH;
}

export async function connectMonitoringHub(
  onEvent: (event: MonitoringHubEvent) => void,
  accessToken?: string,
): Promise<() => void> {
  if (!isSignalREnabled()) {
    return () => undefined;
  }

  const token = accessToken ?? getAccessToken();
  if (!token) {
    console.warn("[SignalR] No access token — monitoring hub not connected.");
    return () => undefined;
  }

  const connection: HubConnection = new HubConnectionBuilder()
    .withUrl(getMonitoringHubUrl(), {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();

  connection.on("snapshotUpdated", () => {
    onEvent({ type: "snapshotUpdated" });
  });

  try {
    await connection.start();
    if (connection.state === HubConnectionState.Connected) {
      await connection.invoke("Subscribe");
    }
  } catch (err) {
    console.warn("[SignalR] Monitoring hub connect failed — using REST polling.", err);
    return () => undefined;
  }

  return () => {
    void connection.invoke("Unsubscribe").catch(() => undefined);
    void connection.stop().catch(() => undefined);
  };
}
