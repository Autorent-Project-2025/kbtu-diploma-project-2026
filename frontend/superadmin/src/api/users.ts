import api from "./axios";
import type { User } from "../types/User";

export interface CreateUserPayload {
  username: string;
  email: string;
  password: string;
  roles?: string[];
}

export interface CreateUserResponse {
  userId: string;
  username: string;
  email: string;
  roles: string[];
}

export async function getUsers(): Promise<User[]> {
  const res = await api.get("/identity/users");
  return (res.data ?? []) as User[];
}

export async function createUser(payload: CreateUserPayload): Promise<CreateUserResponse> {
  const res = await api.post("/identity/users", payload);
  return res.data as CreateUserResponse;
}

export async function getUserById(userId: string): Promise<User> {
  const res = await api.get(`/identity/users/${userId}`);
  return res.data as User;
}

export async function updateUser(userId: string, username: string, email: string): Promise<User> {
  const res = await api.put(`/identity/users/${userId}`, { username, email });
  return res.data as User;
}

export async function deactivateUser(userId: string): Promise<void> {
  await api.patch(`/identity/users/${userId}/deactivate`);
}

export async function activateUser(userId: string): Promise<void> {
  await api.patch(`/identity/users/${userId}/activate`);
}

export async function deleteUser(userId: string): Promise<void> {
  await api.delete(`/identity/users/${userId}`);
}

export async function assignRole(userId: string, roleId: string): Promise<void> {
  await api.post(`/identity/users/${userId}/roles`, { roleId });
}

export async function removeRole(userId: string, roleId: string): Promise<void> {
  await api.delete(`/identity/users/${userId}/roles/${roleId}`);
}
