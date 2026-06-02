<template>
  <div>
  <!-- Loading skeleton -->
  <template v-if="loading && !roles.length">
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
    <!-- Roles sidebar -->
    <div
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
    >
      <div
        class="px-5 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between"
      >
        <h2 class="font-bold text-gray-900 dark:text-white text-sm">Роли</h2>
        <span
          class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400 px-2 py-0.5 rounded-full tabular-nums"
        >
          {{ filteredRoles.length }}
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
            v-model="roleSearchQuery"
            type="text"
            placeholder="Поиск..."
            class="w-full pl-10 pr-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all placeholder-gray-400"
          />
        </div>
      </div>

      <ul class="max-h-[60vh] overflow-y-auto">
        <li v-for="role in filteredRoles" :key="role.id">
          <button
            @click="selectRole(role.id)"
            :class="[
              'w-full px-5 py-3 text-left transition-all duration-150 border-l-[3px]',
              selectedRoleId === role.id
                ? 'bg-violet-50 dark:bg-violet-500/10 border-violet-500'
                : 'border-transparent hover:bg-gray-50 dark:hover:bg-gray-800/50',
            ]"
          >
            <div class="flex items-center justify-between">
              <p
                class="font-semibold text-sm text-gray-900 dark:text-white truncate"
              >
                {{ role.name }}
              </p>
              <span
                class="text-[10px] font-bold text-gray-400 dark:text-gray-500 tabular-nums shrink-0 ml-2"
              >
                {{ role.permissions.length }} perm
              </span>
            </div>
          </button>
        </li>
        <li
          v-if="filteredRoles.length === 0"
          class="px-5 py-8 text-center text-sm text-gray-400 dark:text-gray-500"
        >
          Роли не найдены
        </li>
      </ul>
    </div>

    <!-- Roles right panel -->
    <div class="space-y-6">
      <!-- Create role (collapsible) -->
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <button
          @click="showCreateRole = !showCreateRole"
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
                  d="M12 4v16m8-8H4"
                />
              </svg>
            </div>
            <span class="font-semibold text-sm text-gray-900 dark:text-white"
              >Создать роль</span
            >
          </div>
          <svg
            :class="[
              'w-5 h-5 text-gray-400 transition-transform duration-200',
              showCreateRole ? 'rotate-180' : '',
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
          v-if="showCreateRole"
          class="px-6 pb-6 border-t border-gray-100 dark:border-gray-800 pt-5"
        >
          <form @submit.prevent="createNewRole" class="space-y-4">
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Название</label>
              <input
                v-model="createRoleName"
                type="text"
                required
                placeholder="Например: DataManager"
                class="w-full px-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-all placeholder-gray-400"
              />
            </div>

            <!-- Permission chips selector -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Permissions</label>
              <div
                v-if="createRolePermissionIds.length"
                class="flex flex-wrap gap-1.5 mb-2"
              >
                <span
                  v-for="id in createRolePermissionIds"
                  :key="id"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-violet-50 dark:bg-violet-500/10 text-violet-700 dark:text-violet-300 text-xs font-medium"
                >
                  {{ getPermissionNameById(id) }}
                  <button
                    type="button"
                    @click="
                      createRolePermissionIds =
                        createRolePermissionIds.filter((p) => p !== id)
                    "
                    class="hover:text-red-500 transition-colors ml-0.5"
                  >
                    &times;
                  </button>
                </span>
              </div>
              <select
                v-model="tempPermissionId"
                @change="addCreateRolePermission"
                class="w-full px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить permission...</option>
                <option
                  v-for="p in availableCreatePermissions"
                  :key="p.id"
                  :value="p.id"
                >
                  {{ p.name }}
                </option>
              </select>
            </div>

            <!-- Parent role chips selector -->
            <div>
              <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Родительские роли</label>
              <div
                v-if="createRoleParentRoleIds.length"
                class="flex flex-wrap gap-1.5 mb-2"
              >
                <span
                  v-for="id in createRoleParentRoleIds"
                  :key="id"
                  class="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-blue-50 dark:bg-blue-500/10 text-blue-700 dark:text-blue-300 text-xs font-medium"
                >
                  {{ getRoleNameById(id) }}
                  <button
                    type="button"
                    @click="
                      createRoleParentRoleIds =
                        createRoleParentRoleIds.filter((r) => r !== id)
                    "
                    class="hover:text-red-500 transition-colors ml-0.5"
                  >
                    &times;
                  </button>
                </span>
              </div>
              <select
                v-model="tempParentRoleId"
                @change="addCreateRoleParent"
                class="w-full px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить роль...</option>
                <option
                  v-for="role in roles"
                  :key="role.id"
                  :value="role.id"
                >
                  {{ role.name }}
                </option>
              </select>
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

      <!-- Selected role detail -->
      <div
        v-if="selectedRole"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <div class="px-6 py-5 border-b border-gray-100 dark:border-gray-800">
          <div class="flex items-center gap-3">
            <div
              class="w-10 h-10 rounded-xl bg-violet-100 dark:bg-violet-500/20 flex items-center justify-center"
            >
              <svg
                class="w-5 h-5 text-violet-600 dark:text-violet-400"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
                stroke-width="2"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                />
              </svg>
            </div>
            <div>
              <h2 class="text-lg font-bold text-gray-900 dark:text-white">
                {{ selectedRole.name }}
              </h2>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ selectedRole.permissions.length }} итоговых permissions
              </p>
            </div>
          </div>
        </div>

        <div class="p-6 space-y-6">
          <!-- Direct permissions -->
          <div class="space-y-3">
            <h3
              class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400"
            >
              Прямые permissions
            </h3>
            <div
              v-if="selectedRole.directPermissions.length === 0"
              class="text-sm text-gray-400 dark:text-gray-500 italic"
            >
              Не назначены
            </div>
            <div v-else class="flex flex-wrap gap-2">
              <span
                v-for="name in selectedRole.directPermissions"
                :key="name"
                class="group inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm font-medium text-gray-700 dark:text-gray-300"
              >
                {{ name }}
                <button
                  @click="removePermissionFromSelectedRole(name)"
                  :disabled="actionLoading"
                  class="text-gray-400 hover:text-red-500 transition-colors disabled:opacity-50"
                >
                  <svg
                    class="w-3.5 h-3.5"
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
              </span>
            </div>
            <div class="flex gap-2">
              <select
                v-model="permissionToAssignId"
                class="flex-1 px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить...</option>
                <option
                  v-for="p in availablePermissionsForSelectedRole"
                  :key="p.id"
                  :value="p.id"
                >
                  {{ p.name }}
                </option>
              </select>
              <button
                @click="addPermissionToSelectedRole"
                :disabled="actionLoading || !permissionToAssignId"
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

          <!-- Parent roles -->
          <div
            class="space-y-3 pt-4 border-t border-gray-100 dark:border-gray-800"
          >
            <h3
              class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400"
            >
              Наследуемые роли
            </h3>
            <div
              v-if="selectedRole.parentRoles.length === 0"
              class="text-sm text-gray-400 dark:text-gray-500 italic"
            >
              Наследование не настроено
            </div>
            <div v-else class="flex flex-wrap gap-2">
              <span
                v-for="parent in selectedRole.parentRoles"
                :key="parent.id"
                class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg border border-blue-200 dark:border-blue-700/50 bg-blue-50 dark:bg-blue-500/10 text-sm font-medium text-blue-700 dark:text-blue-300"
              >
                {{ parent.name }}
                <button
                  @click="removeParentRoleFromSelectedRole(parent.id)"
                  :disabled="actionLoading"
                  class="text-blue-400 hover:text-red-500 transition-colors disabled:opacity-50"
                >
                  <svg
                    class="w-3.5 h-3.5"
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
              </span>
            </div>
            <div class="flex gap-2">
              <select
                v-model="parentRoleToAssignId"
                class="flex-1 px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option value="">+ Добавить...</option>
                <option
                  v-for="role in availableParentRolesForSelectedRole"
                  :key="role.id"
                  :value="role.id"
                >
                  {{ role.name }}
                </option>
              </select>
              <button
                @click="addParentRoleToSelectedRole"
                :disabled="actionLoading || !parentRoleToAssignId"
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
              v-if="selectedRole.permissions.length === 0"
              class="text-sm text-gray-400 dark:text-gray-500 italic"
            >
              Нет permissions
            </div>
            <div v-else class="flex flex-wrap gap-1.5">
              <span
                v-for="name in selectedRole.permissions"
                :key="name"
                :class="[
                  'inline-flex px-2.5 py-1 rounded-md text-xs font-semibold',
                  permissionColor(name),
                ]"
              >
                {{ name }}
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty role state -->
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
              d="M15 15l-2 5L9 9l11 4-5 2zm0 0l5 5M7.188 2.239l.777 2.897M5.136 7.965l-2.898-.777M13.95 4.05l-2.122 2.122m-5.657 5.656l-2.12 2.122"
            />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-500 dark:text-gray-400">
          Выберите роль для редактирования
        </p>
        <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
          Настраивайте permissions и наследование
        </p>
      </div>
    </div>
  </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useToast } from "../../composables/useToast";
