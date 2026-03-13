import api from "./axios";

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
}

export interface ActivationTokenStatus {
  isValid: boolean;
  expiresAtUtc?: string | null;
  reason?: "invalid" | "used" | "expired" | null;
}

export async function login(
  email: string,
  password: string,
): Promise<LoginResponse> {
  const res = await api.post("/identity/auth/login", { email, password });
  return {
    accessToken: res.data.accessToken ?? res.data.token,
    refreshToken: res.data.refreshToken ?? "",
  };
}

export async function refreshAccessToken(
  refreshToken: string,
): Promise<LoginResponse> {
  const res = await api.post("/identity/auth/refresh", { refreshToken });
  return {
    accessToken: res.data.accessToken ?? res.data.token,
    refreshToken: res.data.refreshToken ?? refreshToken,
  };
}

export async function getActivationTokenStatus(
  activationToken: string,
): Promise<ActivationTokenStatus> {
  const res = await api.get("/identity/auth/activation-status", {
    params: { activationToken },
  });
  return res.data as ActivationTokenStatus;
}

export async function activateUser(activationToken: string, password: string) {
  const res = await api.post("/identity/auth/activate", {
    activationToken,
    password,
  });
  return res.data;
}
