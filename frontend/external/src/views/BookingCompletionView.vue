<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-5xl mx-auto space-y-8">
      <div class="flex items-center justify-between gap-4">
        <div class="space-y-2">
          <button
            type="button"
            class="inline-flex items-center gap-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
            @click="router.push('/bookings')"
          >
            <span>&larr;</span>
            <span>К списку бронирований</span>
          </button>
          <h1 class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white">
            Завершение поездки
          </h1>
          <p class="text-lg text-gray-600 dark:text-gray-400">
            Бронирование #{{ bookingId }}
          </p>
        </div>

        <span
          v-if="booking"
          :class="statusClass(booking.status)"
          class="inline-flex items-center gap-2 rounded-2xl px-5 py-3 text-sm font-extrabold uppercase tracking-[0.16em]"
        >
          <span class="h-2.5 w-2.5 rounded-full bg-current opacity-80"></span>
          {{ statusLabel(booking.status) }}
        </span>
      </div>

      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-center text-gray-500 dark:text-gray-400"
      >
        Загружаем бронирование...
      </div>

      <div
        v-else-if="loadError"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-6 text-red-700 dark:text-red-300"
      >
        {{ loadError }}
      </div>

      <template v-else-if="booking">
        <section class="grid gap-4 md:grid-cols-2 xl:grid-cols-4">
          <article class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-2">
            <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Плановое начало</p>
            <p class="text-lg font-bold text-gray-900 dark:text-white">{{ formatDateTime(booking.startDate) }}</p>
          </article>

          <article class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-2">
            <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Плановое завершение</p>
            <p class="text-lg font-bold text-gray-900 dark:text-white">{{ formatDateTime(booking.endDate) }}</p>
          </article>

          <article class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-2">
            <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Фактический старт</p>
            <p class="text-lg font-bold text-gray-900 dark:text-white">
              {{ booking.tripStartedAt ? formatDateTime(booking.tripStartedAt) : "Не начата" }}
            </p>
          </article>

          <article class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-2">
            <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Фактическая длительность</p>
            <p class="text-lg font-bold text-gray-900 dark:text-white">
              {{ tripDurationText || "Появится после старта поездки" }}
            </p>
          </article>
        </section>

        <section
          v-if="booking.status === 'active' && isEarlyFinish"
          class="rounded-3xl border border-amber-300/70 dark:border-amber-500/30 bg-amber-50 dark:bg-amber-900/20 shadow-xl p-6 space-y-2"
        >
          <p class="text-sm font-bold uppercase tracking-[0.18em] text-amber-700 dark:text-amber-300">
            Раннее завершение
          </p>
          <p class="text-base text-amber-900 dark:text-amber-100">
            У вас ещё есть оплачённое время до {{ formatDateTime(booking.endDate) }}. Завершить поездку можно раньше, но деньги за оставшееся время не возвращаются.
          </p>
          <p class="text-sm text-amber-700 dark:text-amber-300">
            Осталось примерно {{ remainingTimeText }}.
          </p>
        </section>

        <section
          v-if="booking.status === 'active'"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6"
        >
          <div class="space-y-2">
            <p class="text-sm font-bold uppercase tracking-[0.18em] text-primary-600 dark:text-primary-400">
              Форма завершения
            </p>
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div class="space-y-2">
                <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
                  Загрузите фото машины после поездки
                </h2>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  Нужны ровно 5 обязательных фото: спереди, сзади, левый бок, правый бок и салон.
                </p>
              </div>
              <div class="rounded-2xl border border-primary-200 dark:border-primary-500/30 bg-primary-50 dark:bg-primary-500/10 px-4 py-3 text-sm font-semibold text-primary-700 dark:text-primary-300">
                Загружено {{ completedPhotoCount }} / {{ completionPhotoFields.length }}
              </div>
            </div>
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <article
              v-for="field in completionPhotoFields"
              :key="field.key"
              class="rounded-3xl border-2 p-4 sm:p-5 transition-colors"
              :class="
                activeDropzoneKey === field.key
                  ? 'border-primary-500 bg-primary-50 dark:border-primary-500 dark:bg-primary-500/10'
                  : selectedFiles[field.key]
                    ? 'border-emerald-300 bg-emerald-50 dark:border-emerald-500/40 dark:bg-emerald-500/10'
                    : 'border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50'
              "
              @dragover.prevent="activeDropzoneKey = field.key"
              @dragleave.prevent="activeDropzoneKey = null"
              @drop.prevent="onFileDrop(field.key, $event)"
            >
              <div class="flex items-start justify-between gap-3">
                <div class="space-y-1">
                  <p class="font-semibold text-gray-900 dark:text-white">{{ field.label }}</p>
                  <p class="text-sm text-gray-500 dark:text-gray-400">
                    {{ selectedFileName(field.key) || "Перетащите файл сюда или выберите вручную" }}
                  </p>
                </div>
                <span
                  class="shrink-0 rounded-full px-3 py-1 text-xs font-bold uppercase tracking-[0.16em]"
                  :class="
                    selectedFiles[field.key]
                      ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300'
                      : 'bg-gray-200 text-gray-600 dark:bg-gray-700 dark:text-gray-300'
                  "
                >
                  {{ selectedFiles[field.key] ? "Готово" : "Нужно" }}
                </span>
              </div>

              <div
                class="overflow-hidden rounded-2xl border border-gray-200/80 dark:border-gray-700/80 bg-white/80 dark:bg-gray-900/60"
              >
                <div
                  v-if="photoPreviewUrls[field.key]"
                  class="aspect-[4/3] bg-gray-100 dark:bg-gray-800"
                >
                  <img
                    :src="photoPreviewUrls[field.key] || undefined"
                    :alt="field.label"
                    class="h-full w-full object-cover"
                  />
                </div>
                <div
                  v-else
                  class="aspect-[4/3] flex flex-col items-center justify-center gap-3 px-6 text-center text-gray-400 dark:text-gray-500"
                >
                  <svg
                    class="h-10 w-10"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.6"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M2.25 15.75l5.159-5.159a2.25 2.25 0 013.182 0l5.159 5.159m-1.5-1.5l1.409-1.409a2.25 2.25 0 013.182 0l2.909 2.909M3 10.5a7.5 7.5 0 1115 0 7.5 7.5 0 01-15 0z"
                    />
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M12 16.5v-9m-4.5 4.5h9"
                    />
                  </svg>
                  <p class="text-sm font-medium">
                    JPG, PNG или WEBP
                  </p>
                </div>
              </div>

              <div class="flex flex-wrap items-center gap-3">
                <button
                  type="button"
                  class="px-4 py-2.5 rounded-2xl bg-gray-900 text-white dark:bg-white dark:text-gray-900 text-sm font-semibold transition-colors hover:opacity-90"
                  @click="openFilePicker(field.key)"
                >
                  {{ selectedFiles[field.key] ? "Заменить фото" : "Выбрать фото" }}
                </button>
                <button
                  v-if="selectedFiles[field.key]"
                  type="button"
                  class="px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 transition-colors hover:border-red-400 hover:text-red-600 dark:hover:text-red-400"
                  @click="clearSelectedFile(field.key)"
                >
                  Удалить
                </button>
              </div>

              <input
                :id="`completion-input-${field.key}`"
                type="file"
                accept="image/*"
                class="hidden"
                @change="onFileSelected(field.key, $event)"
              />
            </article>
          </div>

          <div class="flex flex-col sm:flex-row sm:items-center gap-3">
            <button
              type="button"
              class="px-6 py-3 rounded-2xl bg-primary-600 hover:bg-primary-700 disabled:opacity-60 text-white font-bold transition-colors"
              :disabled="submitting || !canSubmitCompletionReview"
              @click="submitCompletion"
            >
              {{ submitting ? "Отправляем..." : "Завершить поездку" }}
            </button>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              После отправки заявка уйдёт консультанту на проверку.
            </p>
          </div>
        </section>

        <section
          v-else-if="booking.status === 'awaitingReview'"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6"
        >
          <div class="space-y-2">
            <p class="text-sm font-bold uppercase tracking-[0.18em] text-violet-600 dark:text-violet-400">
              На проверке
            </p>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
              Заявка передана консультанту
            </h2>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Консультант проверит фотографии и либо подтвердит завершение, либо выставит начисления.
            </p>
          </div>

          <div
            v-if="pendingCharges.length === 0"
            class="rounded-2xl border border-violet-200/70 dark:border-violet-700/40 bg-violet-50 dark:bg-violet-900/20 p-5 text-violet-900 dark:text-violet-100"
          >
            Начислений пока нет. Дождитесь решения консультанта.
          </div>

          <div v-else class="space-y-4">
            <article
              v-for="charge in orderedCharges"
              :key="charge.id"
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 p-5 flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4"
            >
              <div class="space-y-2">
                <p class="text-xs font-bold uppercase tracking-[0.18em]" :class="chargeBadgeClass(charge)">
                  {{ chargeTypeLabel(charge.chargeType) }}
                </p>
                <p class="text-lg font-bold text-gray-900 dark:text-white">
                  {{ formatMoney(charge.amount, charge.currency) }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ charge.description || "Начисление по бронированию" }}
                </p>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  Статус: {{ chargeStatusLabel(charge.status) }}
                </p>
              </div>

              <button
                v-if="isChargePending(charge)"
                type="button"
                class="px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white font-bold transition-colors"
                :disabled="payingChargeId === charge.id"
                @click="payCharge(charge.id)"
              >
                {{ payingChargeId === charge.id ? "Оплачиваем..." : "Оплатить начисление" }}
              </button>
            </article>
          </div>
        </section>

        <section
          v-else-if="booking.status === 'completed'"
          class="rounded-3xl border border-emerald-200/70 dark:border-emerald-700/40 bg-emerald-50 dark:bg-emerald-900/20 shadow-xl p-6 sm:p-8 space-y-4"
        >
          <p class="text-sm font-bold uppercase tracking-[0.18em] text-emerald-700 dark:text-emerald-300">
            Поездка завершена
          </p>
          <h2 class="text-2xl font-bold text-emerald-950 dark:text-emerald-50">
            Бронирование полностью закрыто
          </h2>
          <p class="text-sm text-emerald-800 dark:text-emerald-200">
            Консультант подтвердил завершение, а все начисления по поездке закрыты.
          </p>

          <div class="flex flex-wrap items-center gap-3">
            <button
              v-if="booking.canLeaveComment"
              type="button"
              class="px-5 py-3 rounded-2xl bg-sky-600 hover:bg-sky-700 disabled:opacity-60 text-white font-bold transition-colors"
              :disabled="reviewSubmitting"
              @click="showReviewModal = true"
            >
              {{ reviewSubmitting ? "Отправляем отзыв..." : "Оставить отзыв" }}
            </button>

            <div
              v-else-if="booking.carCommentId"
              class="rounded-2xl border border-sky-200/70 dark:border-sky-700/40 bg-sky-50 dark:bg-sky-900/20 px-4 py-3 text-sm font-semibold text-sky-800 dark:text-sky-200"
            >
              Отзыв уже оставлен.
            </div>
          </div>

          <div v-if="orderedCharges.length > 0" class="grid gap-3">
            <article
              v-for="charge in orderedCharges"
              :key="charge.id"
              class="rounded-2xl border border-emerald-200/60 dark:border-emerald-700/40 bg-white/70 dark:bg-gray-900/60 p-4 flex items-center justify-between gap-4"
            >
              <div>
                <p class="font-semibold text-gray-900 dark:text-white">
                  {{ chargeTypeLabel(charge.chargeType) }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ chargeStatusLabel(charge.status) }}
                </p>
              </div>
              <p class="font-bold text-gray-900 dark:text-white">
                {{ formatMoney(charge.amount, charge.currency) }}
              </p>
            </article>
          </div>
        </section>

        <section
          v-else
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 text-gray-600 dark:text-gray-400"
        >
          Для этого статуса отдельная форма завершения недоступна.
        </section>
      </template>
    </div>

    <ReviewModal
      v-if="booking"
      :is-open="showReviewModal"
      :subject="reviewSubject"
      :submitting="reviewSubmitting"
      @close="closeReviewModal"
      @submit="handleReviewSubmit"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getBooking,
  getBookingCharges,
  payBookingCharge,
  submitBookingCarComment,
  submitBookingCompletionReview,
} from "../api/booking";
import type { Booking, BookingCharge } from "../types/Booking";
import { getTripDuration } from "../utils/bookingUtils";
import { useToast } from "../composables/useToast";
import ReviewModal from "../components/ReviewModal.vue";

