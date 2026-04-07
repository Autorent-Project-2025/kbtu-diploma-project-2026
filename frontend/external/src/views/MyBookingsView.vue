<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-5xl mx-auto">
      <!-- Header -->
      <div class="mb-12 space-y-4 animate-slide-up">
        <h1
          class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
        >
          Мои бронирования
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Управляйте вашими арендами и отслеживайте статус
        </p>

        <!-- Filters -->
        <div class="flex flex-wrap gap-3">
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

      <!-- Bookings List -->
      <div v-if="filteredBookings.length > 0" class="space-y-6">
        <div
          v-for="b in filteredBookings"
          :key="b.id"
          class="group bg-white dark:bg-gray-900 rounded-3xl shadow-lg hover:shadow-2xl transition-all duration-500 overflow-hidden border border-gray-200 dark:border-gray-800 card-hover"
        >
          <div class="p-8">
            <div
              class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6"
            >
              <!-- Left: Car Info -->
              <div class="space-y-4 flex-1">
                <div class="flex items-start gap-4">
                  <!-- Status Indicator -->
                  <div class="flex-shrink-0 mt-1">
                    <div
                      :class="getStatusIndicatorClass(b.computedStatus)"
                      class="w-3 h-3 rounded-full animate-pulse"
                    ></div>
                  </div>

                  <div
                    class="h-24 w-32 flex-shrink-0 overflow-hidden rounded-2xl bg-gray-100 dark:bg-gray-800"
                  >
                    <img
                      v-if="getCoverImage(b)"
                      :src="getCoverImage(b)!"
                      :alt="`${b.carBrand} ${b.carModel}`"
                      class="h-full w-full object-cover"
                    />
                    <div
                      v-else
                      class="flex h-full w-full items-center justify-center text-xs text-gray-500 dark:text-gray-400"
                    >
                      Нет фото
                    </div>
                  </div>

                  <div class="space-y-2 flex-1">
                    <h3
                      class="text-2xl font-bold text-gray-900 dark:text-white"
                    >
                      {{ b.carBrand }} {{ b.carModel }}
                    </h3>

                    <p
                      v-if="b.partnerName"
                      class="text-sm font-medium text-primary-700 dark:text-primary-300"
                    >
                      Партнер: {{ b.partnerName }}
                    </p>

                    <!-- Dates -->
                    <div
                      class="flex flex-col sm:flex-row sm:items-center gap-4 text-sm"
                    >
                      <div
                        class="flex items-center gap-2 text-gray-600 dark:text-gray-400"
                      >
                        <div
                          class="w-8 h-8 rounded-lg bg-primary-50 dark:bg-primary-900/20 flex items-center justify-center"
                        >
                          <svg
                            class="w-4 h-4 text-primary-600 dark:text-primary-400"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              stroke-linecap="round"
                              stroke-linejoin="round"
                              stroke-width="2"
                              d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                            />
                          </svg>
                        </div>
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-500">
                            Начало
                          </p>
                          <p
                            class="font-semibold text-gray-900 dark:text-white"
                          >
                            {{ formatDate(b.startDate) }}
                          </p>
                        </div>
                      </div>

                      <svg
                        class="w-5 h-5 text-gray-400 hidden sm:block"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M13 7l5 5m0 0l-5 5m5-5H6"
                        />
                      </svg>

                      <div
                        class="flex items-center gap-2 text-gray-600 dark:text-gray-400"
                      >
                        <div
                          class="w-8 h-8 rounded-lg bg-primary-50 dark:bg-primary-900/20 flex items-center justify-center"
                        >
                          <svg
                            class="w-4 h-4 text-primary-600 dark:text-primary-400"
                            fill="none"
                            stroke="currentColor"
                            viewBox="0 0 24 24"
                          >
                            <path
                              stroke-linecap="round"
                              stroke-linejoin="round"
                              stroke-width="2"
                              d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                            />
                          </svg>
                        </div>
                        <div>
                          <p class="text-xs text-gray-500 dark:text-gray-500">
                            Окончание
                          </p>
                          <p
                            class="font-semibold text-gray-900 dark:text-white"
                          >
                            {{ formatDate(b.endDate) }}
                          </p>
                        </div>
                      </div>
                    </div>

                    <!-- Duration -->
                    <div
                      v-if="getDuration(b)"
                      class="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400"
                    >
                      <svg
                        class="w-4 h-4"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                        />
                      </svg>
                      <span>{{ getDurationText(b) }}</span>
                    </div>

                    <!-- Price (if available) -->
                    <div
                      v-if="b.usedSubscription"
                      class="flex items-baseline gap-2"
                    >
                      <span
                        class="text-lg font-bold text-emerald-600 dark:text-emerald-400"
                      >
                        Covered by subscription
                      </span>
                    </div>

                    <div v-else-if="b.price" class="flex items-baseline gap-2">
                      <span
                        class="text-3xl font-bold text-gray-900 dark:text-white"
                      >
                        {{ formatMoney(b.price, b.pricingBreakdown?.currency) }}
                      </span>
                      <span class="text-sm text-gray-500 dark:text-gray-400">
                        общая стоимость
                      </span>
                    </div>

                    <div
                      v-if="b.pricingBreakdown"
                      class="rounded-2xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-800/50 px-4 py-3 text-sm text-slate-700 dark:text-slate-200"
                    >
                      <p class="font-semibold text-slate-900 dark:text-white">
                        {{
                          b.usedSubscription
                            ? `Обычная стоимость: ${formatMoney(b.pricingBreakdown.quotedTotalPrice, b.pricingBreakdown.currency)}`
                            : `Снапшот расчета сохранен ${formatDate(b.pricingBreakdown.quotedAtUtc)}`
                        }}
                      </p>
                      <p class="mt-1">
                        {{ formatMoney(b.pricingBreakdown.quotedPriceHour, b.pricingBreakdown.currency) }}/час ·
                        {{ b.pricingBreakdown.billableHours }} ч.
                      </p>
                      <p class="mt-1">
                        x{{ b.pricingBreakdown.ratingCoefficient }} рейтинг ·
                        x{{ b.pricingBreakdown.advanceBookingCoefficient }} раннее бронирование ·
                        x{{ b.pricingBreakdown.availabilityCoefficient }} доступность
                      </p>
                      <p
                        v-if="b.pricingBreakdown.isMarketValueStale"
                        class="mt-1 font-medium text-amber-700 dark:text-amber-300"
                      >
                        Использован последний доступный рыночный снапшот.
                      </p>
                    </div>

                    <div
                      v-if="getGalleryImages(b).length > 0"
                      class="flex flex-wrap gap-2 pt-1"
                    >
                      <div
                        v-for="imageUrl in getGalleryImages(b)"
                        :key="imageUrl"
                        class="h-12 w-16 overflow-hidden rounded-xl bg-gray-100 dark:bg-gray-800"
                      >
                        <img
                          :src="imageUrl"
                          :alt="`${b.carBrand} ${b.carModel}`"
                          class="h-full w-full object-cover"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Right: Status Badge & Actions -->
              <div class="flex flex-col gap-3 flex-shrink-0">
                <span
                  v-if="b.usedSubscription"
                  class="inline-flex items-center justify-center px-4 py-2 rounded-xl text-xs font-bold bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300"
                >
                  SUBSCRIPTION
                </span>

                <span
                  v-if="b.carCommentId"
                  class="inline-flex items-center justify-center px-4 py-2 rounded-xl text-xs font-bold bg-sky-100 text-sky-700 dark:bg-sky-900/30 dark:text-sky-300"
                >
                  ОТЗЫВ ОСТАВЛЕН
                </span>

                <span
                  :class="getStatusClass(b.computedStatus)"
                  class="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-2xl text-sm font-extrabold uppercase tracking-wider shadow-lg"
                >
                  <span
                    :class="getStatusDotClass(b.computedStatus)"
                    class="w-2 h-2 rounded-full"
                  ></span>
                  {{ getStatusText(b.computedStatus) }}
                </span>

                <router-link
                  v-if="canPay(b)"
                  :to="`/bookings/${b.id}/payment`"
                  class="px-6 py-3 bg-amber-600 hover:bg-amber-700 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 flex items-center justify-center gap-2"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M17 9V7a5 5 0 00-10 0v2m-2 0h14a2 2 0 012 2v7a2 2 0 01-2 2H5a2 2 0 01-2-2v-7a2 2 0 012-2z"
                    />
                  </svg>
                  <span>Оплатить</span>
                </router-link>

                <button
                  v-if="canStartTripAction(b)"
                  @click="handleStartTrip(b)"
                  :disabled="startingId === b.id"
                  class="px-6 py-3 bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                >
                  <svg
                    v-if="startingId !== b.id"
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M14.752 11.168l-5.197-3.466A1 1 0 008 8.535v6.93a1 1 0 001.555.832l5.197-3.466a1 1 0 000-1.664z"
                    />
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <span>{{
                    startingId === b.id ? "Запускаем..." : "Начать поездку"
                  }}</span>
                </button>

                <router-link
                  v-if="canCompleteTripAction(b)"
                  :to="`/bookings/${b.id}/complete`"
                  class="px-6 py-3 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 flex items-center justify-center gap-2"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M5 13l4 4L19 7"
                    />
                  </svg>
                  <span>Завершить поездку</span>
                </router-link>

                <router-link
                  v-if="canOpenCompletionDetails(b)"
                  :to="`/bookings/${b.id}/complete`"
                  class="px-6 py-3 bg-violet-600 hover:bg-violet-700 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 flex items-center justify-center gap-2"
                >
                  <svg
                    class="w-5 h-5"
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
                  <span>{{
                    b.status === "completed"
                      ? "Детали завершения"
                      : "Статус завершения"
                  }}</span>
                </router-link>

                <button
                  v-if="b.canLeaveComment"
                  @click="openReviewModal(b)"
                  :disabled="reviewSubmittingId === b.id"
                  class="px-6 py-3 bg-sky-600 hover:bg-sky-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                >
                  <svg
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M8 10h8M8 14h5m-6 7h10a2 2 0 002-2V5a2 2 0 00-2-2H7a2 2 0 00-2 2v14a2 2 0 002 2z"
                    />
                  </svg>
                  <span>{{
                    reviewSubmittingId === b.id
                      ? "Отправляем..."
                      : "Оставить отзыв"
                  }}</span>
                </button>

                <!-- Cancel Button -->
                <button
                  v-if="canCancel(b)"
                  @click="confirmCancel(b)"
                  :disabled="cancelingId === b.id"
                  class="px-6 py-3 bg-red-600 hover:bg-red-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed flex items-center justify-center gap-2"
                >
                  <svg
                    v-if="cancelingId !== b.id"
                    class="w-5 h-5"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                  <span v-if="cancelingId === b.id">Отмена...</span>
                  <span v-else>Отменить</span>
                </button>
              </div>
            </div>
          </div>

          <!-- Glow Effect -->
          <div
            class="absolute inset-0 rounded-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none"
            style="box-shadow: 0 0 40px rgba(59, 130, 246, 0.2)"
          ></div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-32">
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
                d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
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
            to="/cars"
            class="btn-premium inline-flex items-center gap-2"
          >
            <span>Выбрать автомобиль</span>
            <svg
              class="w-5 h-5"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M13 7l5 5m0 0l-5 5m5-5H6"
              />
            </svg>
          </router-link>
        </div>
      </div>
    </div>

    <!-- Cancel Confirmation Modal -->
    <CancelBookingModal
      v-if="bookingToCancel"
      :is-open="showCancelModal"
      :booking="bookingToCancel"
      @close="closeCancelModal"
      @confirm="handleCancelConfirm"
    />

    <ReviewModal
      v-if="bookingToReview && reviewSubject"
      :is-open="showReviewModal"
      :subject="reviewSubject"
      :submitting="isReviewSubmitting"
      @close="closeReviewModal"
      @submit="handleReviewSubmit"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import {
  getMyBookings,
  cancelBooking,
  startBookingTrip,
  submitBookingCarComment,
} from "../api/booking";
import type { Booking } from "../types/Booking";
import {
  computeBookingStatus,
  canCancelBooking,
  canCompleteTrip,
  canStartTrip,
  formatBookingDate,
  getBookingDuration,
  hasCompletionReviewDetails,
} from "../utils/bookingUtils";
import { useToast } from "../composables/useToast";
import CancelBookingModal from "../components/CancelBookingModal.vue";
import ReviewModal from "../components/ReviewModal.vue";

