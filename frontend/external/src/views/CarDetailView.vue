<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto">
      <!-- Back Button -->
      <button
        @click="$router.back()"
        class="mb-6 flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
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
            d="M10 19l-7-7m0 0l7-7m-7 7h18"
          />
        </svg>
        <span class="font-medium">Назад к автомобилям</span>
      </button>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6">
          <div class="relative">
            <div
              class="w-16 h-16 rounded-full border-4 border-primary-200 dark:border-primary-900 border-t-primary-600 dark:border-t-primary-400 animate-spin"
            ></div>
          </div>
          <p class="text-gray-600 dark:text-gray-400 text-lg font-medium">
            Загрузка информации...
          </p>
        </div>
      </div>

      <!-- Car Details -->
      <div v-else-if="car" class="space-y-8">
        <!-- Main Info Grid -->
        <div class="grid lg:grid-cols-2 gap-8">
          <!-- Left Column -->
          <div class="flex flex-col gap-6">
            <!-- Images Gallery -->
            <div class="space-y-4 flex-shrink-0">
              <!-- Main Image -->
              <div
                class="relative h-96 rounded-3xl overflow-hidden bg-gradient-to-br from-gray-200 to-gray-300 dark:from-gray-800 dark:to-gray-900 shadow-2xl"
              >
                <img
                  v-if="currentImage"
                  :src="currentImage"
                  :alt="`${car.brand} ${car.model}`"
                  class="car-detail-main-image w-full h-full object-cover"
                />
                <div
                  v-else
                  class="w-full h-full flex items-center justify-center text-gray-500 dark:text-gray-400"
                >
                  Нет изображения
                </div>

                <!-- Image Navigation Dots -->
                <div
                  v-if="carImages.length > 1"
                  class="absolute bottom-4 left-1/2 -translate-x-1/2 flex gap-2"
                >
                  <button
                    v-for="(img, index) in carImages"
                    :key="index"
                    @click="currentImageIndex = index"
                    :class="[
                      'transition-all',
                      currentImageIndex === index
                        ? 'w-8 h-3 bg-white rounded-full'
                        : 'w-3 h-3 bg-white/50 hover:bg-white/75 rounded-full',
                    ]"
                  ></button>
                </div>
              </div>

              <!-- Thumbnail Gallery -->
              <div v-if="carImages.length > 1" class="grid grid-cols-4 gap-3">
                <button
                  v-for="(img, index) in carImages.slice(0, 4)"
                  :key="index"
                  @click="currentImageIndex = index"
                  :class="[
                    'relative h-24 rounded-xl overflow-hidden transition-all',
                    currentImageIndex === index
                      ? 'ring-4 ring-primary-500 scale-105'
                      : 'ring-2 ring-gray-200 dark:ring-gray-700 hover:ring-primary-300',
                  ]"
                >
                  <img
                    :src="img"
                    :alt="`${car.brand} ${car.model} - фото ${index + 1}`"
                    class="w-full h-full object-cover"
                  />
                </button>
              </div>
            </div>

            <!-- Description -->
            <div
              class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 flex flex-col flex-1"
            >
              <h2 class="text-xl font-bold text-gray-900 dark:text-white mb-3">
                Описание
              </h2>
              <p class="text-gray-700 dark:text-gray-300 leading-relaxed">
                {{ car.description || "Нет описания доступно." }}
              </p>
            </div>
          </div>

          <!-- Right Column -->
          <div class="flex flex-col gap-6 h-full">
            <!-- Top Content -->
            <div class="space-y-6">
              <!-- Title, Rating & Price -->
              <div class="space-y-4">
                <div class="flex items-start justify-between gap-4">
                  <div>
                    <h1
                      class="text-4xl font-extrabold text-gray-900 dark:text-white"
                    >
                      {{ car.brand }} {{ car.model }}
                    </h1>
                    <p class="text-lg text-gray-600 dark:text-gray-400 mt-1">
                      {{ car.year }} год выпуска
                    </p>
                  </div>

                  <!-- Rating Badge (звездочка с цифрой) -->
                  <div
                    v-if="car.rating !== null && car.rating !== undefined"
                    class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-full font-bold text-sm shadow-lg bg-yellow-500 text-white"
                  >
                    <svg class="w-4 h-4 fill-current" viewBox="0 0 20 20">
                      <path
                        d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                      />
                    </svg>
                    <span>{{ car.rating.toFixed(1) }}</span>
                  </div>
                </div>

                <!-- Comments Count -->
                <p
                  v-if="totalComments > 0"
                  class="text-sm text-gray-500 dark:text-gray-400"
                >
                  {{ totalComments }} {{ getCommentsWord(totalComments) }}
                </p>

                <!-- Price -->
                <div
                  class="flex items-baseline gap-3 p-4 bg-primary-50 dark:bg-primary-900/20 rounded-xl"
                >
                  <span
                    class="text-4xl font-extrabold text-primary-600 dark:text-primary-400"
                  >
                    ${{ car.priceHour ?? "N/A" }}
                  </span>
                  <span class="text-lg text-gray-700 dark:text-gray-300"
                    >/час</span
                  >
                  <span class="text-gray-500 dark:text-gray-400 ml-4"
                    >или ${{ car.priceDay ?? "N/A" }}/день</span
                  >
                </div>
              </div>

              <!-- Specifications -->
              <div
                v-if="specifications.length > 0"
                class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800"
              >
                <h2
                  class="text-xl font-bold text-gray-900 dark:text-white mb-4"
                >
                  Характеристики
                </h2>
                <div class="grid grid-cols-2 gap-x-4 gap-y-5">
                  <div
                    v-for="spec in specifications"
                    :key="spec.label"
                    class="flex items-center gap-3"
                  >
                    <div
                      class="w-10 h-10 rounded-lg bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center flex-shrink-0"
                    >
                      <component
                        :is="spec.icon"
                        class="w-5 h-5 text-primary-600 dark:text-primary-400"
                      />
                    </div>
                    <div>
                      <p class="text-xs text-gray-500 dark:text-gray-400">
                        {{ spec.label }}
                      </p>
                      <p class="font-semibold text-gray-900 dark:text-white">
                        {{ spec.value }}
                      </p>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Features -->
              <div
                v-if="features.length > 0"
                class="p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800"
              >
                <h2
                  class="text-xl font-bold text-gray-900 dark:text-white mb-4"
                >
                  Особенности
                </h2>
                <div class="flex flex-wrap gap-2">
                  <span
                    v-for="(feature, index) in features"
                    :key="index"
                    class="px-4 py-2 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300 rounded-full text-sm font-semibold"
                  >
                    {{ feature }}
                  </span>
                </div>
              </div>
            </div>

            <!-- Book Button - прижата к низу -->
            <button
              @click="openBookingModal"
              class="w-full btn-premium py-5 text-lg mt-auto"
            >
              <span>Забронировать автомобиль</span>
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
                  d="M13 7l5 5m0 0l-5 5m5-5H6"
                />
              </svg>
            </button>
          </div>
        </div>

        <!-- Reviews Section -->
        <div
          class="mt-12 p-8 bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800"
        >
          <div class="flex items-center justify-between mb-8">
            <h2 class="text-3xl font-bold text-gray-900 dark:text-white">
              Отзывы
              <span
                v-if="totalComments > 0"
                class="text-gray-500 dark:text-gray-400"
                >({{ totalComments }})</span
              >
            </h2>

            <!-- Add Review Button -->
            <button
              v-if="isAuthenticated"
              @click="openReviewModal"
              class="btn-premium inline-flex items-center gap-2 py-3 px-6"
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
                  d="M12 4v16m8-8H4"
                />
              </svg>
              <span>Оставить отзыв</span>
            </button>
          </div>

          <!-- Login Prompt -->
          <div
            v-if="!isAuthenticated"
            class="mb-8 p-6 bg-primary-50 dark:bg-primary-900/20 rounded-2xl text-center"
          >
            <p class="text-gray-700 dark:text-gray-300 mb-4">
              Войдите, чтобы оставить отзыв
            </p>
            <router-link
              to="/login"
              class="btn-premium inline-flex items-center gap-2 py-3 px-6"
            >
              Войти
            </router-link>
          </div>

          <!-- Reviews List (3 комментария на странице) -->
          <div v-if="comments.length > 0" class="space-y-6">
            <div
              v-for="review in comments"
              :key="review.id"
              class="p-6 bg-gray-50 dark:bg-gray-800 rounded-2xl space-y-3"
            >
              <!-- User Info & Rating -->
              <div class="flex items-start justify-between">
                <div class="flex items-center gap-3">
                  <div
                    class="w-12 h-12 rounded-full bg-primary-100 dark:bg-primary-900/30 flex items-center justify-center"
                  >
                    <span
                      class="text-lg font-bold text-primary-600 dark:text-primary-400"
                    >
                      {{ review.userName.charAt(0).toUpperCase() }}
                    </span>
                  </div>
                  <div>
                    <p class="font-semibold text-gray-900 dark:text-white">
                      {{ review.userName }}
                    </p>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                      {{ formatDate(review.created_On) }}
                    </p>
                  </div>
                </div>

                <!-- Rating -->
                <div class="flex items-center gap-1">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    :class="[
                      'w-5 h-5',
                      i <= review.rating
                        ? 'text-yellow-400 fill-current'
                        : 'text-gray-300 dark:text-gray-600',
                    ]"
                    viewBox="0 0 20 20"
                  >
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                    />
                  </svg>
                </div>
              </div>

              <!-- Comment -->
              <p class="text-gray-700 dark:text-gray-300 leading-relaxed">
                {{ review.content }}
              </p>
            </div>
          </div>

          <!-- Empty State -->
          <div v-else class="text-center py-16">
            <div
              class="w-20 h-20 mx-auto mb-4 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
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
                  d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z"
                />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white mb-2">
              Пока нет отзывов
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              Будьте первым, кто оставит отзыв об этом автомобиле!
            </p>
          </div>

          <!-- Comments Pagination (3 комментария на странице) -->
          <div v-if="totalCommentPages > 1" class="mt-8">
            <Pagination
              :current-page="currentCommentPage"
              :total-pages="totalCommentPages"
              @page-change="handleCommentPageChange"
            />
          </div>
        </div>
      </div>

      <!-- Error State -->
      <div v-else class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6 max-w-md mx-auto">
          <div
            class="w-24 h-24 rounded-full bg-red-100 dark:bg-red-900/20 flex items-center justify-center"
          >
            <svg
              class="w-12 h-12 text-red-600 dark:text-red-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            </svg>
          </div>
          <div class="space-y-2">
            <h3 class="text-2xl font-bold text-gray-900 dark:text-white">
              Автомобиль не найден
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              Запрашиваемый автомобиль не существует или был удален
            </p>
          </div>
          <router-link
            to="/cars"
            class="btn-premium inline-flex items-center gap-2"
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
                d="M10 19l-7-7m0 0l7-7m-7 7h18"
              />
            </svg>
            <span>Вернуться к списку</span>
          </router-link>
        </div>
      </div>
    </div>

    <!-- Booking Modal -->
    <BookingModal
      v-if="car && isBookingModalOpen"
      :is-open="isBookingModalOpen"
      :car="car"
      @close="closeBookingModal"
      @confirm="handleBookingConfirm"
    />

    <!-- Review Modal -->
    <ReviewModal
      v-if="car && isReviewModalOpen"
      :is-open="isReviewModalOpen"
      :car="car"
      @close="closeReviewModal"
      @submit="handleReviewSubmit"
      ref="reviewModalRef"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, h } from "vue";
