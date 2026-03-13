import { reactive } from "vue";
import { login as apiLogin, refreshAccessToken } from "../api/auth";

interface JwtPayload {
  sub?: string;
  exp?: number;
  permissions?: string[] | string;
  [key: string]: unknown;
}

function decodeJwtPayload(token: string): JwtPayload | null {
  const parts = token.split(".");
  if (parts.length !== 3 || !parts[1]) return null;

  try {
    const payloadPart = parts[1];
    const payload = payloadPart
      .replace(/-/g, "+")
      .replace(/_/g, "/")
      .padEnd(Math.ceil(payloadPart.length / 4) * 4, "=");
    return JSON.parse(atob(payload)) as JwtPayload;
  } catch {
    return null;
  }
}

export const auth = reactive({
  token: localStorage.getItem("token") || "",

  async login(email: string, password: string) {
    const { accessToken, refreshToken } = await apiLogin(email, password);
    this.token = accessToken;
    localStorage.setItem("token", accessToken);
    if (refreshToken) {
      localStorage.setItem("refreshToken", refreshToken);
    }
  },

  logout() {
    this.token = "";
    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("tokenTimestamp");
  },

  isTokenExpired(): boolean {
    const token = localStorage.getItem("token");
    if (!token) return true;

    const payload = decodeJwtPayload(token);
    if (payload?.exp) {
      return Date.now() / 1000 >= payload.exp - 10;
    }

    const timestampStr = localStorage.getItem("tokenTimestamp");
    if (!timestampStr) return true;
    const timestamp = parseInt(timestampStr, 10);
    if (isNaN(timestamp)) return true;
    const expiryHours = parseInt(
      import.meta.env.VITE_TOKEN_EXPIRY_HOURS || "3",
      10,
    );
    return Date.now() - timestamp >= expiryHours * 60 * 60 * 1000;
  },

  async tryRefresh(): Promise<boolean> {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) return false;

    try {
      const { accessToken, refreshToken: newRefreshToken } =
        await refreshAccessToken(refreshToken);
      this.token = accessToken;
      localStorage.setItem("token", accessToken);
      if (newRefreshToken) {
        localStorage.setItem("refreshToken", newRefreshToken);
      }
      return true;
    } catch {
      this.logout();
      return false;
    }
  },

  checkTokenValidity(): boolean {
    if (this.isTokenExpired()) {
      this.logout();
      return false;
    }
    return true;
  },

  getPermissions(): string[] {
    const token = this.token || localStorage.getItem("token") || "";
    const payload = decodeJwtPayload(token);
    const claim = payload?.permissions;
    if (!claim) return [];
    if (Array.isArray(claim)) return claim;
    return [claim];
  },

  hasPermission(permission: string): boolean {
    if (!permission) return false;
    const expected = permission.toLowerCase();
    return this.getPermissions().some((p) => {
      const normalized = p.toLowerCase();
      return normalized === expected || normalized === "*";
    });
  },
});