type CompletionPhotoKey =
  | "completionFrontPhotoFile"
  | "completionBackPhotoFile"
  | "completionSideLeftPhotoFile"
  | "completionSideRightPhotoFile"
  | "completionInteriorPhotoFile";

const route = useRoute();
const router = useRouter();
const { success, error } = useToast();

const bookingId = Number(route.params.id);
const booking = ref<Booking | null>(null);
const charges = ref<BookingCharge[]>([]);
const loading = ref(true);
const loadError = ref<string | null>(null);
const submitting = ref(false);
const payingChargeId = ref<number | null>(null);
const showReviewModal = ref(false);
const reviewSubmitting = ref(false);
const now = ref(Date.now());

const completionPhotoFields: Array<{ key: CompletionPhotoKey; label: string }> = [
  { key: "completionFrontPhotoFile", label: "Фото спереди" },
  { key: "completionBackPhotoFile", label: "Фото сзади" },
  { key: "completionSideLeftPhotoFile", label: "Левый бок" },
  { key: "completionSideRightPhotoFile", label: "Правый бок" },
  { key: "completionInteriorPhotoFile", label: "Салон" },
];

function createEmptySelectedFiles(): Record<CompletionPhotoKey, File | null> {
  return {
    completionFrontPhotoFile: null,
    completionBackPhotoFile: null,
    completionSideLeftPhotoFile: null,
    completionSideRightPhotoFile: null,
    completionInteriorPhotoFile: null,
  };
}

