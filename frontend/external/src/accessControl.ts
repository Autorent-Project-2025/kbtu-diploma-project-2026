import { createAccessControl } from "@shared/accessControl";
import { auth } from "./store/auth";

export const access = createAccessControl(auth);
export const {
  allows,
  can,
  canAll,
  canAny,
  requireActorType,
  requireAllRoles,
  requireAnyRole,
  requireRole,
  requireSubjectType,
} = access;
