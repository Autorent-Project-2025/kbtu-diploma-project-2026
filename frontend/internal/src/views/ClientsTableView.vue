<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="space-y-2">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">Клиенты</h1>
          <p class="text-gray-600 dark:text-gray-400">
            Список зарегистрированных клиентов платформы.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div class="px-5 py-3 rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow text-center">
            <p class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ clients.length }}</p>
            <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">Всего</p>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-blue-500 transition-colors disabled:opacity-60"
          >
            Обновить
          </button>
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Empty -->
    <div
      v-else-if="clients.length === 0"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Клиенты не найдены.
    </div>

    <!-- Table -->
    <div
      v-else
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
    >
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-gray-200 dark:border-gray-800">
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Имя</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Телефон</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Дата рождения</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Документы</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
            <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Регистрация</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="client in clients"
            :key="client.id"
            @click="router.push(`/clients/${client.id}`)"
            class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
          >
            <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
              {{ client.id }}
            </td>
            <td class="px-5 py-3 text-gray-900 dark:text-white">
              <div class="flex items-center gap-3">
                <div
                  class="h-8 w-8 rounded-full bg-blue-100 dark:bg-blue-500/20 flex items-center justify-center text-blue-700 dark:text-blue-300 font-bold text-xs shrink-0"
                >
                  {{ (client.firstName?.charAt(0) ?? "").toUpperCase() }}{{ (client.lastName?.charAt(0) ?? "").toUpperCase() }}
                </div>
                <span class="font-medium">{{ client.firstName }} {{ client.lastName }}</span>
              </div>
            </td>
            <td class="px-5 py-3 text-gray-600 dark:text-gray-400">
              {{ client.phoneNumber }}
            </td>
            <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
              {{ formatDate(client.birthDate) }}
            </td>
            <td class="px-5 py-3">
              <div class="flex gap-1.5">
                <span
                  v-if="client.identityDocumentFileName"
                  class="px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300 text-xs font-semibold"
                >
                  Паспорт
                </span>
                <span
                  v-if="client.driverLicenseFileName"
                  class="px-2 py-0.5 rounded-full bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300 text-xs font-semibold"
                >
                  Права
                </span>
              </div>
            </td>
            <td class="px-5 py-3">
              <span
                :class="[
                  'px-2.5 py-0.5 rounded-full text-xs font-semibold',
                  client.bookingActionsBlocked
                    ? 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300'
                    : 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300',
                ]"
              >
                {{ client.bookingActionsBlocked ? "Заблокирован" : "Активен" }}
              </span>
            </td>
            <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
              {{ formatDateTime(client.createdOn) }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getClients, type ClientDto } from "../api/clients";

const router = useRouter();
const clients = ref<ClientDto[]>([]);
const loading = ref(false);

function formatDate(dateStr: string): string {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleDateString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });
}

function formatDateTime(dateStr: string): string {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

async function reload() {
  loading.value = true;
  try {
    clients.value = await getClients();
  } finally {
    loading.value = false;
  }
}

onMounted(reload);
</script>
