import type { AccessControl, AccessRequirement } from "./accessControl";

export type RouteLocationRaw = string | Record<string, unknown>;

export interface RouteLocationNormalizedLike {
  path: string;
  meta: Record<string, unknown>;
}

export type NavigationGuardNext = (
  to?: RouteLocationRaw | false | void,
) => void;

export interface RouteAuthSubject {
  token?: string;
  checkTokenValidity?(): boolean;
}

export interface RouteAccessGuardContext {
  from: RouteLocationNormalizedLike;
  isAuthenticated: boolean;
  token: string;
  to: RouteLocationNormalizedLike;
}

export interface CreateRouteAccessGuardOptions {
  access: AccessControl;
  auth: RouteAuthSubject;
  loginPath: RouteLocationRaw;
  getAllowedRedirect?(
    context: RouteAccessGuardContext,
  ): RouteLocationRaw | null | undefined;
  getForbiddenRedirect(
    context: RouteAccessGuardContext,
  ): RouteLocationRaw | null | undefined;
}

function getStoredToken(): string {
  return localStorage.getItem("token") || "";
}

function getAuthToken(auth: RouteAuthSubject): string {
  return auth.token || getStoredToken();
}

function getString(value: unknown): string | null {
  return typeof value === "string" ? value : null;
}

function getStringList(value: unknown): string[] | null {
  if (typeof value === "string") return [value];
  if (!Array.isArray(value)) return null;

  const values = value.filter((item): item is string => typeof item === "string");
  return values.length > 0 ? values : null;
}

function getRouteRequirement(to: RouteLocationNormalizedLike): AccessRequirement {
  const meta = to.meta;

  return {
    actorType: getString(meta.actorType),
    allPermissions: getStringList(meta.allPermissions),
    allRoles: getStringList(meta.allRoles),
    anyPermissions: getStringList(meta.anyPermissions),
    anyRoles: getStringList(meta.anyRoles),
    permission: getString(meta.permission),
    permissions: getStringList(meta.permissions),
    requiredAllPermissions: getStringList(meta.requiredAllPermissions),
    requiredAllRoles: getStringList(meta.requiredAllRoles),
    requiredAnyPermission: getStringList(meta.requiredAnyPermission),
    requiredAnyRole: getStringList(meta.requiredAnyRole),
    requiredPermission: getString(meta.requiredPermission),
    requiredPermissions: getStringList(meta.requiredPermissions),
    requiredRole: getString(meta.requiredRole),
    requiredRoles: getStringList(meta.requiredRoles),
    role: getString(meta.role),
    roles: getStringList(meta.roles),
    subjectType: getString(meta.subjectType),
  };
}

export function createRouteAccessGuard(options: CreateRouteAccessGuardOptions) {
  return (
    to: RouteLocationNormalizedLike,
    from: RouteLocationNormalizedLike,
    next: NavigationGuardNext,
  ) => {
    const token = getAuthToken(options.auth);
    const requiresAuth = to.meta.requiresAuth === true;
    const isAuthenticated = token
      ? (options.auth.checkTokenValidity?.() ?? true)
      : false;

    const context: RouteAccessGuardContext = {
      from,
      isAuthenticated,
      token,
      to,
    };

    if (requiresAuth && !isAuthenticated) {
      next(options.loginPath);
      return;
    }

    if (isAuthenticated && !options.access.allows(getRouteRequirement(to))) {
      const redirect = options.getForbiddenRedirect(context);
      next(redirect ?? options.loginPath);
      return;
    }

    const allowedRedirect = options.getAllowedRedirect?.(context);
    if (allowedRedirect) {
      next(allowedRedirect);
      return;
    }

    next();
  };
}
