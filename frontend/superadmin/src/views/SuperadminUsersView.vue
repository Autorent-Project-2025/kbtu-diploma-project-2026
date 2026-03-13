<template>
  <div class="max-w-7xl mx-auto px-6 py-8 space-y-6">
    <!-- Hero header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div
        class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-6"
      >
        <div class="space-y-3">
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-violet-600 dark:text-violet-400"
          >
            Superadmin
          </p>
          <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
            Control Center
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Управление пользователями, ролями, permissions и inheritance.
          </p>
        </div>
        <button
          @click="loadData"
          :disabled="loading"
          class="self-start px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-violet-500 transition-colors disabled:opacity-60"
        >
          {{ loading ? "Обновление..." : "Обновить данные" }}
        </button>
      </div>
    </header>

    <!-- Stats -->
    <section class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
      <article
        v-for="stat in statsCards"
        :key="stat.label"
        :class="[
          'rounded-3xl border bg-white dark:bg-gray-900 shadow-xl p-6 space-y-2',
          stat.borderClass,
        ]"
      >
        <p
          :class="[
            'text-xs font-bold uppercase tracking-[0.18em]',
            stat.labelClass,
          ]"
        >
          {{ stat.label }}
        </p>
        <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
          {{ stat.value }}
        </p>
      </article>
    </section>

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

    <!-- Section switcher -->
    <section
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-2 flex gap-2"
    >
      <button
        v-for="section in sections"
        :key="section.value"
        @click="activeSection = section.value"
        :class="[
          'flex-1 px-5 py-3 rounded-2xl text-sm font-bold transition-colors',
          activeSection === section.value
            ? 'bg-gray-900 text-white dark:bg-white dark:text-gray-900'
            : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white',
        ]"
      >
        {{ section.label }}
      </button>
    </section>

    <!-- ROLES section -->
    <section
      v-if="activeSection === 'roles'"
      class="grid xl:grid-cols-[320px,1fr] gap-6 items-start"
    >
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

    <!-- USERS section -->
    <section v-else class="grid xl:grid-cols-[320px,1fr] gap-6 items-start">
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

const sections = [
  { label: "Role Management", value: "roles" as const },
  { label: "User Management", value: "users" as const },
];

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

const selectedRole = computed(
  () => roles.value.find((r) => r.id === selectedRoleId.value) ?? null,
);
const activeUsersCount = computed(
  () => users.value.filter((u) => u.isActive).length,
);

const statsCards = computed(() => [
  {
    label: "Пользователи",
    value: users.value.length,
    borderClass: "border-violet-200/70 dark:border-violet-700/40",
    labelClass: "text-violet-600 dark:text-violet-400",
  },
  {
    label: "Активные",
    value: activeUsersCount.value,
    borderClass: "border-emerald-200/70 dark:border-emerald-700/40",
    labelClass: "text-emerald-600 dark:text-emerald-400",
  },
  {
    label: "Роли",
    value: roles.value.length,
    borderClass: "border-blue-200/70 dark:border-blue-700/40",
    labelClass: "text-blue-600 dark:text-blue-400",
  },
  {
    label: "Permissions",
    value: permissions.value.length,
    borderClass: "border-amber-200/70 dark:border-amber-700/40",
    labelClass: "text-amber-600 dark:text-amber-400",
  },
]);

const filteredUsers = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return users.value;
  return users.value.filter(
    (u) =>
      u.username.toLowerCase().includes(q) || u.email.toLowerCase().includes(q),
  );
});

const filteredRoles = computed(() => {
  const q = roleSearchQuery.value.trim().toLowerCase();
  if (!q) return roles.value;
  return roles.value.filter(
    (r) =>
      r.name.toLowerCase().includes(q) ||
      r.permissions.some((p) => p.toLowerCase().includes(q)),
  );
});

const availableRolesForAssignment = computed(() => {
  if (!selectedUser.value) return [];
  const assigned = new Set(
    selectedUser.value.roles.map((r) => r.toLowerCase()),
  );
  return roles.value.filter((r) => !assigned.has(r.name.toLowerCase()));
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
function resetCreateRoleForm() {
  createRoleName.value = "";
  createRolePermissionIds.value = [];
  createRoleParentRoleIds.value = [];
}

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

async function reloadUsersAndKeepSelection(preferredId = "") {
  const loaded = await getUsers();
  users.value = loaded;
  if (loaded.length === 0) {
    selectedUser.value = null;
    selectedUserId.value = "";
    syncEditableFields();
    return;
  }
  const has = preferredId ? loaded.some((u) => u.id === preferredId) : false;
  const target = has ? preferredId : (loaded[0]?.id ?? "");
  if (!target) {
    selectedUser.value = null;
    selectedUserId.value = "";
    syncEditableFields();
    return;
  }
  await selectUser(target);
}

async function loadData() {
  loading.value = true;
  errorMessage.value = "";
  successMessage.value = "";
  try {
    const [loadedPerms] = await Promise.all([getPermissions()]);
    permissions.value = loadedPerms;
    await reloadRolesAndKeepSelection(selectedRoleId.value);
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
  const user = await getUserById(userId);
  selectedUser.value = user;
  syncEditableFields();
}

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
    activeSection.value = "roles";
    successMessage.value = `Роль ${name} создана.`;
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось создать роль.";
  } finally {
    actionLoading.value = false;
  }
}

function getPermissionIdByName(name: string) {
  return (
    permissions.value.find(
      (p) => p.name.toLowerCase() === name.trim().toLowerCase(),
    )?.id ?? null
  );
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
    activeSection.value = "users";
    successMessage.value = `Пользователь ${created.username} создан.`;
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось создать пользователя.";
  } finally {
    actionLoading.value = false;
  }
}

function getRoleIdByName(name: string) {
  return (
    roles.value.find((r) => r.name.toLowerCase() === name.trim().toLowerCase())
      ?.id ?? null
  );
}

function getRolePermissionsPreview(name: string) {
  const role = roles.value.find(
    (r) => r.name.toLowerCase() === name.trim().toLowerCase(),
  );
  if (!role || role.permissions.length === 0)
    return "Permissions не настроены.";
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

onMounted(async () => {
  await loadData();
});
</script>
