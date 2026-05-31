import { computed, ref } from "vue";
import { getRoles } from "../api/roles";
import {
  activateUser,
  assignRole,
  createUser as createUserApi,
  deactivateUser,
  deleteUser,
  getUserById,
  getUsers,
  removeRole,
  updateUser,
} from "../api/users";
import type { Role } from "../types/Role";
import type { User } from "../types/User";

export interface UserManagerLoadedPayload {
  usersCount: number;
  activeUsersCount: number;
}

/**
 * Owns all state, computed values and actions for user management.
 * Feedback is surfaced via inline `errorMessage`/`successMessage` refs (same UX
 * as the original monolithic view). `onLoaded` lets the parent view mirror the
 * latest counts into its stats strip.
 */
export function useUserManager(
  onLoaded?: (payload: UserManagerLoadedPayload) => void,
) {
  const loading = ref(false);
  const actionLoading = ref(false);
  const errorMessage = ref("");
  const successMessage = ref("");

  const users = ref<User[]>([]);
  const roles = ref<Role[]>([]);
  const selectedUser = ref<User | null>(null);
  const selectedUserId = ref("");
  const searchQuery = ref("");
  const roleToAssignId = ref("");

  const editUsername = ref("");
  const editEmail = ref("");
  const createUsername = ref("");
  const createEmail = ref("");
  const createPassword = ref("");
  const createRoleNames = ref<string[]>([]);

  // ── Computed ──────────────────────────────────────────────────────────────

  const filteredUsers = computed(() => {
    const q = searchQuery.value.trim().toLowerCase();
    if (!q) return users.value;
    return users.value.filter(
      (u) =>
        u.username.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q),
    );
  });

  const availableRolesForAssignment = computed(() => {
    if (!selectedUser.value) return [];
    const assigned = new Set(
      selectedUser.value.roles.map((r) => r.toLowerCase()),
    );
    return roles.value.filter((r) => !assigned.has(r.name.toLowerCase()));
  });

  // ── Helpers ─────────────────────────────────────────────────────────────────

  function getRoleIdByName(name: string): string | null {
    return (
      roles.value.find(
        (r) => r.name.toLowerCase() === name.trim().toLowerCase(),
      )?.id ?? null
    );
  }

  function getRolePermissionsPreview(name: string): string {
    const role = roles.value.find(
      (r) => r.name.toLowerCase() === name.trim().toLowerCase(),
    );
    if (!role || role.permissions.length === 0)
      return "Permissions не настроены.";
    const preview = role.permissions.slice(0, 4).join(", ");
    return role.permissions.length > 4 ? `${preview}...` : preview;
  }

  function syncEditableFields() {
    editUsername.value = selectedUser.value?.username ?? "";
    editEmail.value = selectedUser.value?.email ?? "";
  }

  function resetCreateUserForm() {
    createUsername.value = "";
    createEmail.value = "";
    createPassword.value = "";
    createRoleNames.value = [];
  }

  function emitLoaded() {
    onLoaded?.({
      usersCount: users.value.length,
      activeUsersCount: users.value.filter((u) => u.isActive).length,
    });
  }

  // ── Data loading ──────────────────────────────────────────────────────────

  async function reloadUsersAndKeepSelection(preferredId = "") {
    const loaded = await getUsers();
    users.value = loaded;
    if (loaded.length === 0) {
      selectedUser.value = null;
      selectedUserId.value = "";
      syncEditableFields();
      emitLoaded();
      return;
    }
    const has = preferredId ? loaded.some((u) => u.id === preferredId) : false;
    const target = has ? preferredId : (loaded[0]?.id ?? "");
    if (!target) {
      selectedUser.value = null;
      selectedUserId.value = "";
      syncEditableFields();
      emitLoaded();
      return;
    }
    await selectUser(target);
  }

  async function loadData() {
    loading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      roles.value = await getRoles();
      await reloadUsersAndKeepSelection(selectedUserId.value);
    } catch (e: any) {
      errorMessage.value =
        e?.response?.data?.error || "Не удалось загрузить данные.";
    } finally {
      loading.value = false;
    }
  }

  async function selectUser(userId: string) {
    if (!userId) return;
    selectedUserId.value = userId;
    roleToAssignId.value = "";
    errorMessage.value = "";
    try {
      const user = await getUserById(userId);
      selectedUser.value = user;
      syncEditableFields();
      emitLoaded();
    } catch (e: any) {
      errorMessage.value =
        e?.response?.data?.error || "Не удалось загрузить пользователя.";
    }
  }

  // ── Actions ───────────────────────────────────────────────────────────────

  async function createNewUser() {
    if (actionLoading.value || loading.value) return;
    const username = createUsername.value.trim(),
      email = createEmail.value.trim(),
      password = createPassword.value;
    if (!username || !email || !password) {
      errorMessage.value = "Заполните username, email и password.";
      return;
    }
    const uniqueRoles = [
      ...new Set(createRoleNames.value.map((r) => r.trim()).filter(Boolean)),
    ];
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      const created = await createUserApi({
        username,
        email,
        password,
        roles: uniqueRoles.length > 0 ? uniqueRoles : undefined,
      });
      resetCreateUserForm();
      await reloadUsersAndKeepSelection(created.userId);
      successMessage.value = `Пользователь ${created.username} создан.`;
    } catch (e: any) {
      errorMessage.value =
        e?.response?.data?.error || "Не удалось создать пользователя.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function saveUser() {
    if (!selectedUser.value || actionLoading.value) return;
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      const updated = await updateUser(
        selectedUser.value.id,
        editUsername.value.trim(),
        editEmail.value.trim(),
      );
      selectedUser.value = updated;
      await reloadUsersAndKeepSelection(updated.id);
      successMessage.value = "Пользователь обновлён.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function toggleActive() {
    if (!selectedUser.value || actionLoading.value) return;
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      if (selectedUser.value.isActive) {
        await deactivateUser(selectedUser.value.id);
        successMessage.value = "Пользователь деактивирован.";
      } else {
        await activateUser(selectedUser.value.id);
        successMessage.value = "Пользователь активирован.";
      }
      await reloadUsersAndKeepSelection(selectedUser.value.id);
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function deleteSelectedUser() {
    if (!selectedUser.value || actionLoading.value) return;
    if (!window.confirm(`Удалить пользователя ${selectedUser.value.username}?`))
      return;
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      const deletedId = selectedUser.value.id;
      await deleteUser(deletedId);
      successMessage.value = "Пользователь удалён.";
      const nextId = users.value.find((u) => u.id !== deletedId)?.id ?? "";
      await reloadUsersAndKeepSelection(nextId);
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function assignRoleToSelectedUser() {
    if (!selectedUser.value || actionLoading.value) return;
    if (!roleToAssignId.value) {
      errorMessage.value = "Выберите роль.";
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await assignRole(selectedUser.value.id, roleToAssignId.value);
      roleToAssignId.value = "";
      await reloadUsersAndKeepSelection(selectedUser.value.id);
      successMessage.value = "Роль назначена.";
    } catch (e: any) {
      errorMessage.value = e?.response?.data?.error || "Ошибка.";
    } finally {
      actionLoading.value = false;
    }
  }

  async function removeRoleFromSelectedUser(name: string) {
    if (!selectedUser.value || actionLoading.value) return;
    const id = getRoleIdByName(name);
    if (!id) {
      errorMessage.value = `Роль ${name} не найдена.`;
      return;
    }
    actionLoading.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    try {
      await removeRole(selectedUser.value.id, id);
      await reloadUsersAndKeepSelection(selectedUser.value.id);
      successMessage.value = "Роль удалена.";
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
    users,
    roles,
    selectedUser,
    selectedUserId,
    searchQuery,
    roleToAssignId,
    editUsername,
    editEmail,
    createUsername,
    createEmail,
    createPassword,
    createRoleNames,
    // computed
    filteredUsers,
    availableRolesForAssignment,
    // helpers used by template
    getRolePermissionsPreview,
    // actions
    selectUser,
    loadData,
    createNewUser,
    saveUser,
    toggleActive,
    deleteSelectedUser,
    assignRoleToSelectedUser,
    removeRoleFromSelectedUser,
  };
}