import { getPermissions } from "../../api/permissions";
import {
  assignParentRoleToRole,
  assignPermissionToRole,
  createRole as createRoleApi,
  getRoles,
  removeParentRoleFromRole,
  removePermissionFromRole,
} from "../../api/roles";
import type { Permission } from "../../types/Permission";
import type { Role } from "../../types/Role";

const emit = defineEmits<{
  loaded: [{ rolesCount: number; permissionsCount: number }];
}>();

const toast = useToast();

const loading = ref(false);
const actionLoading = ref(false);

const roles = ref<Role[]>([]);
const permissions = ref<Permission[]>([]);
const selectedRoleId = ref("");
const roleSearchQuery = ref("");

const showCreateRole = ref(false);
const createRoleName = ref("");
const createRolePermissionIds = ref<string[]>([]);
const createRoleParentRoleIds = ref<string[]>([]);
const tempPermissionId = ref("");
const tempParentRoleId = ref("");
const permissionToAssignId = ref("");
const parentRoleToAssignId = ref("");

// ── Computed ──────────────────────────────────────────────────────────────────

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

const availableCreatePermissions = computed(() => {
  const selected = new Set(createRolePermissionIds.value);
  return permissions.value.filter((p) => !selected.has(p.id));
});

// ── Helpers ───────────────────────────────────────────────────────────────────

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

