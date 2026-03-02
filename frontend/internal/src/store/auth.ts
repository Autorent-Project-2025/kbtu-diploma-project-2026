import { reactive } from "vue";
import { login as apiLogin } from "../api/auth";

interface JwtPayload {
  sub?: string;
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
    const token = await apiLogin(email, password);
    this.token = token;
    localStorage.setItem("token", token);
    localStorage.setItem("tokenTimestamp", Date.now().toString());
  },

  logout() {
    this.token = "";
    localStorage.removeItem("token");
    localStorage.removeItem("tokenTimestamp");
  },

  isTokenExpired(): boolean {
    const token = localStorage.getItem("token");
    const timestampStr = localStorage.getItem("tokenTimestamp");

    if (!token || !timestampStr) {
      return true;
    }

    const timestamp = parseInt(timestampStr, 10);
    if (isNaN(timestamp)) {
      return true;
    }

    const expiryHours = parseInt(import.meta.env.VITE_TOKEN_EXPIRY_HOURS || "3", 10);
    const expiryMs = expiryHours * 60 * 60 * 1000;
    return Date.now() - timestamp >= expiryMs;
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

    return this.getPermissions().some(
      (claimPermission) => claimPermission.toLowerCase() === permission.toLowerCase()
    );
  },
});
