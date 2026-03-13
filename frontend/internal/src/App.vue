<template>
  <!-- Auth Labels + Sidebar + content -->
  <div
    v-if="showWorkspace"
    class="min-h-screen bg-gray-50 dark:bg-gray-950 flex"
  >
    <!-- Sidebar -->
    <aside
      class="w-64 flex-shrink-0 bg-gray-900 dark:bg-gray-950 border-r border-gray-800 flex flex-col"
    >
      <div class="p-6 border-b border-gray-800">
        <div class="flex items-center gap-3">
          <div
            class="w-9 h-9 rounded-xl bg-gradient-to-br from-emerald-500 to-emerald-700 flex items-center justify-center text-white text-xs font-extrabold"
          >
            AR
          </div>
          <div>
            <p class="text-white font-bold text-sm">AutoRent</p>
            <p class="text-gray-400 text-xs">Панель заявок</p>
          </div>
        </div>
      </div>

      <nav class="flex-1 p-4 space-y-1">
        <router-link
          to="/tickets"
          exact-active-class="bg-gray-800 text-white border-emerald-500"
          class="flex items-center gap-3 px-4 py-2.5 rounded-xl border border-transparent text-gray-400 hover:text-white hover:bg-gray-800 transition-colors font-semibold text-sm"
        >
          <span>📋</span> Заявки
        </router-link>
      </nav>

      <div class="p-4 border-t border-gray-800">
        <button
          @click="logout"
          class="w-full px-4 py-2.5 rounded-xl border border-gray-700 text-gray-300 hover:text-white hover:border-gray-600 font-semibold text-sm transition-colors"
        >
          Выйти
        </button>
      </div>
    </aside>

    <!-- Main -->
    <main class="flex-1 min-w-0">
      <router-view />
    </main>
  </div>

  <!-- Страница логина -->
  <div v-else class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { auth } from "./store/auth";

const route = useRoute();
const router = useRouter();
const isAuthenticated = computed(() => Boolean(auth.token));
const showWorkspace = computed(
  () => isAuthenticated.value && route.path !== "/login",
);

function logout() {
  auth.logout();
  router.push("/login");
}
</script>
