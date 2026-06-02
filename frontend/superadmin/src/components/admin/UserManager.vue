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
      <!-- Users list -->
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <div
          class="px-5 py-4 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between"
        >
          <h2 class="font-bold text-gray-900 dark:text-white">Пользователи</h2>
          <span
            class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 px-2.5 py-1 rounded-full"
            >{{ filteredUsers.length }}</span
          >
        </div>
        <div class="p-4">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Username или email"
            class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors placeholder-gray-400"
          />
        </div>
        <ul
          class="divide-y divide-gray-100 dark:divide-gray-800 max-h-[60vh] overflow-y-auto"
        >
          <li v-for="user in filteredUsers" :key="user.id">
            <button
              @click="selectUser(user.id)"
              :class="[
                'w-full px-5 py-3.5 text-left transition-colors border-l-4',
                selectedUser?.id === user.id
                  ? 'bg-violet-50 dark:bg-violet-900/20 border-violet-500'
                  : 'hover:bg-gray-50 dark:hover:bg-gray-800/60 border-transparent',
              ]"
            >
              <div class="flex items-center justify-between gap-2">
                <p
                  class="font-bold text-sm text-gray-900 dark:text-white truncate"
                >
                  {{ user.username }}
                </p>
                <span
                  :class="
                    user.isActive
                      ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300'
                      : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300'
                  "
                  class="inline-flex px-2 py-0.5 rounded-full text-xs font-bold flex-shrink-0"
                >
                  {{ user.isActive ? "Active" : "Inactive" }}
                </span>
              </div>
              <p
                class="text-xs text-gray-500 dark:text-gray-400 mt-0.5 truncate"
              >
                {{ user.email }}
              </p>
            </button>
          </li>
          <li
            v-if="filteredUsers.length === 0"
            class="px-5 py-4 text-sm text-gray-400 dark:text-gray-500"
          >
            Пользователи не найдены.
          </li>
        </ul>
      </div>

      <!-- Users right panel -->
      <div class="space-y-6">
        <!-- Create user -->
        <div
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-5"
        >
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">
            Создать пользователя
          </h2>
          <form @submit.prevent="createNewUser" class="space-y-4">
            <div class="grid sm:grid-cols-3 gap-4">
              <div>
                <label
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                  >Username</label
                >
                <input
                  v-model="createUsername"
                  type="text"
                  required
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
                />
              </div>
              <div>
                <label
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                  >Email</label
                >
                <input
                  v-model="createEmail"
                  type="email"
                  required
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
                />
              </div>
              <div>
                <label
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                  >Password</label
                >
                <input
                  v-model="createPassword"
                  type="password"
                  required
                  class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
                />
              </div>
            </div>
            <div>
              <label
                class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                >Роли (опционально)</label
              >
              <select
                v-model="createRoleNames"
                multiple
                size="5"
                class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
              >
                <option v-for="role in roles" :key="role.id" :value="role.name">
                  {{ role.name }}
                </option>
              </select>
              <p class="text-xs text-gray-400 dark:text-gray-500 mt-1.5">
                Если роли не выбраны, назначается роль по умолчанию.
              </p>
            </div>
            <button
              type="submit"
              :disabled="actionLoading || loading"
              class="px-5 py-2.5 rounded-2xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm shadow-lg shadow-violet-500/20 transition-colors"
            >
              Создать пользователя
            </button>
          </form>
        </div>

        <!-- Selected user detail -->
        <div
          v-if="selectedUser"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
        >
          <div>
            <h2 class="text-lg font-bold text-gray-900 dark:text-white">
              {{ selectedUser.username }}
            </h2>
            <p class="text-xs text-gray-400 dark:text-gray-500 font-mono mt-1">
              {{ selectedUser.id }}
            </p>
          </div>

          <div class="grid sm:grid-cols-2 gap-4">
            <div>
              <label
                class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                >Username</label
              >
              <input
                v-model="editUsername"
                type="text"
                class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
              />
            </div>
            <div>
              <label
                class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
                >Email</label
              >
              <input
                v-model="editEmail"
                type="email"
                class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
              />
            </div>
          </div>

          <div class="flex flex-wrap gap-3">
            <button
              @click="saveUser"
              :disabled="actionLoading"
              class="px-5 py-2.5 rounded-2xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm transition-colors"
            >
              Сохранить
            </button>
            <button
              @click="toggleActive"
              :disabled="actionLoading"
              class="px-5 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 hover:border-violet-500 disabled:opacity-60 font-bold text-sm transition-colors"
            >
              {{ selectedUser.isActive ? "Деактивировать" : "Активировать" }}
            </button>
            <button
              @click="deleteSelectedUser"
              :disabled="actionLoading"
              class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 font-bold text-sm transition-colors"
            >
              Удалить
            </button>
          </div>

          <!-- User roles -->
          <div
            class="space-y-3 pt-4 border-t border-gray-100 dark:border-gray-800"
          >
            <h3
              class="text-sm font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
            >
              Роли пользователя
            </h3>
            <p
              v-if="selectedUser.roles.length === 0"
              class="text-sm text-gray-400"
            >
              Роли отсутствуют.
            </p>
            <ul v-else class="space-y-2">
              <li
                v-for="roleName in selectedUser.roles"
                :key="roleName"
                class="rounded-xl border border-gray-200 dark:border-gray-800 px-4 py-3"
              >
                <div class="flex items-start justify-between gap-3">
                  <div>
                    <p class="font-bold text-sm text-gray-900 dark:text-white">
                      {{ roleName }}
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">
                      {{ getRolePermissionsPreview(roleName) }}
                    </p>
                  </div>
                  <button
                    @click="removeRoleFromSelectedUser(roleName)"
                    :disabled="actionLoading"
                    class="text-xs text-gray-400 hover:text-red-500 font-semibold transition-colors disabled:opacity-60 flex-shrink-0"
                  >
                    Убрать
                  </button>
                </div>
              </li>
            </ul>
            <div class="flex gap-2 items-end">
              <div class="flex-1">
                <select
                  v-model="roleToAssignId"
                  class="w-full px-3 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-violet-500"
                >
                  <option value="">Добавить роль...</option>
                  <option
                    v-for="role in availableRolesForAssignment"
                    :key="role.id"
                    :value="role.id"
                  >
                    {{ role.name }}
                  </option>
                </select>
              </div>
              <button
                @click="assignRoleToSelectedUser"
                :disabled="actionLoading"
                class="px-4 py-2 rounded-xl bg-violet-600 hover:bg-violet-700 disabled:opacity-60 text-white font-bold text-sm transition-colors"
              >
                Назначить
              </button>
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
                v-for="permission in selectedUser.permissions"
                :key="permission"
                class="inline-flex px-3 py-1 rounded-full text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-700 dark:text-gray-300"
                >{{ permission }}</span
              >
            </div>
          </div>
        </div>
        <div
          v-else
          class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-400 dark:text-gray-500 text-sm"
        >
          Выберите пользователя слева для редактирования.
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import {
  useUserManager,
  type UserManagerLoadedPayload,
} from "../../composables/useUserManager";

const emit = defineEmits<{
  loaded: [payload: UserManagerLoadedPayload];
}>();

const {
  loading,
  actionLoading,
  errorMessage,
  successMessage,
  roles,
  selectedUser,
  searchQuery,
  roleToAssignId,
  editUsername,
  editEmail,
  createUsername,
  createEmail,
  createPassword,
  createRoleNames,
  filteredUsers,
  availableRolesForAssignment,
  getRolePermissionsPreview,
  selectUser,
  loadData,
  createNewUser,
  saveUser,
  toggleActive,
  deleteSelectedUser,
  assignRoleToSelectedUser,
  removeRoleFromSelectedUser,
} = useUserManager((p) => emit("loaded", p));

defineExpose({ loadData });

onMounted(loadData);
</script>
