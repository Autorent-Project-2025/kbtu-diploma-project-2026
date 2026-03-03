<template>
  <div class="container admin-page">
    <section class="card card-elevated admin-hero">
      <div>
        <h1 class="hero-title">Control Center</h1>
        <p class="hero-subtitle">Управление пользователями, ролями, permissions и inheritance.</p>
      </div>
      <button class="btn btn-outline" @click="loadData" :disabled="loading">
        {{ loading ? "Обновление..." : "Обновить данные" }}
      </button>
    </section>

    <section class="stats-grid">
      <article class="card stat-card">
        <div class="stat-label">Пользователи</div>
        <div class="stat-value">{{ users.length }}</div>
      </article>
      <article class="card stat-card">
        <div class="stat-label">Активные</div>
        <div class="stat-value">{{ activeUsersCount }}</div>
      </article>
      <article class="card stat-card">
        <div class="stat-label">Роли</div>
        <div class="stat-value">{{ roles.length }}</div>
      </article>
      <article class="card stat-card">
        <div class="stat-label">Permissions</div>
        <div class="stat-value">{{ permissions.length }}</div>
      </article>
    </section>

    <div v-if="errorMessage" class="error">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success">{{ successMessage }}</div>

    <section class="section-switch card">
      <button
        class="switch-btn"
        :class="{ 'switch-btn-active': activeSection === 'roles' }"
        @click="activeSection = 'roles'"
      >
        Role Management
      </button>
      <button
        class="switch-btn"
        :class="{ 'switch-btn-active': activeSection === 'users' }"
        @click="activeSection = 'users'"
      >
        User Management
      </button>
    </section>

    <section v-if="activeSection === 'roles'" class="workspace-grid">
      <aside class="card workspace-sidebar">
        <div class="sidebar-header">
          <h2 class="section-title">Роли</h2>
          <span class="sidebar-count">{{ filteredRoles.length }}</span>
        </div>

        <label class="label" for="roleSearch">Поиск роли</label>
        <input
          id="roleSearch"
          v-model="roleSearchQuery"
          class="input"
          type="text"
          placeholder="Название или permission"
        />

        <ul v-if="filteredRoles.length > 0" class="entity-list">
          <li v-for="role in filteredRoles" :key="role.id">
            <button
              class="entity-button"
              :class="{ 'entity-button-active': selectedRole?.id === role.id }"
              @click="selectRole(role.id)"
            >
              <div class="entity-title">{{ role.name }}</div>
              <div class="entity-meta">Effective: {{ role.permissions.length }} permission(s)</div>
            </button>
          </li>
        </ul>

        <div v-else class="empty-text">Роли не найдены.</div>
      </aside>

      <div class="workspace-main">
        <article class="card form-card">
          <h2 class="section-title">Создать роль</h2>

          <form @submit.prevent="createNewRole" class="form-grid form-grid-3">
            <div class="form-field">
              <label class="label" for="createRoleName">Название роли</label>
              <input id="createRoleName" v-model="createRoleName" class="input" type="text" required />
            </div>

            <div class="form-field">
              <label class="label" for="createRolePermissions">Прямые permissions</label>
              <select id="createRolePermissions" v-model="createRolePermissionIds" class="select" multiple size="7">
                <option v-for="permission in permissions" :key="permission.id" :value="permission.id">
                  {{ permission.name }}
                </option>
              </select>
            </div>

            <div class="form-field">
              <label class="label" for="createRoleParents">Родительские роли</label>
              <select id="createRoleParents" v-model="createRoleParentRoleIds" class="select" multiple size="7">
                <option v-for="role in roles" :key="role.id" :value="role.id">
                  {{ role.name }}
                </option>
              </select>
            </div>

            <div class="form-actions">
              <button class="btn btn-primary" type="submit" :disabled="actionLoading || loading">Создать роль</button>
            </div>
          </form>
        </article>

        <article v-if="selectedRole" class="card detail-card">
          <h2 class="section-title">Роль: {{ selectedRole.name }}</h2>

          <div class="detail-columns">
            <section>
              <h3 class="subsection-title">Прямые permissions</h3>
              <div v-if="selectedRole.directPermissions.length === 0" class="empty-text">
                Прямые permissions не назначены.
              </div>
              <ul v-else class="chip-list">
                <li v-for="permissionName in selectedRole.directPermissions" :key="permissionName" class="chip-row">
                  <span class="badge badge-neutral">{{ permissionName }}</span>
                  <button
                    class="btn btn-outline"
                    @click="removePermissionFromSelectedRole(permissionName)"
                    :disabled="actionLoading"
                  >
                    Убрать
                  </button>
                </li>
              </ul>

              <div class="inline-form">
                <div class="form-field">
                  <label class="label" for="permissionToAssign">Добавить permission</label>
                  <select id="permissionToAssign" v-model="permissionToAssignId" class="select">
                    <option value="">Выберите permission</option>
                    <option
                      v-for="permission in availablePermissionsForSelectedRole"
                      :key="permission.id"
                      :value="permission.id"
                    >
                      {{ permission.name }}
                    </option>
                  </select>
                </div>
                <button class="btn btn-primary" @click="addPermissionToSelectedRole" :disabled="actionLoading">
                  Добавить
                </button>
              </div>
            </section>

            <section>
              <h3 class="subsection-title">Наследуемые роли</h3>
              <div v-if="selectedRole.parentRoles.length === 0" class="empty-text">Наследование не настроено.</div>
              <ul v-else class="chip-list">
                <li v-for="parentRole in selectedRole.parentRoles" :key="parentRole.id" class="chip-row">
                  <span class="badge badge-neutral">{{ parentRole.name }}</span>
                  <button
                    class="btn btn-outline"
                    @click="removeParentRoleFromSelectedRole(parentRole.id)"
                    :disabled="actionLoading"
                  >
                    Убрать
                  </button>
                </li>
              </ul>

              <div class="inline-form">
                <div class="form-field">
                  <label class="label" for="parentRoleToAssign">Добавить parent role</label>
                  <select id="parentRoleToAssign" v-model="parentRoleToAssignId" class="select">
                    <option value="">Выберите роль</option>
                    <option v-for="role in availableParentRolesForSelectedRole" :key="role.id" :value="role.id">
                      {{ role.name }}
                    </option>
                  </select>
                </div>
                <button class="btn btn-primary" @click="addParentRoleToSelectedRole" :disabled="actionLoading">
                  Добавить
                </button>
              </div>
            </section>
          </div>

          <section>
            <h3 class="subsection-title">Итоговые permissions</h3>
            <div class="badge-group">
              <span v-for="permissionName in selectedRole.permissions" :key="permissionName" class="badge badge-neutral">
                {{ permissionName }}
              </span>
            </div>
          </section>
        </article>
        <article v-else class="card detail-card">
          <div class="empty-text">Выберите роль слева для редактирования inheritance и permissions.</div>
        </article>
      </div>
    </section>

    <section v-else class="workspace-grid">
      <aside class="card workspace-sidebar">
        <div class="sidebar-header">
          <h2 class="section-title">Пользователи</h2>
          <span class="sidebar-count">{{ filteredUsers.length }}</span>
        </div>

        <label class="label" for="searchUser">Поиск пользователя</label>
        <input
          id="searchUser"
          v-model="searchQuery"
          class="input"
          type="text"
          placeholder="Username или email"
        />

        <ul v-if="filteredUsers.length > 0" class="entity-list">
          <li v-for="user in filteredUsers" :key="user.id">
            <button
              class="entity-button"
              :class="{ 'entity-button-active': selectedUser?.id === user.id }"
              @click="selectUser(user.id)"
            >
              <div class="entity-title">{{ user.username }}</div>
              <div class="entity-meta">{{ user.email }}</div>
              <span :class="user.isActive ? 'badge badge-active' : 'badge badge-inactive'">
                {{ user.isActive ? "Active" : "Inactive" }}
              </span>
            </button>
          </li>
        </ul>
        <div v-else class="empty-text">Пользователи не найдены.</div>
      </aside>

      <div class="workspace-main">
        <article class="card form-card">
          <h2 class="section-title">Создать пользователя</h2>

          <form @submit.prevent="createNewUser" class="form-grid form-grid-3">
            <div class="form-field">
              <label class="label" for="createUsername">Username</label>
              <input id="createUsername" v-model="createUsername" class="input" type="text" required />
            </div>
            <div class="form-field">
              <label class="label" for="createEmail">Email</label>
              <input id="createEmail" v-model="createEmail" class="input" type="email" required />
            </div>
            <div class="form-field">
              <label class="label" for="createPassword">Password</label>
              <input id="createPassword" v-model="createPassword" class="input" type="password" required />
            </div>

            <div class="form-field form-field-wide">
              <label class="label" for="createRoles">Роли (опционально)</label>
              <select id="createRoles" v-model="createRoleNames" class="select" multiple size="7">
                <option v-for="role in roles" :key="role.id" :value="role.name">
                  {{ role.name }}
                </option>
              </select>
              <p class="hint-text">Если роли не выбраны, назначается роль по умолчанию.</p>
            </div>

            <div class="form-actions">
              <button class="btn btn-primary" type="submit" :disabled="actionLoading || loading">
                Создать пользователя
              </button>
            </div>
          </form>
        </article>

        <article v-if="selectedUser" class="card detail-card">
          <h2 class="section-title">Пользователь: {{ selectedUser.username }}</h2>
          <p class="section-subtitle">ID: {{ selectedUser.id }}</p>

          <div class="form-grid form-grid-2">
            <div class="form-field">
              <label class="label" for="username">Username</label>
              <input id="username" v-model="editUsername" class="input" type="text" />
            </div>
            <div class="form-field">
              <label class="label" for="email">Email</label>
              <input id="email" v-model="editEmail" class="input" type="email" />
            </div>
          </div>

          <div class="action-row">
            <button class="btn btn-primary" @click="saveUser" :disabled="actionLoading">Сохранить</button>
            <button class="btn btn-outline" @click="toggleActive" :disabled="actionLoading">
              {{ selectedUser.isActive ? "Деактивировать" : "Активировать" }}
            </button>
            <button class="btn btn-danger" @click="deleteSelectedUser" :disabled="actionLoading">Удалить</button>
          </div>

          <section>
            <h3 class="subsection-title">Роли пользователя</h3>
            <div v-if="selectedUser.roles.length === 0" class="empty-text">Роли отсутствуют.</div>
            <ul v-else class="chip-list">
              <li v-for="roleName in selectedUser.roles" :key="roleName" class="chip-row">
                <div>
                  <strong>{{ roleName }}</strong>
                  <div class="hint-text">{{ getRolePermissionsPreview(roleName) }}</div>
                </div>
                <button class="btn btn-outline" @click="removeRoleFromSelectedUser(roleName)" :disabled="actionLoading">
                  Убрать
                </button>
              </li>
            </ul>

            <div class="inline-form">
              <div class="form-field">
                <label class="label" for="roleSelect">Добавить роль</label>
                <select id="roleSelect" v-model="roleToAssignId" class="select">
                  <option value="">Выберите роль</option>
                  <option v-for="role in availableRolesForAssignment" :key="role.id" :value="role.id">
                    {{ role.name }}
                  </option>
                </select>
              </div>
              <button class="btn btn-primary" @click="assignRoleToSelectedUser" :disabled="actionLoading">
                Назначить
              </button>
            </div>
          </section>

          <section>
            <h3 class="subsection-title">Итоговые permissions пользователя</h3>
            <div class="badge-group">
              <span v-for="permission in selectedUser.permissions" :key="permission" class="badge badge-neutral">
                {{ permission }}
              </span>
            </div>
          </section>
        </article>
        <article v-else class="card detail-card">
          <div class="empty-text">Выберите пользователя слева для редактирования.</div>
        </article>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { getPermissions } from "../api/permissions";
