import { reactive } from "vue";

export interface LoginResponse {
  accessToken: string;
  refreshToken?: string;
}

export interface AuthApi {
  login(email: string, password: string): Promise<LoginResponse>;
  refreshAccessToken(refreshToken: string): Promise<LoginResponse>;
}

export interface CreateAuthStoreOptions {
  api: AuthApi;
  storeLoginEmail?: boolean;
  wildcardPermissions?: boolean;
}

export interface JwtPayload {
  sub?: string;
  exp?: number;
  email?: string;
  preferred_username?: string;
  unique_name?: string;
  upn?: string;
  permissions?: string[] | string;
  role?: string[] | string;
  roles?: string[] | string;
  actor_type?: string;
  subject_type?: string;
  "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"?:
    | string[]
    | string;
  [key: string]: unknown;
}

export function readStringClaim(value: unknown): string | null {
  if (typeof value !== "string") return null;

  const normalized = value.trim().toLowerCase();
  return normalized || null;
}

export function readStringClaims(value: unknown): string[] {
  const values = Array.isArray(value) ? value : [value];

  return values
    .filter((claim): claim is string => typeof claim === "string")
    .map((claim) => claim.trim())
    .filter((claim) => claim.length > 0);
}

export function decodeJwtPayload(token: string): JwtPayload | null {
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

function getStoredToken(): string {
  return localStorage.getItem("token") || "";
}

export function createAuthStore(options: CreateAuthStoreOptions) {
  const state = reactive({
    token: getStoredToken(),
    user: null as unknown,

    async login(email: string, password: string) {
      const { accessToken, refreshToken } = await options.api.login(
        email,
        password,
      );

      state.token = accessToken;
      localStorage.setItem("token", accessToken);
      localStorage.setItem("tokenTimestamp", Date.now().toString());

      if (options.storeLoginEmail) {
        localStorage.setItem("loginEmail", email.trim().toLowerCase());
      }

      if (refreshToken) {
        localStorage.setItem("refreshToken", refreshToken);
      }
    },

    logout() {
      state.token = "";
      state.user = null;
      localStorage.removeItem("token");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("tokenTimestamp");
      localStorage.removeItem("user");
      localStorage.removeItem("loginEmail");
    },

    isTokenExpired(): boolean {
      const token = getStoredToken();
      if (!token) return true;

      const payload = decodeJwtPayload(token);
      if (payload?.exp) {
        return Date.now() / 1000 >= payload.exp - 10;
      }

      const timestampStr = localStorage.getItem("tokenTimestamp");
      if (!timestampStr) return true;

      const timestamp = parseInt(timestampStr, 10);
      if (Number.isNaN(timestamp)) return true;

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
          await options.api.refreshAccessToken(refreshToken);

        state.token = accessToken;
        localStorage.setItem("token", accessToken);
        localStorage.setItem("tokenTimestamp", Date.now().toString());

        if (newRefreshToken) {
          localStorage.setItem("refreshToken", newRefreshToken);
        }

        return true;
      } catch {
        state.logout();
        return false;
      }
    },

    checkTokenValidity(): boolean {
      if (state.isTokenExpired()) {
        state.logout();
        return false;
      }

      return true;
    },

    getPermissions(): string[] {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);
      return readStringClaims(payload?.permissions);
    },

    hasPermission(permission: string): boolean {
      if (!permission) return false;

      const expected = permission.toLowerCase();
      return state.getPermissions().some((claimPermission: string) => {
        const normalized = claimPermission.toLowerCase();
        return (
          normalized === expected ||
          (options.wildcardPermissions === true && normalized === "*")
        );
      });
    },

    getRoles(): string[] {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);
      const roleClaims = [
        ...readStringClaims(payload?.roles),
        ...readStringClaims(payload?.role),
        ...readStringClaims(
          payload?.[
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
          ],
        ),
      ];

      return [...new Set(roleClaims)];
    },

    hasRole(role: string): boolean {
      const expected = readStringClaim(role);
      if (!expected) return false;

      return state.getRoles().some((claimRole: string) => {
        const normalized = readStringClaim(claimRole);
        return (
          normalized === expected ||
          (options.wildcardPermissions === true && normalized === "*")
        );
      });
    },

    getActorType(): string | null {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);
      return readStringClaim(payload?.actor_type);
    },

    getSubjectType(): string | null {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);
      return readStringClaim(payload?.subject_type);
    },

    isActorType(type: string): boolean {
      const actorType = state.getActorType();
      const normalizedType = readStringClaim(type);
      return !!actorType && !!normalizedType && actorType === normalizedType;
    },

    isSubjectType(type: string): boolean {
      const subjectType = state.getSubjectType();
      const normalizedType = readStringClaim(type);
      return !!subjectType && !!normalizedType && subjectType === normalizedType;
    },

    getUserId(): string | null {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);
      return typeof payload?.sub === "string" ? payload.sub : null;
    },

    getEmail(): string | null {
      const token = state.token || getStoredToken();
      const payload = decodeJwtPayload(token);

      return (
        readStringClaim(payload?.email) ||
        readStringClaim(payload?.preferred_username) ||
        readStringClaim(payload?.unique_name) ||
        readStringClaim(payload?.upn) ||
        readStringClaim(localStorage.getItem("loginEmail"))
      );
    },
  });

  return state;
}
