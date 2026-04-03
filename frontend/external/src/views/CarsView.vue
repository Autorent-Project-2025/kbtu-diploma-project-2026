<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto space-y-8">
      <header class="space-y-3 animate-slide-up">
        <h1
          class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
        >
          Доступные модели
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400 max-w-3xl">
          Выберите модель, посмотрите детали и забронируйте. Система сама
          подберет подходящую машину партнера на выбранные даты.
        </p>
      </header>

      <SmartRecommendationPanel class="mb-10" />

      <section
        class="relative overflow-hidden rounded-[32px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.14),_transparent_30%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.14),_transparent_35%),linear-gradient(135deg,_rgba(255,255,255,0.98),_rgba(243,244,246,0.96))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_30%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.18),_transparent_35%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-6 sm:p-8"
      >
        <div
          class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6"
        >
          <div class="space-y-3 max-w-2xl">
            <p
              class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
            >
              Subscription advantage
            </p>

            <h2
              class="text-3xl sm:text-4xl font-extrabold text-gray-900 dark:text-white"
            >
              Rent often? A subscription can save you money
            </h2>

            <p
              class="text-base text-gray-600 dark:text-gray-400 leading-relaxed"
            >
              Choose a recurring plan, get included bookings, and skip the usual
              payment flow for every trip.
            </p>

            <div class="flex flex-wrap gap-3 pt-2">
              <div
                class="px-4 py-2 rounded-2xl bg-white/80 dark:bg-gray-900/80 border border-gray-200 dark:border-gray-800 text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Included bookings
              </div>
              <div
                class="px-4 py-2 rounded-2xl bg-white/80 dark:bg-gray-900/80 border border-gray-200 dark:border-gray-800 text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Faster booking flow
              </div>
              <div
                class="px-4 py-2 rounded-2xl bg-white/80 dark:bg-gray-900/80 border border-gray-200 dark:border-gray-800 text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Better for frequent users
              </div>
            </div>
          </div>

          <div class="flex flex-col sm:flex-row gap-3 shrink-0">
            <router-link
              to="/subscriptions"
              class="px-6 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors text-center"
            >
              View plans
            </router-link>

            <router-link
              to="/car-recommendations"
              class="px-6 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-200 font-semibold hover:border-emerald-500 dark:hover:border-emerald-500 transition-colors text-center"
            >
              Smart recommendations
            </router-link>
          </div>
        </div>
      </section>

      <section
        class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800"
      >
        <div class="flex flex-wrap items-center gap-4">
          <div class="flex items-center gap-2">
            <label
              class="text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Сортировка:</label
            >
            <select
              v-model="sortType"
              class="px-4 py-3 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white font-medium focus:border-primary-500 focus:outline-none transition-colors min-w-[220px]"
            >
              <option value="popular">Популярные</option>
              <option value="price_asc">Сначала дешевые</option>
              <option value="price_desc">Сначала дорогие</option>
              <option value="available">Больше доступных машин</option>
              <option value="newest">Новее модель</option>
            </select>
          </div>

          <button
            @click="loadModelCards"
            :disabled="matching"
            class="px-4 py-3 rounded-xl border-2 border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-200 font-semibold hover:border-primary-500"
          >
            Обновить
          </button>

          <div
            class="ml-auto text-sm text-gray-600 dark:text-gray-400 font-medium"
          >
            Моделей доступно:
            <span class="font-bold text-gray-900 dark:text-white">{{
              sortedModels.length
            }}</span>
          </div>
        </div>
      </section>

      <div v-if="loading" class="text-center py-28">
        <div class="inline-flex flex-col items-center gap-6">
          <div
            class="w-16 h-16 rounded-full border-4 border-primary-200 dark:border-primary-900 border-t-primary-600 dark:border-t-primary-400 animate-spin"
          ></div>
          <p class="text-gray-600 dark:text-gray-400 text-lg font-medium">
            Загрузка доступных моделей...
          </p>
        </div>
      </div>

      <div v-else-if="sortedModels.length === 0" class="text-center py-28">
        <div class="inline-flex flex-col items-center gap-5 max-w-md mx-auto">
          <div
            class="w-20 h-20 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
          >
            <svg
              class="w-10 h-10 text-gray-400"
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
          <p class="text-xl font-bold text-gray-900 dark:text-white">
            Сейчас нет доступных моделей
          </p>
        </div>
      </div>

      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
        <article
          v-for="model in sortedModels"
          :key="model.modelId"
          class="group relative bg-white dark:bg-gray-900 rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all duration-500 border border-gray-200 dark:border-gray-800 flex flex-col"
        >
          <div
            class="relative h-64 overflow-hidden bg-gradient-to-br from-gray-100 to-gray-200 dark:from-gray-800 dark:to-gray-900"
          >
            <img
              :src="model.imageUrl || config.app.defaultCarImage"
              :alt="`${model.brand} ${model.model}`"
              class="w-full h-full object-cover transform group-hover:scale-110 transition-transform duration-700"
            />
            <div
              class="absolute top-4 left-4 glass px-3 py-1.5 rounded-full backdrop-blur-md"
            >
              <span class="text-sm font-semibold text-white">{{
                model.year
              }}</span>
            </div>
            <div
              class="absolute top-4 right-4 glass px-3 py-1.5 rounded-full backdrop-blur-md"
            >
              <span class="text-sm font-semibold text-white"
                >{{ model.availableCarsCount }} авто</span
              >
            </div>
          </div>

          <div class="p-6 flex flex-col gap-4 flex-1">
            <div class="space-y-1">
              <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                {{ model.brand }} {{ model.model }}
              </h2>
              <p class="text-sm text-gray-600 dark:text-gray-400 line-clamp-2">
                {{ model.description || "Описание модели пока не добавлено." }}
              </p>
            </div>

            <div class="flex items-center justify-between text-sm">
              <span class="text-gray-600 dark:text-gray-400">Цена:</span>
              <span class="font-bold text-primary-600 dark:text-primary-400">{{
                formatPriceRange(model)
              }}</span>
            </div>

            <div class="flex items-center justify-between text-sm">
              <span class="text-gray-600 dark:text-gray-400">Рейтинг:</span>
              <span class="font-semibold text-gray-900 dark:text-white">
                {{ model.averageRating ?? "нет" }}
              </span>
            </div>

            <div class="mt-auto grid grid-cols-2 gap-3">
              <router-link
                :to="`/cars/${model.modelId}`"
                class="px-4 py-3 rounded-xl bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-gray-900 dark:text-white font-semibold text-center"
              >
                Подробнее
              </router-link>
              <button
                @click="openBookingModal(model)"
                :disabled="matching"
                class="px-4 py-3 rounded-xl bg-primary-600 hover:bg-primary-700 text-white font-semibold"
              >
                Забронировать
              </button>
            </div>
          </div>
        </article>
      </div>
    </div>

    <BookingModal
      v-if="selectedModelAsCar"
      :is-open="isModalOpen"
      :car="selectedModelAsCar"
      :booking-error="bookingError"
      :suggested-dates="bookingSuggestions"
      @close="closeBookingModal"
      @confirm="handleBookingConfirm"
      @suggestion-click="handleSuggestionClick"
    />

    <LoginRequiredModal
      :is-open="showLoginModal"
      @close="showLoginModal = false"
      @login="goToLogin"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { createBooking } from "../api/booking";
