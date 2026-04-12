<template>
  <div
    class="min-h-screen bg-gray-50 px-4 py-24 transition-colors duration-300 dark:bg-gray-950 sm:px-6 lg:px-8"
  >
    <div class="mx-auto max-w-7xl space-y-8">
      <button
        type="button"
        class="flex items-center gap-2 text-gray-600 transition-colors hover:text-gray-900 dark:text-gray-400 dark:hover:text-white"
        @click="$router.back()"
      >
        <svg
          class="h-5 w-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M10 19l-7-7m0 0 7-7m-7 7h18"
          />
        </svg>
        <span class="font-medium">Назад</span>
      </button>

      <section v-if="loading" class="py-28 text-center">
        <div class="inline-flex flex-col items-center gap-6">
          <div
            class="h-16 w-16 animate-spin rounded-full border-4 border-primary-200 border-t-primary-600 dark:border-primary-900 dark:border-t-primary-400"
          ></div>
          <p class="text-lg font-medium text-gray-600 dark:text-gray-400">
            Загрузка машины партнёра...
          </p>
        </div>
      </section>

      <section v-else-if="errorMessage" class="rounded-3xl border border-red-200 bg-red-50 p-8 text-red-700 dark:border-red-500/30 dark:bg-red-950/20 dark:text-red-300">
        {{ errorMessage }}
      </section>

      <template v-else-if="payload">
        <section class="grid gap-8 lg:grid-cols-[1.2fr_0.8fr]">
          <div class="space-y-4">
            <div
              class="relative h-96 overflow-hidden rounded-3xl bg-gradient-to-br from-gray-200 to-gray-300 shadow-2xl dark:from-gray-800 dark:to-gray-900"
            >
              <img
                v-if="currentImageMeta"
                :src="currentImageMeta.imageUrl"
                :alt="carTitle"
                class="h-full w-full object-cover"
              />
              <span
                v-if="currentImageMeta"
                class="absolute left-4 top-4 rounded-full bg-black/70 px-3 py-1 text-xs font-semibold tracking-wide text-white"
              >
                {{ getCarImageTypeLabel(currentImageMeta.imageType) }}
              </span>
              <div
                v-else
                class="flex h-full w-full items-center justify-center text-gray-500 dark:text-gray-400"
              >
                Нет изображения
              </div>
            </div>

            <div v-if="galleryImages.length > 1" class="grid grid-cols-5 gap-3">
              <button
                v-for="(image, index) in galleryImages.slice(0, 5)"
                :key="`${image.id}-${index}`"
                type="button"
                :class="[
                  'relative h-20 overflow-hidden rounded-xl transition-all',
                  currentImageIndex === index
                    ? 'scale-105 ring-4 ring-primary-500'
                    : 'ring-2 ring-gray-200 hover:ring-primary-300 dark:ring-gray-700',
                ]"
                @click="currentImageIndex = index"
              >
                <img
                  :src="image.imageUrl"
                  :alt="carTitle"
                  class="h-full w-full object-cover"
                />
                <span
                  class="pointer-events-none absolute bottom-2 left-2 rounded-full bg-black/70 px-2 py-1 text-[10px] font-semibold text-white"
                >
                  {{ getCarImageTypeLabel(image.imageType) }}
                </span>
              </button>
            </div>
          </div>

          <div class="space-y-6">
            <div class="space-y-4">
              <div class="flex flex-wrap items-center gap-3">
                <span class="rounded-full bg-primary-100 px-3 py-1 text-xs font-bold uppercase tracking-[0.22em] text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                  Машина партнёра
                </span>
                <span class="rounded-full border border-gray-200 bg-white px-3 py-1 text-xs font-semibold text-gray-600 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-300">
                  {{ statusLabel(payload.car.status) }}
                </span>
              </div>

              <div class="space-y-2">
                <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
                  {{ carTitle }}
                </h1>
                <p class="text-lg text-gray-600 dark:text-gray-400">
                  Перевозчик: {{ payload.carrierName }}
                </p>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  Гос. номер: {{ payload.car.licensePlate }}
                </p>
              </div>

              <div v-if="commercialBadges.length > 0" class="flex flex-wrap gap-2 pt-1">
                <span
                  v-for="badge in commercialBadges"
                  :key="`${payload.car.id}-${badge.key}`"
                  :class="[
                    'rounded-full border px-3 py-1.5 text-sm font-semibold',
                    getCommercialBadgeClasses(badge.key),
                  ]"
                >
                  {{ badge.label }}
                </span>
              </div>

              <div v-if="visibleTags.length > 0" class="flex flex-wrap gap-2 pt-1">
                <span
                  v-for="tag in visibleTags"
                  :key="`${payload.car.id}-${tag}`"
                  class="rounded-full border border-gray-200 bg-white px-3 py-1.5 text-sm font-semibold text-gray-700 dark:border-gray-800 dark:bg-gray-900 dark:text-gray-300"
                >
                  {{ tag }}
                </span>
              </div>
            </div>

            <div class="grid gap-4 sm:grid-cols-2">
              <article class="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-900">
                <p class="text-sm text-gray-500 dark:text-gray-400">Цена за час</p>
                <p class="text-2xl font-bold text-primary-600 dark:text-primary-400">
                  {{ formatPrice(payload.car.priceHour) }}
                </p>
              </article>
              <article class="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-900">
                <p class="text-sm text-gray-500 dark:text-gray-400">Цена за день</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">
                  {{ formatPrice(payload.car.priceDay) }}
                </p>
              </article>
              <article class="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-900">
                <p class="text-sm text-gray-500 dark:text-gray-400">Рейтинг</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">
                  {{ formatRating(payload.car.rating) }}
                </p>
              </article>
              <article class="rounded-2xl border border-gray-200 bg-white p-4 dark:border-gray-800 dark:bg-gray-900">
                <p class="text-sm text-gray-500 dark:text-gray-400">Отзывы</p>
                <p class="text-2xl font-bold text-gray-900 dark:text-white">
                  {{ payload.reviews.length }}
                </p>
              </article>
            </div>

            <div class="rounded-2xl border border-gray-200 bg-white p-6 dark:border-gray-800 dark:bg-gray-900">
              <h2 class="mb-3 text-xl font-bold text-gray-900 dark:text-white">
                Описание
              </h2>
              <p class="text-gray-700 dark:text-gray-300">
                {{ payload.model.description || "Описание отсутствует." }}
              </p>
            </div>

            <div class="rounded-2xl border border-gray-200 bg-white p-6 dark:border-gray-800 dark:bg-gray-900">
              <h2 class="mb-3 text-xl font-bold text-gray-900 dark:text-white">
                Характеристики
              </h2>
              <div class="grid grid-cols-2 gap-3 text-sm">
                <div class="text-gray-600 dark:text-gray-400">
                  Двигатель:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.engine || "—" }}</span>
                </div>
                <div class="text-gray-600 dark:text-gray-400">
                  Трансмиссия:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.transmission || "—" }}</span>
                </div>
                <div class="text-gray-600 dark:text-gray-400">
                  Мест:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.seats ?? "—" }}</span>
                </div>
                <div class="text-gray-600 dark:text-gray-400">
                  Топливо:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.fuelType || "—" }}</span>
                </div>
                <div class="text-gray-600 dark:text-gray-400">
                  Дверей:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.model.doors ?? "—" }}</span>
                </div>
                <div class="text-gray-600 dark:text-gray-400">
                  Цвет:
                  <span class="font-semibold text-gray-900 dark:text-white">{{ payload.car.color || "—" }}</span>
                </div>
              </div>
            </div>

            <div class="space-y-3">
              <button
                type="button"
                class="btn-premium w-full py-5 text-lg disabled:cursor-not-allowed disabled:opacity-60"
                :disabled="payload.car.status !== 0"
                @click="openBookingModal"
              >
                Забронировать эту машину
              </button>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Бронирование создастся именно для этой машины партнёра, без автоподбора другой.
              </p>
            </div>
          </div>
        </section>

        <section class="rounded-3xl border border-gray-200 bg-white p-8 dark:border-gray-800 dark:bg-gray-900">
          <h2 class="mb-6 text-3xl font-bold text-gray-900 dark:text-white">
            Отзывы
          </h2>

          <div v-if="payload.reviews.length > 0" class="space-y-5">
            <article
              v-for="review in payload.reviews"
              :key="`${review.id}-${review.partnerCarId ?? 0}`"
              class="space-y-3 rounded-2xl bg-gray-50 p-6 dark:bg-gray-800"
            >
              <div class="flex items-start justify-between gap-4">
                <div class="flex min-w-0 items-start gap-4">
                  <div
                    class="flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-2xl bg-gradient-to-br from-primary-500 to-primary-700 font-bold text-white shadow-lg"
                  >
                    <img
                      v-if="review.avatarUrl"
                      :src="review.avatarUrl"
                      :alt="review.userName"
                      class="h-full w-full object-cover"
                    />
                    <span v-else>{{ getInitials(review.userName) }}</span>
                  </div>

                  <div class="min-w-0 space-y-1">
                    <p class="font-semibold text-gray-900 dark:text-white">
                      {{ review.userName }}
                    </p>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                      {{ formatDate(review.createdOn) }}
                    </p>
                  </div>
                </div>

                <div class="flex items-center gap-1">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    :class="[
                      'h-5 w-5',
                      i <= review.rating
                        ? 'fill-current text-yellow-400'
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

              <p class="text-gray-700 dark:text-gray-300">
                {{ review.content }}
              </p>
            </article>
          </div>

          <div v-else class="py-16 text-center">
            <p class="text-xl font-bold text-gray-900 dark:text-white">
              Пока нет отзывов
            </p>
            <p class="mt-2 text-gray-600 dark:text-gray-400">
              Для этой машины ещё не оставляли отзывы.
            </p>
          </div>
        </section>
      </template>
    </div>

    <BookingModal
      v-if="bookingSelection"
      :is-open="isBookingModalOpen"
      :selection="bookingSelection"
      :booking-error="bookingError"
      :submitting="creatingBooking"
      @close="closeBookingModal"
      @confirm="handleBookingConfirm"
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
import { createBooking } from "../api/booking";
import { getPartnerCarDetailsPayload, type PartnerCarDetailsPayload } from "../api/cars";
import BookingModal from "../components/BookingModal.vue";
import LoginRequiredModal from "../components/LoginRequiredModal.vue";
import { useAuth } from "../composables/useAuth";
import { useToast } from "../composables/useToast";
import type { BookingPartnerCarSelection } from "../types/Car";
import { getCarImageTypeLabel } from "../utils/carImageType";
import {
  buildBadgesFromKeys,
  getCommercialBadgeClasses,
} from "../utils/commercialBadges";
import { formatMoney } from "../utils/formatMoney";