function createEmptyPreviewUrls(): Record<CompletionPhotoKey, string | null> {
  return {
    completionFrontPhotoFile: null,
    completionBackPhotoFile: null,
    completionSideLeftPhotoFile: null,
    completionSideRightPhotoFile: null,
    completionInteriorPhotoFile: null,
  };
}

const selectedFiles = ref<Record<CompletionPhotoKey, File | null>>(
  createEmptySelectedFiles()
);
const photoPreviewUrls = ref<Record<CompletionPhotoKey, string | null>>(
  createEmptyPreviewUrls()
);
const activeDropzoneKey = ref<CompletionPhotoKey | null>(null);

let timerId: number | null = null;

const tripDuration = computed(() =>
  booking.value
    ? getTripDuration(
        booking.value.tripStartedAt ?? null,
        booking.value.tripCompletedAt ?? null,
        new Date(now.value)
      )
    : null
);

const tripDurationText = computed(() => formatDuration(tripDuration.value));

const remainingTimeText = computed(() => {
  if (!booking.value) return "";
  const diffMs = new Date(booking.value.endDate).getTime() - now.value;
  if (diffMs <= 0) {
    return "0 мин.";
  }

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  if (hours > 0) {
    return `${hours} ч. ${minutes} мин.`;
  }

  return `${minutes} мин.`;
});

