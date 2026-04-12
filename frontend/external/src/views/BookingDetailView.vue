<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-6xl mx-auto space-y-8">
      <button
        type="button"
        class="inline-flex items-center gap-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
        @click="router.push('/bookings')"
      >
        <span>&larr;</span>
        <span>К списку бронирований</span>
      </button>

      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-center text-gray-500 dark:text-gray-400"
      >
        Загружаем бронирование...
      </div>

      <div
        v-else-if="loadError"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-6 text-red-700 dark:text-red-300"
      >
        {{ loadError }}
      </div>

      <template v-else-if="booking">
        <section class="rounded-[32px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
          <div class="grid gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
            <div class="bg-gray-100 dark:bg-gray-800 min-h-[260px]">
              <img
                v-if="heroImage"
                :src="heroImage"
                :alt="bookingTitle"
                class="h-full w-full object-cover"
              />
              <div
                v-else
                class="flex h-full min-h-[260px] items-center justify-center text-gray-400 dark:text-gray-600"
              >
                Нет фото
              </div>
            </div>

            <div class="p-6 sm:p-8 space-y-6">
              <div class="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
                <div class="space-y-3">
                  <p class="text-sm font-bold uppercase tracking-[0.24em] text-primary-600 dark:text-primary-400">
                    Детали бронирования
                  </p>
                  <div>
                    <h1 class="text-3xl sm:text-4xl font-extrabold text-gray-900 dark:text-white">
                      {{ bookingTitle }}
                    </h1>
                    <p
                      v-if="booking.partnerName"
                      class="mt-2 text-base font-medium text-primary-600 dark:text-primary-400"
                    >
                      {{ booking.partnerName }}
                    </p>
                  </div>
                  <div class="flex flex-wrap items-center gap-3">
                    <span
                      :class="statusClass(booking.status)"
                      class="inline-flex items-center gap-2 rounded-2xl px-4 py-2 text-sm font-bold"
                    >
                      <span class="h-2.5 w-2.5 rounded-full bg-current opacity-80"></span>
                      {{ statusLabel(booking.status) }}
                    </span>
                    <span class="text-sm text-gray-500 dark:text-gray-400">
                      Бронирование #{{ booking.id }}
                    </span>
                  </div>
                </div>

                <div class="flex flex-wrap gap-3">
                  <router-link
                    v-if="canPay"
                    :to="`/bookings/${booking.id}/payment`"
                    class="px-5 py-3 rounded-2xl bg-amber-600 hover:bg-amber-700 text-white font-bold transition-colors"
                  >
                    Перейти к оплате
                  </router-link>
                  <button
                    v-else-if="canStart"
                    type="button"
                    class="px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white font-bold transition-colors"
                    :disabled="actionLoading"
                    @click="handleStartTrip"
                  >
                    {{ actionLoading ? "Запускаем..." : "Начать поездку" }}
                  </button>
                  <router-link
                    v-else-if="canOpenCompletion"
                    :to="`/bookings/${booking.id}/complete`"
                    class="px-5 py-3 rounded-2xl bg-violet-600 hover:bg-violet-700 text-white font-bold transition-colors"
                  >
                    {{ booking.status === "active" ? "Завершить поездку" : "Статус завершения" }}
                  </router-link>
                </div>
              </div>

              <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Начало</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ formatDateTime(booking.startDate) }}</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Завершение</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ formatDateTime(booking.endDate) }}</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Факт. старт</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">
                    {{ booking.tripStartedAt ? formatDateTime(booking.tripStartedAt) : "Не начата" }}
                  </p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Стоимость</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ formatMoney(booking.price, booking.pricingBreakdown?.currency) }}</p>
                </article>
              </div>

              <div class="grid gap-4 md:grid-cols-2">
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 space-y-2">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Длительность брони</p>
                  <p class="text-lg font-bold text-gray-900 dark:text-white">{{ bookingDurationText }}</p>
                  <p class="text-sm text-gray-500 dark:text-gray-400">Период по плану</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 space-y-2">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Фактическая поездка</p>
                  <p class="text-lg font-bold text-gray-900 dark:text-white">{{ tripDurationText || "Появится после старта" }}</p>
                  <p class="text-sm text-gray-500 dark:text-gray-400">
                    {{ booking.tripCompletedAt ? "Время уже зафиксировано" : "Считается по текущему времени" }}
                  </p>
                </article>
              </div>

              <div
                v-if="galleryImages.length > 0"
                class="grid grid-cols-2 gap-3 sm:grid-cols-4"
              >
                <div
                  v-for="imageUrl in galleryImages"
                  :key="imageUrl"
                  class="aspect-[4/3] overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-100 dark:bg-gray-800"
                >
                  <img :src="imageUrl" :alt="bookingTitle" class="h-full w-full object-cover" />
                </div>
              </div>
            </div>
          </div>
        </section>

        <section
          v-if="booking.pricingBreakdown"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-4"
        >
          <div class="flex flex-wrap items-center gap-3">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Расчёт стоимости</h2>
            <span
              v-if="booking.pricingBreakdown.isMarketValueStale"
              class="rounded-full bg-amber-100 px-3 py-1 text-xs font-bold text-amber-700 dark:bg-amber-900/30 dark:text-amber-300"
            >
              Рыночные данные были устаревшими
            </span>
          </div>

          <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Базовая цена / час</p>
              <p class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
                {{ formatMoney(basePricePerHour, booking.pricingBreakdown.currency) }}
              </p>
            </article>
            <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Итог / час</p>
              <p class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
                {{ formatMoney(booking.pricingBreakdown.quotedPriceHour, booking.pricingBreakdown.currency) }}
              </p>
            </article>
            <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Оплачиваемые часы</p>
              <p class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
                {{ booking.pricingBreakdown.billableHours }}
              </p>
            </article>
            <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4">
              <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Коэффициенты</p>
              <p class="mt-2 text-sm font-semibold text-gray-900 dark:text-white">
                Рейтинг ×{{ booking.pricingBreakdown.ratingCoefficient }}, доступность ×{{ booking.pricingBreakdown.availabilityCoefficient }}
              </p>
              <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                Заблаговременность ×{{ booking.pricingBreakdown.advanceBookingCoefficient }}
              </p>
            </article>
          </div>
        </section>

        <section class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-4">
          <div>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Начисления</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Все штрафы и пени, связанные с бронированием.
            </p>
          </div>

          <div
            v-if="chargesLoading"
            class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-6 text-center text-gray-500 dark:text-gray-400"
          >
            Загружаем начисления...
          </div>

          <div
            v-else-if="charges.length === 0"
            class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-6 text-center text-gray-500 dark:text-gray-400"
          >
            Начислений пока нет.
          </div>

          <div v-else class="space-y-4">
            <article
              v-for="charge in charges"
              :key="charge.id"
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4"
            >
              <div class="space-y-2">
                <p class="text-xs font-bold uppercase tracking-[0.18em]" :class="chargeBadgeClass(charge.chargeType)">
                  {{ chargeTypeLabel(charge.chargeType) }}
                </p>
                <p class="text-lg font-bold text-gray-900 dark:text-white">
                  {{ formatMoney(charge.amount, charge.currency) }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ charge.description || "Описание начисления появится после обработки." }}
                </p>
              </div>
              <div class="space-y-1 text-sm text-gray-500 dark:text-gray-400">
                <p>Статус: <span class="font-semibold text-gray-900 dark:text-white">{{ chargeStatusLabel(charge.status) }}</span></p>
                <p>Создано: {{ formatDateTime(charge.createdAt) }}</p>
                <p v-if="charge.paidAt">Оплачено: {{ formatDateTime(charge.paidAt) }}</p>
              </div>
            </article>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getBooking, getBookingCharges, startBookingTrip } from "../api/booking";
