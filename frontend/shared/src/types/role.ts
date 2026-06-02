export interface RoleReference {
  id: string;
  name: string;
}

export interface Role {
  id: string;
  name: string;
  permissions: string[];
  directPermissions: string[];
  parentRoles: RoleReference[];
}