import { useRoute, useRouter } from "vue-router";
import type { CarDetails } from "../types/Car";
import { getCarDetails, createCarComment, getCarComments } from "../api/cars";
import { createBooking } from "../api/booking";
import { useToast } from "../composables/useToast";
import { useAuth } from "../composables/useAuth";
import BookingModal from "../components/BookingModal.vue";
import ReviewModal from "../components/ReviewModal.vue";
import Pagination from "../components/Pagination.vue";

const route = useRoute();
const router = useRouter();
const { success, error } = useToast();
const { isAuthenticated } = useAuth();

const loading = ref(true);
const car = ref<CarDetails | null>(null);
const isBookingModalOpen = ref(false);
const isReviewModalOpen = ref(false);
const reviewModalRef = ref<InstanceType<typeof ReviewModal> | null>(null);
const currentImageIndex = ref(0);

// Comments pagination (3 комментария на странице)
const comments = ref<any[]>([]);
const currentCommentPage = ref(1);
const commentPageSize = ref(3);
const totalComments = ref(0);
const totalCommentPages = computed(() =>
  Math.ceil(totalComments.value / commentPageSize.value)
);

const carImages = computed(() => {
  const images: string[] = [];
  if (car.value?.imageUrl) {
    images.push(car.value.imageUrl);
  }
  if (car.value?.images) {
    images.push(...car.value.images);
  }
  return images;
});

