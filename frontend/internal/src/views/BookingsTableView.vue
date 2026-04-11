<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Бронирования</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {{ loading ? "Загрузка..." : `${totalCount} бронирований` }}
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

        <!-- Filters -->
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
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 6" :key="i" class="h-16 bg-white dark:bg-gray-900 rounded-xl animate-pulse" />
      </div>

      <!-- Empty -->
      <div v-else-if="bookings.length === 0" class="text-center py-20">
        <svg class="w-12 h-12 mx-auto text-gray-300 dark:text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
        </svg>
        <p class="text-gray-500 dark:text-gray-400 font-medium">Бронирования не найдены</p>
        <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">Попробуйте изменить фильтры</p>
      </div>

      <!-- Table -->
      <template v-else>
        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-100 dark:border-gray-800">
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">ID</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Автомобиль</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Период</th>
                <th class="text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Стоимость</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Статус</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Подписка</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Создано</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              <tr
                v-for="b in bookings"
                :key="b.id"
                @click="$router.push(`/bookings/${b.id}`)"
                class="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
              >
                <td class="px-6 py-4 text-sm font-medium text-gray-900 dark:text-white">#{{ b.id }}</td>
                <td class="px-6 py-4">
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ b.carBrand }} {{ b.carModel }}</p>
                </td>
                <td class="px-6 py-4">
                  <p class="text-sm text-gray-600 dark:text-gray-400">{{ formatDateTime(b.startTime) }}</p>
                  <p class="text-xs text-gray-400">→ {{ formatDateTime(b.endTime) }}</p>
                </td>
                <td class="px-6 py-4 text-sm font-semibold text-gray-900 dark:text-white text-right">{{ formatPrice(b.totalPrice) }}</td>
                <td class="px-6 py-4">
                  <span :class="['px-2.5 py-1 text-xs font-semibold rounded-full', bookingStatusBadge(b.status)]">
                    {{ bookingStatusLabel(b.status) }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <span v-if="b.usedSubscription" class="px-2 py-0.5 text-xs font-medium rounded-full bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400">
                    Подписка
                  </span>
                  <span v-else class="text-xs text-gray-400">—</span>
                </td>
                <td class="px-6 py-4 text-sm text-gray-400">{{ formatDateTime(b.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="mt-4 flex items-center justify-between">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Страница {{ page }} из {{ totalPages }}
          </p>
          <div class="flex gap-2">
            <button
              @click="goToPage(page - 1)"
              :disabled="page <= 1"
              class="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:border-gray-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Назад
            </button>
            <button
              @click="goToPage(page + 1)"
              :disabled="page >= totalPages"
              class="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:border-gray-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Вперёд
            </button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { getAllBookings, type BookingDto } from "../api/bookings";
import { formatDateTime, formatPrice } from "../utils/formatters";
import { bookingStatusLabel, bookingStatusBadge } from "../utils/statusMaps";
import { useToast } from "../composables/useToast";

const toast = useToast();
const bookings = ref<BookingDto[]>([]);
const loading = ref(true);
const page = ref(1);
const totalCount = ref(0);
const totalPages = ref(1);
const pageSize = 20;
const statusFilter = ref("all");

const statusFilters = [
  { value: "all", label: "Все" },
  { value: "Pending", label: "Ожидание" },
  { value: "Confirmed", label: "Подтверждённые" },
  { value: "Active", label: "Активные" },
  { value: "Completed", label: "Завершённые" },
  { value: "Cancelled", label: "Отменённые" },
];

function setStatusFilter(value: string) {
  statusFilter.value = value;
  page.value = 1;
  reload();
}

async function reload() {
  loading.value = true;
  try {
    const result = await getAllBookings({
      page: page.value,
      pageSize,
      sortOrder: "desc",
      status: statusFilter.value === "all" ? undefined : statusFilter.value,
    });
    bookings.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;
  } catch (e: any) {
    toast.error("Ошибка загрузки: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

function goToPage(p: number) {
  if (p < 1 || p > totalPages.value) return;
  page.value = p;
  reload();
}

onMounted(reload);
</script>