import {
  assignParentRoleToRole,
  assignPermissionToRole,
  createRole as createRoleApi,
  getRoles,
  removeParentRoleFromRole,
  removePermissionFromRole,
} from "../api/roles";
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
import type { Permission } from "../types/Permission";
import type { Role } from "../types/Role";
import type { User } from "../types/User";

const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");
const activeSection = ref<"roles" | "users">("roles");

const users = ref<User[]>([]);
const roles = ref<Role[]>([]);
const permissions = ref<Permission[]>([]);
const selectedUser = ref<User | null>(null);
const selectedUserId = ref("");
const selectedRoleId = ref("");

const editUsername = ref("");
const editEmail = ref("");
const roleToAssignId = ref("");
const searchQuery = ref("");
const roleSearchQuery = ref("");
const createUsername = ref("");
const createEmail = ref("");
const createPassword = ref("");
const createRoleNames = ref<string[]>([]);

const createRoleName = ref("");
const createRolePermissionIds = ref<string[]>([]);
const createRoleParentRoleIds = ref<string[]>([]);
const permissionToAssignId = ref("");
const parentRoleToAssignId = ref("");

const selectedRole = computed(() => roles.value.find((role) => role.id === selectedRoleId.value) ?? null);
const activeUsersCount = computed(() => users.value.filter((user) => user.isActive).length);

