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

                  <div class="space-y-2 flex-1">
                    <h3
                      class="text-2xl font-bold text-gray-900 dark:text-white"
                    >
                      {{ b.carBrand }} {{ b.carModel }}
                    </h3>

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
                    <div v-if="b.price" class="flex items-baseline gap-2">
                      <span
                        class="text-3xl font-bold text-gray-900 dark:text-white"
                        >${{ b.price }}</span
                      >
                      <span class="text-sm text-gray-500 dark:text-gray-400"
                        >общая стоимость</span
                      >
                    </div>
                  </div>
                </div>
              </div>

              <!-- Right: Status Badge & Actions -->
              <div class="flex flex-col gap-3 flex-shrink-0">
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
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { getMyBookings, cancelBooking } from "../api/booking";
import type { Booking } from "../types/Booking";
import {
  computeBookingStatus,
  canCancelBooking,
  formatBookingDate,
  getBookingDuration,
} from "../utils/bookingUtils";
import { useToast } from "../composables/useToast";
import CancelBookingModal from "../components/CancelBookingModal.vue";

interface BookingWithComputedStatus extends Booking {
  computedStatus: ReturnType<typeof computeBookingStatus>;
}

const bookings = ref<BookingWithComputedStatus[]>([]);
const currentFilter = ref<
  "all" | "paymentPending" | "upcoming" | "active" | "completed" | "canceled"
>("all");
const cancelingId = ref<number | null>(null);
const bookingToCancel = ref<BookingWithComputedStatus | null>(null);
const showCancelModal = ref(false);
const { success, error } = useToast();

// Filters configuration
const filters = computed(() => {
  const all = bookings.value.length;
  const paymentPending = bookings.value.filter(
    (b) => b.computedStatus === "paymentPending"
  ).length;
  const upcoming = bookings.value.filter(
    (b) => b.computedStatus === "upcoming"
  ).length;
  const active = bookings.value.filter(
    (b) => b.computedStatus === "active"
  ).length;
  const completed = bookings.value.filter(
    (b) => b.computedStatus === "completed"
  ).length;
  const canceled = bookings.value.filter(
    (b) => b.computedStatus === "canceled"
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

onMounted(async () => {
  await loadBookings();
});

async function loadBookings() {
  try {
    const data = await getMyBookings();

    // Обрабатываем оба формата ответа
    const items = Array.isArray(data) ? data : data.items;

    bookings.value = items.map((b: Booking) => ({
      ...b,
      computedStatus: computeBookingStatus(b),
    }));
  } catch (e) {
    console.error("Failed to load bookings", e);
    error("Не удалось загрузить бронирования");
    bookings.value = []; // Очищаем список при ошибке
  }
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

function canCancel(booking: BookingWithComputedStatus): boolean {
  return canCancelBooking(booking);
}

function canPay(booking: BookingWithComputedStatus): boolean {
  return booking.computedStatus === "paymentPending";
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
    error("Не удалось отменить бронирование");
  } finally {
    cancelingId.value = null;
  }
}

function getStatusClass(status: ReturnType<typeof computeBookingStatus>) {
  switch (status) {
    case "paymentPending":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case "upcoming":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case "active":
      return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300";
    case "completed":
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
    case "canceled":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
}

function getStatusIndicatorClass(
  status: ReturnType<typeof computeBookingStatus>
) {
  switch (status) {
    case "paymentPending":
      return "bg-amber-500";
    case "upcoming":
      return "bg-blue-500";
    case "active":
      return "bg-green-500";
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
