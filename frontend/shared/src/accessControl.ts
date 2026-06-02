export interface AccessControlSubject {
  getPermissions(): string[];
  hasPermission?(permission: string): boolean;
  getRoles?(): string[];
  hasRole?(role: string): boolean;
  getActorType?(): string | null;
  getSubjectType?(): string | null;
  isActorType?(type: string): boolean;
  isSubjectType?(type: string): boolean;
}

export interface AccessRequirement {
  permission?: string | null;
  requiredPermission?: string | null;
  permissions?: string | string[] | null;
  requiredPermissions?: string | string[] | null;
  anyPermissions?: string[] | null;
  requiredAnyPermission?: string[] | null;
  allPermissions?: string[] | null;
  requiredAllPermissions?: string[] | null;
  role?: string | null;
  requiredRole?: string | null;
  roles?: string | string[] | null;
  requiredRoles?: string | string[] | null;
  anyRoles?: string[] | null;
  requiredAnyRole?: string[] | null;
  allRoles?: string[] | null;
  requiredAllRoles?: string[] | null;
  actorType?: string | null;
  subjectType?: string | null;
}

function normalizeValue(value: string | null | undefined): string | null {
  if (typeof value !== "string") return null;
  const normalized = value.trim().toLowerCase();
  return normalized || null;
}

function normalizeList(
  value: string | string[] | null | undefined,
): string[] {
  const values = Array.isArray(value) ? value : [value];

  return values
    .map(normalizeValue)
    .filter((item): item is string => item !== null);
}

export function createAccessControl(subject: AccessControlSubject) {
  function can(permission: string | null | undefined): boolean {
    const expected = normalizeValue(permission);
    if (!expected) return false;

    if (subject.hasPermission) {
      return subject.hasPermission(expected);
    }

    return subject.getPermissions().some((permissionClaim) => {
      const normalizedClaim = normalizeValue(permissionClaim);
      return normalizedClaim === expected || normalizedClaim === "*";
    });
  }

  function canAny(permissions: string[] | null | undefined): boolean {
    const required = normalizeList(permissions);
    return required.length > 0 && required.some(can);
  }

  function canAll(permissions: string[] | null | undefined): boolean {
    const required = normalizeList(permissions);
    return required.length > 0 && required.every(can);
  }

  function requireRole(role: string | null | undefined): boolean {
    const expected = normalizeValue(role);
    if (!expected) return false;

    if (subject.hasRole) {
      return subject.hasRole(expected);
    }

    return (subject.getRoles?.() ?? []).some((roleClaim) => {
      const normalizedClaim = normalizeValue(roleClaim);
      return normalizedClaim === expected || normalizedClaim === "*";
    });
  }

  function requireAnyRole(roles: string[] | null | undefined): boolean {
    const required = normalizeList(roles);
    return required.length > 0 && required.some(requireRole);
  }

  function requireAllRoles(roles: string[] | null | undefined): boolean {
    const required = normalizeList(roles);
    return required.length > 0 && required.every(requireRole);
  }

  function requireActorType(actorType: string | null | undefined): boolean {
    const expected = normalizeValue(actorType);
    if (!expected) return false;

    if (subject.isActorType) {
      return subject.isActorType(expected);
    }

    return normalizeValue(subject.getActorType?.()) === expected;
  }

  function requireSubjectType(subjectType: string | null | undefined): boolean {
    const expected = normalizeValue(subjectType);
    if (!expected) return false;

    if (subject.isSubjectType) {
      return subject.isSubjectType(expected);
    }

    return normalizeValue(subject.getSubjectType?.()) === expected;
  }

  function allows(requirement: AccessRequirement): boolean {
    const singlePermission =
      requirement.requiredPermission ?? requirement.permission;
    if (singlePermission && !can(singlePermission)) return false;

    const permissions =
      requirement.requiredPermissions ?? requirement.permissions;
    if (permissions && !canAll(normalizeList(permissions))) return false;

    const anyPermissions =
      requirement.requiredAnyPermission ?? requirement.anyPermissions;
    if (anyPermissions && !canAny(anyPermissions)) return false;

    const allPermissions =
      requirement.requiredAllPermissions ?? requirement.allPermissions;
    if (allPermissions && !canAll(allPermissions)) return false;

    const singleRole = requirement.requiredRole ?? requirement.role;
    if (singleRole && !requireRole(singleRole)) return false;

    const roles = requirement.requiredRoles ?? requirement.roles;
    if (roles && !requireAllRoles(normalizeList(roles))) return false;

    const anyRoles = requirement.requiredAnyRole ?? requirement.anyRoles;
    if (anyRoles && !requireAnyRole(anyRoles)) return false;

    const allRoles = requirement.requiredAllRoles ?? requirement.allRoles;
    if (allRoles && !requireAllRoles(allRoles)) return false;

    if (
      requirement.actorType &&
      !requireActorType(requirement.actorType)
    ) {
      return false;
    }

    if (
      requirement.subjectType &&
      !requireSubjectType(requirement.subjectType)
    ) {
      return false;
    }

    return true;
  }

  return {
    allows,
    can,
    canAll,
    canAny,
    requireActorType,
    requireAllRoles,
    requireAnyRole,
    requireRole,
    requireSubjectType,
  };
}

export type AccessControl = ReturnType<typeof createAccessControl>;