const currentImage = computed(() => {
  return carImages.value[currentImageIndex.value] || "";
});

const specifications = computed(() => {
  if (!car.value?.specifications) return [];

  const specs = car.value.specifications;
  return [
    {
      label: "Двигатель",
      value: specs.engine || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M13 10V3L4 14h7v7l9-11h-7z",
            }),
          ]
        ),
    },
    {
      label: "Коробка передач",
      value: specs.transmission || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15",
            }),
          ]
        ),
    },
    {
      label: "Мест",
      value: specs.seats?.toString() || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z",
            }),
          ]
        ),
    },
    {
      label: "Топливо",
      value: specs.fuelType || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z",
            }),
          ]
        ),
    },
    {
      label: "Цвет",
      value: specs.color || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M7 21a4 4 0 01-4-4V5a2 2 0 012-2h4a2 2 0 012 2v12a4 4 0 01-4 4zm0 0h12a2 2 0 002-2v-4a2 2 0 00-2-2h-2.343M11 7.343l1.657-1.657a2 2 0 012.828 0l2.829 2.829a2 2 0 010 2.828l-8.486 8.485M7 17h.01",
            }),
          ]
        ),
    },
    {
      label: "Дверей",
      value: specs.doors?.toString() || "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M8 7h12m0 0l-4-4m4 4l-4 4m0 6H4m0 0l4 4m-4-4l4-4",
            }),
          ]
        ),
    },
    {
      label: "Пробег",
      value: specs.mileage ? `${specs.mileage} км` : "N/A",
      icon: () =>
        h(
          "svg",
          {
            class: "w-5 h-5",
            fill: "none",
            stroke: "currentColor",
            viewBox: "0 0 24 24",
          },
          [
            h("path", {
              "stroke-linecap": "round",
              "stroke-linejoin": "round",
              "stroke-width": "2",
              d: "M13 10V3L4 14h7v7l9-11h-7z",
            }),
          ]
        ),
    },
  ].filter((spec) => spec.value !== "N/A");
});

