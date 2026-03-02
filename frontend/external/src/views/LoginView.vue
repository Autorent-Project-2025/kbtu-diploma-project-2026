<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="w-full max-w-md space-y-8">
      <!-- Header -->
      <div class="text-center space-y-2 animate-slide-up">
        <h2 class="text-4xl font-extrabold text-gray-900 dark:text-white">
          С возвращением!
        </h2>
        <p class="text-gray-600 dark:text-gray-400">Войдите чтобы продолжить</p>
      </div>

      <!-- Form Card -->
      <div
        class="glass p-8 rounded-3xl shadow-2xl border border-gray-200 dark:border-gray-800 space-y-6 animate-slide-up"
        style="animation-delay: 0.1s"
      >
        <form @submit.prevent="onLogin" class="space-y-6">
          <!-- Email -->
          <div class="space-y-2">
            <label
              for="email"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
            >
              Email
            </label>
            <div class="relative">
              <div
                class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"
              >
                <svg
                  class="w-5 h-5 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M16 12a4 4 0 10-8 0 4 4 0 008 0zm0 0v1.5a2.5 2.5 0 005 0V12a9 9 0 10-9 9m4.5-1.206a8.959 8.959 0 01-4.5 1.207"
                  />
                </svg>
              </div>
              <input
                id="email"
                v-model="email"
                type="email"
                required
                placeholder="example@mail.com"
                class="w-full pl-12 pr-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
              />
            </div>
          </div>

          <!-- Password -->
          <div class="space-y-2">
            <label
              for="password"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
            >
              Пароль
            </label>
            <div class="relative">
              <div
                class="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none"
              >
                <svg
                  class="w-5 h-5 text-gray-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                  />
                </svg>
              </div>
              <input
                id="password"
                v-model="password"
                type="password"
                required
                placeholder="••••••••"
                class="w-full pl-12 pr-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
              />
            </div>
          </div>

          <!-- Submit Button -->
          <button
            type="submit"
            :disabled="loading"
            class="w-full btn-premium flex items-center justify-center gap-2"
          >
            <span v-if="!loading">Войти</span>
            <span v-else>Вход...</span>
            <svg
              v-if="!loading"
              class="w-5 h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 7l5 5m0 0l-5 5m5-5H6"
              />
            </svg>
            <svg
              v-else
              class="w-5 h-5 animate-spin"
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
              ></circle>
              <path
                class="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              ></path>
            </svg>
          </button>
        </form>

        <!-- Divider -->
        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div
              class="w-full border-t border-gray-200 dark:border-gray-700"
            ></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span
              class="px-4 bg-white dark:bg-gray-900 text-gray-500 dark:text-gray-400"
            >
              Нет аккаунта?
            </span>
          </div>
        </div>

        <!-- Register Link -->
        <div class="text-center">
          <router-link
            to="/apply"
            class="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 font-semibold transition-colors inline-flex items-center gap-1 group"
          >
            <span>Подать заявку</span>
            <svg
              class="w-4 h-4 transform group-hover:translate-x-1 transition-transform"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 7l5 5m0 0l-5 5m5-5H6"
              />
            </svg>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { auth } from "../store/auth";
import { useToast } from "../composables/useToast";

const router = useRouter();
const email = ref("");
const password = ref("");
const loading = ref(false);
const { success, error } = useToast();

async function onLogin() {
  if (loading.value) return;

  loading.value = true;
  try {
    await auth.login(email.value, password.value);
    success("Добро пожаловать!");
    router.push("/cars");
  } catch (err) {
    error("Ошибка входа! Проверьте email и пароль.");
  } finally {
    loading.value = false;
  }
}
</script>
