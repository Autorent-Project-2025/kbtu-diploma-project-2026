import api from "./axios";

export async function login(email: string, password: string): Promise<string> {
  const res = await api.post("/identity/auth/login", { email, password });
  return res.data.accessToken ?? res.data.token;
}
