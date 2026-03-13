<template>
  <div
    class="min-h-screen flex items-center justify-center px-4 py-12 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.15),_transparent_40%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.12),_transparent_40%),linear-gradient(135deg,_#f9fafb,_#f3f4f6)] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.18),_transparent_40%),linear-gradient(135deg,_#111827,_#030712)]"
  >
    <div class="w-full max-w-5xl grid lg:grid-cols-[1.1fr,1fr] gap-8">
      <!-- LeftBar - Desciption -->
      <div
        class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.2),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-10 flex flex-col justify-between"
      >
        <div>
          <div class="flex items-center gap-3 mb-10">
            <div
              class="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500 to-emerald-700 flex items-center justify-center text-white font-extrabold text-sm"
            >
              AR
            </div>
            <span class="text-white font-bold">AutoRent</span>
          </div>
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-400 mb-4"
          >
            Internal Panel
          </p>
          <h1 class="text-4xl font-extrabold text-white mb-4">Панель заявок</h1>
          <p class="text-gray-400 text-lg leading-relaxed">
            Внутренний кабинет для проверки новых регистраций и связанных
            документов.
          </p>
        </div>

        <dl class="mt-10 space-y-5">
          <div
            v-for="item in features"
            :key="item.title"
            class="border-t border-white/10 pt-5"
          >
            <dt class="text-white font-semibold text-sm mb-1">
              {{ item.title }}
            </dt>
            <dd class="text-gray-400 text-sm">{{ item.desc }}</dd>
          </div>
        </dl>
      </div>

      <!-- RightBar - Panel -->
      <div
        class="rounded-[28px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-10 flex flex-col justify-center"
      >
        <div class="mb-8">
          <h2 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            Вход в кабинет
          </h2>
          <p class="mt-2 text-gray-500 dark:text-gray-400">
            Используйте рабочую учётную запись
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
              class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]"
              for="email"
              >Email</label
            >
            <input
              id="email"
              v-model="email"
              type="email"
              required
              autocomplete="username"
              class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
              placeholder="you@autorent.kz"
            />
          </div>

          <div>
            <label
              class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]"
              for="password"
              >Пароль</label
            >
            <input
              id="password"
              v-model="password"
              type="password"
              required
              autocomplete="current-password"
              class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
              placeholder="••••••••"
            />
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full px-6 py-3.5 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-300 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
          >
            {{ loading ? "Входим..." : "Войти" }}
          </button>
        </form>

        <p class="mt-6 text-xs text-gray-400 dark:text-gray-500 text-center">
          Если вход не проходит — обратитесь к администратору.
        </p>
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

const features = [
  {
    title: "Одна очередь",
    desc: "Новые регистрации клиентов, партнёров и авто собраны в одном рабочем списке.",
  },
  {
    title: "Документы под рукой",
    desc: "Паспорт, права и файлы по авто открываются прямо из карточки заявки.",
  },
  {
    title: "Быстрое решение",
    desc: "После проверки можно сразу подтвердить или вернуть с причиной отказа.",
  },
];

async function onSubmit() {
  if (loading.value) return;
  loading.value = true;
  errorMessage.value = "";

  try {
    await auth.login(email.value, password.value);

    if (!auth.hasPermission("Ticket.View")) {
      auth.logout();
      errorMessage.value = "Недостаточно прав для панели менеджера.";
      return;
    }

    router.push("/tickets");
  } catch {
    errorMessage.value = "Ошибка входа. Проверьте email и пароль.";
  } finally {
    loading.value = false;
  }
}
</script>
