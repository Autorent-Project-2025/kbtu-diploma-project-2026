<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Back -->
    <router-link
      to="/super"
      class="inline-flex items-center gap-2 text-sm font-semibold text-violet-600 dark:text-violet-400 hover:underline"
    >
      ← Обзор системы
    </router-link>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Not found -->
    <div
      v-else-if="!manager"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Менеджер не найден.
    </div>

    <template v-else>
      <!-- Manager card -->
      <div class="rounded-[28px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-2xl p-8">
        <div class="flex flex-col sm:flex-row sm:items-center gap-6">
          <div class="h-16 w-16 rounded-full bg-violet-100 dark:bg-violet-500/20 flex items-center justify-center text-violet-700 dark:text-violet-300 font-extrabold text-2xl shrink-0">
            {{ manager.username.charAt(0).toUpperCase() }}
          </div>
          <div class="space-y-1 flex-1 min-w-0">
            <div class="flex items-center gap-3 flex-wrap">
              <h1 class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ manager.username }}</h1>
              <span
                :class="[
                  'px-2.5 py-0.5 rounded-full text-xs font-semibold',
                  manager.isActive
                    ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300'
                    : 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400',
                ]"
              >
                {{ manager.isActive ? "Активен" : "Неактивен" }}
              </span>
            </div>
            <p class="text-gray-600 dark:text-gray-400">{{ manager.email }}</p>
            <div class="flex flex-wrap gap-1.5 mt-1">
              <span
                v-for="role in manager.roles"
                :key="role"
                class="px-2 py-0.5 rounded-full bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300 text-xs font-semibold"
              >
                {{ role }}
              </span>
            </div>
          </div>

          <!-- Stats -->
          <div class="flex rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden shrink-0 shadow">
            <div
              v-for="(stat, i) in managerStats"
              :key="stat.label"
              :class="['px-5 py-3 text-center', i > 0 ? 'border-l border-gray-200 dark:border-gray-800' : '']"
            >
              <p class="text-2xl font-extrabold" :class="stat.color">{{ stat.value }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">
                {{ stat.label }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- Tickets reviewed by this manager -->
      <div class="space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Рассмотренные заявки</h2>

        <div v-if="reviewedTickets.length === 0" class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-10 text-center text-gray-500 dark:text-gray-400 font-medium">
          Этот менеджер ещё не рассматривал заявки.
        </div>

        <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-200 dark:border-gray-800">
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Тип</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Email / Имя</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Создана</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Рассмотрена</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="ticket in reviewedTickets"
                :key="ticket.id"
                class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors"
              >
                <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400 max-w-[120px] truncate" :title="ticket.id">
                  {{ ticket.id.slice(0, 8) }}…
                </td>
                <td class="px-5 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', ticketTypeBadge(ticket.ticketType)]">
                    {{ ticketTypeLabel(ticket.ticketType) }}
                  </span>
                </td>
                <td class="px-5 py-3 text-gray-900 dark:text-white">
                  <p class="font-medium">{{ ticket.fullName || ticket.email }}</p>
                  <p v-if="ticket.fullName" class="text-xs text-gray-500 dark:text-gray-400">{{ ticket.email }}</p>
                </td>
                <td class="px-5 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', statusBadge(ticket.status)]">
                    {{ statusLabel(ticket.status) }}
                  </span>
                </td>
                <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ formatDate(ticket.createdAt) }}
                </td>
                <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ ticket.reviewedAt ? formatDate(ticket.reviewedAt) : "—" }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getAllTickets } from "../api/tickets";
import { getUsers, type UserDto } from "../api/users";
import type { Ticket } from "../types/Ticket";

const route = useRoute();
const managerId = route.params.id as string;

const manager = ref<UserDto | null>(null);
const tickets = ref<Ticket[]>([]);
const loading = ref(false);

const reviewedTickets = computed(() =>
  tickets.value.filter((t) => t.reviewedByManagerId === managerId)
);

const managerStats = computed(() => {
  const reviewed = reviewedTickets.value;
  return [
    { label: "Всего", value: reviewed.length, color: "text-gray-900 dark:text-white" },
    { label: "Одобрено", value: reviewed.filter((t) => t.status === 1).length, color: "text-emerald-600 dark:text-emerald-400" },
    { label: "Отклонено", value: reviewed.filter((t) => t.status === 2).length, color: "text-red-600 dark:text-red-400" },
  ];
});

function ticketTypeLabel(type: number): string {
  const map: Record<number, string> = {
    1: "Клиент",
    2: "Партнёр",
    3: "Авто партнёра",
    4: "Завершение поездки",
    5: "Отмена бронирования",
  };
  return map[type] ?? String(type);
}

function ticketTypeBadge(type: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300",
    2: "bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300",
    3: "bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-300",
    4: "bg-teal-100 text-teal-700 dark:bg-teal-500/20 dark:text-teal-300",
    5: "bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300",
  };
  return map[type] ?? "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300";
}

function statusLabel(status: number): string {
  return ["На рассмотрении", "Одобрено", "Отклонено", "Штраф"][status] ?? String(status);
}

function statusBadge(status: number): string {
  const map: Record<number, string> = {
    0: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300",
    1: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
    2: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300",
    3: "bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300",
  };
  return map[status] ?? "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400";
}

function formatDate(dateStr: string): string {
  return new Date(dateStr).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

onMounted(async () => {
  loading.value = true;
  try {
    const [users, allTickets] = await Promise.all([getUsers(), getAllTickets()]);
    manager.value = users.find((u) => u.id === managerId) ?? null;
    tickets.value = allTickets;
  } finally {
    loading.value = false;
  }
});
</script>
