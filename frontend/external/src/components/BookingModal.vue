<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-3 backdrop-blur-sm sm:items-center sm:p-4"
        @click.self="closeModal"
      >
        <div
          class="relative my-auto flex max-h-[calc(100dvh-1.5rem)] w-full max-w-md flex-col overflow-hidden rounded-3xl bg-white shadow-2xl transition-all dark:bg-gray-900 sm:max-h-[calc(100dvh-2rem)]"
          @click.stop
        >
          <div
            class="relative shrink-0 bg-gradient-to-br from-primary-600 to-primary-700 px-5 py-5 text-white sm:px-6 sm:py-7"
          >
            <button
              aria-label="Закрыть"
              class="absolute right-4 top-4 rounded-full p-2 transition-colors hover:bg-white/20"
              @click="closeModal"
            >
              <svg
                class="h-6 w-6"
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
              <p class="text-sm text-primary-100">
                {{ selection.brand }} {{ selection.model }}
              </p>
              <p
                v-if="isDirectPartnerCar && directPartnerMeta"
                class="text-xs text-primary-100/90"
              >
                {{ directPartnerMeta }}
              </p>
            </div>
          </div>

          <div class="min-h-0 flex-1 space-y-4 overflow-y-auto px-5 py-4 sm:space-y-5 sm:p-6">
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
                  :min="minDate"
                  class="w-full rounded-xl border-2 border-gray-200 bg-gray-50 px-4 py-3 pr-12 text-gray-900 transition-all focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-700 dark:bg-gray-800 dark:text-white"
                  required
                  type="datetime-local"
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
                  :min="startDate || minDate"
                  class="w-full rounded-xl border-2 border-gray-200 bg-gray-50 px-4 py-3 pr-12 text-gray-900 transition-all focus:border-transparent focus:outline-none focus:ring-2 focus:ring-primary-500 dark:border-gray-700 dark:bg-gray-800 dark:text-white"
                  required
                  type="datetime-local"
                />
              </div>
            </div>

            <div
              v-if="duration"
              class="flex items-center gap-3 rounded-xl bg-primary-50 p-4 dark:bg-primary-900/20"
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
              v-if="isDirectPartnerCar"
              class="rounded-2xl border border-amber-200 bg-amber-50 p-4 dark:border-amber-500/30 dark:bg-amber-950/20"
            >
              <p
                class="text-xs font-semibold uppercase tracking-[0.2em] text-amber-700 dark:text-amber-300"
              >
                Конкретная машина
              </p>

              <div v-if="suggestedAvailabilityWindows.length > 0" class="mt-3 space-y-3">
                <p class="text-sm font-semibold text-gray-900 dark:text-white">
                  Свободные окна
                </p>
                <div class="grid gap-2">
                  <button
                    v-for="window in suggestedAvailabilityWindows"
                    :key="`${window.startTime}-${window.endTime}`"
                    type="button"
                    class="flex items-center justify-between rounded-xl border border-amber-200 bg-white px-4 py-3 text-left transition-colors hover:border-primary-400 hover:bg-primary-50 dark:border-amber-500/20 dark:bg-gray-900 dark:hover:bg-primary-900/20"
                    @click="applySuggestedWindow(window)"
                  >
                    <span class="text-sm font-medium text-gray-800 dark:text-gray-100">
                      {{ formatSuggestionRange(window.startTime, window.endTime) }}
                    </span>
                    <span class="text-xs font-semibold text-primary-700 dark:text-primary-300">
                      Подставить
                    </span>
                  </button>
                </div>
              </div>

              <div v-if="busySlotPreview.length > 0" class="mt-4">
                <p class="text-xs font-semibold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">
                  Ближайшие занятые интервалы
                </p>
                <div class="mt-2 flex flex-wrap gap-2">
                  <span
                    v-for="slot in busySlotPreview"
                    :key="`${slot.bookingId}-${slot.startTime}`"
                    class="rounded-full bg-white px-3 py-1 text-xs font-medium text-gray-700 dark:bg-gray-900 dark:text-gray-300"
                  >
                    {{ formatBusySlot(slot) }}
                  </span>
                </div>
              </div>
            </div>

            <div
              v-if="mySubscription"
              class="rounded-2xl border border-emerald-200 bg-emerald-50 p-4 dark:border-emerald-800 dark:bg-emerald-950/30"
            >
              <div class="flex items-start justify-between gap-4">
                <div>
                  <p
                    class="text-xs font-semibold uppercase tracking-[0.2em] text-emerald-600 dark:text-emerald-400"
                  >
                    Active subscription
                  </p>
                  <p class="mt-1 text-lg font-bold text-gray-900 dark:text-white">
                    {{ mySubscription.planName }}
                  </p>
                  <p class="text-sm text-gray-600 dark:text-gray-300">
                    Remaining bookings: {{ mySubscription.remainingBookings }}
                  </p>
                </div>
              </div>

              <label
                class="mt-4 flex cursor-pointer items-center gap-3"
                :class="{
                  'cursor-not-allowed opacity-50':
                    mySubscription.remainingBookings <= 0,
                }"
              >
                <input
                  v-model="useSubscription"
                  :disabled="mySubscription.remainingBookings <= 0"
                  class="rounded"
                  type="checkbox"
                />
                <span class="text-sm font-medium text-gray-700 dark:text-gray-200">
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
              class="mt-4 rounded-2xl border border-blue-200 bg-blue-50 p-4 dark:border-blue-800 dark:bg-slate-900"
            >
              <div class="mb-2 flex items-center justify-between">
                <span class="text-sm font-medium text-gray-600 dark:text-gray-300">
                  Estimated total
                </span>
                <span class="text-xl font-bold text-blue-600 dark:text-blue-400">
                  {{ useSubscription ? 0 : pricePreview.finalPrice }}
                  {{ pricePreview.currency }}
                </span>
              </div>

              <div
                v-if="!useSubscription"
                class="space-y-1 text-sm text-gray-600 dark:text-gray-400"
              >
                <p>
                  Базовая цена за час: {{ formatPriceAmount(basePriceHour, pricePreview.currency) }}
                </p>
                <p>Рейтинг: {{ pricePreview.rating }} (x{{ pricePreview.ratingCoefficient }})</p>
                <p>
                  Предварительное бронирование: {{ pricePreview.daysBeforeBooking }} дн. (x{{
                    pricePreview.advanceBookingCoefficient
                  }})
                </p>
                <p>Коэффициент доступности: x{{ pricePreview.availabilityCoefficient }}</p>
                <p>
                  Итоговая цена за час: {{ formatPriceAmount(pricePreview.priceHour, pricePreview.currency) }}
                </p>
                <p
                  v-if="pricePreview.isMarketValueStale"
                  class="pt-1 font-medium text-amber-700 dark:text-amber-300"
                >
                  Рыночная стоимость устарела, цена рассчитана по последнему доступному снапшоту.
                </p>
              </div>

              <div
                v-else
                class="text-sm font-medium text-emerald-700 dark:text-emerald-300"
              >
                Это бронирование будет оформлено по активной подписке.
              </div>

              <p
                v-if="!isDirectPartnerCar"
                class="mt-3 text-sm text-blue-700 dark:text-blue-300"
              >
                Машину партнера и данные по поездке покажем сразу после создания брони.
              </p>
            </div>

            <div
              v-if="displayError"
              class="flex items-center gap-3 rounded-xl bg-red-50 p-4 dark:bg-red-900/20"
            >
              <p class="text-sm text-red-600 dark:text-red-400">
                {{ displayError }}
              </p>
            </div>
          </div>

          <div
            class="shrink-0 border-t border-gray-200 bg-gray-50 px-5 py-3 dark:border-gray-700 dark:bg-gray-800/50 sm:px-6 sm:py-4"
          >
            <div class="flex gap-3">
            <button
              class="flex-1 rounded-xl border-2 border-gray-300 bg-white px-6 py-3 font-semibold text-gray-700 transition-all active:scale-95 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
              @click="closeModal"
            >
              Отмена
            </button>
            <button
              :disabled="!canConfirm || submitting || validatingSelection"
              class="flex-1 rounded-xl bg-primary-600 px-6 py-3 font-semibold text-white transition-all active:scale-95 hover:bg-primary-700 hover:shadow-lg disabled:cursor-not-allowed disabled:bg-gray-400 disabled:hover:shadow-none"
              @click="confirmBooking"
            >
              <span v-if="submitting">Загрузка...</span>
              <span v-else-if="validatingSelection">Проверяем...</span>
              <span v-else>Забронировать</span>
            </button>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, ref, watch } from "vue";
