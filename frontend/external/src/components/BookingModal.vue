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
                {{ selection.brand }} {{ selection.model }}
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
              v-if="matchedPreview"
              class="rounded-2xl border border-slate-200 dark:border-slate-700 bg-slate-50 dark:bg-slate-900/60 p-4 space-y-4"
            >
              <div class="flex items-start gap-4">
                <div
                  class="h-24 w-32 shrink-0 overflow-hidden rounded-2xl bg-gray-200 dark:bg-gray-800"
                >
                  <img
                    v-if="previewHeroImage"
                    :src="previewHeroImage"
                    :alt="`${selection.brand} ${selection.model}`"
                    class="h-full w-full object-cover"
                  />
                  <div
                    v-else
                    class="flex h-full w-full items-center justify-center text-xs text-gray-500 dark:text-gray-400"
                  >
                    Нет фото
                  </div>
                </div>

                <div class="min-w-0 flex-1 space-y-1">
                  <p class="text-xs font-bold uppercase tracking-[0.2em] text-slate-500 dark:text-slate-400">
                    Подобранная машина
                  </p>
                  <p class="text-lg font-bold text-gray-900 dark:text-white">
                    {{ selection.brand }} {{ selection.model }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-300">
                    Партнер: {{ matchedPreview.partnerName }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-300">
                    {{ matchedPreview.modelYear }} год
                    <span v-if="matchedPreview.rating != null">
                      · рейтинг {{ matchedPreview.rating }}
                    </span>
                    <span v-if="matchedPreview.licensePlate">
                      · {{ matchedPreview.licensePlate }}
                    </span>
                  </p>
                  <p
                    v-if="matchedPreview.listedPriceHour != null"
                    class="text-sm font-medium text-primary-700 dark:text-primary-300"
                  >
                    Ставка партнера: {{ matchedPreview.listedPriceHour }} KZT/час
                  </p>
                </div>
              </div>

              <div
                v-if="previewGallery.length > 0"
                class="grid grid-cols-4 gap-2"
              >
                <div
                  v-for="imageUrl in previewGallery.slice(0, 4)"
                  :key="imageUrl"
                  class="h-16 overflow-hidden rounded-xl bg-gray-200 dark:bg-gray-800"
                >
                  <img
                    :src="imageUrl"
                    :alt="`${selection.brand} ${selection.model}`"
                    class="h-full w-full object-cover"
                  />
                </div>
              </div>
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
              :disabled="!canConfirm || submitting || validatingSelection"
              class="flex-1 px-6 py-3 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed disabled:hover:shadow-none"
            >
              <span v-if="submitting">Загрузка...</span>
              <span v-else-if="validatingSelection">Проверяем...</span>
              <span v-else>Забронировать</span>
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { getBookingPricePreview, type BookingPricePreview } from "../api/booking";
import { matchCarByModel } from "../api/cars";
import api from "../api/axios";
import { getPublicPartnerCarDetails } from "../api/partnerCars";
import { getPartnerPublicProfileByRelatedUserId } from "../api/partners";
import type { BookingModelSelection } from "../types/Car";

interface Props {
  isOpen: boolean;
  selection: BookingModelSelection;
  bookingError?: string;
  submitting?: boolean;
}

interface Emits {
  (e: "close"): void;
  (
    e: "confirm",
    payload: {
      startDate: string;
      endDate: string;
      useSubscription: boolean;
      partnerCarId: number;
    },
  ): void;
}

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

type MatchedPartnerCarPreview = {
  partnerCarId: number;
  partnerUserId: string;
  partnerName: string;
  licensePlate: string;
  modelYear: number;
  rating: number | null;
  ratingsCount: number;
  listedPriceHour: number | null;
  coverImageUrl: string | null;
  imageUrls: string[];
};

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const startDate = ref("");
const endDate = ref("");
const validationError = ref("");
const availabilityError = ref("");
const pricePreview = ref<BookingPricePreview | null>(null);
const loadingPrice = ref(false);
const mySubscription = ref<MySubscription | null>(null);
const useSubscription = ref(false);
const matchedPartnerCarId = ref<number | null>(null);
const matchedPreview = ref<MatchedPartnerCarPreview | null>(null);
const validatingSelection = ref(false);
let previewRequestId = 0;

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
      availabilityError.value = "";
      pricePreview.value = null;
      matchedPartnerCarId.value = null;
      matchedPreview.value = null;
      validatingSelection.value = false;
      useSubscription.value = false;

      await loadMySubscription();
    } else {
      previewRequestId += 1;
      pricePreview.value = null;
      matchedPartnerCarId.value = null;
      matchedPreview.value = null;
      loadingPrice.value = false;
      availabilityError.value = "";
      mySubscription.value = null;
      validatingSelection.value = false;
      useSubscription.value = false;
    }
  },
);

