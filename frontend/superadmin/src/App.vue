<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Topbar -->
    <header
      class="sticky top-0 z-20 border-b border-gray-200 dark:border-gray-800 bg-white/90 dark:bg-gray-900/90 backdrop-blur-md"
    >
      <div
        class="max-w-7xl mx-auto px-6 py-3.5 flex items-center justify-between gap-4"
      >
        <div class="flex items-center gap-3">
          <div
            class="w-9 h-9 rounded-xl bg-gradient-to-br from-violet-500 to-violet-700 flex items-center justify-center text-white text-xs font-extrabold"
          >
            AR
          </div>
          <div>
            <p class="font-bold text-gray-900 dark:text-white text-sm">
              AutoRent Superadmin
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400">
              Управление пользователями и access control
            </p>
          </div>
        </div>
        <button
          v-if="isAuthenticated"
          @click="logout"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-gray-400 transition-colors"
        >
          Выйти
        </button>
      </div>
    </header>

    <router-view />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { auth } from "./store/auth";

const router = useRouter();
const isAuthenticated = computed(() => Boolean(auth.token));

function logout() {
  auth.logout();
  router.push("/login");
}
</script>
