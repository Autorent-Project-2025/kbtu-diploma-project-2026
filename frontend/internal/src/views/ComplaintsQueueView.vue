<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Жалобы</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {{ loading ? "Загрузка..." : `${complaints.length} жалоб` }}
              <span
                v-if="newCount > 0"
                class="ml-2 inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-bold bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400"
              >
                {{ newCount }} новых
              </span>
            </p>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="p-2.5 rounded-xl border border-gray-200 dark:border-gray-700 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:border-gray-300 transition-colors"
          >
            <svg :class="['w-4 h-4', loading && 'animate-spin']" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </button>
        </div>

        <!-- Status filters -->
        <div class="mt-4 flex flex-wrap gap-2">
          <button
            v-for="f in statusFilters"
            :key="f.value"
            @click="setStatusFilter(f.value)"
            :class="[
              'px-3 py-2 rounded-xl text-xs font-semibold border transition-colors',
              statusFilter === f.value
                ? 'bg-emerald-600 text-white border-emerald-600'
                : 'bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400 border-gray-200 dark:border-gray-700 hover:border-gray-300',
            ]"
          >
            {{ f.label }}
          </button>
        </div>

        <!-- Category & Priority filters -->
        <div class="mt-3 flex flex-wrap gap-3">
          <select
            v-model="categoryFilter"
            @change="reload"
            class="px-3 py-2 rounded-xl text-xs font-semibold border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400"
          >
            <option :value="undefined">Все категории</option>
            <option v-for="(label, key) in categoryLabels" :key="key" :value="Number(key)">{{ label }}</option>
          </select>
          <select
            v-model="priorityFilter"
            @change="reload"
            class="px-3 py-2 rounded-xl text-xs font-semibold border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400"
          >
            <option :value="undefined">Все приоритеты</option>
            <option v-for="(label, key) in priorityLabels" :key="key" :value="Number(key)">{{ label }}</option>
          </select>
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 6" :key="i" class="h-16 bg-white dark:bg-gray-900 rounded-xl animate-pulse" />
      </div>

      <!-- Empty -->
      <div v-else-if="filteredComplaints.length === 0" class="text-center py-20">
        <svg class="w-12 h-12 mx-auto text-gray-300 dark:text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
        </svg>
        <p class="text-gray-500 dark:text-gray-400 font-medium">Жалобы не найдены</p>
        <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">Попробуйте изменить фильтры</p>
      </div>

      <!-- Table -->
      <template v-else>
        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-100 dark:border-gray-800">
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Тема</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Категория</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Статус</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Приоритет</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Заявитель</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Бронирование</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Создано</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Назначена</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              <tr
                v-for="c in filteredComplaints"
                :key="c.id"
                @click="$router.push(`/complaints/${c.id}`)"
                class="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
              >
                <td class="px-6 py-4">
                  <p class="text-sm font-semibold text-gray-900 dark:text-white truncate max-w-xs">{{ c.subject }}</p>
                </td>
                <td class="px-6 py-4">
                  <span class="text-sm text-gray-600 dark:text-gray-400">{{ categoryLabels[c.category] ?? "Другое" }}</span>
                </td>
                <td class="px-6 py-4">
                  <span :class="['px-2.5 py-1 text-xs font-semibold rounded-full', complaintStatusBadge(c.status)]">
                    {{ statusLabels[c.status] ?? "—" }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <span :class="['px-2.5 py-1 text-xs font-semibold rounded-full', priorityBadge(c.priority)]">
                    {{ priorityLabels[c.priority] ?? "—" }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <span class="text-sm text-gray-600 dark:text-gray-400">{{ reporterLabels[c.reporterActorType] ?? "—" }}</span>
                </td>
                <td class="px-6 py-4 text-sm font-medium text-gray-900 dark:text-white">#{{ c.bookingId }}</td>
                <td class="px-6 py-4 text-sm text-gray-400">{{ formatDateTime(c.createdAt) }}</td>
                <td class="px-6 py-4 text-sm text-gray-400">
                  {{ c.assignedToManagerId ? "Да" : "—" }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { getAllComplaints } from "../api/complaints";
import type { Complaint } from "../types/Complaint";
import { formatDateTime } from "../utils/formatters";
import { useToast } from "../composables/useToast";

const toast = useToast();
const complaints = ref<Complaint[]>([]);
const loading = ref(true);
const statusFilter = ref<number | "all">("all");
const categoryFilter = ref<number | undefined>(undefined);
const priorityFilter = ref<number | undefined>(undefined);

const categoryLabels: Record<number, string> = {
  1: "Состояние авто",
  2: "Задержка передачи",
  3: "Качество сервиса",
  4: "Безопасность",
  5: "Поведение клиента",
  99: "Другое",
};

const statusLabels: Record<number, string> = {
  1: "Новая",
  2: "На рассмотрении",
  3: "Ожидает ответа",
  4: "Решена",
  5: "Отклонена",
};

const priorityLabels: Record<number, string> = {
  1: "Обычный",
  2: "Высокий",
  3: "Срочный",
};

const reporterLabels: Record<number, string> = {
  1: "Клиент",
  2: "Партнёр",
};

const statusFilters = [
  { value: "all" as const, label: "Все" },
  { value: 1, label: "Новые" },
  { value: 2, label: "На рассмотрении" },
  { value: 3, label: "Ожидает ответа" },
  { value: 4, label: "Решённые" },
  { value: 5, label: "Отклонённые" },
];

function complaintStatusBadge(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    4: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    5: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-500";
}

function priorityBadge(priority: number): string {
  const map: Record<number, string> = {
    1: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[priority] ?? "bg-gray-100 text-gray-500";
}

const newCount = computed(() => complaints.value.filter((c) => c.status === 1).length);

const filteredComplaints = computed(() => {
  if (statusFilter.value === "all") return complaints.value;
  return complaints.value.filter((c) => c.status === statusFilter.value);
});

function setStatusFilter(value: number | "all") {
  statusFilter.value = value;
}

async function reload() {
  loading.value = true;
  try {
    complaints.value = await getAllComplaints({
      category: categoryFilter.value,
      priority: priorityFilter.value,
    });
  } catch (e: any) {
    toast.error("Ошибка загрузки: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

onMounted(reload);
</script>
