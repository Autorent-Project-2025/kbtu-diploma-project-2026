<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-7xl mx-auto space-y-8">
      <button
        @click="$router.back()"
        class="flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        <span class="font-medium">Назад к моделям</span>
      </button>

      <div v-if="loading" class="text-center py-28">
        <div class="inline-flex flex-col items-center gap-6">
          <div class="w-16 h-16 rounded-full border-4 border-primary-200 dark:border-primary-900 border-t-primary-600 dark:border-t-primary-400 animate-spin"></div>
          <p class="text-gray-600 dark:text-gray-400 text-lg font-medium">Загрузка информации о модели...</p>
        </div>
      </div>

      <template v-else-if="payload">
        <section class="grid lg:grid-cols-2 gap-8">
          <div class="space-y-4">
            <div class="relative h-96 rounded-3xl overflow-hidden bg-gradient-to-br from-gray-200 to-gray-300 dark:from-gray-800 dark:to-gray-900 shadow-2xl">
              <img
                v-if="currentImage"
                :src="currentImage"
                :alt="`${payload.model.brand} ${payload.model.model}`"
                class="w-full h-full object-cover"
              />
              <div v-else class="w-full h-full flex items-center justify-center text-gray-500 dark:text-gray-400">
                Нет изображения
              </div>
            </div>

            <div v-if="modelImages.length > 1" class="grid grid-cols-5 gap-3">
              <button
                v-for="(img, index) in modelImages.slice(0, 5)"
                :key="img.id"
                @click="currentImageIndex = index"
                :class="[
                  'relative h-20 rounded-xl overflow-hidden transition-all',
                  currentImageIndex === index
                    ? 'ring-4 ring-primary-500 scale-105'
                    : 'ring-2 ring-gray-200 dark:ring-gray-700 hover:ring-primary-300',
                ]"
              >
                <img :src="img.imageUrl" :alt="`${payload.model.brand} ${payload.model.model}`" class="w-full h-full object-cover" />
              </button>
            </div>
          </div>

          <div class="space-y-6">
            <div class="space-y-2">
              <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
                {{ payload.model.brand }} {{ payload.model.model }}
              </h1>
              <p class="text-lg text-gray-600 dark:text-gray-400">{{ payload.model.year }} год выпуска</p>
            </div>

            <div class="grid sm:grid-cols-2 gap-4">
              <article class="p-4 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
                <p class="text-sm text-gray-500 dark:text-gray-400">Доступно машин</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ payload.availability?.availableCarsCount ?? 0 }}</p>
              </article>
              <article class="p-4 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
                <p class="text-sm text-gray-500 dark:text-gray-400">Цена</p>
                <p class="text-2xl font-bold text-primary-600 dark:text-primary-400">{{ formatPriceRange() }}</p>
              </article>
              <article class="p-4 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
                <p class="text-sm text-gray-500 dark:text-gray-400">Рейтинг модели</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ payload.availability?.averageRating ?? payload.model.rating ?? "нет" }}</p>
              </article>
              <article class="p-4 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
                <p class="text-sm text-gray-500 dark:text-gray-400">Отзывы</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">{{ payload.reviews.length }}</p>
              </article>
            </div>

            <div class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
              <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-3">Описание</h2>
              <p class="text-gray-700 dark:text-gray-300">{{ payload.model.description || "Описание отсутствует." }}</p>
            </div>

            <div class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800">
              <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-3">Характеристики</h2>
              <div class="grid grid-cols-2 gap-3 text-sm">
                <div class="text-gray-600 dark:text-gray-400">Двигатель: <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.engine || "—" }}</span></div>
                <div class="text-gray-600 dark:text-gray-400">Трансмиссия: <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.transmission || "—" }}</span></div>
                <div class="text-gray-600 dark:text-gray-400">Мест: <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.seats ?? "—" }}</span></div>
                <div class="text-gray-600 dark:text-gray-400">Топливо: <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.fuelType || "—" }}</span></div>
                <div class="text-gray-600 dark:text-gray-400">Дверей: <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.doors ?? "—" }}</span></div>
              </div>
            </div>

            <button
              @click="openBookingModal"
              class="w-full btn-premium py-5 text-lg"
            >
              Забронировать
            </button>
          </div>
        </section>

        <section class="p-8 bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800">
          <h2 class="text-3xl font-bold text-gray-900 dark:text-white mb-6">Отзывы</h2>

          <div v-if="payload.reviews.length > 0" class="space-y-5">
            <article
              v-for="review in payload.reviews"
              :key="`${review.id}-${review.partnerCarId ?? 0}`"
              class="p-6 bg-gray-50 dark:bg-gray-800 rounded-2xl space-y-3"
            >
              <div class="flex items-start justify-between gap-4">
                <div class="space-y-1">
                  <p class="font-semibold text-gray-900 dark:text-white">{{ review.userName }}</p>
                  <p class="text-sm text-gray-500 dark:text-gray-400">{{ formatDate(review.createdOn) }}</p>
                  <p class="text-sm text-primary-700 dark:text-primary-300">
                    Перевозчик: {{ review.carrierName }}
                  </p>
                </div>

                <div class="flex items-center gap-1">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    :class="[
                      'w-5 h-5',
                      i <= review.rating ? 'text-yellow-400 fill-current' : 'text-gray-300 dark:text-gray-600',
                    ]"
                    viewBox="0 0 20 20"
                  >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                </div>
              </div>

              <p class="text-gray-700 dark:text-gray-300">{{ review.content }}</p>
            </article>
          </div>

          <div v-else class="py-16 text-center">
            <p class="text-xl font-bold text-gray-900 dark:text-white">Пока нет отзывов</p>
            <p class="text-gray-600 dark:text-gray-400 mt-2">Для этой модели еще не оставляли отзывы.</p>
          </div>
        </section>
      </template>

      <div v-else class="text-center py-28">
        <p class="text-2xl font-bold text-gray-900 dark:text-white">Модель не найдена</p>
      </div>
    </div>

    <BookingModal
      v-if="modalCar"
      :is-open="isBookingModalOpen"
      :car="modalCar"
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
import { useRoute, useRouter } from "vue-router";
import BookingModal from "../components/BookingModal.vue";
import LoginRequiredModal from "../components/LoginRequiredModal.vue";
import { createBooking } from "../api/booking";
import {
  getModelDetailsPayload,
  matchCarByModel,
  type ModelDetailsPayload,
} from "../api/cars";
import { useAuth } from "../composables/useAuth";
import { useToast } from "../composables/useToast";
import type { Car } from "../types/Car";

