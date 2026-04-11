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
          Мои бронирования
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Управляйте вашими арендами и отслеживайте статус
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

      <!-- Bookings List -->
      <div v-if="filteredBookings.length > 0" class="space-y-6">
        <div
          v-for="b in paginatedBookings"
          :key="b.id"
          class="group relative overflow-hidden rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-md hover:shadow-xl transition-all duration-300"
        >
          <div class="flex flex-col sm:flex-row">
            <!-- Car image panel -->
            <div
              class="relative sm:w-56 sm:flex-shrink-0 h-48 sm:h-auto overflow-hidden bg-gray-100 dark:bg-gray-800 rounded-t-3xl sm:rounded-l-3xl sm:rounded-tr-none"
            >
              <img
                v-if="getCoverImage(b)"
                :src="getCoverImage(b)!"
                :alt="`${b.carBrand} ${b.carModel}`"
                class="w-full h-full object-cover"
              />
              <div
                v-else
                class="flex h-full w-full items-center justify-center text-gray-400 dark:text-gray-600"
              >
                <svg
                  class="w-12 h-12"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="1.5"
                    d="M3 9l9-7 9 7v11a2 2 0 01-2 2H5a2 2 0 01-2-2z"
                  />
                </svg>
              </div>
              <!-- Status strip on image -->
              <div class="absolute bottom-0 left-0 right-0 p-2.5">
                <span
                  :class="getStatusClass(b.computedStatus)"
                  class="inline-flex items-center gap-1.5 px-3 py-1 rounded-xl text-xs font-bold uppercase tracking-wide backdrop-blur-sm shadow"
                >
                  <span
                    :class="getStatusDotClass(b.computedStatus)"
                    class="w-1.5 h-1.5 rounded-full"
                  ></span>
                  {{ getStatusText(b.computedStatus) }}
                </span>
              </div>
            </div>

            <!-- Content -->
            <div class="flex-1 p-5 flex flex-col gap-4">
              <!-- Top row: car name + badges -->
              <div class="flex items-start justify-between gap-3">
                <div>
                  <h3
                    class="text-xl font-bold text-gray-900 dark:text-white leading-tight"
                  >
                    {{ b.carBrand }} {{ b.carModel }}
                  </h3>
                  <p
                    v-if="b.partnerName"
                    class="text-sm text-primary-600 dark:text-primary-400 font-medium mt-0.5"
                  >
                    {{ b.partnerName }}
                  </p>
                </div>
                <div class="flex flex-col items-end gap-1.5 flex-shrink-0">
                  <span
                    v-if="b.usedSubscription"
                    class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300"
                  >
                    SUB
                  </span>
                  <span
                    v-if="b.carCommentId"
                    class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold bg-sky-100 text-sky-700 dark:bg-sky-900/30 dark:text-sky-300"
                  >
                    ★ Отзыв
                  </span>
                </div>
              </div>

              <!-- Dates + duration -->
              <div class="flex flex-wrap items-center gap-3 text-sm">
                <div
                  class="flex items-center gap-2 text-gray-700 dark:text-gray-300"
                >
                  <svg
                    class="w-4 h-4 text-gray-400"
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
                  <span class="font-semibold">{{
                    formatDate(b.startDate)
                  }}</span>
                </div>
                <svg
                  class="w-4 h-4 text-gray-300 dark:text-gray-600"
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
                  class="flex items-center gap-2 text-gray-700 dark:text-gray-300"
                >
                  <svg
                    class="w-4 h-4 text-gray-400"
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
                  <span class="font-semibold">{{ formatDate(b.endDate) }}</span>
                </div>
                <span
                  v-if="getDuration(b)"
                  class="text-gray-400 dark:text-gray-500 text-xs"
                  >·</span
                >
                <span
                  v-if="getDuration(b)"
                  class="text-gray-500 dark:text-gray-400 text-xs font-medium"
                  >{{ getDurationText(b) }}</span
                >
              </div>

              <!-- Price row -->
              <div class="flex items-end justify-between gap-4 flex-wrap">
                <div>
                  <div v-if="b.usedSubscription">
                    <span
                      class="text-lg font-bold text-emerald-600 dark:text-emerald-400"
                      >Покрыто подпиской</span
                    >
                    <p
                      v-if="b.pricingBreakdown"
                      class="text-xs text-gray-400 mt-0.5"
                    >
                      Стоимость без подписки:
                      {{
                        formatMoney(
                          b.pricingBreakdown.quotedTotalPrice,
                          b.pricingBreakdown.currency,
                        )
                      }}
                    </p>
                  </div>
                  <div v-else-if="b.price">
                    <div class="flex items-baseline gap-2">
                      <span
                        class="text-2xl font-extrabold text-gray-900 dark:text-white"
                      >
                        {{ formatMoney(b.price, b.pricingBreakdown?.currency) }}
                      </span>
                      <span class="text-xs text-gray-400">итого</span>
                    </div>
                    <p
                      v-if="b.pricingBreakdown"
                      class="text-xs text-gray-400 dark:text-gray-500 mt-0.5"
                    >
                      {{
                        formatMoney(
                          b.pricingBreakdown.quotedPriceHour,
                          b.pricingBreakdown.currency,
                        )
                      }}/ч · {{ b.pricingBreakdown.billableHours }} ч. · ×{{
                        b.pricingBreakdown.ratingCoefficient
                      }}
                      рейтинг · ×{{
                        b.pricingBreakdown.advanceBookingCoefficient
                      }}
                      заблаговременность
                    </p>
                  </div>
                </div>

                <!-- Actions -->
                <div class="flex items-center gap-2 flex-shrink-0">
                  <component
                    :is="getPrimaryAction(b).to ? 'router-link' : 'button'"
                    v-bind="getPrimaryActionProps(b)"
                    @click="handlePrimaryAction(b, $event)"
                    :disabled="isPrimaryActionDisabled(b)"
                    class="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold text-white shadow transition-all active:scale-95 disabled:cursor-not-allowed disabled:opacity-50"
                    :class="getPrimaryActionButtonClass(b)"
                  >
                    {{ getPrimaryActionLabel(b) }}
                  </component>

                  <div class="relative">
                    <button
                      type="button"
                      @click="toggleActionMenu(b.id)"
                      class="inline-flex items-center justify-center w-10 h-10 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 transition"
                    >
                      <svg
                        class="w-4 h-4"
                        fill="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <circle cx="12" cy="5" r="1.5" />
                        <circle cx="12" cy="12" r="1.5" />
                        <circle cx="12" cy="19" r="1.5" />
                      </svg>
                    </button>

                    <div
                      v-if="openActionMenuId === b.id"
                      class="absolute right-0 bottom-full mb-2 z-20 w-52 overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-2xl"
                    >
                      <div class="p-1.5 space-y-0.5">
                        <router-link
                          v-if="canOpenCompletionDetails(b)"
                          :to="`/bookings/${b.id}/complete`"
                          class="flex items-center rounded-xl px-3 py-2.5 text-sm font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-800 transition"
                          @click="closeActionMenu"
                        >
                          {{
                            b.status === "completed"
                              ? "Детали завершения"
                              : "Статус завершения"
                          }}
                        </router-link>
                        <button
                          v-if="b.canLeaveComment"
                          @click="openReviewFromMenu(b)"
                          :disabled="reviewSubmittingId === b.id"
                          class="flex w-full items-center rounded-xl px-3 py-2.5 text-left text-sm font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-800 transition disabled:opacity-60"
                        >
                          {{
                            reviewSubmittingId === b.id
                              ? "Отправляем..."
                              : "Оставить отзыв"
                          }}
                        </button>
                        <button
                          v-if="canCancel(b)"
                          @click="cancelFromMenu(b)"
                          :disabled="cancelingId === b.id"
                          class="flex w-full items-center rounded-xl px-3 py-2.5 text-left text-sm font-medium text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition disabled:opacity-60"
                        >
                          {{
                            cancelingId === b.id
                              ? "Отмена..."
                              : "Отменить бронирование"
                          }}
                        </button>
                        <button
                          v-if="canFileComplaint(b)"
                          @click="openComplaintFromMenu(b)"
                          class="flex w-full items-center rounded-xl px-3 py-2.5 text-left text-sm font-medium text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-800 transition"
                        >
                          {{ bookingComplaintMap[b.id] ? 'Открыть обращение' : 'Подать жалобу' }}
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 pt-4">
          <button
            :disabled="currentPage <= 1"
            @click="currentPage--"
            class="px-4 py-2 rounded-xl text-sm font-bold border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:border-primary-500 dark:hover:border-primary-500 transition-colors disabled:opacity-40 disabled:pointer-events-none"
          >
            ←
          </button>

          <template v-for="p in paginationRange" :key="p">
            <span v-if="p === '...'" class="px-2 text-gray-400 text-sm select-none">...</span>
            <button
              v-else
              @click="currentPage = p as number"
              :class="[
                'w-10 h-10 rounded-xl text-sm font-bold transition-all',
                currentPage === p
                  ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/40'
                  : 'border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:border-primary-500 dark:hover:border-primary-500',
              ]"
            >
              {{ p }}
            </button>
          </template>

          <button
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
            class="px-4 py-2 rounded-xl text-sm font-bold border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:border-primary-500 dark:hover:border-primary-500 transition-colors disabled:opacity-40 disabled:pointer-events-none"
          >
            →
          </button>
        </div>

        <p class="text-center text-sm text-gray-400 dark:text-gray-500">
          {{ filteredBookings.length }} из {{ bookings.length }} бронирований
        </p>
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

    <CreateComplaintModal
      v-if="bookingToComplain"
      :is-open="showComplaintModal"
      :booking-id="bookingToComplain.id"
      :is-partner="isPartnerUser"
      @close="closeComplaintModal"
      @submit="handleComplaintSubmit"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed, watch } from "vue";
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
import CreateComplaintModal from "../components/CreateComplaintModal.vue";
import { getMyComplaintByBooking } from "../api/complaints";
import { auth } from "../store/auth";
import { useRouter } from "vue-router";

