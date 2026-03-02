import api from "./axios";

export async function login(email: string, password: string) {
  const res = await api.post("/identity/auth/login", { email, password });
  return res.data.accessToken ?? res.data.token;
}

export async function activateUser(activationToken: string, password: string) {
  const res = await api.post("/identity/auth/activate", { activationToken, password });
  return res.data;
}
