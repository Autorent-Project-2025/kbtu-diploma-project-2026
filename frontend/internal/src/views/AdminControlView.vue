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
            @click="refreshActive"
            :disabled="isRefreshing"
            class="group p-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-200 hover:border-violet-500 hover:text-violet-600 dark:hover:text-violet-400 transition-all disabled:opacity-50"
            title="Обновить данные"
          >
            <svg
              :class="[
                'w-5 h-5 transition-transform duration-300',
                isRefreshing ? 'animate-spin' : 'group-hover:rotate-180',
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
          {{ section.value === 'roles' ? rolesCount : usersCount }}
        </span>
      </button>
    </div>

    <!-- Role manager -->
    <RoleManager
      v-show="activeSection === 'roles'"
      ref="roleManagerRef"
      @loaded="onRolesLoaded"
    />

    <!-- User manager -->
    <UserManager
      v-show="activeSection === 'users'"
      ref="userManagerRef"
      @loaded="onUsersLoaded"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import RoleManager from "../components/admin/RoleManager.vue";
import UserManager from "../components/admin/UserManager.vue";

const sections = [
  { label: "Роли", value: "roles" as const },
  { label: "Пользователи", value: "users" as const },
];

const activeSection = ref<"roles" | "users">("roles");
const isRefreshing = ref(false);

const roleManagerRef = ref<InstanceType<typeof RoleManager> | null>(null);
const userManagerRef = ref<InstanceType<typeof UserManager> | null>(null);

// Stats state — populated via events from child components
const rolesCount = ref(0);
const permissionsCount = ref(0);
const usersCount = ref(0);
const activeUsersCount = ref(0);

const statsStrip = computed(() => [
  { label: "Пользователи", value: usersCount.value },
  { label: "Активные", value: activeUsersCount.value },
  { label: "Роли", value: rolesCount.value },
  { label: "Permissions", value: permissionsCount.value },
]);

function onRolesLoaded(payload: { rolesCount: number; permissionsCount: number }) {
  rolesCount.value = payload.rolesCount;
  permissionsCount.value = payload.permissionsCount;
}

function onUsersLoaded(payload: { usersCount: number; activeUsersCount: number }) {
  usersCount.value = payload.usersCount;
  activeUsersCount.value = payload.activeUsersCount;
}

async function refreshActive() {
  isRefreshing.value = true;
  try {
    if (activeSection.value === "roles") {
      await roleManagerRef.value?.loadData();
    } else {
      await userManagerRef.value?.loadData();
    }
  } finally {
    isRefreshing.value = false;
  }
}
</script>