const filteredUsers = computed(() => {
  const query = searchQuery.value.trim().toLowerCase();
  if (!query) return users.value;

  return users.value.filter(
    (user) => user.username.toLowerCase().includes(query) || user.email.toLowerCase().includes(query)
  );
});

const filteredRoles = computed(() => {
  const query = roleSearchQuery.value.trim().toLowerCase();
  if (!query) return roles.value;

  return roles.value.filter((role) => {
    if (role.name.toLowerCase().includes(query)) {
      return true;
    }

    return role.permissions.some((permission) => permission.toLowerCase().includes(query));
  });
});

const availableRolesForAssignment = computed(() => {
  if (!selectedUser.value) return [];

  const assignedRoleNames = new Set(selectedUser.value.roles.map((role) => role.toLowerCase()));
  return roles.value.filter((role) => !assignedRoleNames.has(role.name.toLowerCase()));
});

const availablePermissionsForSelectedRole = computed(() => {
  if (!selectedRole.value) return [];

  const directPermissionNames = new Set(selectedRole.value.directPermissions.map((permission) => permission.toLowerCase()));
  return permissions.value.filter((permission) => !directPermissionNames.has(permission.name.toLowerCase()));
});

const availableParentRolesForSelectedRole = computed(() => {
  if (!selectedRole.value) return [];

  const currentRole = selectedRole.value;
  const currentParentRoleIds = new Set(currentRole.parentRoles.map((role) => role.id));

  return roles.value.filter((role) => {
    if (role.id === currentRole.id) {
      return false;
    }

    if (currentParentRoleIds.has(role.id)) {
      return false;
    }

    const roleAncestors = collectAncestorRoleIds(role.id);
    return !roleAncestors.has(currentRole.id);
  });
});

