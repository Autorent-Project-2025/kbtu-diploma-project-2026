export interface User {
  id: string;
  username: string;
  email: string;
  isActive: boolean;
  roles: string[];
  permissions: string[];
}
