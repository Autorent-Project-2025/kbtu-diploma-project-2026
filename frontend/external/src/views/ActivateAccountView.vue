<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="w-full max-w-md space-y-8">
      <div class="text-center space-y-2">
        <h2 class="text-4xl font-extrabold text-gray-900 dark:text-white">Установка пароля</h2>
        <p class="text-gray-600 dark:text-gray-400">Завершите активацию аккаунта</p>
      </div>

      <div class="glass p-8 rounded-3xl shadow-2xl border border-gray-200 dark:border-gray-800 space-y-6">
        <div
          v-if="tokenMissing"
          class="rounded-xl border border-rose-300/80 bg-rose-50 p-4 text-rose-800 dark:border-rose-500/30 dark:bg-rose-900/20 dark:text-rose-300"
        >
          Токен активации не найден в ссылке
        </div>
        <div
          v-else-if="tokenChecking"
          class="rounded-xl border border-blue-300/80 bg-blue-50 p-4 text-blue-800 dark:border-blue-500/30 dark:bg-blue-900/20 dark:text-blue-300"
        >
          Проверяем ссылку активации...
        </div>
        <div
          v-else-if="tokenErrorMessage"
          class="rounded-xl border border-rose-300/80 bg-rose-50 p-4 text-rose-800 dark:border-rose-500/30 dark:bg-rose-900/20 dark:text-rose-300"
        >
          {{ tokenErrorMessage }}
        </div>

        <form v-else @submit.prevent="onActivate" class="space-y-4">
          <div class="space-y-2">
            <label for="password" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Пароль
            </label>
            <input
              id="password"
              v-model="password"
              type="password"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="confirmPassword" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Повторите пароль
            </label>
            <input
              id="confirmPassword"
              v-model="confirmPassword"
              type="password"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full btn-premium flex items-center justify-center"
          >
            <span v-if="!loading">Установить пароль</span>
            <span v-else>Сохранение...</span>
          </button>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { activateUser, getActivationTokenStatus } from "../api/auth";
import { useToast } from "../composables/useToast";

const route = useRoute();
const router = useRouter();
const password = ref("");
const confirmPassword = ref("");
const loading = ref(false);
const { success, error } = useToast();

const token = computed(() => String(route.query.token ?? ""));
const tokenMissing = computed(() => token.value.trim().length === 0);
const tokenChecking = ref(false);
const tokenErrorMessage = ref("");

async function onActivate() {
  if (loading.value || tokenMissing.value || tokenErrorMessage.value) return;

  if (password.value.trim().length < 6) {
    error("Минимальная длина пароля 6 символов");
    return;
  }

  if (password.value !== confirmPassword.value) {
    error("Пароли не совпадают");
    return;
  }

  loading.value = true;
  try {
    await activateUser(token.value, password.value);
    success("Пароль установлен. Теперь войдите в систему.");
    router.push("/login");
  } catch (e: any) {
    const errorMsg = e?.response?.data?.error || "Не удалось активировать аккаунт.";
    if (e?.response?.status === 401) {
      tokenErrorMessage.value = errorMsg;
    }
    error(errorMsg);
  } finally {
    loading.value = false;
  }
}

function mapTokenErrorMessage(reason?: string | null, expiresAtUtc?: string | null): string {
  if (reason === "used") {
    return "Эта ссылка уже была использована. Запросите новую ссылку у менеджера.";
  }

  if (reason === "expired") {
    if (expiresAtUtc) {
      const expiresDate = new Date(expiresAtUtc);
      if (!Number.isNaN(expiresDate.getTime())) {
        return `Срок действия ссылки истек (${expiresDate.toLocaleString()}). Ссылка действует 24 часа.`;
      }
    }

    return "Срок действия ссылки истек. Ссылка действует 24 часа.";
  }

  return "Ссылка активации недействительна. Запросите новую ссылку у менеджера.";
}

async function checkActivationToken() {
  if (tokenMissing.value) return;

  tokenChecking.value = true;
  tokenErrorMessage.value = "";
  try {
    const status = await getActivationTokenStatus(token.value);
    if (!status.isValid) {
      tokenErrorMessage.value = mapTokenErrorMessage(status.reason, status.expiresAtUtc);
    }
  } catch {
    tokenErrorMessage.value = "Не удалось проверить ссылку активации. Попробуйте обновить страницу.";
  } finally {
    tokenChecking.value = false;
  }
}

onMounted(async () => {
  await checkActivationToken();
});
</script>
