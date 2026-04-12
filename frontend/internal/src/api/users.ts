import api from "./axios";

export interface UserDto {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  subjectType: string;
  actorType: string;
  roles: string[];
  permissions: string[];
}

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

export async function getUsers(): Promise<UserDto[]> {
  const res = await api.get("/identity/users");
  return (res.data ?? []) as UserDto[];
}

export async function getManagers(): Promise<UserDto[]> {
  const users = await getUsers();
  return users.filter((u) =>
    u.roles.some((r) => r.toLowerCase() === "manager")
  );
}

export async function getUserById(userId: string): Promise<UserDto> {
  const res = await api.get(`/identity/users/${userId}`);
  return res.data as UserDto;
}

export async function createUser(payload: CreateUserPayload): Promise<CreateUserResponse> {
  const res = await api.post("/identity/users", payload);
  return res.data as CreateUserResponse;
}

export async function updateUser(userId: string, username: string, email: string): Promise<UserDto> {
  const res = await api.put(`/identity/users/${userId}`, { username, email });
  return res.data as UserDto;
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
