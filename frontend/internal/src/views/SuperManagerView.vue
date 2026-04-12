<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(139,92,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-6">
        <div class="space-y-3">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-violet-600 dark:text-violet-400">
            Super Manager Panel
          </p>
          <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
            Обзор системы
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Все заявки платформы и список менеджеров.
          </p>
        </div>

        <div class="flex flex-wrap gap-3 items-center">
          <div class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden">
            <div
              v-for="(stat, i) in statsStrip"
              :key="stat.label"
              :class="['px-5 py-3 text-center', i > 0 ? 'border-l border-gray-200 dark:border-gray-800' : '']"
            >
              <p class="text-2xl font-extrabold" :class="stat.color">{{ stat.value }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">
                {{ stat.label }}
              </p>
            </div>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-violet-500 transition-colors disabled:opacity-60"
          >
            Обновить
          </button>
        </div>
      </div>
    </header>

    <!-- Tabs -->
    <div class="flex gap-1 rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-1.5 w-fit shadow">
      <button
        v-for="tab in tabs"
        :key="tab.id"
        @click="activeTab = tab.id"
        :class="[
          'px-5 py-2 rounded-xl text-sm font-semibold transition-colors',
          activeTab === tab.id
            ? 'bg-violet-600 text-white shadow'
            : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100',
        ]"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- TICKETS TAB -->
    <template v-else-if="activeTab === 'tickets'">
      <!-- Status filter -->
      <div class="flex flex-wrap gap-2">
        <button
          v-for="f in statusFilters"
          :key="f.value"
          @click="statusFilter = f.value"
          :class="[
            'px-4 py-1.5 rounded-full text-sm font-semibold border transition-colors',
            statusFilter === f.value
              ? 'bg-violet-600 border-violet-600 text-white'
              : 'border-gray-300 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:border-violet-400',
          ]"
        >
          {{ f.label }}
          <span class="ml-1.5 opacity-70">{{ ticketCountByStatus(f.value) }}</span>
        </button>
      </div>

      <div v-if="filteredTickets.length === 0" class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium">
        Нет заявок с выбранным статусом.
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
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Менеджер</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="ticket in filteredTickets"
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
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs font-mono">
                <template v-if="ticket.reviewedByManagerId">
                  <router-link
                    :to="`/super/managers/${ticket.reviewedByManagerId}`"
                    class="text-violet-600 dark:text-violet-400 hover:underline"
                  >
                    {{ ticket.reviewedByManagerId.slice(0, 8) }}…
                  </router-link>
                </template>
                <span v-else class="text-gray-400 dark:text-gray-600">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- MANAGERS TAB -->
    <template v-else-if="activeTab === 'managers'">
      <div v-if="managers.length === 0" class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium">
        Менеджеры не найдены.
      </div>

      <div v-else class="grid sm:grid-cols-2 xl:grid-cols-3 gap-4">
        <router-link
          v-for="manager in managers"
          :key="manager.id"
          :to="`/super/managers/${manager.id}`"
          class="group rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-5 shadow hover:border-violet-400 dark:hover:border-violet-500 hover:shadow-lg transition-all space-y-3"
        >
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-full bg-violet-100 dark:bg-violet-500/20 flex items-center justify-center text-violet-700 dark:text-violet-300 font-bold text-lg shrink-0">
              {{ manager.username.charAt(0).toUpperCase() }}
            </div>
            <div class="min-w-0">
              <p class="font-semibold text-gray-900 dark:text-white truncate">{{ manager.username }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 truncate">{{ manager.email }}</p>
            </div>
            <span
              :class="[
                'ml-auto shrink-0 h-2 w-2 rounded-full',
                manager.isActive ? 'bg-emerald-500' : 'bg-gray-400'
              ]"
              :title="manager.isActive ? 'Активен' : 'Неактивен'"
            />
          </div>
          <div class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
            <span>Тикетов: {{ reviewedCount(manager.id) }}</span>
            <span class="text-violet-600 dark:text-violet-400 font-semibold group-hover:underline">Подробнее →</span>
          </div>
        </router-link>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { getAllTickets } from "../api/tickets";
import { getManagers, type UserDto } from "../api/users";
import type { Ticket } from "../types/Ticket";

const tickets = ref<Ticket[]>([]);
const managers = ref<UserDto[]>([]);
const loading = ref(false);
const activeTab = ref<"tickets" | "managers">("tickets");
const statusFilter = ref<number | "all">("all");

const tabs = [
  { id: "tickets" as const, label: "Все заявки" },
  { id: "managers" as const, label: "Менеджеры" },
];

const statusFilters = [
  { value: "all" as const, label: "Все" },
  { value: 0, label: "На рассмотрении" },
  { value: 1, label: "Одобрено" },
  { value: 2, label: "Отклонено" },
  { value: 3, label: "Штраф" },
];

const filteredTickets = computed(() => {
  if (statusFilter.value === "all") return tickets.value;
  return tickets.value.filter((t) => t.status === statusFilter.value);
});

function ticketCountByStatus(value: number | "all"): number {
  if (value === "all") return tickets.value.length;
  return tickets.value.filter((t) => t.status === value).length;
}

function reviewedCount(managerId: string): number {
  return tickets.value.filter((t) => t.reviewedByManagerId === managerId).length;
}

const statsStrip = computed(() => [
  { label: "Всего", value: tickets.value.length, color: "text-gray-900 dark:text-white" },
  { label: "Ожидают", value: tickets.value.filter((t) => t.status === 0).length, color: "text-amber-600 dark:text-amber-400" },
  { label: "Одобрено", value: tickets.value.filter((t) => t.status === 1).length, color: "text-emerald-600 dark:text-emerald-400" },
  { label: "Отклонено", value: tickets.value.filter((t) => t.status === 2).length, color: "text-red-600 dark:text-red-400" },
]);

function ticketTypeLabel(type: number): string {
  const map: Record<number, string> = {
    1: "Клиент",
    2: "Партнёр",
    3: "Авто партнёра",
    4: "Завершение поездки",
  };
  return map[type] ?? String(type);
}

function ticketTypeBadge(type: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300",
    2: "bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300",
    3: "bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-300",
    4: "bg-teal-100 text-teal-700 dark:bg-teal-500/20 dark:text-teal-300",
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

async function reload() {
  loading.value = true;
  try {
    const [t, m] = await Promise.all([getAllTickets(), getManagers()]);
    tickets.value = t;
    managers.value = m;
  } finally {
    loading.value = false;
  }
}

onMounted(reload);
</script>
