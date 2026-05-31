<template>
  <div class="space-y-6">
    <!-- Error / success -->
    <div
      v-if="errorMessage"
      class="rounded-2xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 px-5 py-4 text-red-700 dark:text-red-300 font-medium"
    >
      {{ errorMessage }}
    </div>
    <div
      v-if="successMessage"
      class="rounded-2xl border border-emerald-300/70 dark:border-emerald-500/30 bg-emerald-50 dark:bg-emerald-900/20 px-5 py-4 text-emerald-700 dark:text-emerald-300 font-medium"
    >
      {{ successMessage }}
    </div>

    <section class="grid xl:grid-cols-[320px,1fr] gap-6 items-start">
      <!-- Roles list -->
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <div
          class="px-5 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between"
        >
          <h2 class="font-bold text-gray-900 dark:text-white">Роли</h2>
          <span
            class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 px-2.5 py-1 rounded-full"
            >{{ filteredRoles.length }}</span
          >
        </div>
        <div class="p-4">
          <input
            v-model="roleSearchQuery"
            type="text"
            placeholder="Название или permission"
            class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors placeholder-gray-400"
          />
        </div>
        <ul
          class="divide-y divide-gray-100 dark:divide-gray-800 max-h-[60vh] overflow-y-auto"
        >
          <li v-for="role in filteredRoles" :key="role.id">
            <button
              @click="selectRole(role.id)"
              :class="[
                'w-full px-5 py-3.5 text-left transition-colors border-l-4',
                selectedRole?.id === role.id
                  ? 'bg-violet-50 dark:bg-violet-900/20 border-violet-500'
                  : 'hover:bg-gray-50 dark:hover:bg-gray-800/60 border-transparent',
              ]"
            >
              <p class="font-bold text-sm text-gray-900 dark:text-white">
                {{ role.name }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                Permissions: {{ role.permissions.length }}
              </p>
            </button>
          </li>
          <li
            v-if="filteredRoles.length === 0"
            class="px-5 py-4 text-sm text-gray-400 dark:text-gray-500"
          >
            Роли не найдены.
          </li>
        </ul>
      </div>

      <!-- Roles right panel -->
      <div class="space-y-6">
        <!-- Create role -->
        <div
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-5"
        >
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">
            Создать роль
          </h2>
          <form @submit.prevent="createNewRole" class="space-y-4">
            <div>
              <label
                class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                >Название роли</label
              >
              <input
                v-model="createRoleName"
                type="text"
                required
                class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
              />
            </div>
            <div class="grid sm:grid-cols-2 gap-4">
              <div>
                <label
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                  >Прямые permissions</label
                >
                <select
                  v-model="createRolePermissionIds"
                  multiple
                  size="6"
                  class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
                >
                  <option v-for="p in permissions" :key="p.id" :value="p.id">
                    {{ p.name }}
                  </option>
                </select>
              </div>
              <div>
                <label
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                  >Родительские роли</label
                >
                <select
                  v-model="createRoleParentRoleIds"
                  multiple
                  size="6"
                  class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
                >
                  <option v-for="role in roles" :key="role.id" :value="role.id">
                    {{ role.name }}
                  </option>
                </select>
              </div>
            </div>
            <button
              type="submit"
              :disabled="actionLoading || loading"
              class="px-5 py-2.5 rounded-2xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm shadow-lg shadow-violet-500/20 transition-colors"
            >
              Создать роль
            </button>
          </form>
        </div>

        <!-- Selected role detail -->
        <div
          v-if="selectedRole"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
        >
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">
            Роль: {{ selectedRole.name }}
          </h2>

          <div class="grid sm:grid-cols-2 gap-6">
            <!-- Direct permissions -->
            <div class="space-y-3">
              <h3
                class="text-sm font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
              >
                Прямые permissions
              </h3>
              <p
                v-if="selectedRole.directPermissions.length === 0"
                class="text-sm text-gray-400"
              >
                Не назначены.
              </p>
              <ul v-else class="space-y-2">
                <li
                  v-for="name in selectedRole.directPermissions"
                  :key="name"
                  class="flex items-center justify-between gap-3 rounded-xl border border-gray-200 dark:border-gray-800 px-3 py-2"
                >
                  <span
                    class="text-sm font-medium text-gray-700 dark:text-gray-300"
                    >{{ name }}</span
                  >
                  <button
                    @click="removePermissionFromSelectedRole(name)"
                    :disabled="actionLoading"
                    class="text-xs text-gray-400 hover:text-red-500 font-semibold transition-colors disabled:opacity-60"
                  >
                    Убрать
                  </button>
                </li>
              </ul>
              <div class="flex gap-2 items-end">
                <div class="flex-1">
                  <select
                    v-model="permissionToAssignId"
                    class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
                  >
                    <option value="">Выберите permission</option>
                    <option
                      v-for="p in availablePermissionsForSelectedRole"
                      :key="p.id"
                      :value="p.id"
                    >
                      {{ p.name }}
                    </option>
                  </select>
                </div>
                <button
                  @click="addPermissionToSelectedRole"
                  :disabled="actionLoading"
                  class="px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm transition-colors"
                >
                  +
                </button>
              </div>
            </div>

            <!-- Parent roles -->
            <div class="space-y-3">
              <h3
                class="text-sm font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
              >
                Наследуемые роли
              </h3>
              <p
                v-if="selectedRole.parentRoles.length === 0"
                class="text-sm text-gray-400"
              >
                Наследование не настроено.
              </p>
              <ul v-else class="space-y-2">
                <li
                  v-for="parent in selectedRole.parentRoles"
                  :key="parent.id"
                  class="flex items-center justify-between gap-3 rounded-xl border border-gray-200 dark:border-gray-800 px-3 py-2"
                >
                  <span
                    class="text-sm font-medium text-gray-700 dark:text-gray-300"
                    >{{ parent.name }}</span
                  >
                  <button
                    @click="removeParentRoleFromSelectedRole(parent.id)"
                    :disabled="actionLoading"
                    class="text-xs text-gray-400 hover:text-red-500 font-semibold transition-colors disabled:opacity-60"
                  >
                    Убрать
                  </button>
                </li>
              </ul>
              <div class="flex gap-2 items-end">
                <div class="flex-1">
                  <select
                    v-model="parentRoleToAssignId"
                    class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
                  >
                    <option value="">Выберите роль</option>
                    <option
                      v-for="role in availableParentRolesForSelectedRole"
                      :key="role.id"
                      :value="role.id"
                    >
                      {{ role.name }}
                    </option>
                  </select>
                </div>
                <button
                  @click="addParentRoleToSelectedRole"
                  :disabled="actionLoading"
                  class="px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm transition-colors"
                >
                  +
                </button>
              </div>
            </div>
          </div>

          <!-- Effective permissions -->
          <div
            class="space-y-3 pt-4 border-t border-gray-100 dark:border-gray-800"
          >
            <h3
              class="text-sm font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
            >
              Итоговые permissions
            </h3>
            <div class="flex flex-wrap gap-2">
              <span
                v-for="name in selectedRole.permissions"
                :key="name"
                class="inline-flex px-3 py-1 rounded-full text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300"
                >{{ name }}</span
              >
            </div>
          </div>
        </div>
        <div
          v-else
          class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-400 dark:text-gray-500 text-sm"
        >
          Выберите роль слева для редактирования.
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import {
  useRoleManager,
  type RoleManagerLoadedPayload,
} from "../../composables/useRoleManager";

const emit = defineEmits<{
  loaded: [payload: RoleManagerLoadedPayload];
}>();

const {
  loading,
  actionLoading,
  errorMessage,
  successMessage,
  roles,
  permissions,
  roleSearchQuery,
  createRoleName,
  createRolePermissionIds,
  createRoleParentRoleIds,
  permissionToAssignId,
  parentRoleToAssignId,
  selectedRole,
  filteredRoles,
  availablePermissionsForSelectedRole,
  availableParentRolesForSelectedRole,
  selectRole,
  loadData,
  createNewRole,
  addPermissionToSelectedRole,
  removePermissionFromSelectedRole,
  addParentRoleToSelectedRole,
  removeParentRoleFromSelectedRole,
} = useRoleManager((p) => emit("loaded", p));

defineExpose({ loadData });

onMounted(loadData);
</script>