import {
  getAvailableModelCards,
  matchCarByModel,
  type AvailableModelCard,
} from "../api/cars";
import BookingModal from "../components/BookingModal.vue";
import LoginRequiredModal from "../components/LoginRequiredModal.vue";
import { useAuth } from "../composables/useAuth";
import { useToast } from "../composables/useToast";
import { config } from "../config";
import type { Car } from "../types/Car";
import { formatMoney } from "../utils/formatMoney";
import SmartRecommendationPanel from "../components/SmartRecommendationPanel.vue";

const router = useRouter();
const { isAuthenticated } = useAuth();
const { success, error } = useToast();

const loading = ref(true);
const matching = ref(false);
const sortType = ref<
  "popular" | "price_asc" | "price_desc" | "available" | "newest"
>("popular");

const models = ref<AvailableModelCard[]>([]);
const selectedModel = ref<AvailableModelCard | null>(null);
const isModalOpen = ref(false);
const showLoginModal = ref(false);
const bookingError = ref("");
const bookingSuggestions = ref<string[]>([]);

const sortedModels = computed(() => {
  return [...models.value].sort((left, right) => {
    if (sortType.value === "newest") {
      return right.year - left.year;
    }

    if (sortType.value === "available") {
      return right.availableCarsCount - left.availableCarsCount;
    }

    if (sortType.value === "price_asc") {
      return (
        (left.minPriceHour ?? Number.MAX_SAFE_INTEGER) -
        (right.minPriceHour ?? Number.MAX_SAFE_INTEGER)
      );
    }

    if (sortType.value === "price_desc") {
      return (right.maxPriceHour ?? 0) - (left.maxPriceHour ?? 0);
    }

    return (right.averageRating ?? 0) - (left.averageRating ?? 0);
  });
});

