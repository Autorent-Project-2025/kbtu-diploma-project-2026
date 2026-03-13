<template>
  <div
    class="min-h-[calc(100vh-57px)] flex items-center justify-center px-4 py-12 bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.15),_transparent_40%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.12),_transparent_40%)] dark:bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.18),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.18),_transparent_40%)]"
  >
    <div class="w-full max-w-md">
      <div
        class="rounded-[28px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-2xl p-10"
      >
        <div class="mb-8">
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-violet-600 dark:text-violet-400 mb-3"
          >
            Superadmin
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            Вход в панель
          </h1>
          <p class="mt-2 text-gray-500 dark:text-gray-400 text-sm">
            Доступ только для ролей с правом User.View
          </p>
        </div>

        <div
          v-if="errorMessage"
          class="mb-6 rounded-2xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 px-4 py-3 text-red-700 dark:text-red-300 text-sm font-medium"
        >
          {{ errorMessage }}
        </div>

        <form class="space-y-5" @submit.prevent="onSubmit">
          <div>
            <label
              class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
              for="email"
              >Email</label
            >
            <input
              id="email"
              v-model="email"
              type="email"
              required
              class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
              placeholder="admin@autorent.kz"
            />
          </div>

          <div>
            <label
              class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400 mb-2"
              for="password"
              >Пароль</label
            >
            <input
              id="password"
              v-model="password"
              type="password"
              required
              class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-violet-500 focus:ring-2 focus:ring-violet-500/20 transition-colors"
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full px-6 py-3.5 rounded-2xl bg-violet-600 hover:bg-violet-700 disabled:bg-gray-300 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-violet-500/20 transition-colors"
          >
            {{ loading ? "Входим..." : "Войти" }}
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { auth } from "../store/auth";

const router = useRouter();
const email = ref("");
const password = ref("");
const loading = ref(false);
const errorMessage = ref("");

async function onSubmit() {
  if (loading.value) return;
  loading.value = true;
  errorMessage.value = "";
  try {
    await auth.login(email.value, password.value);
    if (!auth.hasPermission("User.View")) {
      auth.logout();
      errorMessage.value = "Недостаточно прав для superadmin panel.";
      return;
    }
    router.push("/users");
  } catch {
    errorMessage.value = "Ошибка входа. Проверьте email и пароль.";
  } finally {
    loading.value = false;
  }
}
</script>