const isEarlyFinish = computed(() => {
  if (!booking.value || booking.value.status !== "active") {
    return false;
  }

  return now.value < new Date(booking.value.endDate).getTime();
});

const pendingCharges = computed(() =>
  charges.value.filter((charge) => isChargePending(charge))
);

const orderedCharges = computed(() =>
  [...charges.value].sort((left, right) => {
    if (isChargePending(left) && !isChargePending(right)) return -1;
    if (!isChargePending(left) && isChargePending(right)) return 1;
    return new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime();
  })
);

const canSubmitCompletionReview = computed(() =>
  completionPhotoFields.every((field) => selectedFiles.value[field.key] instanceof File)
);

const completedPhotoCount = computed(
  () => completionPhotoFields.filter((field) => selectedFiles.value[field.key]).length
);

const reviewSubject = computed(() => ({
  brand: booking.value?.carBrand ?? "",
  model: booking.value?.carModel ?? "",
  year: null,
}));

onMounted(async () => {
  if (!Number.isFinite(bookingId) || bookingId <= 0) {
    loadError.value = "Некорректный идентификатор бронирования.";
    loading.value = false;
    return;
  }

  await loadBookingDetails();
  timerId = window.setInterval(() => {
    now.value = Date.now();
  }, 30_000);
});

