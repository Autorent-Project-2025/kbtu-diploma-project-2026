import api from "./axios";
import type { Role } from "../types/Role";

export async function getRoles(): Promise<Role[]> {
  const res = await api.get("/identity/roles");
  const roles = Array.isArray(res.data) ? res.data : [];

  return roles.map((role: any) => ({
    id: role.id,
    name: role.name,
    permissions: Array.isArray(role.permissions) ? role.permissions : [],
    directPermissions: Array.isArray(role.directPermissions)
      ? role.directPermissions
      : Array.isArray(role.permissions)
      ? role.permissions
      : [],
    parentRoles: Array.isArray(role.parentRoles)
      ? role.parentRoles.map((parentRole: any) => ({
          id: parentRole.id,
          name: parentRole.name,
        }))
      : [],
  })) as Role[];
}

export interface CreateRolePayload {
  name: string;
  permissionIds?: string[];
  parentRoleIds?: string[];
}

export async function createRole(payload: CreateRolePayload): Promise<void> {
  await api.post("/identity/roles", payload);
}

export async function assignPermissionToRole(roleId: string, permissionId: string): Promise<void> {
  await api.post(`/identity/roles/${roleId}/permissions`, { permissionId });
}

export async function removePermissionFromRole(roleId: string, permissionId: string): Promise<void> {
  await api.delete(`/identity/roles/${roleId}/permissions/${permissionId}`);
}

export async function assignParentRoleToRole(roleId: string, parentRoleId: string): Promise<void> {
  await api.post(`/identity/roles/${roleId}/parents`, { parentRoleId });
}

export async function removeParentRoleFromRole(roleId: string, parentRoleId: string): Promise<void> {
  await api.delete(`/identity/roles/${roleId}/parents/${parentRoleId}`);
}