interface BookingWithComputedStatus extends Booking {
  computedStatus: ReturnType<typeof computeBookingStatus>;
}

const bookings = ref<BookingWithComputedStatus[]>([]);
const currentFilter = ref<
  | "all"
  | "paymentPending"
  | "upcoming"
  | "active"
  | "awaitingReview"
  | "completed"
  | "canceled"
>("all");
const cancelingId = ref<number | null>(null);
const startingId = ref<number | null>(null);
const bookingToCancel = ref<BookingWithComputedStatus | null>(null);
const showCancelModal = ref(false);
const bookingToReview = ref<BookingWithComputedStatus | null>(null);
const showReviewModal = ref(false);
const reviewSubmittingId = ref<number | null>(null);
const { success, error } = useToast();

// Filters configuration
const filters = computed(() => {
  const all = bookings.value.length;
  const paymentPending = bookings.value.filter(
    (b) => b.computedStatus === "paymentPending",
  ).length;
  const upcoming = bookings.value.filter(
    (b) => b.computedStatus === "upcoming",
  ).length;
  const active = bookings.value.filter(
    (b) => b.computedStatus === "active",
  ).length;
  const awaitingReview = bookings.value.filter(
    (b) => b.computedStatus === "awaitingReview",
  ).length;
  const completed = bookings.value.filter(
    (b) => b.computedStatus === "completed",
  ).length;
  const canceled = bookings.value.filter(
    (b) => b.computedStatus === "canceled",
  ).length;

  return [
    { label: "Все", value: "all" as const, count: all },
    {
      label: "Ожидают оплаты",
      value: "paymentPending" as const,
      count: paymentPending,
    },
    { label: "Предстоящие", value: "upcoming" as const, count: upcoming },
    { label: "Активные", value: "active" as const, count: active },
    {
      label: "На проверке",
      value: "awaitingReview" as const,
      count: awaitingReview,
    },
    { label: "Завершенные", value: "completed" as const, count: completed },
    { label: "Отмененные", value: "canceled" as const, count: canceled },
  ];
});

