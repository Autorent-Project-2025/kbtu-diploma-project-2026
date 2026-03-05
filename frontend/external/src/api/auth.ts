import api from "./axios";

export interface ActivationTokenStatus {
  isValid: boolean;
  expiresAtUtc?: string | null;
  reason?: "invalid" | "used" | "expired" | null;
}

export async function login(email: string, password: string) {
  const res = await api.post("/identity/auth/login", { email, password });
  return res.data.accessToken ?? res.data.token;
}

export async function getActivationTokenStatus(activationToken: string): Promise<ActivationTokenStatus> {
  const res = await api.get("/identity/auth/activation-status", {
    params: { activationToken },
  });
  return res.data as ActivationTokenStatus;
}

export async function activateUser(activationToken: string, password: string) {
  const res = await api.post("/identity/auth/activate", { activationToken, password });
  return res.data;
}