function collectAncestorRoleIds(roleId: string, visited = new Set<string>()): Set<string> {
  const role = roles.value.find((item) => item.id === roleId);
  if (!role) return visited;

  for (const parentRole of role.parentRoles) {
    if (visited.has(parentRole.id)) {
      continue;
    }

    visited.add(parentRole.id);
    collectAncestorRoleIds(parentRole.id, visited);
  }

  return visited;
}

function selectRole(roleId: string) {
  selectedRoleId.value = roleId;
  permissionToAssignId.value = "";
  parentRoleToAssignId.value = "";
}

function syncEditableFields() {
  if (!selectedUser.value) {
    editUsername.value = "";
    editEmail.value = "";
    return;
  }

  editUsername.value = selectedUser.value.username;
  editEmail.value = selectedUser.value.email;
}

function resetCreateUserForm() {
  createUsername.value = "";
  createEmail.value = "";
  createPassword.value = "";
  createRoleNames.value = [];
}

function resetCreateRoleForm() {
  createRoleName.value = "";
  createRolePermissionIds.value = [];
  createRoleParentRoleIds.value = [];
}

async function reloadRolesAndKeepSelection(preferredRoleId = "") {
  const loadedRoles = await getRoles();
  roles.value = loadedRoles;

  if (loadedRoles.length === 0) {
    selectedRoleId.value = "";
    return;
  }

  const hasPreferredRole = preferredRoleId
    ? loadedRoles.some((role) => role.id === preferredRoleId)
    : false;

  selectRole(hasPreferredRole ? preferredRoleId : loadedRoles[0]?.id ?? "");
}

