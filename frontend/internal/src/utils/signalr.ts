import * as signalR from "@microsoft/signalr";

const rawApiUrl = import.meta.env.VITE_API_URL || "http://localhost:9186";
const baseUrl = rawApiUrl.replace(/\/+$/, "");

export function createChatConnection(): signalR.HubConnection {
  const token = localStorage.getItem("token") || "";

  return new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/chat/hubs/conversation`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}
