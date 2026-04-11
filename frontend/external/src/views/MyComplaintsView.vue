<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="mb-12 space-y-5 animate-slide-up">
        <h1
          class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
        >
          Мои обращения
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Отслеживайте статус ваших жалоб и обращений
        </p>

        <!-- Filters -->
        <div
          class="flex flex-wrap gap-3 rounded-3xl border border-white/10 bg-white/60 p-3 shadow-xl backdrop-blur-md dark:bg-gray-900/70 dark:border-gray-800"
        >
          <button
            v-for="filter in filters"
            :key="filter.value"
            @click="currentFilter = filter.value"
            :class="[
              'px-5 py-2.5 rounded-xl font-extrabold text-sm transition-all',
              currentFilter === filter.value
                ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/50'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 border border-gray-200 dark:border-gray-700',
            ]"
          >
            {{ filter.label }}
            <span
              v-if="filter.count > 0"
              :class="[
                'ml-2 px-2 py-0.5 rounded-full text-xs font-extrabold',
                currentFilter === filter.value
                  ? 'bg-white/20'
                  : 'bg-gray-100 dark:bg-gray-700',
              ]"
            >
              {{ filter.count }}
            </span>
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-4">
          <svg
            class="w-10 h-10 animate-spin text-primary-600"
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
          <p class="text-gray-500 dark:text-gray-400">Загрузка обращений...</p>
        </div>
      </div>

      <!-- Complaints List -->
      <div v-else-if="filteredComplaints.length > 0" class="space-y-4">
        <router-link
          v-for="complaint in filteredComplaints"
          :key="complaint.id"
          :to="`/complaints/${complaint.id}`"
          class="group block relative overflow-hidden rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-md hover:shadow-xl transition-all duration-300 hover:border-primary-300 dark:hover:border-primary-700"
        >
          <div class="p-5">
            <div class="flex flex-col sm:flex-row sm:items-start gap-4">
              <!-- Left: Info -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-3 mb-2">
                  <h3
                    class="text-lg font-bold text-gray-900 dark:text-white leading-tight truncate group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors"
                  >
                    {{ complaint.subject }}
                  </h3>
                  <span
                    :class="getStatusClass(complaint.status)"
                    class="inline-flex items-center gap-1.5 px-3 py-1 rounded-xl text-xs font-bold uppercase tracking-wide flex-shrink-0"
                  >
                    <span
                      :class="getStatusDotClass(complaint.status)"
                      class="w-1.5 h-1.5 rounded-full"
                    ></span>
                    {{ statusLabels[complaint.status] || 'Неизвестно' }}
                  </span>
                </div>

                <div class="flex flex-wrap items-center gap-3 text-sm">
                  <span
                    class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300"
                  >
                    {{ categoryLabels[complaint.category] || 'Другое' }}
                  </span>
                  <span
                    v-if="complaint.snapshotData"
                    class="text-gray-600 dark:text-gray-400 font-medium"
                  >
                    {{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}
                  </span>
                  <span class="text-gray-400 dark:text-gray-500 text-xs">
                    {{ formatDate(complaint.createdAt) }}
                  </span>
                </div>
              </div>

              <!-- Arrow indicator -->
              <div class="flex-shrink-0 hidden sm:flex items-center">
                <svg
                  class="w-5 h-5 text-gray-300 group-hover:text-primary-500 transition-colors"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </div>
            </div>
          </div>
        </router-link>
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6 max-w-md mx-auto">
          <div
            class="w-24 h-24 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
          >
            <svg
              class="w-12 h-12 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          <div class="space-y-2">
            <h3 class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ getEmptyStateTitle() }}
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              {{ getEmptyStateDescription() }}
            </p>
          </div>
          <router-link
            to="/bookings"
            class="btn-premium inline-flex items-center gap-2"
          >
            <span>К бронированиям</span>
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7l5 5m0 0l-5 5m5-5H6" />
            </svg>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { getMyComplaints } from "../api/complaints";
import type { Complaint } from "../types/Complaint";
import { useToast } from "../composables/useToast";

const complaints = ref<Complaint[]>([]);
const loading = ref(true);
const currentFilter = ref<"all" | "active" | "closed">("all");

const { error } = useToast();

const statusLabels: Record<number, string> = {
  1: "Новая",
  2: "На рассмотрении",
  3: "Ожидает вашего ответа",
  4: "Решена",
  5: "Отклонена",
};

const categoryLabels: Record<number, string> = {
  1: "Состояние авто",
  2: "Задержка передачи",
  3: "Качество сервиса",
  4: "Безопасность",
  5: "Поведение клиента",
  99: "Другое",
};

const filters = computed(() => {
  const all = complaints.value.length;
  const active = complaints.value.filter(
    (c) => c.status === 1 || c.status === 2 || c.status === 3,
  ).length;
  const closed = complaints.value.filter(
    (c) => c.status === 4 || c.status === 5,
  ).length;

  return [
    { label: "Все", value: "all" as const, count: all },
    { label: "Активные", value: "active" as const, count: active },
    { label: "Закрытые", value: "closed" as const, count: closed },
  ];
});

const filteredComplaints = computed(() => {
  if (currentFilter.value === "all") return complaints.value;
  if (currentFilter.value === "active")
    return complaints.value.filter((c) => c.status === 1 || c.status === 2 || c.status === 3);
  return complaints.value.filter((c) => c.status === 4 || c.status === 5);
});

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function getStatusClass(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300",
    2: "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300",
    3: "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300",
    4: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300",
    5: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300",
  };
  return map[status] ?? "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
}

function getStatusDotClass(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-600 dark:bg-blue-400",
    2: "bg-violet-600 dark:bg-violet-400",
    3: "bg-amber-600 dark:bg-amber-400",
    4: "bg-green-600 dark:bg-green-400",
    5: "bg-red-600 dark:bg-red-400",
  };
  return map[status] ?? "bg-gray-600 dark:bg-gray-400";
}

function getEmptyStateTitle(): string {
  if (currentFilter.value === "all") return "Нет обращений";
  if (currentFilter.value === "active") return "Нет активных обращений";
  return "Нет закрытых обращений";
}

function getEmptyStateDescription(): string {
  if (currentFilter.value === "all")
    return "У вас пока нет обращений. Вы можете подать жалобу из раздела бронирований.";
  return "Попробуйте выбрать другой фильтр";
}

onMounted(async () => {
  try {
    complaints.value = await getMyComplaints();
  } catch (e) {
    console.error("Failed to load complaints:", e);
    error("Не удалось загрузить обращения");
    complaints.value = [];
  } finally {
    loading.value = false;
  }
});
</script>