interface BookingWithComputedStatus extends Booking {
  computedStatus: ReturnType<typeof computeBookingStatus>;
}

const bookings = ref<BookingWithComputedStatus[]>([]);
const currentPage = ref(1);
const perPage = 10;
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
const openActionMenuId = ref<number | null>(null);
const bookingToComplain = ref<BookingWithComputedStatus | null>(null);
const showComplaintModal = ref(false);
const isPartnerUser = computed(() => auth.isActorType("partner"));
const router = useRouter();
const bookingComplaintMap = ref<Record<number, string>>({});
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

const totalPages = computed(() => Math.ceil(filteredBookings.value.length / perPage));

const paginatedBookings = computed(() => {
  const start = (currentPage.value - 1) * perPage;
  return filteredBookings.value.slice(start, start + perPage);
});

watch(currentFilter, () => {
  currentPage.value = 1;
});

const paginationRange = computed(() => {
  const total = totalPages.value;
  const current = currentPage.value;
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

  const pages: (number | string)[] = [1];
  const left = Math.max(2, current - 1);
  const right = Math.min(total - 1, current + 1);

  if (left > 2) pages.push("...");
  for (let i = left; i <= right; i++) pages.push(i);
  if (right < total - 1) pages.push("...");
  pages.push(total);

  return pages;
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

function toggleActionMenu(bookingId: number) {
  openActionMenuId.value =
    openActionMenuId.value === bookingId ? null : bookingId;
}

function closeActionMenu() {
  openActionMenuId.value = null;
}

function getPrimaryActionLabel(booking: BookingWithComputedStatus) {
  if (canPay(booking)) return "Оплатить";
  if (canStartTripAction(booking))
    return startingId.value === booking.id ? "Запускаем..." : "Начать поездку";
  if (canCompleteTripAction(booking)) return "Завершить поездку";
  if (canOpenCompletionDetails(booking))
    return booking.status === "completed"
      ? "Детали завершения"
      : "Статус завершения";
  if (booking.canLeaveComment)
    return reviewSubmittingId.value === booking.id
      ? "Отправляем..."
      : "Оставить отзыв";
  return "Открыть";
}

function getPrimaryActionButtonClass(booking: BookingWithComputedStatus) {
  if (canPay(booking)) return "bg-amber-600 hover:bg-amber-700";
  if (canStartTripAction(booking)) return "bg-emerald-600 hover:bg-emerald-700";
  if (canCompleteTripAction(booking))
    return "bg-primary-600 hover:bg-primary-700";
  if (canOpenCompletionDetails(booking))
    return "bg-violet-600 hover:bg-violet-700";
  if (booking.canLeaveComment) return "bg-sky-600 hover:bg-sky-700";
  return "bg-gray-900 hover:bg-gray-800 dark:bg-white dark:text-gray-900 dark:hover:bg-gray-200";
}

function getPrimaryActionProps(booking: BookingWithComputedStatus) {
  if (canPay(booking)) return { to: `/bookings/${booking.id}/payment` };
  if (canCompleteTripAction(booking) || canOpenCompletionDetails(booking))
    return { to: `/bookings/${booking.id}/complete` };
  return { type: "button" };
}

function getPrimaryAction(booking: BookingWithComputedStatus) {
  return {
    to:
      canPay(booking) ||
      canCompleteTripAction(booking) ||
      canOpenCompletionDetails(booking),
  };
}

function isPrimaryActionDisabled(booking: BookingWithComputedStatus) {
  if (canStartTripAction(booking)) return startingId.value === booking.id;
  if (booking.canLeaveComment) return reviewSubmittingId.value === booking.id;
  return false;
}

function handlePrimaryAction(
  booking: BookingWithComputedStatus,
  event?: Event,
) {
  if (
    canPay(booking) ||
    canCompleteTripAction(booking) ||
    canOpenCompletionDetails(booking)
  )
    return;
  event?.preventDefault();
  if (canStartTripAction(booking)) {
    void handleStartTrip(booking);
    return;
  }
  if (booking.canLeaveComment) {
    openReviewModal(booking);
  }
}

function openReviewFromMenu(booking: BookingWithComputedStatus) {
  closeActionMenu();
  openReviewModal(booking);
}

function cancelFromMenu(booking: BookingWithComputedStatus) {
  closeActionMenu();
  confirmCancel(booking);
}

function canFileComplaint(booking: BookingWithComputedStatus): boolean {
  const status = booking.status;
  if (
    status === "active" ||
    status === "awaitingReview" ||
    status === "completed"
  ) {
    return true;
  }
  if (status === "canceled" && booking.tripStartedAt) {
    return true;
  }
  return false;
}

async function openComplaintFromMenu(booking: BookingWithComputedStatus) {
  closeActionMenu();
  // Check if complaint already exists for this booking
  const existingId = bookingComplaintMap.value[booking.id];
  if (existingId) {
    router.push(`/complaints/${existingId}`);
    return;
  }
  // Double-check with API
  try {
    const existing = await getMyComplaintByBooking(booking.id);
    if (existing) {
      bookingComplaintMap.value[booking.id] = existing.id;
      router.push(`/complaints/${existing.id}`);
      return;
    }
  } catch { /* ignore */ }
  bookingToComplain.value = booking;
  showComplaintModal.value = true;
}

function closeComplaintModal() {
  showComplaintModal.value = false;
  setTimeout(() => {
    bookingToComplain.value = null;
  }, 300);
}

function handleComplaintSubmit() {
  closeComplaintModal();
  success("Жалоба успешно отправлена");
  router.push("/complaints");
}

onMounted(async () => {
  await loadBookings();
  // Prefetch complaint existence for bookings that can file complaints
  const eligible = bookings.value.filter(canFileComplaint);
  await Promise.allSettled(
    eligible.map(async (b) => {
      try {
        const existing = await getMyComplaintByBooking(b.id);
        if (existing) bookingComplaintMap.value[b.id] = existing.id;
      } catch { /* ignore */ }
    }),
  );
});

async function loadBookings() {
  try {
    const data = await getMyBookings({ page: 1, pageSize: 100 });

    const items = Array.isArray(data) ? data : data.items;

    bookings.value = items.map(toBookingWithComputedStatus);
    currentPage.value = 1;
  } catch (e) {
    console.error("Failed to load bookings", e);
    error("Не удалось загрузить бронирования");
    bookings.value = [];
  }
}

function toBookingWithComputedStatus(
  booking: Booking,
): BookingWithComputedStatus {
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

function formatMoney(
  amount: number | null | undefined,
  currency = "KZT",
): string {
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
  closeActionMenu();
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
  closeActionMenu();
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
  const index = bookings.value.findIndex(
    (item) => item.id === updatedBooking.id,
  );
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
