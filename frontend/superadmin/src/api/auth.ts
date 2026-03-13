import api from "./axios";

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
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
