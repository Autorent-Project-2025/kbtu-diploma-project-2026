import * as signalR from "@microsoft/signalr";
import { config } from "../config";

export function createChatConnection(): signalR.HubConnection {
  const token = localStorage.getItem("token") || "";

  return new signalR.HubConnectionBuilder()
    .withUrl(`${config.api.baseURL}/chat/hubs/conversation`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}
