<template>
  <div class="container">
    <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px">
      <div>
        <h1 style="margin: 0">Superadmin Panel</h1>
        <p style="margin: 4px 0 0; color: #6b7280">Управление пользователями, ролями и доступами</p>
      </div>
      <button class="btn btn-outline" @click="loadData" :disabled="loading">Обновить</button>
    </div>

    <div v-if="errorMessage" class="error" style="margin-bottom: 12px">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success" style="margin-bottom: 12px">{{ successMessage }}</div>

    <div v-if="loading">Загрузка...</div>
    <div v-else-if="users.length === 0" class="card">Пользователей пока нет.</div>

    <div v-else style="display: grid; gap: 16px; grid-template-columns: 340px 1fr">
      <aside class="card">
        <label class="label" for="searchUser">Поиск пользователей</label>
        <input
          id="searchUser"
          v-model="searchQuery"
          class="input"
          type="text"
          placeholder="Username или email"
          style="margin-bottom: 12px"
        />

        <ul style="list-style: none; margin: 0; padding: 0; display: grid; gap: 8px">
          <li v-for="user in filteredUsers" :key="user.id">
            <button
              class="btn btn-outline"
              style="width: 100%; text-align: left; padding: 12px"
              @click="selectUser(user.id)"
            >
              <div style="font-weight: 600">{{ user.username }}</div>
              <div style="font-size: 13px; color: #6b7280">{{ user.email }}</div>
              <div style="margin-top: 6px">
                <span :class="user.isActive ? 'badge badge-active' : 'badge badge-inactive'">
                  {{ user.isActive ? "Active" : "Inactive" }}
                </span>
              </div>
            </button>
          </li>
        </ul>
      </aside>

      <section class="card" v-if="selectedUser">
        <h2 style="margin-top: 0; margin-bottom: 12px">Пользователь</h2>

        <div style="display: grid; gap: 10px; margin-bottom: 12px">
          <div><strong>ID:</strong> {{ selectedUser.id }}</div>
          <div><strong>Статус:</strong> {{ selectedUser.isActive ? "Active" : "Inactive" }}</div>
        </div>

        <div style="display: grid; gap: 12px; grid-template-columns: 1fr 1fr; margin-bottom: 12px">
          <div>
            <label class="label" for="username">Username</label>
            <input id="username" v-model="editUsername" class="input" type="text" />
          </div>
          <div>
            <label class="label" for="email">Email</label>
            <input id="email" v-model="editEmail" class="input" type="email" />
          </div>
        </div>

        <div style="display: flex; gap: 8px; margin-bottom: 16px; flex-wrap: wrap">
          <button class="btn btn-primary" @click="saveUser" :disabled="actionLoading">Сохранить</button>
          <button
            class="btn btn-outline"
            @click="toggleActive"
            :disabled="actionLoading"
          >
            {{ selectedUser.isActive ? "Деактивировать" : "Активировать" }}
          </button>
          <button class="btn btn-danger" @click="deleteSelectedUser" :disabled="actionLoading">
            Удалить
          </button>
        </div>

        <div style="margin-top: 16px">
          <h3 style="margin-top: 0">Роли и права</h3>

          <div v-if="selectedUser.roles.length === 0" style="color: #6b7280; margin-bottom: 10px">
            Роли отсутствуют.
          </div>

          <ul v-else style="list-style: none; margin: 0 0 12px; padding: 0; display: grid; gap: 8px">
            <li
              v-for="roleName in selectedUser.roles"
              :key="roleName"
              style="display: flex; align-items: center; justify-content: space-between; border: 1px solid #e5e7eb; border-radius: 8px; padding: 8px 10px"
            >
              <div style="display: grid; gap: 4px">
                <strong>{{ roleName }}</strong>
                <span style="font-size: 12px; color: #6b7280">
                  {{ getRolePermissionsPreview(roleName) }}
                </span>
              </div>
              <button class="btn btn-outline" @click="removeRoleFromSelectedUser(roleName)" :disabled="actionLoading">
                Убрать
              </button>
            </li>
          </ul>

          <div style="display: flex; gap: 8px; align-items: end; flex-wrap: wrap">
            <div style="min-width: 260px">
              <label class="label" for="roleSelect">Добавить роль</label>
              <select id="roleSelect" v-model="roleToAssignId" class="select">
                <option value="">Выберите роль</option>
                <option v-for="role in availableRolesForAssignment" :key="role.id" :value="role.id">
                  {{ role.name }}
                </option>
              </select>
            </div>
            <button class="btn btn-primary" @click="assignRoleToSelectedUser" :disabled="actionLoading">
              Назначить роль
            </button>
          </div>

          <div style="margin-top: 14px">
            <strong>Итоговые permissions:</strong>
            <div style="margin-top: 6px; display: flex; gap: 6px; flex-wrap: wrap">
              <span v-for="permission in selectedUser.permissions" :key="permission" class="badge badge-neutral">
                {{ permission }}
              </span>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { getRoles } from "../api/roles";
import {
  activateUser,
  assignRole,
  deactivateUser,
  deleteUser,
  getUserById,
  getUsers,
  removeRole,
  updateUser,
} from "../api/users";
import type { Role } from "../types/Role";
import type { User } from "../types/User";

const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

const users = ref<User[]>([]);
const roles = ref<Role[]>([]);
const selectedUser = ref<User | null>(null);
const selectedUserId = ref("");

const editUsername = ref("");
const editEmail = ref("");
const roleToAssignId = ref("");
const searchQuery = ref("");

const filteredUsers = computed(() => {
  const query = searchQuery.value.trim().toLowerCase();
  if (!query) return users.value;

  return users.value.filter(
    (user) =>
      user.username.toLowerCase().includes(query) ||
      user.email.toLowerCase().includes(query)
  );
});

const availableRolesForAssignment = computed(() => {
  if (!selectedUser.value) return [];

  const assignedRoleNames = new Set(selectedUser.value.roles.map((role) => role.toLowerCase()));
  return roles.value.filter((role) => !assignedRoleNames.has(role.name.toLowerCase()));
});

function syncEditableFields() {
  if (!selectedUser.value) {
    editUsername.value = "";
    editEmail.value = "";
    return;
  }

  editUsername.value = selectedUser.value.username;
  editEmail.value = selectedUser.value.email;
}

async function loadData() {
  loading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const [loadedRoles] = await Promise.all([getRoles()]);
    roles.value = loadedRoles;
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
    const updated = await updateUser(
      selectedUser.value.id,
      editUsername.value.trim(),
      editEmail.value.trim()
    );

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
