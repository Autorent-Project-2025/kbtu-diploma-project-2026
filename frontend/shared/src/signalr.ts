import * as signalR from "@microsoft/signalr";
import { normalizeApiBaseUrl } from "./apiClient";

export function createChatConnection(baseURL: string): signalR.HubConnection {
  const normalizedBaseUrl = normalizeApiBaseUrl(baseURL);

  return new signalR.HubConnectionBuilder()
    .withUrl(`${normalizedBaseUrl}/chat/hubs/conversation`, {
      accessTokenFactory: () => localStorage.getItem("token") || "",
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}