const features = computed(() => {
  if (!car.value?.features) return [];
  return car.value.features.map((f: any) => f.name || f) || [];
});

onMounted(async () => {
  const carId = Number(route.params.id);
  if (isNaN(carId)) {
    loading.value = false;
    return;
  }

  await loadCarDetails(carId);
  await loadComments(carId);
});

async function loadCarDetails(id: number) {
  try {
    const data = await getCarDetails(id);

    // Логируем для отладки
    console.log("CAR DETAILS FROM API:", data);

    // Нормализуем данные - если specifications нет, создаём из корневых полей
    car.value = {
      ...data,
      specifications: data.specifications ?? {
        engine: data.engine,
        transmission: data.transmission,
        fuelType: data.fuelType,
        seats: data.seats,
        doors: data.doors,
        color: data.color,
        mileage: data.mileage,
      },
    };
  } catch (e) {
    console.error("Ошибка загрузки деталей:", e);
    car.value = null;
  } finally {
    loading.value = false;
  }
}

async function loadComments(carId: number) {
  try {
    const response = await getCarComments(carId, {
      page: currentCommentPage.value,
      pageSize: commentPageSize.value,
    });

    // Проверяем формат ответа
    if (response.items) {
      comments.value = response.items;
      totalComments.value = response.totalCount;
    } else {
      // Fallback - если ответ не пагинированный
      comments.value = [];
      totalComments.value = 0;
    }
  } catch (e) {
    console.error("Ошибка загрузки комментариев:", e);
    comments.value = [];
    totalComments.value = 0;
  }
}

function handleCommentPageChange(page: number) {
  currentCommentPage.value = page;
  if (car.value) {
    loadComments(car.value.id);
  }
}

function getCommentsWord(count: number): string {
  const lastDigit = count % 10;
  const lastTwoDigits = count % 100;

  if (lastTwoDigits >= 11 && lastTwoDigits <= 14) {
    return "отзывов";
  }

  if (lastDigit === 1) {
    return "отзыв";
  }

  if (lastDigit >= 2 && lastDigit <= 4) {
    return "отзыва";
  }

  return "отзывов";
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("ru-RU", {
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(date);
}

function openBookingModal() {
  isBookingModalOpen.value = true;
}

function closeBookingModal() {
  isBookingModalOpen.value = false;
}

async function handleBookingConfirm(startDate: string, endDate: string) {
  if (!car.value) return;

  try {
    await createBooking(car.value.id, startDate, endDate);
    success(`${car.value.brand} ${car.value.model} успешно забронирован!`);
    closeBookingModal();
  } catch (e) {
    console.error("Ошибка бронирования:", e);
    error("Ошибка бронирования. Попробуйте снова.");
  }
}

function openReviewModal() {
  isReviewModalOpen.value = true;
}

function closeReviewModal() {
  isReviewModalOpen.value = false;
}

async function handleReviewSubmit(rating: number, content: string) {
  if (!car.value) return;

  try {
    await createCarComment(car.value.id, content, rating);
    success("Отзыв успешно добавлен!");

    closeReviewModal();

    // Перезагружаем детали и комментарии
    await loadCarDetails(car.value.id);
    currentCommentPage.value = 1; // Сбрасываем на первую страницу
    await loadComments(car.value.id);
  } catch (e: any) {
    console.error("Ошибка добавления отзыва:", e);
    error(e.response?.data?.message || "Не удалось добавить отзыв");
  } finally {
    if (reviewModalRef.value) {
      reviewModalRef.value.setSubmitting(false);
    }
  }
}
</script>