import {
  getBookingPricePreview,
  getPartnerCarAvailability,
  type BookingPricePreview,
} from "../api/booking";
import { matchCarByModel } from "../api/cars";
import api from "../api/axios";
import type { BookingBusySlot, BookingSelection } from "../types/Car";
import { formatMoney } from "../utils/formatMoney";

interface Props {
  isOpen: boolean;
  selection: BookingSelection;
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

type RefreshSelectionResult = "ready" | "changed" | "unavailable" | "stale";

type AvailabilityWindow = {
  startTime: string;
  endTime: string;
};

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const ACTIVE_BOOKING_STATUSES = new Set(["pending", "confirmed", "active"]);
const HOURLY_BASE_FACTOR = 0.0001;
const DEFAULT_SLOT_DURATION_MS = 24 * 60 * 60 * 1000;
const MIN_DURATION_MS = 60 * 60 * 1000;

const startDate = ref("");
const endDate = ref("");
const validationError = ref("");
const availabilityError = ref("");
const pricePreview = ref<BookingPricePreview | null>(null);
const loadingPrice = ref(false);
const mySubscription = ref<MySubscription | null>(null);
const useSubscription = ref(false);
const matchedPartnerCarId = ref<number | null>(null);
const validatingSelection = ref(false);
let previewRequestId = 0;

const isDirectPartnerCar = computed(() => props.selection.kind === "partnerCar");
const directSelection = computed(() =>
  props.selection.kind === "partnerCar" ? props.selection : null,
);

const directPartnerMeta = computed(() => {
  if (!directSelection.value) {
    return null;
  }

  const parts = [];
  if (directSelection.value.carrierName?.trim()) {
    parts.push(`Перевозчик: ${directSelection.value.carrierName.trim()}`);
  }

  if (directSelection.value.licensePlate?.trim()) {
    parts.push(`Гос. номер: ${directSelection.value.licensePlate.trim()}`);
  }

  return parts.join(" | ") || null;
});

const minDate = computed(() => toLocalDateTimeInputValue(new Date()));

const directBusySlots = computed<BookingBusySlot[]>(() => {
  if (!directSelection.value) {
    return [];
  }

  return directSelection.value.busySlots
    .filter((slot: BookingBusySlot) => {
      const normalizedStatus = (slot.status ?? "").trim().toLowerCase();
      const endTime = new Date(slot.endTime).getTime();
      return ACTIVE_BOOKING_STATUSES.has(normalizedStatus) && endTime > Date.now();
    })
    .sort((left: BookingBusySlot, right: BookingBusySlot) => {
      const leftTime = new Date(left.startTime).getTime();
      const rightTime = new Date(right.startTime).getTime();
      return leftTime - rightTime;
    });
});

const busySlotPreview = computed(() => directBusySlots.value.slice(0, 4));

const duration = computed(() => {
  const start = parseLocalDate(startDate.value);
  const end = parseLocalDate(endDate.value);

  if (!start || !end) {
    return null;
  }

  const diffMs = end.getTime() - start.getTime();
  if (diffMs <= 0) {
    return null;
  }

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  return { days, hours, minutes, totalMinutes, diffMs };
});

const requestedDurationMs = computed(() =>
  Math.max(duration.value?.diffMs ?? DEFAULT_SLOT_DURATION_MS, MIN_DURATION_MS),
);

const suggestedAvailabilityWindows = computed(() => {
  if (!isDirectPartnerCar.value) {
    return [];
  }

  const anchor = parseLocalDate(startDate.value) ?? new Date();
  return buildAvailabilityWindows(
    directBusySlots.value,
    anchor,
    requestedDurationMs.value,
    4,
  );
});

watch(
  () => props.isOpen,
  async (isOpen) => {
    if (isOpen) {
      const now = new Date();
      startDate.value = toLocalDateTimeInputValue(now);
      endDate.value = toLocalDateTimeInputValue(
        new Date(now.getTime() + DEFAULT_SLOT_DURATION_MS),
      );

      validationError.value = "";
      availabilityError.value = "";
      pricePreview.value = null;
      matchedPartnerCarId.value = null;
      validatingSelection.value = false;
      useSubscription.value = false;

      await loadMySubscription();
    } else {
      previewRequestId += 1;
      pricePreview.value = null;
      matchedPartnerCarId.value = null;
      loadingPrice.value = false;
      availabilityError.value = "";
      mySubscription.value = null;
      validatingSelection.value = false;
      useSubscription.value = false;
    }
  },
);

watch(
  [
    startDate,
    endDate,
    () => props.selection.kind,
    () =>
      props.selection.kind === "model"
        ? props.selection.modelId
        : props.selection.partnerCarId,
    () => props.isOpen,
  ],
  () => {
    if (!props.isOpen) {
      return;
    }

    if (!isValid.value) {
      previewRequestId += 1;
      matchedPartnerCarId.value = null;
      pricePreview.value = null;
      availabilityError.value = "";
      return;
    }

    void refreshSelectionAvailability();
  },
);

const displayError = computed(() =>
  validationError.value || availabilityError.value || props.bookingError?.trim() || "",
);

const canConfirm = computed(
  () =>
    isValid.value &&
    matchedPartnerCarId.value !== null &&
    pricePreview.value !== null &&
    !loadingPrice.value,
);

const basePriceHour = computed(() => {
  if (!pricePreview.value) {
    return null;
  }

  return Number((pricePreview.value.marketValueKzt * HOURLY_BASE_FACTOR).toFixed(2));
});

const isValid = computed(() => {
  const start = parseLocalDate(startDate.value);
  const end = parseLocalDate(endDate.value);
  const now = new Date();

  if (!startDate.value || !endDate.value || !start || !end) {
    validationError.value = "Заполните обе даты";
    return false;
  }

  if (start < now) {
    validationError.value = "Дата начала не может быть в прошлом";
    return false;
  }

  if (end <= start) {
    validationError.value = "Дата окончания должна быть позже даты начала";
    return false;
  }

  if (end.getTime() - start.getTime() < MIN_DURATION_MS) {
    validationError.value = "Минимальная продолжительность аренды - 1 час";
    return false;
  }

  validationError.value = "";
  return true;
});

async function loadMySubscription() {
  try {
    const { data } = await api.get("/subscriptions/my");
    mySubscription.value = data;
  } catch {
    mySubscription.value = null;
  }
}

async function refreshSelectionAvailability(options?: {
  expectedPartnerCarId?: number | null;
}): Promise<RefreshSelectionResult> {
  const start = parseLocalDate(startDate.value);
  const end = parseLocalDate(endDate.value);

  if (!start || !end) {
    matchedPartnerCarId.value = null;
    pricePreview.value = null;
    return "unavailable";
  }

  const currentRequestId = ++previewRequestId;
  const startIso = start.toISOString();
  const endIso = end.toISOString();

  try {
    loadingPrice.value = true;
    availabilityError.value = "";
    matchedPartnerCarId.value = null;
    pricePreview.value = null;

    if (props.selection.kind === "partnerCar") {
      const availability = await getPartnerCarAvailability(
        props.selection.partnerCarId,
        startIso,
        endIso,
      );

      if (currentRequestId !== previewRequestId) {
        return "stale";
      }

      if (!availability.available) {
        availabilityError.value = buildDirectAvailabilityError(start);
        return "unavailable";
      }

      const preview = await getBookingPricePreview(
        props.selection.partnerCarId,
        startIso,
        endIso,
      );

      if (currentRequestId !== previewRequestId) {
        return "stale";
      }

      matchedPartnerCarId.value = props.selection.partnerCarId;
      pricePreview.value = preview;
      return "ready";
    }

    const matchResult = await matchCarByModel({
      modelId: props.selection.modelId,
      startTime: startIso,
      endTime: endIso,
    });

    if (currentRequestId !== previewRequestId) {
      return "stale";
    }

    if (!matchResult.isAvailable || !matchResult.partnerCarId) {
      availabilityError.value = buildModelAvailabilityError(
        matchResult.suggestedStartTimesUtc,
      );
      return "unavailable";
    }

    const preview = await getBookingPricePreview(
      matchResult.partnerCarId,
      startIso,
      endIso,
    );

    if (currentRequestId !== previewRequestId) {
      return "stale";
    }

    const matchChanged =
      options?.expectedPartnerCarId != null &&
      options.expectedPartnerCarId !== matchResult.partnerCarId;

    matchedPartnerCarId.value = matchResult.partnerCarId;
    pricePreview.value = preview;

    if (matchChanged) {
      availabilityError.value =
        "На эти даты подобралась другая машина. Мы обновили стоимость, проверьте её и нажмите «Забронировать» ещё раз.";
      return "changed";
    }

    return "ready";
  } catch (error) {
    console.error("Failed to refresh booking selection:", error);
    matchedPartnerCarId.value = null;
    pricePreview.value = null;
    availabilityError.value =
      props.selection.kind === "partnerCar"
        ? "Не удалось проверить доступность выбранной машины."
        : "Не удалось подобрать машину для расчёта стоимости.";
    return "unavailable";
  } finally {
    if (currentRequestId === previewRequestId) {
      loadingPrice.value = false;
    }
  }
}

function closeModal() {
  emit("close");
}

async function confirmBooking() {
  if (!canConfirm.value || matchedPartnerCarId.value === null) {
    return;
  }

  validatingSelection.value = true;

  try {
    const refreshResult = await refreshSelectionAvailability({
      expectedPartnerCarId: matchedPartnerCarId.value,
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

function buildModelAvailabilityError(suggestedStartTimesUtc?: string[]): string {
  const suggestions = (suggestedStartTimesUtc ?? [])
    .slice(0, 3)
    .map((value) => formatPointInTime(value))
    .filter(Boolean);

  if (suggestions.length === 0) {
    return "На выбранные даты машин этой модели нет.";
  }

  return `На выбранные даты машин этой модели нет. Ближайшие варианты: ${suggestions.join(", ")}.`;
}

function buildDirectAvailabilityError(anchorStart: Date): string {
  const suggestions = buildAvailabilityWindows(
    directBusySlots.value,
    anchorStart,
    requestedDurationMs.value,
    3,
  )
    .map((window) => formatSuggestionRange(window.startTime, window.endTime))
    .filter(Boolean);

  if (suggestions.length === 0) {
    return "Эта машина занята на выбранные даты. Попробуйте другой интервал.";
  }

  return `Эта машина занята на выбранные даты. Ближайшие свободные окна: ${suggestions.join(", ")}.`;
}

function buildAvailabilityWindows(
  busySlots: BookingBusySlot[],
  anchorStart: Date,
  durationMs: number,
  limit: number,
): AvailabilityWindow[] {
  const normalizedDurationMs = Math.max(durationMs, MIN_DURATION_MS);
  const results: AvailabilityWindow[] = [];
  let cursor = Math.max(anchorStart.getTime(), Date.now());

  const sortedBusySlots = busySlots
    .map((slot) => ({
      startTimeMs: new Date(slot.startTime).getTime(),
      endTimeMs: new Date(slot.endTime).getTime(),
    }))
    .filter((slot) => Number.isFinite(slot.startTimeMs) && Number.isFinite(slot.endTimeMs))
    .sort((left, right) => left.startTimeMs - right.startTimeMs);

  for (const busySlot of sortedBusySlots) {
    if (busySlot.endTimeMs <= cursor) {
      continue;
    }

    if (cursor < busySlot.startTimeMs) {
      let slotCursor = cursor;
      while (
        slotCursor + normalizedDurationMs <= busySlot.startTimeMs &&
        results.length < limit
      ) {
        results.push({
          startTime: new Date(slotCursor).toISOString(),
          endTime: new Date(slotCursor + normalizedDurationMs).toISOString(),
        });
        slotCursor += normalizedDurationMs;
      }
    }

    if (busySlot.endTimeMs > cursor) {
      cursor = busySlot.endTimeMs;
    }

    if (results.length >= limit) {
      return results;
    }
  }

  while (results.length < limit) {
    results.push({
      startTime: new Date(cursor).toISOString(),
      endTime: new Date(cursor + normalizedDurationMs).toISOString(),
    });
    cursor += normalizedDurationMs;
  }

  return results;
}

function applySuggestedWindow(window: AvailabilityWindow) {
  startDate.value = toLocalDateTimeInputValue(new Date(window.startTime));
  endDate.value = toLocalDateTimeInputValue(new Date(window.endTime));
}

function formatBusySlot(slot: BookingBusySlot): string {
  return formatSuggestionRange(slot.startTime, slot.endTime);
}

function formatSuggestionRange(startTime: string, endTime: string): string {
  const start = formatPointInTime(startTime);
  const end = formatPointInTime(endTime);
  return start && end ? `${start} - ${end}` : "";
}

function formatPointInTime(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatPriceAmount(value: number | null | undefined, currency: string): string {
  return formatMoney(value, currency);
}

function parseLocalDate(value: string): Date | null {
  if (!value) {
    return null;
  }

  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
}

function toLocalDateTimeInputValue(date: Date): string {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}
</script>