const filteredBookings = computed(() => {
  if (currentFilter.value === "all") {
    return bookings.value;
  }
  return bookings.value.filter((b) => b.computedStatus === currentFilter.value);
});

const reviewSubject = computed(() => {
  if (!bookingToReview.value) {
    return null;
  }

  return {
    brand: bookingToReview.value.carBrand,
    model: bookingToReview.value.carModel,
    year: null,
  };
});

const isReviewSubmitting = computed(
  () =>
    bookingToReview.value != null &&
    reviewSubmittingId.value === bookingToReview.value.id,
);

onMounted(async () => {
  await loadBookings();
});

async function loadBookings() {
  try {
    const data = await getMyBookings();

    // Обрабатываем оба формата ответа
    const items = Array.isArray(data) ? data : data.items;

    bookings.value = items.map(toBookingWithComputedStatus);
  } catch (e) {
    console.error("Failed to load bookings", e);
    error("Не удалось загрузить бронирования");
    bookings.value = []; // Очищаем список при ошибке
  }
}

function toBookingWithComputedStatus(booking: Booking): BookingWithComputedStatus {
  return {
    ...booking,
    computedStatus: computeBookingStatus(booking),
  };
}

function formatDate(dateString: string): string {
  return formatBookingDate(dateString);
}

function getDuration(booking: Booking) {
  return getBookingDuration(booking.startDate, booking.endDate);
}

