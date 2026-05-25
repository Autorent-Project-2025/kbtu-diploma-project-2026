<template>
  <div>
  <!-- Loading skeleton -->
  <template v-if="loading && !users.length">
    <div class="grid xl:grid-cols-[340px,1fr] gap-6 items-start">
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-5 space-y-3"
      >
        <div
          class="h-5 w-20 rounded bg-gray-200 dark:bg-gray-800 animate-pulse"
        />
        <div
          class="h-10 rounded-xl bg-gray-100 dark:bg-gray-800 animate-pulse"
        />
        <div
          v-for="n in 6"
          :key="n"
          class="h-14 rounded-xl bg-gray-100 dark:bg-gray-800/60 animate-pulse"
        />
      </div>
      <div class="space-y-6">
        <div
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-4"
        >
          <div
            class="h-6 w-32 rounded bg-gray-200 dark:bg-gray-800 animate-pulse"
          />
          <div class="grid grid-cols-2 gap-4">
            <div
              class="h-10 rounded-xl bg-gray-100 dark:bg-gray-800 animate-pulse"
            />
            <div
              class="h-10 rounded-xl bg-gray-100 dark:bg-gray-800 animate-pulse"
            />
          </div>
          <div
            class="h-10 w-36 rounded-2xl bg-gray-200 dark:bg-gray-800 animate-pulse"
          />
        </div>
      </div>
    </div>
  </template>

  <section
    v-else
    class="grid xl:grid-cols-[340px,1fr] gap-6 items-start"
  >
    <!-- Users sidebar -->
    <div
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
    >
      <div
        class="px-5 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between"
      >
        <h2 class="font-bold text-gray-900 dark:text-white text-sm">
          Пользователи
        </h2>
        <span
          class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400 px-2 py-0.5 rounded-full tabular-nums"
        >
          {{ filteredUsers.length }}
        </span>
      </div>

      <div class="p-3">
        <div class="relative">
          <svg
            class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
            />
          </svg>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Поиск..."
            class="w-full pl-10 pr-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all placeholder-gray-400"
          />
        </div>
      </div>

      <ul class="max-h-[60vh] overflow-y-auto">
        <li v-for="user in filteredUsers" :key="user.id">
          <button
            @click="selectUser(user.id)"
            :class="[
              'w-full px-4 py-3 text-left transition-all duration-150 border-l-[3px]',
              selectedUser?.id === user.id
                ? 'bg-violet-50 dark:bg-violet-500/10 border-violet-500'
                : 'border-transparent hover:bg-gray-50 dark:hover:bg-gray-800/50',
            ]"
          >
            <div class="flex items-center gap-3">
              <div
                :class="[
                  'w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold shrink-0',
                  user.isActive
                    ? 'bg-violet-100 dark:bg-violet-500/20 text-violet-700 dark:text-violet-300'
                    : 'bg-gray-200 dark:bg-gray-700 text-gray-500 dark:text-gray-400',
                ]"
              >
                {{ userInitials(user.username) }}
              </div>
              <div class="min-w-0 flex-1">
                <div class="flex items-center gap-2">
                  <p
                    class="font-semibold text-sm text-gray-900 dark:text-white truncate"
                  >
                    {{ user.username }}
                  </p>
                  <span
                    :class="[
                      'w-2 h-2 rounded-full shrink-0',
                      user.isActive ? 'bg-emerald-500' : 'bg-gray-400',
                    ]"
                    :title="user.isActive ? 'Active' : 'Inactive'"
                  />
                </div>
                <p
                  class="text-xs text-gray-400 dark:text-gray-500 truncate"
                >
                  {{ user.email }}
                </p>
              </div>
            </div>
          </button>
        </li>
        <li
          v-if="filteredUsers.length === 0"
          class="px-5 py-8 text-center text-sm text-gray-400 dark:text-gray-500"
        >
          Пользователи не найдены
        </li>
      </ul>
    </div>

    <!-- Users right panel -->
    <div class="space-y-6">
      <!-- Create user (collapsible) -->
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <button
          @click="showCreateUser = !showCreateUser"
          class="w-full px-6 py-4 flex items-center justify-between hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
        >
          <div class="flex items-center gap-3">
            <div
              class="w-8 h-8 rounded-lg bg-violet-100 dark:bg-violet-500/20 flex items-center justify-center"
            >
              <svg
                class="w-4 h-4 text-violet-600 dark:text-violet-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z"
                />
              </svg>
            </div>
            <span class="font-semibold text-sm text-gray-900 dark:text-white"
              >Создать пользователя</span
            >
          </div>
          <svg
            :class="[
              'w-5 h-5 text-gray-400 transition-transform duration-200',
              showCreateUser ? 'rotate-180' : '',
            ]"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </button>

        <div
          v-if="showCreateUser"
          class="px-6 pb-6 border-t border-gray-100 dark:border-gray-800 pt-5"
        >
          <form @submit.prevent="createNewUser" class="space-y-4">
            <div class="grid sm:grid-cols-3 gap-3">
              <div>
                <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Username</label>
                <input
                  v-model="createUsername"
                  type="text"
                  required
                  placeholder="john_doe"
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all placeholder-gray-400"
                />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Email</label>
                <input
                  v-model="createEmail"
                  type="email"
                  required
                  placeholder="john@example.com"
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all placeholder-gray-400"
                />
              </div>
              <div>
                <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Password</label>
                <input
                  v-model="createPassword"
                  type="password"
                  required
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all"
                />
              </div>
            </div>

            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Роли</label>
              <div
                v-if="createRoleNames.length"
                class="flex flex-wrap gap-1.5 mb-2"
              >
                <span
                  v-for="name in createRoleNames"
                  :key="name"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-blue-50 dark:bg-blue-500/10 text-blue-700 dark:text-blue-300 text-xs font-medium"
                >
                  {{ name }}
                  <button
                    type="button"
                    @click="
                      createRoleNames = createRoleNames.filter(
                        (r) => r !== name,
                      )
                    "
                    class="hover:text-red-500 transition-colors ml-0.5"
                  >
                    &times;
                  </button>
                </span>
              </div>
              <select
                v-model="tempCreateRoleName"
                @change="addCreateUserRole"
                class="w-full px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить роль...</option>
                <option
                  v-for="role in availableCreateUserRoles"
                  :key="role.id"
                  :value="role.name"
                >
                  {{ role.name }}
                </option>
              </select>
              <p class="text-xs text-gray-400 dark:text-gray-500 mt-1.5">
                Если роли не выбраны, назначается роль по умолчанию.
              </p>
            </div>

            <button
              type="submit"
              :disabled="actionLoading"
              class="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-violet-600 hover:bg-violet-700 active:bg-violet-800 disabled:opacity-50 text-white font-semibold text-sm shadow-lg shadow-violet-500/20 transition-all"
            >
              <svg
                v-if="actionLoading"
                class="w-4 h-4 animate-spin"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                />
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                />
              </svg>
              Создать
            </button>
          </form>
        </div>
      </div>

      <!-- Selected user detail -->
      <div
        v-if="selectedUser"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <!-- User header -->
        <div class="px-6 py-5 border-b border-gray-100 dark:border-gray-800">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div
                :class="[
                  'w-12 h-12 rounded-xl flex items-center justify-center text-sm font-bold',
                  selectedUser.isActive
                    ? 'bg-violet-100 dark:bg-violet-500/20 text-violet-700 dark:text-violet-300'
                    : 'bg-gray-200 dark:bg-gray-700 text-gray-500 dark:text-gray-400',
                ]"
              >
                {{ userInitials(selectedUser.username) }}
              </div>
              <div>
                <h2
                  class="text-lg font-bold text-gray-900 dark:text-white"
                >
                  {{ selectedUser.username }}
                </h2>
                <p
                  class="text-xs text-gray-400 dark:text-gray-500 font-mono"
                >
                  {{ selectedUser.id }}
                </p>
              </div>
            </div>
            <span
              :class="[
                'px-3 py-1 rounded-full text-xs font-bold',
                selectedUser.isActive
                  ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300'
                  : 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300',
              ]"
            >
              {{ selectedUser.isActive ? "Active" : "Inactive" }}
            </span>
          </div>
        </div>

        <div class="p-6 space-y-6">
          <!-- Edit fields -->
          <div class="grid sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Username</label>
              <input
                v-model="editUsername"
                type="text"
                class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all"
              />
            </div>
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Email</label>
              <input
                v-model="editEmail"
                type="email"
                class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all"
              />
            </div>
          </div>

          <!-- Action buttons -->
          <div class="flex flex-wrap gap-2">
            <button
              @click="saveUser"
              :disabled="actionLoading"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 active:bg-violet-800 disabled:opacity-50 text-white font-semibold text-sm transition-all"
            >
              <svg
                v-if="actionLoading"
                class="w-4 h-4 animate-spin"
                fill="none"
                viewBox="0 0 24 24"
              >
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                />
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                />
              </svg>
              Сохранить
            </button>
            <button
              @click="toggleActive"
              :disabled="actionLoading"
              :class="[
                'px-4 py-2 rounded-xl border font-semibold text-sm transition-all disabled:opacity-50',
                selectedUser.isActive
                  ? 'border-amber-300 dark:border-amber-600/50 text-amber-700 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-500/10'
                  : 'border-emerald-300 dark:border-emerald-600/50 text-emerald-700 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-500/10',
              ]"
            >
              {{
                selectedUser.isActive ? "Деактивировать" : "Активировать"
              }}
            </button>
            <button
              @click="confirmDeleteUser"
              :disabled="actionLoading"
              class="px-4 py-2 rounded-xl border border-red-200 dark:border-red-600/50 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-500/10 font-semibold text-sm transition-all disabled:opacity-50"
            >
              Удалить
            </button>
          </div>

          <!-- User roles -->
          <div
            class="space-y-3 pt-4 border-t border-gray-100 dark:border-gray-800"
          >
            <h3
              class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400"
            >
              Роли
            </h3>
            <div
              v-if="selectedUser.roles.length === 0"
              class="text-sm text-gray-400 dark:text-gray-500 italic"
            >
              Роли отсутствуют
            </div>
            <div v-else class="space-y-2">
              <div
                v-for="roleName in selectedUser.roles"
                :key="roleName"
                class="flex items-center justify-between gap-3 px-4 py-3 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50"
              >
                <div class="min-w-0">
                  <p
                    class="font-semibold text-sm text-gray-900 dark:text-white"
                  >
                    {{ roleName }}
                  </p>
                  <p
                    class="text-xs text-gray-400 dark:text-gray-500 truncate"
                  >
                    {{ getRolePermissionsPreview(roleName) }}
                  </p>
                </div>
                <button
                  @click="removeRoleFromSelectedUser(roleName)"
                  :disabled="actionLoading"
                  class="text-gray-400 hover:text-red-500 transition-colors disabled:opacity-50 shrink-0"
                >
                  <svg
                    class="w-4 h-4"
                    fill="none"
                    viewBox="0 0 24 24"
                    stroke="currentColor"
                    stroke-width="2"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </div>
            </div>
            <div class="flex gap-2">
              <select
                v-model="roleToAssignId"
                class="flex-1 px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить роль...</option>
                <option
                  v-for="role in availableRolesForAssignment"
                  :key="role.id"
                  :value="role.id"
                >
                  {{ role.name }}
                </option>
              </select>
              <button
                @click="assignRoleToSelectedUser"
                :disabled="actionLoading || !roleToAssignId"
                class="px-3.5 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 disabled:opacity-40 text-white text-sm font-semibold transition-all"
              >
                <svg
                  class="w-4 h-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M12 4v16m8-8H4"
                  />
                </svg>
              </button>
            </div>
          </div>

          <!-- Effective permissions -->
          <div
            class="space-y-3 pt-4 border-t border-gray-100 dark:border-gray-800"
          >
            <h3
              class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400"
            >
              Итоговые permissions
            </h3>
            <div
              v-if="selectedUser.permissions.length === 0"
              class="text-sm text-gray-400 dark:text-gray-500 italic"
            >
              Нет permissions
            </div>
            <div v-else class="flex flex-wrap gap-1.5">
              <span
                v-for="permission in selectedUser.permissions"
                :key="permission"
                :class="[
                  'inline-flex px-2.5 py-1 rounded-md text-xs font-semibold',
                  permissionColor(permission),
                ]"
              >
                {{ permission }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty user state -->
      <div
        v-else
        class="rounded-3xl border-2 border-dashed border-gray-200 dark:border-gray-800 p-12 text-center"
      >
        <div
          class="w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center mx-auto mb-4"
        >
          <svg
            class="w-6 h-6 text-gray-400"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="1.5"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
            />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
          Выберите пользователя
        </p>
        <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
          Управляйте ролями и настройками
        </p>
      </div>
    </div>
  </section>

  <!-- Confirm delete modal -->
  <ConfirmModal
    :show="confirmModal.show"
    :title="confirmModal.title"
    :message="confirmModal.message"
    confirm-text="Удалить"
    variant="danger"
    @confirm="onConfirmDelete"
    @cancel="confirmModal.show = false"
  />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useToast } from "../../composables/useToast";
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
  type UserDto,
} from "../../api/users";
import { getRoles } from "../../api/roles";
import type { Role } from "../../types/Role";
import ConfirmModal from "../ConfirmModal.vue";

