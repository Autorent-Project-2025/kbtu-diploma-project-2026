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
