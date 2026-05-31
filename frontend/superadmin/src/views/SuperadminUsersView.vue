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
          @click="refreshAll"
          :disabled="isRefreshing"
          class="self-start px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-violet-500 transition-colors disabled:opacity-60"
        >
          {{ isRefreshing ? "Обновление..." : "Обновить данные" }}
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

    <!-- Role management -->
    <RoleManager
      v-show="activeSection === 'roles'"
      ref="roleManagerRef"
      @loaded="onRolesLoaded"
    />

    <!-- User management -->
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
  { label: "Role Management", value: "roles" as const },
  { label: "User Management", value: "users" as const },
];

const activeSection = ref<"roles" | "users">("roles");
const isRefreshing = ref(false);

const roleManagerRef = ref<InstanceType<typeof RoleManager> | null>(null);
const userManagerRef = ref<InstanceType<typeof UserManager> | null>(null);

// Stats — populated via `loaded` events from the manager components.
const usersCount = ref(0);
const activeUsersCount = ref(0);
const rolesCount = ref(0);
const permissionsCount = ref(0);

const statsCards = computed(() => [
  {
    label: "Пользователи",
    value: usersCount.value,
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
    value: rolesCount.value,
    borderClass: "border-blue-200/70 dark:border-blue-700/40",
    labelClass: "text-blue-600 dark:text-blue-400",
  },
  {
    label: "Permissions",
    value: permissionsCount.value,
    borderClass: "border-amber-200/70 dark:border-amber-700/40",
    labelClass: "text-amber-600 dark:text-amber-400",
  },
]);

function onRolesLoaded(payload: {
  rolesCount: number;
  permissionsCount: number;
}) {
  rolesCount.value = payload.rolesCount;
  permissionsCount.value = payload.permissionsCount;
}

function onUsersLoaded(payload: {
  usersCount: number;
  activeUsersCount: number;
}) {
  usersCount.value = payload.usersCount;
  activeUsersCount.value = payload.activeUsersCount;
}

async function refreshAll() {
  if (isRefreshing.value) return;
  isRefreshing.value = true;
  try {
    await Promise.all([
      roleManagerRef.value?.loadData(),
      userManagerRef.value?.loadData(),
    ]);
  } finally {
    isRefreshing.value = false;
  }
}
</script>