watch([startDate, endDate, () => props.selection.modelId, () => props.isOpen], () => {
  if (!props.isOpen) {
    return;
  }

  if (!isValid.value) {
    previewRequestId += 1;
    matchedPartnerCarId.value = null;
    matchedPreview.value = null;
    pricePreview.value = null;
    availabilityError.value = "";
    return;
  }

  void refreshMatchedSelection();
});

const displayError = computed(() => {
  return validationError.value || availabilityError.value || props.bookingError?.trim() || "";
});

const canConfirm = computed(() => {
  return (
    isValid.value &&
    matchedPartnerCarId.value !== null &&
    matchedPreview.value !== null &&
    !loadingPrice.value
  );
});

const previewHeroImage = computed(() => {
  return (
    matchedPreview.value?.coverImageUrl ??
    matchedPreview.value?.imageUrls[0] ??
    props.selection.imageUrl ??
    null
  );
});

const previewGallery = computed(() => {
  const preview = matchedPreview.value;
  if (!preview) {
    return [];
  }

  const gallery = [...(preview.imageUrls ?? [])];
  const heroImage = preview.coverImageUrl ?? gallery[0] ?? null;
  if (!heroImage) {
    return gallery;
  }

  const heroIndex = gallery.findIndex((imageUrl) => imageUrl === heroImage);
  if (heroIndex >= 0) {
    gallery.splice(heroIndex, 1);
  }

  return gallery;
});

async function loadMySubscription() {
  try {
    const { data } = await api.get("/subscriptions/my");
    mySubscription.value = data;
  } catch {
    mySubscription.value = null;
  }
}

async function fetchPricePreview() {
  await refreshMatchedSelection();
}

