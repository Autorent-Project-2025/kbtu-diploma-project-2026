import api from "./axios";
import type { Role } from "../types/Role";

export async function getRoles(): Promise<Role[]> {
  const res = await api.get("/identity/roles");
  return (res.data ?? []) as Role[];
}
