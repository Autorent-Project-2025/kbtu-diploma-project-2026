import { createChatConnection as createSharedChatConnection } from "@shared/signalr";
import { normalizeApiBaseUrl } from "@shared/apiClient";

export function createChatConnection() {
  return createSharedChatConnection(
    normalizeApiBaseUrl(import.meta.env.VITE_API_URL || "http://localhost:9186"),
  );
}
