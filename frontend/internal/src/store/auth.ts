import { createAuthStore } from "@shared/auth";

export const auth = createAuthStore({
  api: {
    async login(email, password) {
      const api = await import("../api/auth");
      return api.login(email, password);
    },
    async refreshAccessToken(refreshToken) {
      const api = await import("../api/auth");
      return api.refreshAccessToken(refreshToken);
    },
  },
  wildcardPermissions: true,
});