async function refreshMatchedSelection(options?: {
  expectedPartnerCarId?: number | null;
  preserveCurrentPreview?: boolean;
}): Promise<"ready" | "changed" | "unavailable" | "stale"> {
  if (!startDate.value || !endDate.value || !props.selection?.modelId) {
    matchedPartnerCarId.value = null;
    matchedPreview.value = null;
    pricePreview.value = null;
    return "unavailable";
  }

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);

  if (
    Number.isNaN(start.getTime()) ||
    Number.isNaN(end.getTime()) ||
    end <= start
  ) {
    matchedPartnerCarId.value = null;
    matchedPreview.value = null;
    pricePreview.value = null;
    return "unavailable";
  }

  const currentRequestId = ++previewRequestId;
  const preserveCurrentPreview = options?.preserveCurrentPreview ?? false;

  try {
    loadingPrice.value = true;
    availabilityError.value = "";
    if (!preserveCurrentPreview) {
      matchedPartnerCarId.value = null;
      matchedPreview.value = null;
      pricePreview.value = null;
    }

    const matchResult = await matchCarByModel({
      modelId: props.selection.modelId,
      startTime: start.toISOString(),
      endTime: end.toISOString(),
    });

    if (currentRequestId !== previewRequestId) {
      return "stale";
    }

    if (!matchResult.isAvailable || !matchResult.partnerCarId) {
      matchedPartnerCarId.value = null;
      matchedPreview.value = null;
      pricePreview.value = null;
      availabilityError.value = buildAvailabilityError(
        matchResult.suggestedStartTimesUtc,
      );
      return "unavailable";
    }

    const [preview, publicPartnerCar, partnerProfile] = await Promise.all([
      getBookingPricePreview(
        matchResult.partnerCarId,
        start.toISOString(),
        end.toISOString(),
      ),
      getPublicPartnerCarDetails(matchResult.partnerCarId),
      matchResult.partnerUserId
        ? getPartnerPublicProfileByRelatedUserId(matchResult.partnerUserId).catch(() => null)
        : Promise.resolve(null),
    ]);

    if (currentRequestId !== previewRequestId) {
      return "stale";
    }

    const nextPreview = buildMatchedPreview(
      matchResult.partnerCarId,
      matchResult.partnerUserId ?? publicPartnerCar.partnerUserId,
      publicPartnerCar,
      partnerProfile?.carrierName ?? null,
    );
    const matchChanged =
      options?.expectedPartnerCarId != null &&
      options.expectedPartnerCarId !== matchResult.partnerCarId;

    matchedPartnerCarId.value = matchResult.partnerCarId;
    pricePreview.value = preview;
    matchedPreview.value = nextPreview;

    if (matchChanged) {
      availabilityError.value =
        "На эти даты подобрана другая машина. Проверьте обновленный preview и нажмите «Забронировать» еще раз.";
      return "changed";
    }

    return "ready";
  } catch (error) {
    console.error("Failed to fetch price preview:", error);
    matchedPartnerCarId.value = null;
    matchedPreview.value = null;
    pricePreview.value = null;
    availabilityError.value = "Не удалось подобрать машину для расчета стоимости.";
    return "unavailable";
  } finally {
    if (currentRequestId === previewRequestId) {
      loadingPrice.value = false;
    }
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

async function confirmBooking() {
  if (!canConfirm.value || matchedPartnerCarId.value === null) return;

  validatingSelection.value = true;

  try {
    const refreshResult = await refreshMatchedSelection({
      expectedPartnerCarId: matchedPartnerCarId.value,
      preserveCurrentPreview: true,
    });

    if (refreshResult !== "ready" || matchedPartnerCarId.value === null) {
      return;
    }

    emit("confirm", {
      startDate: new Date(startDate.value).toISOString(),
      endDate: new Date(endDate.value).toISOString(),
      useSubscription: useSubscription.value,
      partnerCarId: matchedPartnerCarId.value,
    });
  } finally {
    validatingSelection.value = false;
  }
}

function buildAvailabilityError(suggestedStartTimesUtc?: string[]): string {
  const suggestions = (suggestedStartTimesUtc ?? [])
    .slice(0, 3)
    .map(formatSuggestionDate)
    .filter(Boolean);

  if (suggestions.length === 0) {
    return "На выбранные даты машин этой модели нет.";
  }

  return `На выбранные даты машин этой модели нет. Ближайшие варианты: ${suggestions.join(", ")}.`;
}

function formatSuggestionDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function buildMatchedPreview(
  partnerCarId: number,
  partnerUserId: string,
  publicPartnerCar: Awaited<ReturnType<typeof getPublicPartnerCarDetails>>,
  carrierName: string | null,
): MatchedPartnerCarPreview {
  const imageUrls = (publicPartnerCar.images ?? [])
    .map((image) => image.imageUrl)
    .filter((imageUrl): imageUrl is string => Boolean(imageUrl));

  return {
    partnerCarId,
    partnerUserId,
    partnerName: carrierName?.trim() || "Партнер",
    licensePlate: publicPartnerCar.licensePlate ?? "",
    modelYear: publicPartnerCar.modelYear,
    rating: publicPartnerCar.rating ?? null,
    ratingsCount: publicPartnerCar.ratingsCount,
    listedPriceHour: publicPartnerCar.priceHour ?? null,
    coverImageUrl: imageUrls[0] ?? null,
    imageUrls,
  };
}
</script>