const selectedModelAsCar = computed<Car | null>(() => {
  if (!selectedModel.value) {
    return null;
  }

  return {
    id: selectedModel.value.modelId,
    brand: selectedModel.value.brand,
    model: selectedModel.value.model,
    year: selectedModel.value.year,
    priceHour: selectedModel.value.minPriceHour ?? null,
    priceDay: null,
    imageUrl: selectedModel.value.imageUrl,
    rating: selectedModel.value.averageRating ?? null,
    description: selectedModel.value.description ?? null,
  };
});

onMounted(async () => {
  await loadModelCards();
});

async function loadModelCards() {
  loading.value = true;
  try {
    models.value = await getAvailableModelCards();
  } catch (e) {
    console.error("Ошибка загрузки моделей:", e);
    error("Не удалось загрузить доступные модели.");
  } finally {
    loading.value = false;
  }
}

function formatPriceRange(model: AvailableModelCard): string {
  if (model.minPriceHour == null && model.maxPriceHour == null) {
    return "по запросу";
  }

  if (
    model.minPriceHour != null &&
    model.maxPriceHour != null &&
    model.minPriceHour !== model.maxPriceHour
  ) {
    return `${formatMoney(model.minPriceHour)} - ${formatMoney(model.maxPriceHour)}/час`;
  }

  const singlePrice = model.minPriceHour ?? model.maxPriceHour;
  return singlePrice != null ? `${formatMoney(singlePrice)}/час` : "по запросу";
}

function openBookingModal(model: AvailableModelCard) {
  if (!isAuthenticated.value) {
    showLoginModal.value = true;
    return;
  }

  selectedModel.value = model;
  bookingError.value = "";
  bookingSuggestions.value = [];
  isModalOpen.value = true;
}

function closeBookingModal() {
  isModalOpen.value = false;
  bookingError.value = "";
  bookingSuggestions.value = [];
  setTimeout(() => {
    selectedModel.value = null;
  }, 200);
}

function goToLogin() {
  showLoginModal.value = false;
  router.push("/login");
}

function handleSuggestionClick() {
  bookingError.value = "";
}

async function handleBookingConfirm(payloadData: {
  startDate: string;
  endDate: string;
  useSubscription: boolean;
}) {
  if (!selectedModel.value) {
    return;
  }

  matching.value = true;
  bookingError.value = "";
  bookingSuggestions.value = [];

  try {
    const matchResult = await matchCarByModel({
      modelId: selectedModel.value.modelId,
      startTime: payloadData.startDate,
      endTime: payloadData.endDate,
    });

    if (!matchResult.isAvailable || !matchResult.partnerCarId) {
      bookingError.value = "На выбранные даты машин этой модели нет.";
      bookingSuggestions.value = (
        matchResult.suggestedStartTimesUtc ?? []
      ).slice(0, 5);
      return;
    }

    const booking = await createBooking(
      matchResult.partnerCarId,
      payloadData.startDate,
      payloadData.endDate,
      payloadData.useSubscription,
    );

    success(
      payloadData.useSubscription
        ? `${selectedModel.value.brand} ${selectedModel.value.model}: бронь создана по подписке.`
        : `${selectedModel.value.brand} ${selectedModel.value.model}: бронь создана, завершите оплату.`,
    );
    closeBookingModal();
    if (payloadData.useSubscription) {
      await router.push("/bookings");
    } else {
      await router.push(`/bookings/${booking.id}/payment`);
    }
  } catch (e) {
    console.error("Ошибка автоподбора и бронирования:", e);
    bookingError.value = "Не удалось забронировать машину. Попробуйте снова.";
    error("Не удалось забронировать машину.");
  } finally {
    matching.value = false;
  }
}
</script>
