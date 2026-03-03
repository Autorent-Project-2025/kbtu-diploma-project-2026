import api from "./axios";
import type { Permission } from "../types/Permission";

export async function getPermissions(): Promise<Permission[]> {
  const res = await api.get("/identity/permissions");
  return (res.data ?? []) as Permission[];
}
