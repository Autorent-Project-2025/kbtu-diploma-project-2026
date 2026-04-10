<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="space-y-2">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">Бронирования</h1>
          <p class="text-gray-600 dark:text-gray-400">
            Все бронирования платформы.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden">
            <div class="px-5 py-3 text-center">
              <p class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ totalCount }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">Всего</p>
            </div>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-emerald-500 transition-colors disabled:opacity-60"
          >
            Обновить
          </button>
        </div>
      </div>
    </header>

    <!-- Status filter -->
    <div class="flex flex-wrap gap-2">
      <button
        v-for="f in statusFilters"
        :key="f.value"
        @click="statusFilter = f.value"
        :class="[
          'px-4 py-1.5 rounded-full text-sm font-semibold border transition-colors',
          statusFilter === f.value
            ? 'bg-emerald-600 border-emerald-600 text-white'
            : 'border-gray-300 dark:border-gray-700 text-gray-600 dark:text-gray-400 hover:border-emerald-400',
        ]"
      >
        {{ f.label }}
      </button>
    </div>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Empty -->
    <div
      v-else-if="filteredBookings.length === 0"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Бронирования не найдены.
    </div>

    <template v-else>
      <!-- Table -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-gray-200 dark:border-gray-800">
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Автомобиль</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Период</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Стоимость</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Подписка</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Создано</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="booking in filteredBookings"
              :key="booking.id"
              @click="router.push(`/bookings/${booking.id}`)"
              class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
            >
              <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                {{ booking.id }}
              </td>
              <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">
                {{ booking.carBrand }} {{ booking.carModel }}
              </td>
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                <p>{{ formatDateTime(booking.startTime) }}</p>
                <p class="text-gray-400 dark:text-gray-500">{{ formatDateTime(booking.endTime) }}</p>
              </td>
              <td class="px-5 py-3 text-gray-900 dark:text-white font-semibold whitespace-nowrap">
                <template v-if="booking.totalPrice">{{ formatPrice(booking.totalPrice) }}</template>
                <template v-else>—</template>
              </td>
              <td class="px-5 py-3">
                <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', bookingStatusBadge(booking.status)]">
                  {{ bookingStatusLabel(booking.status) }}
                </span>
              </td>
              <td class="px-5 py-3">
                <span
                  v-if="booking.usedSubscription"
                  class="px-2 py-0.5 rounded-full bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300 text-xs font-semibold"
                >
                  Да
                </span>
                <span v-else class="text-gray-400 dark:text-gray-600 text-xs">—</span>
              </td>
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                {{ formatDateTime(booking.createdAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex items-center justify-center gap-2">
        <button
          @click="goToPage(page - 1)"
          :disabled="page <= 1"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Назад
        </button>
        <span class="text-sm text-gray-500 dark:text-gray-400 font-medium">
          {{ page }} / {{ totalPages }}
        </span>
        <button
          @click="goToPage(page + 1)"
          :disabled="page >= totalPages"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Вперёд
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getAllBookings, type BookingDto } from "../api/bookings";

const router = useRouter();
const bookings = ref<BookingDto[]>([]);
const loading = ref(false);
const page = ref(1);
const totalCount = ref(0);
const totalPages = ref(0);
const pageSize = 20;
const statusFilter = ref<string | "all">("all");

const statusFilters = [
  { value: "all" as const, label: "Все" },
  { value: "Pending", label: "Ожидание" },
  { value: "Confirmed", label: "Подтверждено" },
  { value: "Active", label: "Активно" },
  { value: "Completed", label: "Завершено" },
  { value: "Cancelled", label: "Отменено" },
];

const filteredBookings = computed(() => {
  if (statusFilter.value === "all") return bookings.value;
  return bookings.value.filter((b) => b.status === statusFilter.value);
});

const bookingStatusStyles: Record<string, { label: string; css: string }> = {
  Pending: { label: "Ожидание", css: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300" },
  PaymentPending: { label: "Оплата", css: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300" },
  Confirmed: { label: "Подтверждено", css: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300" },
  Active: { label: "Активно", css: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300" },
  Completed: { label: "Завершено", css: "bg-teal-100 text-teal-700 dark:bg-teal-500/20 dark:text-teal-300" },
  CompletionReviewPending: { label: "На проверке", css: "bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300" },
  Cancelled: { label: "Отменено", css: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300" },
  Expired: { label: "Истекло", css: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400" },
};

function bookingStatusLabel(status?: string): string {
  if (!status) return "—";
  return bookingStatusStyles[status]?.label ?? status;
}

function bookingStatusBadge(status?: string): string {
  if (!status) return "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400";
  return bookingStatusStyles[status]?.css ?? "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400";
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("ru-RU", { style: "currency", currency: "KZT", maximumFractionDigits: 0 }).format(value);
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
    const result = await getAllBookings(page.value, pageSize, undefined, "desc");
    bookings.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;
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