async function loadData() {
  loading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const [loadedPermissions] = await Promise.all([getPermissions()]);
    permissions.value = loadedPermissions;

    await reloadRolesAndKeepSelection(selectedRoleId.value);
    await reloadUsersAndKeepSelection(selectedUserId.value);
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось загрузить данные.";
  } finally {
    loading.value = false;
  }
}

async function reloadUsersAndKeepSelection(preferredUserId = "") {
  const loadedUsers = await getUsers();
  users.value = loadedUsers;

  if (loadedUsers.length === 0) {
    selectedUser.value = null;
    selectedUserId.value = "";
    syncEditableFields();
    return;
  }

  const hasPreferred = preferredUserId
    ? loadedUsers.some((user) => user.id === preferredUserId)
    : false;

  const targetUserId = hasPreferred ? preferredUserId : loadedUsers[0]?.id ?? "";
  if (!targetUserId) {
    selectedUser.value = null;
    selectedUserId.value = "";
    syncEditableFields();
    return;
  }

  await selectUser(targetUserId);
}

async function selectUser(userId: string) {
  if (!userId) return;

  selectedUserId.value = userId;
  roleToAssignId.value = "";
  errorMessage.value = "";

  const user = await getUserById(userId);
  selectedUser.value = user;
  syncEditableFields();
}

async function createNewRole() {
  if (actionLoading.value || loading.value) return;

  const roleName = createRoleName.value.trim();
  if (!roleName) {
    errorMessage.value = "Введите название роли.";
    return;
  }

  const uniquePermissionIds = Array.from(new Set(createRolePermissionIds.value.filter((id) => id)));
  const uniqueParentRoleIds = Array.from(new Set(createRoleParentRoleIds.value.filter((id) => id)));

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await createRoleApi({
      name: roleName,
      permissionIds: uniquePermissionIds,
      parentRoleIds: uniqueParentRoleIds,
    });

    resetCreateRoleForm();
    await reloadRolesAndKeepSelection(selectedRoleId.value);

    const createdRole = roles.value.find((role) => role.name.toLowerCase() === roleName.toLowerCase());
    if (createdRole) {
      selectRole(createdRole.id);
    }

    activeSection.value = "roles";
    successMessage.value = `Роль ${roleName} создана.`;
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось создать роль.";
  } finally {
    actionLoading.value = false;
  }
}

function getPermissionIdByName(permissionName: string): string | null {
  const normalizedPermissionName = permissionName.trim().toLowerCase();
  const permission = permissions.value.find((item) => item.name.toLowerCase() === normalizedPermissionName);
  return permission?.id ?? null;
}

async function addPermissionToSelectedRole() {
  if (!selectedRole.value || actionLoading.value) return;
  if (!permissionToAssignId.value) {
    errorMessage.value = "Выберите permission для добавления.";
    return;
  }

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await assignPermissionToRole(selectedRole.value.id, permissionToAssignId.value);
    permissionToAssignId.value = "";
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    successMessage.value = "Permission добавлен в роль.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось добавить permission.";
  } finally {
    actionLoading.value = false;
  }
}

async function removePermissionFromSelectedRole(permissionName: string) {
  if (!selectedRole.value || actionLoading.value) return;

  const permissionId = getPermissionIdByName(permissionName);
  if (!permissionId) {
    errorMessage.value = `Permission ${permissionName} не найден в справочнике.`;
    return;
  }

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await removePermissionFromRole(selectedRole.value.id, permissionId);
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    successMessage.value = "Permission убран из роли.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось убрать permission.";
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
    await assignParentRoleToRole(selectedRole.value.id, parentRoleToAssignId.value);
    parentRoleToAssignId.value = "";
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    successMessage.value = "Наследование роли добавлено.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось добавить наследование роли.";
  } finally {
    actionLoading.value = false;
  }
}

