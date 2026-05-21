import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import { getAccessToken } from "@/shared/auth/tokenStorage";

const HUB_PATH = "/hubs/staff";

export type StaffHubEvent = {
  type: "shiftChanged";
  flightNo?: string;
  delayMinutes?: number;
};

export function getStaffHubUrl(): string {
  const base = import.meta.env.VITE_API_BASE_URL?.replace(/\/$/, "") ?? "";
  if (base) return `${base}${HUB_PATH}`;
  return HUB_PATH;
}

export async function connectStaffHub(
  onEvent: (event: StaffHubEvent) => void,
  accessToken?: string,
): Promise<() => void> {
  if (import.meta.env.VITE_SIGNALR_ENABLED !== "true") {
    return () => undefined;
  }

  const token = accessToken ?? getAccessToken();
  if (!token) {
    return () => undefined;
  }

  const connection: HubConnection = new HubConnectionBuilder()
    .withUrl(getStaffHubUrl(), {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(import.meta.env.DEV ? LogLevel.Information : LogLevel.Warning)
    .build();

  connection.on("shiftChanged", (payload?: { flightNo?: string; delayMinutes?: number }) => {
    onEvent({
      type: "shiftChanged",
      flightNo: payload?.flightNo,
      delayMinutes: payload?.delayMinutes,
    });
  });

  try {
    await connection.start();
    if (connection.state === HubConnectionState.Connected) {
      await connection.invoke("Subscribe");
    }
  } catch (err) {
    console.warn("[SignalR] Staff hub connect failed.", err);
    return () => undefined;
  }

  return () => {
    void connection.invoke("Unsubscribe").catch(() => undefined);
    void connection.stop().catch(() => undefined);
  };
}