onBeforeUnmount(() => {
  if (timerId !== null) {
    window.clearInterval(timerId);
  }

  revokeAllPreviewUrls();
});

async function loadBookingDetails() {
  try {
    loading.value = true;
    loadError.value = null;

    const currentBooking = await getBooking(bookingId);
    booking.value = currentBooking;

    if (
      currentBooking.status === "awaitingReview" ||
      currentBooking.status === "completed"
    ) {
      charges.value = await getBookingCharges(bookingId);
    } else {
      charges.value = [];
    }
  } catch (e) {
    console.error("Failed to load booking completion view", e);
    loadError.value = "Не удалось загрузить данные по завершению поездки.";
  } finally {
    loading.value = false;
  }
}

function revokePreviewUrl(key: CompletionPhotoKey) {
  const previewUrl = photoPreviewUrls.value[key];
  if (previewUrl) {
    URL.revokeObjectURL(previewUrl);
    photoPreviewUrls.value[key] = null;
  }
}

function revokeAllPreviewUrls() {
  completionPhotoFields.forEach((field) => revokePreviewUrl(field.key));
}

function setSelectedFile(key: CompletionPhotoKey, file: File | null) {
  revokePreviewUrl(key);
  selectedFiles.value[key] = null;

  if (!file) {
    return;
  }

  if (!file.type.startsWith("image/")) {
    error("Можно загрузить только изображения.");
    return;
  }

  selectedFiles.value[key] = file;
  photoPreviewUrls.value[key] = URL.createObjectURL(file);
}

function onFileSelected(key: CompletionPhotoKey, event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0] ?? null;
  input.value = "";
  setSelectedFile(key, file);
}

function onFileDrop(key: CompletionPhotoKey, event: DragEvent) {
  activeDropzoneKey.value = null;
  const file = event.dataTransfer?.files?.[0] ?? null;
  setSelectedFile(key, file);
}

function openFilePicker(key: CompletionPhotoKey) {
  const input = document.getElementById(`completion-input-${key}`) as
    | HTMLInputElement
    | null;
  input?.click();
}

function clearSelectedFile(key: CompletionPhotoKey) {
  setSelectedFile(key, null);
}

function selectedFileName(key: CompletionPhotoKey): string {
  return selectedFiles.value[key]?.name ?? "";
}

async function submitCompletion() {
  if (!booking.value) {
    return;
  }

  if (!canSubmitCompletionReview.value) {
    error("Загрузите все 5 обязательных фотографий.");
    return;
  }

  if (
    isEarlyFinish.value &&
    !window.confirm("У вас ещё есть оплаченное время. Вы уверены, что хотите завершить поездку раньше?")
  ) {
    return;
  }

  submitting.value = true;
  try {
    const result = await submitBookingCompletionReview(booking.value.id, {
      completionFrontPhotoFile: selectedFiles.value.completionFrontPhotoFile as File,
      completionBackPhotoFile: selectedFiles.value.completionBackPhotoFile as File,
      completionSideLeftPhotoFile: selectedFiles.value.completionSideLeftPhotoFile as File,
      completionSideRightPhotoFile: selectedFiles.value.completionSideRightPhotoFile as File,
      completionInteriorPhotoFile: selectedFiles.value.completionInteriorPhotoFile as File,
    });

    booking.value = result.booking;
    charges.value = [];
    revokeAllPreviewUrls();
    selectedFiles.value = createEmptySelectedFiles();
    photoPreviewUrls.value = createEmptyPreviewUrls();
    success(
      result.latePenaltyAmount > 0
        ? `Поездка отправлена на проверку. Предварительная пеня: ${formatMoney(result.latePenaltyAmount)}`
        : "Поездка отправлена на проверку консультанту."
    );
  } catch (e: any) {
    console.error("Failed to submit booking completion review", e);
    error(
      e?.response?.data?.detail ||
        e?.response?.data?.error ||
        "Не удалось отправить завершение поездки."
    );
  } finally {
    submitting.value = false;
  }
}

