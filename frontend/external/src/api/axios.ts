import { createApiClient } from "@shared/apiClient";
import { config } from "../config";
import { auth } from "../store/auth";

const api = createApiClient({
  baseURL: config.api.baseURL,
  auth,
  camelCaseResponse: true,
  forbiddenRedirectPath: "/403",
});

export default api;
