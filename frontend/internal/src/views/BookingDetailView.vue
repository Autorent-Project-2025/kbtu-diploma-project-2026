<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex items-center gap-4">
        <router-link
          to="/bookings"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-emerald-500 transition-colors"
        >
          Назад
        </router-link>
        <div class="space-y-1">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            {{ loading ? "Загрузка..." : `Бронирование #${booking?.id ?? ""}` }}
          </h1>
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

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Бронирование не найдено.
    </div>

    <template v-else-if="booking">
      <!-- Status -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-4">
            <span :class="['px-3 py-1 rounded-full text-sm font-semibold', statusBadge(booking.status)]">
              {{ statusLabel(booking.status) }}
            </span>
            <span v-if="booking.usedSubscription" class="px-3 py-1 rounded-full bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300 text-sm font-semibold">
              По подписке
            </span>
          </div>
          <button
            v-if="canCancel"
            @click="onCancel"
            :disabled="cancelling"
            class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-semibold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ cancelling ? "Отмена..." : "Отменить бронирование" }}
          </button>
        </div>
      </div>

      <!-- Car & pricing -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Автомобиль и стоимость</h2>
        <dl class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Автомобиль</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ booking.carBrand }} {{ booking.carModel }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Цена/час</dt>
            <dd class="text-gray-900 dark:text-white font-medium">
              {{ booking.priceHour ? formatPrice(booking.priceHour) : "—" }}
            </dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Итого</dt>
            <dd class="text-gray-900 dark:text-white font-bold text-lg">
              {{ booking.totalPrice ? formatPrice(booking.totalPrice) : "—" }}
            </dd>
          </div>
        </dl>
      </div>

      <!-- Timing -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Время</h2>
        <dl class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Начало</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ formatDateTime(booking.startTime) }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Окончание</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ formatDateTime(booking.endTime) }}</dd>
          </div>
          <div v-if="booking.tripStartedAt">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Поездка начата</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ formatDateTime(booking.tripStartedAt) }}</dd>
          </div>
          <div v-if="booking.tripCompletedAt">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Поездка завершена</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ formatDateTime(booking.tripCompletedAt) }}</dd>
          </div>
        </dl>
      </div>

      <!-- Participants -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Участники</h2>
        <dl class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Клиент (User ID)</dt>
            <dd class="text-gray-600 dark:text-gray-400 font-mono text-sm">{{ booking.userId }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Партнёр (User ID)</dt>
            <dd class="text-gray-600 dark:text-gray-400 font-mono text-sm">{{ booking.partnerUserId }}</dd>
          </div>
          <div v-if="booking.partnerName">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Имя партнёра</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ booking.partnerName }}</dd>
          </div>
        </dl>
      </div>

      <!-- Meta -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Мета</h2>
        <dl class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">ID бронирования</dt>
            <dd class="text-gray-600 dark:text-gray-400 font-mono">{{ booking.id }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">ID машины</dt>
            <dd class="text-gray-600 dark:text-gray-400 font-mono">{{ booking.partnerCarId }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Создано</dt>
            <dd class="text-gray-600 dark:text-gray-400 text-sm">{{ formatDateTime(booking.createdAt) }}</dd>
          </div>
        </dl>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getBooking, cancelBooking, type BookingDto } from "../api/bookings";
import { auth } from "../store/auth";
import { useToast } from "../composables/useToast";

const route = useRoute();
const toast = useToast();

const loading = ref(false);
const cancelling = ref(false);
const notFound = ref(false);
const booking = ref<BookingDto | null>(null);

const cancellableStatuses = new Set(["Pending", "PaymentPending", "Confirmed"]);

const canCancel = computed(() => {
  if (!booking.value?.status) return false;
  if (!cancellableStatuses.has(booking.value.status)) return false;
  return auth.hasPermission("Booking.Update");
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

function statusLabel(status?: string): string {
  if (!status) return "—";
  return bookingStatusStyles[status]?.label ?? status;
}

function statusBadge(status?: string): string {
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

async function loadBooking() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    booking.value = await getBooking(id);
  } catch {
    notFound.value = true;
  } finally {
    loading.value = false;
  }
}

async function onCancel() {
  if (cancelling.value || !booking.value) return;
  if (!confirm("Вы уверены, что хотите отменить это бронирование?")) return;

  cancelling.value = true;
  try {
    await cancelBooking(booking.value.id);
    toast.success("Бронирование отменено");
    await loadBooking();
  } catch {
    toast.error("Ошибка при отмене бронирования");
  } finally {
    cancelling.value = false;
  }
}

onMounted(loadBooking);
</script>