function getDurationText(booking: Booking): string {
  const duration = getDuration(booking);
  if (!duration) return "";

  const parts = [];
  if (duration.days > 0) parts.push(`${duration.days} дн.`);
  if (duration.hours > 0) parts.push(`${duration.hours} ч.`);
  if (duration.minutes > 0 && duration.days === 0)
    parts.push(`${duration.minutes} мин.`);

  return parts.join(" ");
}

function formatMoney(amount: number | null | undefined, currency = "KZT"): string {
  if (amount == null) {
    return "Цена не рассчитана";
  }

  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function getCoverImage(booking: Booking): string | null {
  return booking.coverImageUrl ?? booking.imageUrls?.[0] ?? null;
}

function getGalleryImages(booking: Booking): string[] {
  const coverImage = getCoverImage(booking);

  return (booking.imageUrls ?? [])
    .filter((imageUrl) => imageUrl !== coverImage)
    .slice(0, 3);
}

function canCancel(booking: BookingWithComputedStatus): boolean {
  return canCancelBooking(booking);
}

function canPay(booking: BookingWithComputedStatus): boolean {
  return booking.computedStatus === "paymentPending";
}

function canStartTripAction(booking: BookingWithComputedStatus): boolean {
  return canStartTrip(booking);
}

function canCompleteTripAction(booking: BookingWithComputedStatus): boolean {
  return canCompleteTrip(booking);
}

function canOpenCompletionDetails(booking: BookingWithComputedStatus): boolean {
  if (booking.status === "active") {
    return false;
  }

  return hasCompletionReviewDetails(booking);
}

function confirmCancel(booking: BookingWithComputedStatus) {
  bookingToCancel.value = booking;
  showCancelModal.value = true;
}

function closeCancelModal() {
  showCancelModal.value = false;
  setTimeout(() => {
    bookingToCancel.value = null;
  }, 300);
}

function openReviewModal(booking: BookingWithComputedStatus) {
  bookingToReview.value = booking;
  showReviewModal.value = true;
}

function closeReviewModal() {
  if (isReviewSubmitting.value) {
    return;
  }

  showReviewModal.value = false;
  setTimeout(() => {
    bookingToReview.value = null;
  }, 300);
}

async function handleCancelConfirm() {
  if (!bookingToCancel.value) return;

  const bookingId = bookingToCancel.value.id;
  cancelingId.value = bookingId;

  try {
    await cancelBooking(bookingId);
    success("Бронирование успешно отменено");
    closeCancelModal();
    await loadBookings(); // Перезагружаем список
  } catch (e) {
    console.error("Failed to cancel booking", e);
    error(
      (e as any)?.response?.data?.detail ||
        (e as any)?.response?.data?.error ||
        "Не удалось отменить бронирование",
    );
  } finally {
    cancelingId.value = null;
  }
}

async function handleStartTrip(booking: BookingWithComputedStatus) {
  startingId.value = booking.id;

  try {
    await startBookingTrip(booking.id);
    success("Поездка начата");
    await loadBookings();
  } catch (e) {
    console.error("Failed to start trip", e);
    error(
      (e as any)?.response?.data?.detail ||
        (e as any)?.response?.data?.error ||
        "Не удалось начать поездку",
    );
  } finally {
    startingId.value = null;
  }
}

async function handleReviewSubmit(rating: number, content: string) {
  if (!bookingToReview.value) {
    return;
  }

  reviewSubmittingId.value = bookingToReview.value.id;

  try {
    const result = await submitBookingCarComment(bookingToReview.value.id, {
      rating,
      content,
    });

    updateBookingInList(result.booking);
    reviewSubmittingId.value = null;
    showReviewModal.value = false;
    setTimeout(() => {
      bookingToReview.value = null;
    }, 300);
    success("Отзыв успешно опубликован");
  } catch (e) {
    console.error("Failed to submit booking car comment", e);
    error(
      (e as any)?.response?.data?.detail ||
        (e as any)?.response?.data?.error ||
        "Не удалось отправить отзыв",
    );
  } finally {
    reviewSubmittingId.value = null;
  }
}

function updateBookingInList(updatedBooking: Booking) {
  const index = bookings.value.findIndex((item) => item.id === updatedBooking.id);
  if (index === -1) {
    return;
  }

  bookings.value[index] = toBookingWithComputedStatus(updatedBooking);
}

function getStatusClass(status: ReturnType<typeof computeBookingStatus>) {
  switch (status) {
    case "paymentPending":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case "upcoming":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case "active":
      return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300";
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

function getStatusIndicatorClass(
  status: ReturnType<typeof computeBookingStatus>,
) {
  switch (status) {
    case "paymentPending":
      return "bg-amber-500";
    case "upcoming":
      return "bg-blue-500";
    case "active":
      return "bg-green-500";
    case "awaitingReview":
      return "bg-violet-500";
    case "completed":
      return "bg-gray-500";
    case "canceled":
      return "bg-red-500";
    default:
      return "bg-gray-500";
  }
}

function getStatusDotClass(status: ReturnType<typeof computeBookingStatus>) {
  switch (status) {
    case "paymentPending":
      return "bg-amber-600 dark:bg-amber-400";
    case "upcoming":
      return "bg-blue-600 dark:bg-blue-400";
    case "active":
      return "bg-green-600 dark:bg-green-400";
    case "awaitingReview":
      return "bg-violet-600 dark:bg-violet-400";
    case "completed":
      return "bg-gray-600 dark:bg-gray-400";
    case "canceled":
      return "bg-red-600 dark:bg-red-400";
    default:
      return "bg-gray-600 dark:bg-gray-400";
  }
}

function getStatusText(status: ReturnType<typeof computeBookingStatus>) {
  switch (status) {
    case "paymentPending":
      return "Ожидает оплаты";
    case "upcoming":
      return "Предстоящее";
    case "active":
      return "Активное";
    case "awaitingReview":
      return "На проверке";
    case "completed":
      return "Завершено";
    case "canceled":
      return "Отменено";
    default:
      return "Неизвестно";
  }
}

function getEmptyStateTitle(): string {
  if (currentFilter.value === "all") {
    return "Нет бронирований";
  }
  return `Нет ${filters.value
    .find((f) => f.value === currentFilter.value)
    ?.label.toLowerCase()} бронирований`;
}

function getEmptyStateDescription(): string {
  if (currentFilter.value === "all") {
    return "Вы еще не арендовали ни одного автомобиля";
  }
  return "Попробуйте выбрать другой фильтр";
}
</script>