const emit = defineEmits<{
  loaded: [{ usersCount: number; activeUsersCount: number }];
}>();

const toast = useToast();

const loading = ref(false);
const actionLoading = ref(false);

const users = ref<UserDto[]>([]);
const roles = ref<Role[]>([]);
const selectedUser = ref<UserDto | null>(null);
const selectedUserId = ref("");
const searchQuery = ref("");
const roleToAssignId = ref("");

const editUsername = ref("");
const editEmail = ref("");

const showCreateUser = ref(false);
const createUsername = ref("");
const createEmail = ref("");
const createPassword = ref("");
const createRoleNames = ref<string[]>([]);
const tempCreateRoleName = ref("");

const confirmModal = reactive({
  show: false,
  title: "",
  message: "",
  pendingUserId: "",
});

// ── Computed ──────────────────────────────────────────────────────────────────

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

const availableCreateUserRoles = computed(() => {
  const selected = new Set(
    createRoleNames.value.map((n) => n.toLowerCase()),
  );
  return roles.value.filter((r) => !selected.has(r.name.toLowerCase()));
});

// ── Helpers ───────────────────────────────────────────────────────────────────

function userInitials(username: string): string {
  return username.slice(0, 2).toUpperCase();
}

function permissionColor(name: string): string {
  const prefix = name.split(".")[0]?.toLowerCase() ?? "";
  const colors: Record<string, string> = {
    user: "bg-violet-100 dark:bg-violet-500/15 text-violet-700 dark:text-violet-300",
    role: "bg-purple-100 dark:bg-purple-500/15 text-purple-700 dark:text-purple-300",
    ticket:
      "bg-emerald-100 dark:bg-emerald-500/15 text-emerald-700 dark:text-emerald-300",
    client:
      "bg-blue-100 dark:bg-blue-500/15 text-blue-700 dark:text-blue-300",
    partnercar:
      "bg-amber-100 dark:bg-amber-500/15 text-amber-700 dark:text-amber-300",
    booking:
      "bg-rose-100 dark:bg-rose-500/15 text-rose-700 dark:text-rose-300",
    partner:
      "bg-orange-100 dark:bg-orange-500/15 text-orange-700 dark:text-orange-300",
  };
  return (
    colors[prefix] ??
    "bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300"
  );
}

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