const route = useRoute();
const router = useRouter();
const { isAuthenticated } = useAuth();
const { success, error } = useToast();

const loading = ref(true);
const creatingBooking = ref(false);
const errorMessage = ref("");
const payload = ref<PartnerCarDetailsPayload | null>(null);
const currentImageIndex = ref(0);

const isBookingModalOpen = ref(false);
const showLoginModal = ref(false);
const bookingError = ref("");

const galleryImages = computed(() => {
  if (!payload.value) {
    return [];
  }

  const partnerImages = payload.value.car.images ?? [];
  return partnerImages.length > 0 ? partnerImages : payload.value.model.images ?? [];
});

const currentImageMeta = computed(
  () => galleryImages.value[currentImageIndex.value] ?? null,
);

const carTitle = computed(() => {
  if (!payload.value) {
    return "";
  }

  return `${payload.value.model.brand} ${payload.value.model.model} ${payload.value.model.year}`;
});

const commercialBadges = computed(() => {
  if (!payload.value) {
    return [];
  }

  return buildBadgesFromKeys(payload.value.commercialBadgeKeys);
});

const visibleTags = computed(() => {
  if (!payload.value) {
    return [];
  }

  const badgeKeySet = new Set(payload.value.commercialBadgeKeys);

  return payload.value.tags.filter(
    (tag) => !badgeKeySet.has(tag.trim().toLowerCase()),
  );
});

