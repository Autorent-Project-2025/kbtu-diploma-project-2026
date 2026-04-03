<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        @click.self="closeModal"
      >
        <div
          class="relative w-full max-w-md bg-white dark:bg-gray-900 rounded-3xl shadow-2xl overflow-hidden transform transition-all"
          @click.stop
        >
          <div
            class="relative bg-gradient-to-br from-primary-600 to-primary-700 px-6 py-8 text-white"
          >
            <button
              @click="closeModal"
              class="absolute top-4 right-4 p-2 rounded-full hover:bg-white/20 transition-colors"
              aria-label="Закрыть"
            >
              <svg
                class="w-6 h-6"
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
            </button>

            <div class="space-y-2">
              <h2 class="text-2xl font-bold">Выберите даты</h2>
              <p class="text-primary-100 text-sm">
                {{ car.brand }} {{ car.model }}
              </p>
            </div>
          </div>

          <div class="p-6 space-y-6">
            <div class="space-y-2">
              <label
                for="start-date"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Дата начала
              </label>
              <div class="relative">
                <input
                  id="start-date"
                  v-model="startDate"
                  type="datetime-local"
                  :min="minDate"
                  class="w-full px-4 py-3 pr-12 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                  required
                />
              </div>
            </div>

            <div class="space-y-2">
              <label
                for="end-date"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Дата окончания
              </label>
              <div class="relative">
                <input
                  id="end-date"
                  v-model="endDate"
                  type="datetime-local"
                  :min="startDate || minDate"
                  class="w-full px-4 py-3 pr-12 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                  required
                />
              </div>
            </div>

            <div
              v-if="duration"
              class="flex items-center gap-3 p-4 bg-primary-50 dark:bg-primary-900/20 rounded-xl"
            >
              <div class="flex-1">
                <p class="text-sm font-semibold text-gray-900 dark:text-white">
                  Продолжительность аренды
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ duration.days > 0 ? `${duration.days} дн. ` : "" }}
                  {{ duration.hours }} ч.
                  {{ duration.minutes > 0 ? `${duration.minutes} мин.` : "" }}
                </p>
              </div>
            </div>

            <div
              v-if="mySubscription"
              class="rounded-2xl border border-emerald-200 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/30 p-4"
            >
              <div class="flex items-start justify-between gap-4">
                <div>
                  <p
                    class="text-xs uppercase tracking-[0.2em] text-emerald-600 dark:text-emerald-400 font-semibold"
                  >
                    Active subscription
                  </p>
                  <p
                    class="mt-1 text-lg font-bold text-gray-900 dark:text-white"
                  >
                    {{ mySubscription.planName }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-300">
                    Remaining bookings: {{ mySubscription.remainingBookings }}
                  </p>
                </div>
              </div>

              <label
                class="mt-4 flex items-center gap-3 cursor-pointer"
                :class="{
                  'opacity-50 cursor-not-allowed':
                    mySubscription.remainingBookings <= 0,
                }"
              >
                <input
                  v-model="useSubscription"
                  type="checkbox"
                  :disabled="mySubscription.remainingBookings <= 0"
                  class="rounded"
                />
                <span
                  class="text-sm font-medium text-gray-700 dark:text-gray-200"
                >
                  Use subscription for this booking
                </span>
              </label>
            </div>

            <div
              v-if="loadingPrice"
              class="mt-4 text-sm text-gray-500 dark:text-gray-400"
            >
              Рассчитываем стоимость...
            </div>

            <div
              v-if="pricePreview"
              class="mt-4 rounded-2xl border border-blue-200 dark:border-blue-800 p-4 bg-blue-50 dark:bg-slate-900"
            >
              <div class="flex items-center justify-between mb-2">
                <span
                  class="text-sm font-medium text-gray-600 dark:text-gray-300"
                >
                  Estimated total
                </span>
                <span
                class="text-xl font-bold text-blue-600 dark:text-blue-400"
                >
                  {{ useSubscription ? 0 : pricePreview.finalPrice }}
                  {{ pricePreview.currency }}
                </span>
              </div>

              <div
                v-if="!useSubscription"
                class="text-sm text-gray-600 dark:text-gray-400 space-y-1"
              >
                <p>
                  Рыночная стоимость: {{ pricePreview.marketValueKzt }}
                  {{ pricePreview.currency }}
                </p>
                <p>Ставка за час: {{ pricePreview.priceHour }} {{ pricePreview.currency }}</p>
                <p>Оплачиваемые часы: {{ pricePreview.billableHours }}</p>
                <p>Рейтинг: {{ pricePreview.rating }} (x{{ pricePreview.ratingCoefficient }})</p>
                <p>
                  Предварительное бронирование: {{ pricePreview.daysBeforeBooking }} дн. (x{{
                    pricePreview.advanceBookingCoefficient
                  }})
                </p>
                <p>Доступных машин этой модели: {{ pricePreview.currentAvailableCarsCount }}</p>
                <p>Коэффициент доступности: x{{ pricePreview.availabilityCoefficient }}</p>
                <p
                  v-if="pricePreview.isMarketValueStale"
                  class="pt-1 font-medium text-amber-700 dark:text-amber-300"
                >
                  Рыночная стоимость устарела, цена рассчитана по последнему доступному снапшоту.
                </p>
              </div>

              <div
                v-else
                class="text-sm text-emerald-700 dark:text-emerald-300 font-medium"
              >
                Это бронирование будет оформлено по активной подписке.
              </div>
            </div>

            <div
              v-if="displayError"
              class="flex items-center gap-3 p-4 bg-red-50 dark:bg-red-900/20 rounded-xl"
            >
              <p class="text-sm text-red-600 dark:text-red-400">
                {{ displayError }}
              </p>
            </div>
          </div>

          <div
            class="flex gap-3 px-6 py-4 bg-gray-50 dark:bg-gray-800/50 border-t border-gray-200 dark:border-gray-700"
          >
            <button
              @click="closeModal"
              class="flex-1 px-6 py-3 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-semibold rounded-xl border-2 border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700 transition-all active:scale-95"
            >
              Отмена
            </button>
            <button
              @click="confirmBooking"
              :disabled="!isValid || isLoading"
              class="flex-1 px-6 py-3 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed disabled:hover:shadow-none"
            >
              <span v-if="!isLoading">Забронировать</span>
              <span v-else>Загрузка...</span>
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import axios from "axios";
import type { Car } from "../types/Car";