function addCreateUserRole() {
  if (
    tempCreateRoleName.value &&
    !createRoleNames.value.includes(tempCreateRoleName.value)
  ) {
    createRoleNames.value.push(tempCreateRoleName.value);
  }
  tempCreateRoleName.value = "";
}

// ── Data loading ──────────────────────────────────────────────────────────────

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

function emitLoaded() {
  emit("loaded", {
    usersCount: users.value.length,
    activeUsersCount: users.value.filter((u) => u.isActive).length,
  });
}

async function loadData() {
  loading.value = true;
  try {
    const [loadedRoles] = await Promise.all([getRoles()]);
    roles.value = loadedRoles;
    await reloadUsersAndKeepSelection(selectedUserId.value);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось загрузить данные.");
  } finally {
    loading.value = false;
  }
}

async function selectUser(userId: string) {
  if (!userId) return;
  selectedUserId.value = userId;
  roleToAssignId.value = "";
  try {
    const user = await getUserById(userId);
    selectedUser.value = user;
    syncEditableFields();
    emitLoaded();
  } catch (e: any) {
    toast.error("Не удалось загрузить пользователя.");
  }
}

// ── Actions ───────────────────────────────────────────────────────────────────

async function createNewUser() {
  if (actionLoading.value || loading.value) return;
  const username = createUsername.value.trim(),
    email = createEmail.value.trim(),
    password = createPassword.value;
  if (!username || !email || !password) {
    toast.warning("Заполните username, email и password.");
    return;
  }
  const uniqueRoles = [
    ...new Set(createRoleNames.value.map((r) => r.trim()).filter(Boolean)),
  ];
  actionLoading.value = true;
  try {
    const created = await createUserApi({
      username,
      email,
      password,
      roles: uniqueRoles.length > 0 ? uniqueRoles : undefined,
    });
    resetCreateUserForm();
    await reloadUsersAndKeepSelection(created.userId);
    showCreateUser.value = false;
    toast.success(`Пользователь ${created.username} создан.`);
  } catch (e: any) {
    toast.error(
      e?.response?.data?.error || "Не удалось создать пользователя.",
    );
  } finally {
    actionLoading.value = false;
  }
}