import { useToast } from "../composables/useToast";
import type { Booking, BookingCharge } from "../types/Booking";
import {
  canStartTrip,
  getBookingDuration,
  getTripDuration,
  hasCompletionReviewDetails,
} from "../utils/bookingUtils";

const HOURLY_BASE_FACTOR = 0.0001;

const route = useRoute();
const router = useRouter();
const { success, error } = useToast();

const booking = ref<Booking | null>(null);
const charges = ref<BookingCharge[]>([]);
const loading = ref(true);
const chargesLoading = ref(false);
const loadError = ref<string | null>(null);
const actionLoading = ref(false);

const bookingId = Number(route.params.id);

const bookingTitle = computed(() =>
  booking.value
    ? `${booking.value.carBrand} ${booking.value.carModel}`
    : `Бронирование #${route.params.id}`,
);

const heroImage = computed(
  () => booking.value?.coverImageUrl ?? booking.value?.imageUrls?.[0] ?? null,
);

const galleryImages = computed(() => {
  if (!booking.value) {
    return [];
  }

  return (booking.value.imageUrls ?? [])
    .filter((imageUrl) => imageUrl !== heroImage.value)
    .slice(0, 4);
});

const bookingDurationText = computed(() => {
  if (!booking.value) {
    return "—";
  }

  return (
    formatDuration(getBookingDuration(booking.value.startDate, booking.value.endDate)) ||
    "—"
  );
});

