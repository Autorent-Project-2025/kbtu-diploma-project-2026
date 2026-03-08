<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-6xl mx-auto space-y-8">
      <button
        @click="router.push('/bookings')"
        class="inline-flex items-center gap-2 text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white transition-colors"
      >
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7m0 0l7-7m-7 7h18" />
        </svg>
        <span class="font-medium">Назад к бронированиям</span>
      </button>

      <div class="grid lg:grid-cols-[1.2fr,0.8fr] gap-8">
        <section class="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl p-8 space-y-6">
          <div class="space-y-3">
            <p class="text-sm font-bold uppercase tracking-[0.25em] text-amber-600 dark:text-amber-400">
              Mock Payment
            </p>
            <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
              Завершите оплату бронирования
            </h1>
            <p class="text-gray-600 dark:text-gray-400 max-w-2xl">
              Это тестовый checkout. Карта не списывается, а результат определяется псевдо-данными.
            </p>
          </div>

          <div v-if="loading" class="py-16 text-center">
            <div class="inline-flex flex-col items-center gap-4">
              <div class="w-14 h-14 rounded-full border-4 border-amber-200 dark:border-amber-900 border-t-amber-600 dark:border-t-amber-400 animate-spin"></div>
              <p class="text-gray-600 dark:text-gray-400 font-medium">Подготавливаем mock checkout...</p>
            </div>
          </div>

          <template v-else-if="booking && payment">
            <div class="grid md:grid-cols-2 gap-4">
              <article class="p-5 rounded-2xl bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
                <p class="text-sm text-gray-500 dark:text-gray-400">Автомобиль</p>
                <p class="mt-2 text-2xl font-bold text-gray-900 dark:text-white">
                  {{ booking.carBrand }} {{ booking.carModel }}
                </p>
                <p class="mt-2 text-sm text-gray-600 dark:text-gray-400">
                  {{ formatDate(booking.startDate) }} -> {{ formatDate(booking.endDate) }}
                </p>
              </article>

              <article class="p-5 rounded-2xl bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-800">
                <p class="text-sm text-amber-700 dark:text-amber-300">К оплате</p>
                <p class="mt-2 text-3xl font-extrabold text-amber-900 dark:text-amber-100">
                  {{ formatAmount(payment.amount, payment.currency) }}
                </p>
                <p class="mt-2 text-sm text-amber-700/80 dark:text-amber-200/80">
                  Бронь останется в статусе `pending`, пока mock-оплата не станет успешной.
                </p>
              </article>
            </div>

            <div
              :class="[
                'rounded-2xl border px-5 py-4 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3',
                payment.paymentStatus === 'succeeded'
                  ? 'bg-emerald-50 dark:bg-emerald-900/20 border-emerald-200 dark:border-emerald-800'
                  : payment.paymentStatus === 'failed' || payment.paymentStatus === 'expired'
                    ? 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800'
                    : 'bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800',
              ]"
            >
              <div class="space-y-1">
                <p class="text-xs font-bold uppercase tracking-[0.2em]" :class="badgeTextClass">
                  Payment Status
                </p>
                <p class="text-lg font-bold text-gray-900 dark:text-white">
                  {{ getPaymentStatusText(payment.paymentStatus) }}
                </p>
                <p v-if="payment.failureReason" class="text-sm text-gray-600 dark:text-gray-300">
                  {{ payment.failureReason }}
                </p>
              </div>

              <div v-if="payment.cardLast4" class="text-sm text-gray-600 dark:text-gray-300">
                Последняя успешная/последняя карта: **** {{ payment.cardLast4 }}
              </div>
            </div>

            <div class="grid xl:grid-cols-[1fr,0.85fr] gap-6">
              <form
                class="space-y-5 p-6 rounded-3xl bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700"
                @submit.prevent="handleSubmit"
              >
                <div class="space-y-1">
                  <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Псевдо-данные карты</h2>
                  <p class="text-sm text-gray-600 dark:text-gray-400">
                    Храним только last4 и holder. Полный номер и CVV не сохраняются.
                  </p>
                </div>

                <label class="block space-y-2">
                  <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">Имя держателя</span>
                  <input
                    v-model="form.cardHolder"
                    type="text"
                    autocomplete="cc-name"
                    class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-amber-500"
                    placeholder="TEST USER"
                    :disabled="submitDisabled"
                  />
                </label>

                <label class="block space-y-2">
                  <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">Номер карты</span>
                  <input
                    v-model="form.cardNumber"
                    type="text"
                    inputmode="numeric"
                    autocomplete="cc-number"
                    class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-amber-500"
                    placeholder="4242 4242 4242 4242"
                    :disabled="submitDisabled"
                  />
                </label>

                <div class="grid sm:grid-cols-3 gap-4">
                  <label class="block space-y-2">
                    <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">Месяц</span>
                    <input
                      v-model="form.expiryMonth"
                      type="number"
                      min="1"
                      max="12"
                      class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-amber-500"
                      :disabled="submitDisabled"
                    />
                  </label>

                  <label class="block space-y-2">
                    <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">Год</span>
                    <input
                      v-model="form.expiryYear"
                      type="number"
                      :min="currentYear"
                      :max="currentYear + 20"
                      class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-amber-500"
                      :disabled="submitDisabled"
                    />
                  </label>

                  <label class="block space-y-2">
                    <span class="text-sm font-semibold text-gray-700 dark:text-gray-300">CVV</span>
                    <input
                      v-model="form.cvv"
                      type="password"
                      inputmode="numeric"
                      autocomplete="cc-csc"
                      class="w-full px-4 py-3 rounded-xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-700 text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-amber-500"
                      placeholder="123"
                      :disabled="submitDisabled"
                    />
                  </label>
                </div>

                <p v-if="formError" class="text-sm font-medium text-red-600 dark:text-red-400">
                  {{ formError }}
                </p>

                <div class="flex flex-wrap gap-3">
                  <button
                    type="submit"
                    class="px-6 py-3 rounded-xl bg-amber-600 hover:bg-amber-700 disabled:bg-gray-400 text-white font-bold transition-colors"
                    :disabled="submitDisabled || !payment.sessionKey"
                  >
                    {{ submitting ? "Обрабатываем..." : "Оплатить mock-картой" }}
                  </button>

                  <button
                    v-if="payment.canRetry"
                    type="button"
                    class="px-6 py-3 rounded-xl border border-gray-300 dark:border-gray-600 text-gray-800 dark:text-gray-100 font-semibold hover:border-amber-500"
                    :disabled="submitting"
                    @click="restartSession"
                  >
                    Новая сессия
                  </button>

                  <button
                    v-if="payment.paymentStatus === 'succeeded'"
                    type="button"
                    class="px-6 py-3 rounded-xl border border-emerald-300 dark:border-emerald-700 text-emerald-700 dark:text-emerald-300 font-semibold hover:bg-emerald-50 dark:hover:bg-emerald-900/20"
                    @click="router.push('/bookings')"
                  >
                    Перейти к бронированиям
                  </button>
                </div>
              </form>

              <aside class="space-y-4">
                <article class="p-6 rounded-3xl bg-gray-900 text-white shadow-xl">
                  <p class="text-sm uppercase tracking-[0.2em] text-amber-300 font-bold">Тестовые карты</p>
                  <div class="mt-5 space-y-4">
                    <button
                      v-for="sample in samples"
                      :key="sample.number"
                      type="button"
                      class="w-full text-left px-4 py-4 rounded-2xl bg-white/10 hover:bg-white/15 border border-white/10 transition-colors"
                      @click="applySample(sample.number)"
                    >
                      <p class="font-bold">{{ sample.number }}</p>
                      <p class="text-sm text-gray-200 mt-1">{{ sample.label }}</p>
                    </button>
                  </div>
                </article>

                <article class="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800">
                  <p class="text-sm uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 font-bold">Что произойдет</p>
                  <ol class="mt-4 space-y-3 text-sm text-gray-600 dark:text-gray-300">
                    <li>1. После успешного submit бронь станет `confirmed`.</li>
                    <li>2. `booking-service` положит outbox-событие в оплату.</li>
                    <li>3. `payment-service` начислит партнёру сумму в `pending`.</li>
                    <li>4. После `completed` эта сумма перейдёт в `available`.</li>
                  </ol>
                </article>
              </aside>
            </div>
          </template>

          <div v-else class="py-16 text-center">
            <p class="text-xl font-bold text-gray-900 dark:text-white">Бронь не найдена</p>
          </div>
        </section>

        <aside class="space-y-6">
          <article class="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl p-6">
            <p class="text-sm uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 font-bold">Статус брони</p>
            <p class="mt-3 text-3xl font-extrabold text-gray-900 dark:text-white">
              {{ booking ? getBookingStatusText(booking.status) : "..." }}
            </p>
            <p class="mt-3 text-sm text-gray-600 dark:text-gray-400">
              Пока статус `pending`, эта бронь удерживает слот автомобиля, но партнёру деньги ещё не начислены.
            </p>
          </article>

          <article class="bg-gradient-to-br from-amber-500 to-orange-600 rounded-3xl shadow-xl p-6 text-white">
            <p class="text-sm uppercase tracking-[0.2em] font-bold text-amber-100">Mock Rules</p>
            <ul class="mt-4 space-y-3 text-sm text-amber-50">
              <li>`4242 4242 4242 4242` -> success</li>
              <li>`4000 0000 0000 0002` -> declined</li>
              <li>`4000 0000 0000 9995` -> insufficient funds</li>
              <li>Любой другой валидный 16-значный номер -> success</li>
            </ul>
          </article>
        </aside>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from "axios";
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getBooking,
  getBookingPaymentStatus,
  startBookingPayment,
  submitBookingPayment,
} from "../api/booking";
import { useToast } from "../composables/useToast";
import type { Booking, BookingPaymentState, BookingPaymentStatus } from "../types/Booking";