function getPermissionNameById(id: string): string {
  return permissions.value.find((p) => p.id === id)?.name ?? id;
}

function getRoleNameById(id: string): string {
  return roles.value.find((r) => r.id === id)?.name ?? id;
}

function getPermissionIdByName(name: string): string | null {
  return (
    permissions.value.find(
      (p) => p.name.toLowerCase() === name.trim().toLowerCase(),
    )?.id ?? null
  );
}

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

function addCreateRolePermission() {
  if (
    tempPermissionId.value &&
    !createRolePermissionIds.value.includes(tempPermissionId.value)
  ) {
    createRolePermissionIds.value.push(tempPermissionId.value);
  }
  tempPermissionId.value = "";
}

function addCreateRoleParent() {
  if (
    tempParentRoleId.value &&
    !createRoleParentRoleIds.value.includes(tempParentRoleId.value)
  ) {
    createRoleParentRoleIds.value.push(tempParentRoleId.value);
  }
  tempParentRoleId.value = "";
}

// ── Data loading ──────────────────────────────────────────────────────────────

async function reloadRolesAndKeepSelection(preferredId = "") {
  const loaded = await getRoles();
  roles.value = loaded;
  if (loaded.length === 0) {
    selectedRoleId.value = "";
    return;
  }
  const has = preferredId ? loaded.some((r) => r.id === preferredId) : false;
  selectRole(has ? preferredId : (loaded[0]?.id ?? ""));
}

async function loadData() {
  loading.value = true;
  try {
    const [loadedPerms] = await Promise.all([getPermissions()]);
    permissions.value = loadedPerms;
    await reloadRolesAndKeepSelection(selectedRoleId.value);
    emit("loaded", {
      rolesCount: roles.value.length,
      permissionsCount: permissions.value.length,
    });
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось загрузить роли.");
  } finally {
    loading.value = false;
  }
}

// ── Actions ───────────────────────────────────────────────────────────────────

async function createNewRole() {
  if (actionLoading.value || loading.value) return;
  const name = createRoleName.value.trim();
  if (!name) {
    toast.warning("Введите название роли.");
    return;
  }
  actionLoading.value = true;
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
    showCreateRole.value = false;
    emit("loaded", {
      rolesCount: roles.value.length,
      permissionsCount: permissions.value.length,
    });
    toast.success(`Роль ${name} создана.`);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось создать роль.");
  } finally {
    actionLoading.value = false;
  }
}

async function addPermissionToSelectedRole() {
  if (!selectedRole.value || actionLoading.value) return;
  if (!permissionToAssignId.value) {
    toast.warning("Выберите permission.");
    return;
  }
  actionLoading.value = true;
  try {
    await assignPermissionToRole(
      selectedRole.value.id,
      permissionToAssignId.value,
    );
    permissionToAssignId.value = "";
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    toast.success("Permission добавлен.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function removePermissionFromSelectedRole(name: string) {
  if (!selectedRole.value || actionLoading.value) return;
  const id = getPermissionIdByName(name);
  if (!id) {
    toast.error(`Permission ${name} не найден.`);
    return;
  }
  actionLoading.value = true;
  try {
    await removePermissionFromRole(selectedRole.value.id, id);
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    toast.success("Permission убран.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function addParentRoleToSelectedRole() {
  if (!selectedRole.value || actionLoading.value) return;
  if (!parentRoleToAssignId.value) {
    toast.warning("Выберите parent role.");
    return;
  }
  actionLoading.value = true;
  try {
    await assignParentRoleToRole(
      selectedRole.value.id,
      parentRoleToAssignId.value,
    );
    parentRoleToAssignId.value = "";
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    toast.success("Наследование добавлено.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

async function removeParentRoleFromSelectedRole(parentId: string) {
  if (!selectedRole.value || actionLoading.value) return;
  actionLoading.value = true;
  try {
    await removeParentRoleFromRole(selectedRole.value.id, parentId);
    await reloadRolesAndKeepSelection(selectedRole.value.id);
    toast.success("Наследование удалено.");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Ошибка.");
  } finally {
    actionLoading.value = false;
  }
}

// ── Expose for parent to call refresh ─────────────────────────────────────────

defineExpose({ loadData, rolesCount: computed(() => roles.value.length) });

onMounted(async () => {
  await loadData();
});
</script>
