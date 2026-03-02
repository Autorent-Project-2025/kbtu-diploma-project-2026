import api from "./axios";

export async function login(email: string, password: string) {
  const res = await api.post("/identity/auth/login", { email, password });
  return res.data.accessToken ?? res.data.token;
}

export async function register(username: string, email: string, password: string) {
  const res = await api.post("/identity/auth/register", { username, email, password });
  return res.data;
}