const bookingSelection = computed<BookingPartnerCarSelection | null>(() => {
  if (!payload.value) {
    return null;
  }

  return {
    kind: "partnerCar",
    partnerCarId: payload.value.car.id,
    partnerUserId: payload.value.car.partnerUserId,
    carrierName: payload.value.carrierName,
    licensePlate: payload.value.car.licensePlate,
    busySlots: payload.value.busySlots,
    brand: payload.value.model.brand,
    model: payload.value.model.model,
    year: payload.value.model.year,
    priceHour: payload.value.car.priceHour ?? null,
    priceDay: payload.value.car.priceDay ?? null,
    imageUrl: galleryImages.value[0]?.imageUrl ?? null,
    rating: payload.value.car.rating ?? null,
    description: payload.value.model.description ?? null,
  };
});

onMounted(async () => {
  await loadPartnerCarDetails();
});

async function loadPartnerCarDetails() {
  loading.value = true;
  errorMessage.value = "";

  try {
    const partnerCarId = Number(route.params.id);
    if (!Number.isInteger(partnerCarId) || partnerCarId <= 0) {
      payload.value = null;
      errorMessage.value = "Некорректный идентификатор машины.";
      return;
    }

    payload.value = await getPartnerCarDetailsPayload(partnerCarId);
    currentImageIndex.value = 0;
  } catch (loadError) {
    console.error("Failed to load partner car details:", loadError);
    payload.value = null;
    errorMessage.value = "Не удалось загрузить машину партнёра.";
    error("Не удалось загрузить машину партнёра.");
  } finally {
    loading.value = false;
  }
}