async function payCharge(chargeId: number) {
  if (!booking.value) {
    return;
  }

  payingChargeId.value = chargeId;
  try {
    await payBookingCharge(booking.value.id, chargeId);
    success("Начисление успешно оплачено.");
    await loadBookingDetails();
  } catch (e: any) {
    console.error("Failed to pay booking charge", e);
    error(
      e?.response?.data?.detail ||
        e?.response?.data?.error ||
        "Не удалось оплатить начисление."
    );
  } finally {
    payingChargeId.value = null;
  }
}

function closeReviewModal() {
  if (reviewSubmitting.value) {
    return;
  }

  showReviewModal.value = false;
}

async function handleReviewSubmit(rating: number, content: string) {
  if (!booking.value) {
    return;
  }

  reviewSubmitting.value = true;
  try {
    const result = await submitBookingCarComment(booking.value.id, {
      rating,
      content,
    });

    booking.value = result.booking;
    showReviewModal.value = false;
    success("Отзыв успешно опубликован.");
  } catch (e: any) {
    console.error("Failed to submit booking car comment", e);
    error(
      e?.response?.data?.detail ||
        e?.response?.data?.error ||
        "Не удалось отправить отзыв."
    );
  } finally {
    reviewSubmitting.value = false;
  }
}

function isChargePending(charge: BookingCharge) {
  return charge.status.trim().toLowerCase() === "pending";
}

function chargeTypeLabel(chargeType: string) {
  const normalized = chargeType.trim().toLowerCase();
  if (normalized === "latepenalty") return "Пеня за опоздание";
  if (normalized === "damagefine") return "Штраф за повреждение";
  return chargeType;
}

function chargeStatusLabel(status: string) {
  const normalized = status.trim().toLowerCase();
  if (normalized === "pending") return "Ожидает оплаты";
  if (normalized === "paid") return "Оплачен";
  if (normalized === "canceled") return "Отменён";
  return status;
}

function chargeBadgeClass(charge: BookingCharge) {
  const normalized = charge.chargeType.trim().toLowerCase();
  if (normalized === "damagefine") {
    return "text-red-700 dark:text-red-300";
  }

  if (normalized === "latepenalty") {
    return "text-amber-700 dark:text-amber-300";
  }

  return "text-gray-700 dark:text-gray-300";
}

function statusLabel(status: Booking["status"]) {
  switch (status) {
    case "pending":
      return "Ожидает оплаты";
    case "confirmed":
      return "Подтверждено";
    case "active":
      return "В поездке";
    case "awaitingReview":
      return "На проверке";
    case "completed":
      return "Завершено";
    case "canceled":
      return "Отменено";
    default:
      return status;
  }
}

function statusClass(status: Booking["status"]) {
  switch (status) {
    case "active":
      return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
    case "awaitingReview":
      return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
    case "completed":
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
    case "confirmed":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case "pending":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case "canceled":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
}

function formatMoney(amount: number, currency = "KZT") {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function formatDateTime(value: string) {
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value));
}

function formatDuration(
  duration: { days: number; hours: number; minutes: number } | null
) {
  if (!duration) {
    return "";
  }

  const parts: string[] = [];
  if (duration.days > 0) parts.push(`${duration.days} дн.`);
  if (duration.hours > 0) parts.push(`${duration.hours} ч.`);
  if (duration.minutes > 0 || parts.length === 0) parts.push(`${duration.minutes} мин.`);
  return parts.join(" ");
}
</script>
