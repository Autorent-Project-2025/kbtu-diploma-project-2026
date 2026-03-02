import api from "./axios";

export async function login(email: string, password: string) {
  const res = await api.post("/users/auth/login", { email, password });
  return res.data.token;
}

export async function register(name: string, email: string, password: string) {
  const res = await api.post("/users/auth/register", { name, email, password });
  return res.data.token;
}
