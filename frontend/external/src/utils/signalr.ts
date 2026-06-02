import { createChatConnection as createSharedChatConnection } from "@shared/signalr";
import { config } from "../config";

export function createChatConnection() {
  return createSharedChatConnection(config.api.baseURL);
}