const route = useRoute();
const router = useRouter();
const { success, error } = useToast();

const booking = ref<Booking | null>(null);
const payment = ref<BookingPaymentStatus | null>(null);
const loading = ref(true);
const submitting = ref(false);
const formError = ref("");

const currentYear = new Date().getFullYear();
const form = reactive({
  cardHolder: "TEST USER",
  cardNumber: "4242 4242 4242 4242",
  expiryMonth: new Date().getMonth() + 1,
  expiryYear: currentYear + 1,
  cvv: "123",
});

const samples = [
  { number: "4242 4242 4242 4242", label: "Успешная оплата" },
  { number: "4000 0000 0000 0002", label: "Карта отклонена" },
  { number: "4000 0000 0000 9995", label: "Недостаточно средств" },
];

const submitDisabled = computed(() => {
  if (!payment.value) {
    return true;
  }

  return (
    submitting.value ||
    payment.value.paymentStatus === "succeeded" ||
    payment.value.bookingStatus === "canceled" ||
    !payment.value.requiresInput
  );
});

onMounted(async () => {
  await loadCheckout();
});

async function loadCheckout(startSession = true) {
  loading.value = true;
  formError.value = "";

  try {
    const bookingId = Number(route.params.id);
    if (Number.isNaN(bookingId) || bookingId <= 0) {
      throw new Error("Некорректный идентификатор бронирования.");
    }

    booking.value = await getBooking(bookingId);
    payment.value = startSession
      ? await startBookingPayment(bookingId)
      : await getBookingPaymentStatus(bookingId);
  } catch (e) {
    console.error("Failed to load booking payment page", e);
    error(resolveErrorMessage(e, "Не удалось открыть страницу оплаты."));
    booking.value = null;
    payment.value = null;
  } finally {
    loading.value = false;
  }
}