const tripDurationText = computed(() =>
  formatDuration(
    getTripDuration(
      booking.value?.tripStartedAt ?? null,
      booking.value?.tripCompletedAt ?? null,
    ),
  ),
);

const basePricePerHour = computed(() => {
  if (!booking.value?.pricingBreakdown) {
    return null;
  }

  return Number(
    (booking.value.pricingBreakdown.marketValueKzt * HOURLY_BASE_FACTOR).toFixed(
      2,
    ),
  );
});

const canPay = computed(() => booking.value?.status === "pending");
const canStart = computed(() =>
  booking.value ? canStartTrip(booking.value) : false,
);
const canOpenCompletion = computed(() =>
  booking.value
    ? booking.value.status === "active" ||
      hasCompletionReviewDetails(booking.value)
    : false,
);

onMounted(async () => {
  if (!Number.isFinite(bookingId) || bookingId <= 0) {
    loadError.value = "Некорректный идентификатор бронирования.";
    loading.value = false;
    return;
  }

  await loadBookingDetails();
});

async function loadBookingDetails() {
  loading.value = true;
  loadError.value = null;

  try {
    booking.value = await getBooking(bookingId);
    await loadCharges();
  } catch (loadCause) {
    console.error("Failed to load booking details", loadCause);
    loadError.value = "Не удалось загрузить детали бронирования.";
    booking.value = null;
    charges.value = [];
  } finally {
    loading.value = false;
  }
}

async function loadCharges() {
  if (!booking.value) {
    charges.value = [];
    return;
  }

  chargesLoading.value = true;
  try {
    charges.value = await getBookingCharges(booking.value.id);
  } catch (loadCause) {
    console.error("Failed to load booking charges", loadCause);
    charges.value = [];
  } finally {
    chargesLoading.value = false;
  }
}

async function handleStartTrip() {
  if (!booking.value || actionLoading.value) {
    return;
  }

  actionLoading.value = true;
  try {
    await startBookingTrip(booking.value.id);
    success("Поездка начата.");
    await loadBookingDetails();
  } catch (startCause: any) {
    console.error("Failed to start trip from booking details", startCause);
    error(
      startCause?.response?.data?.detail ||
        startCause?.response?.data?.error ||
        "Не удалось начать поездку.",
    );
  } finally {
    actionLoading.value = false;
  }
}

function statusLabel(status: Booking["status"]) {
  switch (status) {
    case "pending":
      return "Ожидает оплаты";
    case "confirmed":
      return "Подтверждено";
    case "active":
      return "В поездке";
    case "awaitingReview":
      return "На проверке";
    case "completed":
      return "Завершено";
    case "canceled":
      return "Отменено";
    default:
      return status;
  }
}

function statusClass(status: Booking["status"]) {
  switch (status) {
    case "pending":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case "confirmed":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case "active":
      return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
    case "awaitingReview":
      return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
    case "completed":
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
    case "canceled":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
}

function chargeTypeLabel(chargeType: string) {
  const normalized = chargeType.trim().toLowerCase();
  if (normalized === "latepenalty") return "Пеня за просрочку";
  if (normalized === "damagefine") return "Штраф за повреждение";
  return chargeType;
}

function chargeStatusLabel(status: string) {
  const normalized = status.trim().toLowerCase();
  if (normalized === "pending") return "Ожидает оплаты";
  if (normalized === "paid") return "Оплачен";
  if (normalized === "canceled") return "Отменён";
  return status;
}

function chargeBadgeClass(chargeType: string) {
  const normalized = chargeType.trim().toLowerCase();
  if (normalized === "damagefine") return "text-red-700 dark:text-red-300";
  if (normalized === "latepenalty") return "text-amber-700 dark:text-amber-300";
  return "text-gray-700 dark:text-gray-300";
}

function formatMoney(amount: number | null | undefined, currency = "KZT") {
  if (amount == null) {
    return "Не рассчитано";
  }

  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatDuration(
  duration: { days: number; hours: number; minutes: number } | null,
) {
  if (!duration) {
    return "";
  }

  const parts: string[] = [];
  if (duration.days > 0) parts.push(`${duration.days} дн.`);
  if (duration.hours > 0) parts.push(`${duration.hours} ч.`);
  if (duration.minutes > 0 || parts.length === 0) parts.push(`${duration.minutes} мин.`);
  return parts.join(" ");
}
</script>
