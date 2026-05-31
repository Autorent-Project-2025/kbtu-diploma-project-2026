import { computed, ref } from "vue";
import { getPermissions } from "../api/permissions";
import {
  assignParentRoleToRole,
  assignPermissionToRole,
  createRole as createRoleApi,
  getRoles,
  removeParentRoleFromRole,
  removePermissionFromRole,
} from "../api/roles";
import type { Permission } from "../types/Permission";
import type { Role } from "../types/Role";

export interface RoleManagerLoadedPayload {
  rolesCount: number;
  permissionsCount: number;
}

/**
 * Owns all state, computed values and actions for role/permission management.
 * Feedback is surfaced via inline `errorMessage`/`successMessage` refs (same UX
 * as the original monolithic view). `onLoaded` lets the parent view mirror the
 * latest counts into its stats strip.
 */
export function useRoleManager(
  onLoaded?: (payload: RoleManagerLoadedPayload) => void,
) {
  const loading = ref(false);
  const actionLoading = ref(false);
  const errorMessage = ref("");
  const successMessage = ref("");

  const roles = ref<Role[]>([]);
  const permissions = ref<Permission[]>([]);
  const selectedRoleId = ref("");
  const roleSearchQuery = ref("");

  const createRoleName = ref("");
  const createRolePermissionIds = ref<string[]>([]);
  const createRoleParentRoleIds = ref<string[]>([]);
  const permissionToAssignId = ref("");
  const parentRoleToAssignId = ref("");

  // ── Computed ──────────────────────────────────────────────────────────────

  const selectedRole = computed(
    () => roles.value.find((r) => r.id === selectedRoleId.value) ?? null,
  );

  const filteredRoles = computed(() => {
    const q = roleSearchQuery.value.trim().toLowerCase();
    if (!q) return roles.value;
    return roles.value.filter(
      (r) =>
        r.name.toLowerCase().includes(q) ||
        r.permissions.some((p) => p.toLowerCase().includes(q)),
    );
  });

  const availablePermissionsForSelectedRole = computed(() => {
    if (!selectedRole.value) return [];
    const direct = new Set(
      selectedRole.value.directPermissions.map((p) => p.toLowerCase()),
    );
    return permissions.value.filter((p) => !direct.has(p.name.toLowerCase()));
  });

  const availableParentRolesForSelectedRole = computed(() => {
    if (!selectedRole.value) return [];
    const cur = selectedRole.value;
    const parentIds = new Set(cur.parentRoles.map((r) => r.id));
    return roles.value.filter((r) => {
      if (r.id === cur.id || parentIds.has(r.id)) return false;
      return !collectAncestorRoleIds(r.id).has(cur.id);
    });
  });

  // ── Helpers ─────────────────────────────────────────────────────────────────

  function collectAncestorRoleIds(
    roleId: string,
    visited = new Set<string>(),
  ): Set<string> {
    const role = roles.value.find((r) => r.id === roleId);
    if (!role) return visited;
    for (const p of role.parentRoles) {
      if (visited.has(p.id)) continue;
      visited.add(p.id);
      collectAncestorRoleIds(p.id, visited);
    }
    return visited;
  }

  function getPermissionIdByName(name: string): string | null {
    return (
      permissions.value.find(
        (p) => p.name.toLowerCase() === name.trim().toLowerCase(),
      )?.id ?? null
    );
  }

  function selectRole(id: string) {
    selectedRoleId.value = id;
    permissionToAssignId.value = "";
    parentRoleToAssignId.value = "";
  }

  function resetCreateRoleForm() {
    createRoleName.value = "";
    createRolePermissionIds.value = [];
    createRoleParentRoleIds.value = [];
  }

  function emitLoaded() {
    onLoaded?.({
      rolesCount: roles.value.length,
      permissionsCount: permissions.value.length,
    });
  }

  // ── Data loading ──────────────────────────────────────────────────────────

  async function reloadRolesAndKeepSelection(preferredId = "") {
    const loaded = await getRoles();
    roles.value = loaded;
    if (loaded.length === 0) {
      selectedRoleId.value = "";
    } else {
      const has = preferredId
        ? loaded.some((r) => r.id === preferredId)
        : false;
      selectRole(has ? preferredId : (loaded[0]?.id ?? ""));
    }
    emitLoaded();
  }

  async function loadData() {
    loading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      permissions.value = await getPermissions();
      await reloadRolesAndKeepSelection(selectedRoleId.value);
    } catch (e: any) {
      errorMessage.value =
        e?.response?.data?.error || "Не удалось загрузить данные.";
    } finally {
      loading.value = false;
    }
  }

  // ── Actions ───────────────────────────────────────────────────────────────

  async function createNewRole() {
    if (actionLoading.value || loading.value) return;
    const name = createRoleName.value.trim();
    if (!name) {
      errorMessage.value = "Введите название роли.";
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await createRoleApi({
        name,
        permissionIds: [
          ...new Set(createRolePermissionIds.value.filter(Boolean)),
        ],
        parentRoleIds: [
          ...new Set(createRoleParentRoleIds.value.filter(Boolean)),
        ],
      });
      resetCreateRoleForm();
      await reloadRolesAndKeepSelection(selectedRoleId.value);
      const created = roles.value.find(
        (r) => r.name.toLowerCase() === name.toLowerCase(),
      );
      if (created) selectRole(created.id);
      successMessage.value = `Роль ${name} создана.`;
    } catch (e: any) {
      errorMessage.value =
        e?.response?.data?.error || "Не удалось создать роль.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function addPermissionToSelectedRole() {
    if (!selectedRole.value || actionLoading.value) return;
    if (!permissionToAssignId.value) {
      errorMessage.value = "Выберите permission.";
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await assignPermissionToRole(
        selectedRole.value.id,
        permissionToAssignId.value,
      );
      permissionToAssignId.value = "";
      await reloadRolesAndKeepSelection(selectedRole.value.id);
      successMessage.value = "Permission добавлен.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function removePermissionFromSelectedRole(name: string) {
    if (!selectedRole.value || actionLoading.value) return;
    const id = getPermissionIdByName(name);
    if (!id) {
      errorMessage.value = `Permission ${name} не найден.`;
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await removePermissionFromRole(selectedRole.value.id, id);
      await reloadRolesAndKeepSelection(selectedRole.value.id);
      successMessage.value = "Permission убран.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function addParentRoleToSelectedRole() {
    if (!selectedRole.value || actionLoading.value) return;
    if (!parentRoleToAssignId.value) {
      errorMessage.value = "Выберите parent role.";
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await assignParentRoleToRole(
        selectedRole.value.id,
        parentRoleToAssignId.value,
      );
      parentRoleToAssignId.value = "";
      await reloadRolesAndKeepSelection(selectedRole.value.id);
      successMessage.value = "Наследование добавлено.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function removeParentRoleFromSelectedRole(parentId: string) {
    if (!selectedRole.value || actionLoading.value) return;
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await removeParentRoleFromRole(selectedRole.value.id, parentId);
      await reloadRolesAndKeepSelection(selectedRole.value.id);
      successMessage.value = "Наследование удалено.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  return {
    // state
    loading,
    actionLoading,
    errorMessage,
    successMessage,
    roles,
    permissions,
    selectedRoleId,
    roleSearchQuery,
    createRoleName,
    createRolePermissionIds,
    createRoleParentRoleIds,
    permissionToAssignId,
    parentRoleToAssignId,
    // computed
    selectedRole,
    filteredRoles,
    availablePermissionsForSelectedRole,
    availableParentRolesForSelectedRole,
    // actions
    selectRole,
    loadData,
    createNewRole,
    addPermissionToSelectedRole,
    removePermissionFromSelectedRole,
    addParentRoleToSelectedRole,
    removeParentRoleFromSelectedRole,
  };
}