const route = useRoute();
const router = useRouter();
const { isAuthenticated } = useAuth();
const { success, error } = useToast();

const loading = ref(true);
const matching = ref(false);
const payload = ref<ModelDetailsPayload | null>(null);
const currentImageIndex = ref(0);

const isBookingModalOpen = ref(false);
const showLoginModal = ref(false);
const bookingError = ref("");
const bookingSuggestions = ref<string[]>([]);

const modelImages = computed(() => payload.value?.model.images ?? []);
const currentImage = computed(() => modelImages.value[currentImageIndex.value]?.imageUrl ?? "");

const modalCar = computed<Car | null>(() => {
  if (!payload.value) {
    return null;
  }

  return {
    id: payload.value.model.id,
    brand: payload.value.model.brand,
    model: payload.value.model.model,
    year: payload.value.model.year,
    priceHour: payload.value.minPriceHour,
    priceDay: null,
    imageUrl: modelImages.value[0]?.imageUrl ?? null,
    rating: payload.value.availability?.averageRating ?? payload.value.model.rating ?? null,
    description: payload.value.model.description ?? null,
  };
});

onMounted(async () => {
  await loadModelDetails();
});

async function loadModelDetails() {
  loading.value = true;
  try {
    const modelId = Number(route.params.id);
    if (Number.isNaN(modelId) || modelId <= 0) {
      payload.value = null;
      return;
    }

    payload.value = await getModelDetailsPayload(modelId);
    currentImageIndex.value = 0;
  } catch (e) {
    console.error("Ошибка загрузки деталей модели:", e);
    payload.value = null;
    error("Не удалось загрузить детали модели.");
  } finally {
    loading.value = false;
  }
}

function formatPriceRange() {
  if (!payload.value) {
    return "по запросу";
  }

  const min = payload.value.minPriceHour;
  const max = payload.value.maxPriceHour;

  if (min == null && max == null) {
    return "по запросу";
  }

  if (min != null && max != null && min !== max) {
    return `$${min} - $${max}/час`;
  }

  const singlePrice = min ?? max;
  return singlePrice != null ? `$${singlePrice}/час` : "по запросу";
}

function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function openBookingModal() {
  if (!isAuthenticated.value) {
    showLoginModal.value = true;
    return;
  }

  bookingError.value = "";
  bookingSuggestions.value = [];
  isBookingModalOpen.value = true;
}

function closeBookingModal() {
  isBookingModalOpen.value = false;
  bookingError.value = "";
  bookingSuggestions.value = [];
}

function handleSuggestionClick() {
  bookingError.value = "";
}

function goToLogin() {
  showLoginModal.value = false;
  router.push("/login");
}

async function handleBookingConfirm(startDate: string, endDate: string) {
  if (!payload.value) {
    return;
  }

  matching.value = true;
  bookingError.value = "";
  bookingSuggestions.value = [];

  try {
    const matchResult = await matchCarByModel({
      modelId: payload.value.model.id,
      startTime: startDate,
      endTime: endDate,
    });

    if (!matchResult.isAvailable || !matchResult.partnerCarId) {
      bookingError.value = "На выбранные даты машин этой модели нет.";
      bookingSuggestions.value = (matchResult.suggestedStartTimesUtc ?? []).slice(0, 5);
      return;
    }

    await createBooking(matchResult.partnerCarId, startDate, endDate, {
      partnerId: matchResult.partnerId ?? undefined,
      priceHour: matchResult.priceHour ?? null,
    });

    success(`${payload.value.model.brand} ${payload.value.model.model} успешно забронирована.`);
    closeBookingModal();
    await loadModelDetails();
  } catch (e) {
    console.error("Ошибка бронирования:", e);
    bookingError.value = "Не удалось забронировать машину. Попробуйте снова.";
    error("Не удалось забронировать машину.");
  } finally {
    matching.value = false;
  }
}
</script>