function statusLabel(status: number): string {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Недоступна";
}

function formatPrice(value: number | null | undefined): string {
  if (value == null) {
    return "По запросу";
  }

  return formatMoney(value);
}

function formatRating(value: number | null | undefined): string {
  if (value == null) {
    return "Без рейтинга";
  }

  return `${value.toFixed(1)} / 5`;
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

function getInitials(name: string): string {
  const parts = (name ?? "")
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2);

  if (parts.length === 0) {
    return "?";
  }

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

function openBookingModal() {
  if (!isAuthenticated.value) {
    showLoginModal.value = true;
    return;
  }

  bookingError.value = "";
  isBookingModalOpen.value = true;
}

function closeBookingModal() {
  isBookingModalOpen.value = false;
  bookingError.value = "";
}

function goToLogin() {
  showLoginModal.value = false;
  router.push("/login");
}

async function handleBookingConfirm(payloadData: {
  startDate: string;
  endDate: string;
  useSubscription: boolean;
  partnerCarId: number;
}) {
  if (!payload.value) {
    return;
  }

  creatingBooking.value = true;
  bookingError.value = "";

  try {
    const booking = await createBooking(
      payloadData.partnerCarId,
      payloadData.startDate,
      payloadData.endDate,
      payloadData.useSubscription,
    );

    success(
      payloadData.useSubscription
        ? `${carTitle.value}: бронь создана по подписке.`
        : `${carTitle.value}: бронь создана, завершите оплату.`,
    );

    closeBookingModal();

    if (payloadData.useSubscription) {
      await router.push("/bookings");
    } else {
      await router.push(`/bookings/${booking.id}/payment`);
    }
  } catch (bookingCreateError) {
    console.error("Failed to create direct partner car booking:", bookingCreateError);
    bookingError.value = "Не удалось забронировать машину. Попробуйте снова.";
    error("Не удалось забронировать машину.");
  } finally {
    creatingBooking.value = false;
  }
}
</script>
