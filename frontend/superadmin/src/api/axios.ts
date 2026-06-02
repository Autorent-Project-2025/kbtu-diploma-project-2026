import { createApiClient, normalizeApiBaseUrl } from "@shared/apiClient";
import { auth } from "../store/auth";

const api = createApiClient({
  baseURL: normalizeApiBaseUrl(
    import.meta.env.VITE_API_URL || "http://localhost:9186",
  ),
  auth,
});

export default api;