async function removeParentRoleFromSelectedRole(parentRoleId: string) {
  if (!selectedRole.value || actionLoading.value) return;

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await removeParentRoleFromRole(selectedRole.value.id, parentRoleId);
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    successMessage.value = "Наследование роли удалено.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось удалить наследование роли.";
  } finally {
    actionLoading.value = false;
  }
}

async function createNewUser() {
  if (actionLoading.value || loading.value) return;

  const username = createUsername.value.trim();
  const email = createEmail.value.trim();
  const password = createPassword.value;

  if (!username || !email || !password) {
    errorMessage.value = "Заполните username, email и password.";
    return;
  }

  const uniqueRoles = Array.from(
    new Set(
      createRoleNames.value
        .map((roleName) => roleName.trim())
        .filter((roleName) => roleName.length > 0)
    )
  );

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
    activeSection.value = "users";
    successMessage.value = `Пользователь ${created.username} создан.`;
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось создать пользователя.";
  } finally {
    actionLoading.value = false;
  }
}

function getRoleIdByName(roleName: string): string | null {
  const normalizedRoleName = roleName.trim().toLowerCase();
  const role = roles.value.find((item) => item.name.toLowerCase() === normalizedRoleName);
  return role?.id ?? null;
}

function getRolePermissionsPreview(roleName: string): string {
  const normalizedRoleName = roleName.trim().toLowerCase();
  const role = roles.value.find((item) => item.name.toLowerCase() === normalizedRoleName);

  if (!role || role.permissions.length === 0) {
    return "Permissions не настроены.";
  }

  const preview = role.permissions.slice(0, 4).join(", ");
  return role.permissions.length > 4 ? `${preview}...` : preview;
}

async function saveUser() {
  if (!selectedUser.value || actionLoading.value) return;

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const updated = await updateUser(selectedUser.value.id, editUsername.value.trim(), editEmail.value.trim());
    selectedUser.value = updated;
    await reloadUsersAndKeepSelection(updated.id);
    successMessage.value = "Пользователь обновлён.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось сохранить пользователя.";
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
    errorMessage.value = e?.response?.data?.error || "Не удалось изменить статус пользователя.";
  } finally {
    actionLoading.value = false;
  }
}

async function deleteSelectedUser() {
  if (!selectedUser.value || actionLoading.value) return;
  if (!window.confirm(`Удалить пользователя ${selectedUser.value.username}?`)) return;

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const deletedUserId = selectedUser.value.id;
    await deleteUser(deletedUserId);
    successMessage.value = "Пользователь удалён.";

    const nextUserId = users.value.find((user) => user.id !== deletedUserId)?.id ?? "";
    await reloadUsersAndKeepSelection(nextUserId);
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось удалить пользователя.";
  } finally {
    actionLoading.value = false;
  }
}

async function assignRoleToSelectedUser() {
  if (!selectedUser.value || actionLoading.value) return;
  if (!roleToAssignId.value) {
    errorMessage.value = "Выберите роль для назначения.";
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
    errorMessage.value = e?.response?.data?.error || "Не удалось назначить роль.";
  } finally {
    actionLoading.value = false;
  }
}

async function removeRoleFromSelectedUser(roleName: string) {
  if (!selectedUser.value || actionLoading.value) return;

  const roleId = getRoleIdByName(roleName);
  if (!roleId) {
    errorMessage.value = `Роль ${roleName} не найдена в справочнике ролей.`;
    return;
  }

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    await removeRole(selectedUser.value.id, roleId);
    await reloadUsersAndKeepSelection(selectedUser.value.id);
    successMessage.value = "Роль удалена у пользователя.";
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось убрать роль.";
  } finally {
    actionLoading.value = false;
  }
}

onMounted(async () => {
  await loadData();
});
</script>