interface Props {
  isOpen: boolean;
  car: Car;
  bookingError?: string;
  suggestedDates?: string[];
}

interface Emits {
  (e: "close"): void;
  (
    e: "confirm",
    payload: { startDate: string; endDate: string; useSubscription: boolean },
  ): void;
}

type PricePreview = {
  partnerCarId: number;
  marketValueKzt: number;
  rating: number;
  currentAvailableCarsCount: number;
  daysBeforeBooking: number;
  billableHours: number;
  ratingCoefficient: number;
  advanceBookingCoefficient: number;
  availabilityCoefficient: number;
  priceHour: number;
  finalPrice: number;
  currency: string;
  isMarketValueStale: boolean;
};

type MySubscription = {
  id: number;
  subscriptionPlanId: number;
  planName: string;
  status: string;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  includedBookings: number;
  usedBookings: number;
  remainingBookings: number;
};

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const isLoading = ref(false);
const startDate = ref("");
const endDate = ref("");
const validationError = ref("");
const pricePreview = ref<PricePreview | null>(null);
const loadingPrice = ref(false);
const mySubscription = ref<MySubscription | null>(null);
const useSubscription = ref(false);

const minDate = computed(() => {
  const now = new Date();
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
  return now.toISOString().slice(0, 16);
});

watch(
  () => props.isOpen,
  async (isOpen) => {
    if (isOpen) {
      const now = new Date();
      now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
      startDate.value = now.toISOString().slice(0, 16);

      const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
      endDate.value = tomorrow.toISOString().slice(0, 16);

      validationError.value = "";
      useSubscription.value = false;

      await Promise.all([fetchPricePreview(), loadMySubscription()]);
    } else {
      pricePreview.value = null;
      loadingPrice.value = false;
      mySubscription.value = null;
      useSubscription.value = false;
    }
  },
);

watch([startDate, endDate], () => {
  if (!isValid.value) {
    pricePreview.value = null;
    return;
  }

  fetchPricePreview();
});

const displayError = computed(() => {
  return props.bookingError?.trim() || validationError.value;
});

async function loadMySubscription() {
  try {
    const { data } = await axios.get("/subscriptions/my");
    mySubscription.value = data;
  } catch {
    mySubscription.value = null;
  }
}

async function fetchPricePreview() {
  if (!startDate.value || !endDate.value || !props.car?.id) {
    pricePreview.value = null;
    return;
  }

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);

  if (
    Number.isNaN(start.getTime()) ||
    Number.isNaN(end.getTime()) ||
    end <= start
  ) {
    pricePreview.value = null;
    return;
  }

  try {
    loadingPrice.value = true;

    const { data } = await axios.get("/bookings/price-preview", {
      params: {
        partnerCarId: props.car.id,
        startTime: start.toISOString(),
        endTime: end.toISOString(),
      },
    });

    pricePreview.value = data;
  } catch (error) {
    console.error("Failed to fetch price preview:", error);
    pricePreview.value = null;
  } finally {
    loadingPrice.value = false;
  }
}

const duration = computed(() => {
  if (!startDate.value || !endDate.value) return null;

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const diffMs = end.getTime() - start.getTime();

  if (diffMs <= 0) return null;

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  return { days, hours, minutes, totalMinutes };
});

const isValid = computed(() => {
  if (!startDate.value || !endDate.value) {
    validationError.value = "Заполните обе даты";
    return false;
  }

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const now = new Date();

  if (start < now) {
    validationError.value = "Дата начала не может быть в прошлом";
    return false;
  }

  if (end <= start) {
    validationError.value = "Дата окончания должна быть позже даты начала";
    return false;
  }

  const diffMs = end.getTime() - start.getTime();
  const minDuration = 60 * 60 * 1000;

  if (diffMs < minDuration) {
    validationError.value = "Минимальная продолжительность аренды - 1 час";
    return false;
  }

  validationError.value = "";
  return true;
});

function closeModal() {
  emit("close");
}

function confirmBooking() {
  if (!isValid.value) return;

  emit("confirm", {
    startDate: new Date(startDate.value).toISOString(),
    endDate: new Date(endDate.value).toISOString(),
    useSubscription: useSubscription.value,
  });
}
</script>