async function saveUser() {
  if (!selectedUser.value || actionLoading.value) return;
  actionLoading.value = true;
  try {
    const updated = await updateUser(
      selectedUser.value.id,
      editUsername.value.trim(),
      editEmail.value.trim(),
    );
    selectedUser.value = updated;
    await reloadUsersAndKeepSelection(updated.id);
    toast.success("Пользователь обновлён.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function toggleActive() {
  if (!selectedUser.value || actionLoading.value) return;
  actionLoading.value = true;
  try {
    if (selectedUser.value.isActive) {
      await deactivateUser(selectedUser.value.id);
      toast.success("Пользователь деактивирован.");
    } else {
      await activateUser(selectedUser.value.id);
      toast.success("Пользователь активирован.");
    }
    await reloadUsersAndKeepSelection(selectedUser.value.id);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

function confirmDeleteUser() {
  if (!selectedUser.value) return;
  const username = selectedUser.value.username;
  confirmModal.title = "Удалить пользователя?";
  confirmModal.message = `Пользователь "${username}" будет удалён. Это действие нельзя отменить.`;
  confirmModal.pendingUserId = selectedUser.value.id;
  confirmModal.show = true;
}

async function onConfirmDelete() {
  confirmModal.show = false;
  if (!selectedUser.value || actionLoading.value) return;
  actionLoading.value = true;
  try {
    const deletedId = selectedUser.value.id;
    await deleteUser(deletedId);
    toast.success("Пользователь удалён.");
    const nextId = users.value.find((u) => u.id !== deletedId)?.id ?? "";
    await reloadUsersAndKeepSelection(nextId);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function assignRoleToSelectedUser() {
  if (!selectedUser.value || actionLoading.value) return;
  if (!roleToAssignId.value) {
    toast.warning("Выберите роль.");
    return;
  }
  actionLoading.value = true;
  try {
    await assignRole(selectedUser.value.id, roleToAssignId.value);
    roleToAssignId.value = "";
    await reloadUsersAndKeepSelection(selectedUser.value.id);
    toast.success("Роль назначена.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function removeRoleFromSelectedUser(name: string) {
  if (!selectedUser.value || actionLoading.value) return;
  const id = getRoleIdByName(name);
  if (!id) {
    toast.error(`Роль ${name} не найдена.`);
    return;
  }
  actionLoading.value = true;
  try {
    await removeRole(selectedUser.value.id, id);
    await reloadUsersAndKeepSelection(selectedUser.value.id);
    toast.success("Роль удалена.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

// ── Expose for parent to call refresh ─────────────────────────────────────────

defineExpose({
  loadData,
  usersCount: computed(() => users.value.length),
  activeUsersCount: computed(() => users.value.filter((u) => u.isActive).length),
});

onMounted(async () => {
  await loadData();
});
</script>