async function handleSubmit() {
  if (!booking.value || !payment.value?.sessionKey) {
    return;
  }

  submitting.value = true;
  formError.value = "";

  try {
    payment.value = await submitBookingPayment(booking.value.id, {
      sessionKey: payment.value.sessionKey,
      cardHolder: form.cardHolder,
      cardNumber: form.cardNumber,
      expiryMonth: Number(form.expiryMonth),
      expiryYear: Number(form.expiryYear),
      cvv: form.cvv,
    });

    booking.value = await getBooking(booking.value.id);

    if (payment.value.paymentStatus === "succeeded") {
      success("Mock-оплата успешна. Бронь подтверждена.");
      return;
    }

    error(getPaymentStatusText(payment.value.paymentStatus));
  } catch (e) {
    console.error("Failed to submit mock payment", e);
    formError.value = resolveErrorMessage(e, "Не удалось выполнить mock-оплату.");
  } finally {
    submitting.value = false;
  }
}

async function restartSession() {
  if (!booking.value) {
    return;
  }

  try {
    payment.value = await startBookingPayment(booking.value.id);
    formError.value = "";
  } catch (e) {
    console.error("Failed to restart mock payment session", e);
    error(resolveErrorMessage(e, "Не удалось создать новую платёжную сессию."));
  }
}

function applySample(cardNumber: string) {
  form.cardNumber = cardNumber;
}

function formatDate(value: string): string {
  const date = new Date(value);
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatAmount(amount: number | null | undefined, currency: string): string {
  if (amount == null) {
    return "Сумма не рассчитана";
  }

  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function getPaymentStatusText(status: BookingPaymentState): string {
  switch (status) {
    case "started":
      return "Ожидает ввода данных карты";
    case "succeeded":
      return "Оплата подтверждена";
    case "failed":
      return "Оплата не прошла";
    case "expired":
      return "Сессия истекла";
    case "canceled":
      return "Платёж отменён";
    default:
      return "Сессия ещё не начата";
  }
}

function getBookingStatusText(status: Booking["status"]): string {
  switch (status) {
    case "pending":
      return "Ожидает оплаты";
    case "confirmed":
      return "Подтверждено";
    case "active":
      return "Активно";
    case "completed":
      return "Завершено";
    case "canceled":
      return "Отменено";
    default:
      return "Неизвестно";
  }
}

const badgeTextClass = computed(() => {
  switch (payment.value?.paymentStatus) {
    case "succeeded":
      return "text-emerald-700 dark:text-emerald-300";
    case "failed":
    case "expired":
      return "text-red-700 dark:text-red-300";
    default:
      return "text-blue-700 dark:text-blue-300";
  }
});

function resolveErrorMessage(value: unknown, fallback: string): string {
  if (axios.isAxiosError(value)) {
    const detail =
      value.response?.data?.detail ??
      value.response?.data?.error ??
      value.response?.data?.message;

    if (typeof detail === "string" && detail.trim()) {
      return detail;
    }
  }

  if (value instanceof Error && value.message.trim()) {
    return value.message;
  }

  return fallback;
}
</script>
