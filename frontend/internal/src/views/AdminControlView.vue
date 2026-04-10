<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div
        class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4"
      >
        <div class="space-y-2">
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-violet-600 dark:text-violet-400"
          >
            Administration
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            Control Center
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Управление пользователями, ролями и permissions.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div
            class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden"
          >
            <div
              v-for="(stat, i) in statsStrip"
              :key="stat.label"
              :class="[
                'px-4 py-2.5 text-center',
                i > 0 ? 'border-l border-gray-200 dark:border-gray-800' : '',
              ]"
            >
              <p
                class="text-xl font-extrabold text-gray-900 dark:text-white tabular-nums"
              >
                {{ stat.value }}
              </p>
              <p
                class="text-[10px] text-gray-500 dark:text-gray-400 font-bold uppercase tracking-wider"
              >
                {{ stat.label }}
              </p>
            </div>
          </div>
          <button
            @click="loadData"
            :disabled="loading"
            class="group p-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-200 hover:border-violet-500 hover:text-violet-600 dark:hover:text-violet-400 transition-all disabled:opacity-50"
            title="Обновить данные"
          >
            <svg
              :class="[
                'w-5 h-5 transition-transform duration-300',
                loading ? 'animate-spin' : 'group-hover:rotate-180',
              ]"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              stroke-width="2"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
              />
            </svg>
          </button>
        </div>
      </div>
    </header>

    <!-- Tab switcher -->
    <div
      class="flex gap-1 p-1 rounded-2xl bg-gray-100 dark:bg-gray-800/50 border border-gray-200/70 dark:border-gray-800 w-fit"
    >
      <button
        v-for="section in sections"
        :key="section.value"
        @click="activeSection = section.value"
        :class="[
          'flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-semibold transition-all duration-200',
          activeSection === section.value
            ? 'bg-white dark:bg-gray-900 text-gray-900 dark:text-white shadow-sm'
            : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-300',
        ]"
      >
        {{ section.label }}
        <span
          :class="[
            'text-xs px-2 py-0.5 rounded-full font-bold tabular-nums',
            activeSection === section.value
              ? 'bg-violet-100 dark:bg-violet-500/20 text-violet-600 dark:text-violet-400'
              : 'bg-gray-200 dark:bg-gray-700 text-gray-500 dark:text-gray-400',
          ]"
        >
          {{ section.value === "roles" ? roles.length : users.length }}
        </span>
      </button>
    </div>

    <!-- Loading skeleton -->
    <template v-if="loading && !roles.length && !users.length">
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

    <!-- ==================== ROLES SECTION ==================== -->
    <section
      v-else-if="activeSection === 'roles'"
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
                selectedRole?.id === role.id
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

    <!-- ==================== USERS SECTION ==================== -->
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

    <!-- Confirmation modal -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200 ease-out"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150 ease-in"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div
          v-if="confirmModal.show"
          class="fixed inset-0 z-50 flex items-center justify-center p-4"
        >
          <div
            class="fixed inset-0 bg-black/50 backdrop-blur-sm"
            @click="confirmModal.show = false"
          />
          <Transition
            enter-active-class="transition duration-200 ease-out"
            enter-from-class="opacity-0 scale-95"
            enter-to-class="opacity-100 scale-100"
            leave-active-class="transition duration-150 ease-in"
            leave-from-class="opacity-100 scale-100"
            leave-to-class="opacity-0 scale-95"
          >
            <div
              v-if="confirmModal.show"
              class="relative bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 shadow-2xl p-6 max-w-sm w-full space-y-4"
            >
              <div
                class="w-10 h-10 rounded-xl bg-red-100 dark:bg-red-500/20 flex items-center justify-center"
              >
                <svg
                  class="w-5 h-5 text-red-600 dark:text-red-400"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  stroke-width="2"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
                  />
                </svg>
              </div>
              <h3 class="text-lg font-bold text-gray-900 dark:text-white">
                {{ confirmModal.title }}
              </h3>
              <p class="text-sm text-gray-600 dark:text-gray-400">
                {{ confirmModal.message }}
              </p>
              <div class="flex gap-3 justify-end pt-2">
                <button
                  @click="confirmModal.show = false"
                  class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 font-semibold text-sm hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors"
                >
                  Отмена
                </button>
                <button
                  @click="
                    confirmModal.onConfirm();
                    confirmModal.show = false;
                  "
                  class="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-700 text-white font-semibold text-sm transition-colors"
                >
                  Удалить
                </button>
              </div>
            </div>
          </Transition>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useToast } from "../composables/useToast";
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
  type UserDto,
} from "../api/users";
import type { Permission } from "../types/Permission";
import type { Role } from "../types/Role";

const toast = useToast();

const sections = [
  { label: "Роли", value: "roles" as const },
  { label: "Пользователи", value: "users" as const },
];

const loading = ref(false);
const actionLoading = ref(false);
const activeSection = ref<"roles" | "users">("roles");

const users = ref<UserDto[]>([]);
const roles = ref<Role[]>([]);
const permissions = ref<Permission[]>([]);
const selectedUser = ref<UserDto | null>(null);
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

const showCreateRole = ref(false);
const showCreateUser = ref(false);
const tempPermissionId = ref("");
const tempParentRoleId = ref("");
const tempCreateRoleName = ref("");

const confirmModal = reactive({
  show: false,
  title: "",
  message: "",
  onConfirm: () => {},
});

const selectedRole = computed(
  () => roles.value.find((r) => r.id === selectedRoleId.value) ?? null,
);
const activeUsersCount = computed(
  () => users.value.filter((u) => u.isActive).length,
);

const statsStrip = computed(() => [
  { label: "Пользователи", value: users.value.length },
  { label: "Активные", value: activeUsersCount.value },
  { label: "Роли", value: roles.value.length },
  { label: "Permissions", value: permissions.value.length },
]);

const filteredUsers = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return users.value;
  return users.value.filter(
    (u) =>
      u.username.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q),
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

const availableCreatePermissions = computed(() => {
  const selected = new Set(createRolePermissionIds.value);
  return permissions.value.filter((p) => !selected.has(p.id));
});

const availableCreateUserRoles = computed(() => {
  const selected = new Set(
    createRoleNames.value.map((n) => n.toLowerCase()),
  );
  return roles.value.filter((r) => !selected.has(r.name.toLowerCase()));
});

function userInitials(username: string): string {
  return username.slice(0, 2).toUpperCase();
}

function permissionColor(name: string): string {
  const prefix = name.split(".")[0].toLowerCase();
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

function addCreateUserRole() {
  if (
    tempCreateRoleName.value &&
    !createRoleNames.value.includes(tempCreateRoleName.value)
  ) {
    createRoleNames.value.push(tempCreateRoleName.value);
  }
  tempCreateRoleName.value = "";
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
  try {
    const [loadedPerms] = await Promise.all([getPermissions()]);
    permissions.value = loadedPerms;
    await reloadRolesAndKeepSelection(selectedRoleId.value);
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
  } catch (e: any) {
    toast.error("Не удалось загрузить пользователя.");
  }
}

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
    toast.success(`Роль ${name} создана.`);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось создать роль.");
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

function getRoleIdByName(name: string) {
  return (
    roles.value.find(
      (r) => r.name.toLowerCase() === name.trim().toLowerCase(),
    )?.id ?? null
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
  confirmModal.onConfirm = () => deleteSelectedUser();
  confirmModal.show = true;
}

async function deleteSelectedUser() {
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

onMounted(async () => {
  await loadData();
});
</script>
