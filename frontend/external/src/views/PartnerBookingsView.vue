<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto space-y-8">
      <header
        class="relative overflow-hidden rounded-[32px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8 sm:p-10"
      >
        <div
          class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-6"
        >
          <div class="space-y-4 max-w-3xl">
            <p
              class="text-sm font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
            >
              Partner Dashboard
            </p>
            <h1
              class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
            >
              Финансы и клиентские бронирования
            </h1>
            <p class="text-lg text-gray-600 dark:text-gray-300">
              Доступный баланс, прибыль по дням и все бронирования по вашим
              машинам.
            </p>
          </div>

          <div class="flex flex-col items-start gap-1 shrink-0">
            <p
              class="text-xs font-semibold text-gray-400 dark:text-gray-600 uppercase tracking-wider px-1"
            >
              Перейти
            </p>
            <div class="flex items-center gap-1">
              <router-link
                to="/partner/me"
                class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 hover:text-emerald-700 dark:hover:text-emerald-300 border border-gray-200 dark:border-gray-700 transition-colors"
              >
                <svg
                  class="w-3.5 h-3.5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                  />
                </svg>
                Профиль
              </router-link>
              <router-link
                to="/partner/cars"
                class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl text-sm font-semibold text-gray-700 dark:text-gray-300 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 hover:text-emerald-700 dark:hover:text-emerald-300 border border-gray-200 dark:border-gray-700 transition-colors"
              >
                <svg
                  class="w-3.5 h-3.5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13 10V3L4 14h7v7l9-11h-7z"
                  />
                </svg>
                Мои машины
              </router-link>
            </div>
          </div>
        </div>
      </header>

      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-10 text-center"
      >
        <div
          class="w-8 h-8 rounded-full border-2 border-emerald-500 border-t-transparent animate-spin mx-auto mb-3"
        ></div>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          Загружаем данные партнёра...
        </p>
      </div>

      <div
        v-else-if="errorMessage"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-8 text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </div>

      <template v-else>
        <PartnerBookingsPeriodSelector
          v-model:selected-period="selectedPeriod"
          :period-options="periodOptions"
        />

        <section class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
          <article
            class="rounded-3xl border border-emerald-200/70 dark:border-emerald-700/40 bg-white dark:bg-gray-900 shadow-xl p-6"
          >
            <div class="flex items-start justify-between mb-3">
              <p
                class="text-xs uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-400 font-bold"
              >
                Доступный баланс
              </p>
              <div
                class="w-7 h-7 rounded-lg bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center"
              >
                <svg
                  class="w-3.5 h-3.5 text-emerald-600 dark:text-emerald-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
            </div>
            <p class="text-3xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.availableAmount) }}
            </p>
            <p class="text-xs text-gray-500 dark:text-gray-400 mt-2">
              Можно выводить прямо сейчас
            </p>
          </article>

          <article
            class="rounded-3xl border border-amber-200/70 dark:border-amber-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-amber-600 dark:text-amber-400 font-bold"
            >
              Pending
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.pendingAmount) }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Уже начислено, но ещё не доступно к payout.
            </p>
          </article>

          <article
            class="rounded-3xl border border-blue-200/70 dark:border-blue-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-blue-600 dark:text-blue-400 font-bold"
            >
              Reserved
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.reservedAmount) }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Уже зарезервировано под выплату.
            </p>
          </article>

          <article
            class="rounded-3xl border border-violet-200/70 dark:border-violet-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-violet-600 dark:text-violet-400 font-bold"
            >
              Бронирований за период
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ bookingsInSelectedPeriod.length }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Последние {{ selectedPeriod }} дней · Completed:
              {{ completedBookingsCount }} · Активных: {{ activeBookingsCount }}
            </p>
          </article>
        </section>

        <section class="grid xl:grid-cols-[1.1fr,0.9fr] gap-6">
          <article
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
          >
            <div class="flex items-start justify-between gap-4">
              <div>
                <p
                  class="text-sm uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 font-bold"
                >
                  Чистая прибыль по дням
                </p>
                <h2
                  class="mt-2 text-2xl font-bold text-gray-900 dark:text-white"
                >
                  Последние {{ selectedPeriod }} дней
                </h2>
              </div>
              <div class="text-right">
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  За период
                </p>
                <p
                  class="text-xl font-bold text-emerald-600 dark:text-emerald-400"
                >
                  {{ formatMoney(totalRealizedProfit) }}
                </p>
              </div>
            </div>

            <div class="space-y-2">
              <!-- Bars -->
              <div
                class="h-72 flex items-end"
                :class="selectedPeriod <= 14 ? 'gap-3' : 'gap-1'"
              >
                <div
                  v-for="point in profitPoints"
                  :key="point.isoDate"
                  class="flex-1 min-w-0 h-full flex items-end"
                >
                  <div
                    class="w-full bg-gradient-to-t from-emerald-600 to-emerald-400 dark:from-emerald-500 dark:to-emerald-300 transition-all duration-300"
                    :class="selectedPeriod <= 14 ? 'rounded-t-2xl' : 'rounded-t-lg'"
                    :style="{
                      height: `${point.heightPercent}%`,
                      minHeight: point.amount > 0 ? '8px' : '3px',
                    }"
                    :title="`${point.label}: ${formatMoney(point.amount)}`"
                  ></div>
                </div>
              </div>
              <!-- Labels -->
              <div
                class="flex"
                :class="selectedPeriod <= 14 ? 'gap-3' : 'gap-1'"
              >
                <div
                  v-for="(point, idx) in profitPoints"
                  :key="point.isoDate"
                  class="flex-1 min-w-0 text-center"
                >
                  <template v-if="selectedPeriod <= 14 || idx % 5 === 0 || idx === profitPoints.length - 1">
                    <p
                      class="font-bold text-gray-500 dark:text-gray-400 uppercase"
                      :class="selectedPeriod <= 14 ? 'text-[11px] tracking-[0.12em]' : 'text-[9px] tracking-[0.08em]'"
                    >
                      {{ point.shortLabel }}
                    </p>
                    <p
                      v-if="selectedPeriod <= 14"
                      class="text-xs font-semibold text-gray-700 dark:text-gray-200 truncate"
                    >
                      {{ formatCompactMoney(point.amount) }}
                    </p>
                  </template>
                </div>
              </div>
            </div>
          </article>

          <article
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
          >
            <div class="flex items-start justify-between gap-4">
              <div>
                <p
                  class="text-sm uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 font-bold"
                >
                  Выручка и поток броней
                </p>
                <h2
                  class="mt-2 text-2xl font-bold text-gray-900 dark:text-white"
                >
                  Последние {{ selectedPeriod }} дней
                </h2>
              </div>
              <div class="text-right">
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  Валовый оборот
                </p>
                <p class="text-xl font-bold text-blue-600 dark:text-blue-400">
                  {{ formatMoney(totalGrossRevenue) }}
                </p>
              </div>
            </div>

            <div
              class="rounded-3xl bg-gray-50 dark:bg-gray-950 border border-gray-200 dark:border-gray-800 p-4"
            >
              <svg viewBox="0 0 720 240" class="w-full h-56">
                <defs>
                  <linearGradient id="revenueArea" x1="0" x2="0" y1="0" y2="1">
                    <stop
                      offset="0%"
                      stop-color="rgb(59,130,246)"
                      stop-opacity="0.45"
                    />
                    <stop
                      offset="100%"
                      stop-color="rgb(59,130,246)"
                      stop-opacity="0.03"
                    />
                  </linearGradient>
                </defs>
                <path :d="revenueAreaPath" fill="url(#revenueArea)" />
                <path
                  :d="revenueLinePath"
                  fill="none"
                  stroke="rgb(59,130,246)"
                  stroke-width="4"
                  stroke-linejoin="round"
                  stroke-linecap="round"
                />
                <circle
                  v-for="point in revenueChartPoints"
                  :key="point.isoDate"
                  :cx="point.x"
                  :cy="point.y"
                  r="5"
                  fill="rgb(59,130,246)"
                />
              </svg>

              <div class="grid grid-cols-7 gap-3 mt-4">
                <div
                  v-for="point in revenuePoints"
                  :key="point.isoDate"
                  class="text-center space-y-1"
                >
                  <p
                    class="text-[11px] font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
                  >
                    {{ point.shortLabel }}
                  </p>
                  <p
                    class="text-xs font-semibold text-gray-700 dark:text-gray-200"
                  >
                    {{ point.bookingsCount }} бр.
                  </p>
                </div>
              </div>
            </div>
          </article>
        </section>

        <PartnerBookingsTable
          :bookings="filteredBookings"
          :selected-period="selectedPeriod"
          v-model:status-filter="statusFilter"
          :status-filters="statusFilters"
          :canceling-id="cancelingId"
          :format-money="formatMoney"
          :format-date-time="formatDateTime"
          :get-booking-status-label="getBookingStatusLabel"
          :get-booking-status-class="getBookingStatusClass"
          :has-pending-partner-cancellation="hasPendingPartnerCancellation"
          :resolve-car-name="resolveCarName"
          :resolve-license-plate="resolveLicensePlate"
          @export-csv="exportFilteredBookingsCsv"
          @request-cancel="openPartnerCancelModal"
        />
      </template>

      <PartnerCancelBookingModal
        :open="showCancelModal"
        :booking="cancelBookingDraft"
        v-model:reason="cancelReason"
        :submitting="cancelingId !== null"
        :resolve-car-name="resolveCarName"
        :format-date-time="formatDateTime"
        @close="closePartnerCancelModal"
        @submit="submitPartnerCancelRequest"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from "axios";
import { computed, onMounted, ref } from "vue";
import { getMyPartnerCars, type PartnerCarSummary } from "../api/partnerCars";
import {
  getMyPartnerBookings,
  getMyPartnerLedger,
  getMyPartnerWallet,
  cancelPartnerBooking,
} from "../api/partners";
import { useToast } from "../composables/useToast";

import type { BookingStatus } from "../types/Booking";
import type {
  PartnerBooking,
  PartnerLedgerEntry,
  PartnerWallet,
} from "../types/Partner";
import PartnerBookingsPeriodSelector from "../components/partner/PartnerBookingsPeriodSelector.vue";
import PartnerBookingsTable from "../components/partner/PartnerBookingsTable.vue";
import PartnerCancelBookingModal from "../components/partner/PartnerCancelBookingModal.vue";

type BookingFilter = "all" | BookingStatus;
type AnalyticsPeriod = 7 | 14 | 30;

interface ChartPoint {
  isoDate: string;
  label: string;
  shortLabel: string;
  amount: number;
  bookingsCount: number;
  heightPercent: number;
}

const periodOptions: AnalyticsPeriod[] = [7, 14, 30];
const { error, success } = useToast();
const cancelingId = ref<number | null>(null);
const showCancelModal = ref(false);
const cancelBookingDraft = ref<PartnerBooking | null>(null);
const cancelReason = ref("");

const loading = ref(true);
const errorMessage = ref("");
const wallet = ref<PartnerWallet>(createEmptyWallet());
const ledgerEntries = ref<PartnerLedgerEntry[]>([]);
const bookings = ref<PartnerBooking[]>([]);
const cars = ref<PartnerCarSummary[]>([]);
const selectedPeriod = ref<AnalyticsPeriod>(14);
const statusFilter = ref<BookingFilter>("all");

const bookingsInSelectedPeriod = computed(() => {
  const periodStart = buildPeriodStartDate(selectedPeriod.value);
  return bookings.value.filter((booking) =>
    isOnOrAfterDate(booking.createdAt, periodStart),
  );
});

const statusFilters = computed(() => {
  const counts = {
    all: bookingsInSelectedPeriod.value.length,
    pending: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "pending",
    ).length,
    confirmed: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "confirmed",
    ).length,
    active: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "active",
    ).length,
    awaitingReview: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "awaitingReview",
    ).length,
    completed: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "completed",
    ).length,
    canceled: bookingsInSelectedPeriod.value.filter(
      (booking) => booking.status === "canceled",
    ).length,
  };

  return [
    { label: "Все", value: "all" as const, count: counts.all },
    { label: "Pending", value: "pending" as const, count: counts.pending },
    {
      label: "Confirmed",
      value: "confirmed" as const,
      count: counts.confirmed,
    },
    { label: "Active", value: "active" as const, count: counts.active },
    {
      label: "Awaiting Review",
      value: "awaitingReview" as const,
      count: counts.awaitingReview,
    },
    {
      label: "Completed",
      value: "completed" as const,
      count: counts.completed,
    },
    { label: "Canceled", value: "canceled" as const, count: counts.canceled },
  ];
});

const filteredBookings = computed(() => {
  const sorted = [...bookingsInSelectedPeriod.value].sort(
    (left, right) =>
      new Date(right.startTime).getTime() - new Date(left.startTime).getTime(),
  );

  if (statusFilter.value === "all") {
    return sorted;
  }

  return sorted.filter((booking) => booking.status === statusFilter.value);
});

const carsById = computed(() => {
  return new Map(cars.value.map((car) => [car.id, car] as const));
});

const completedBookingsCount = computed(() => {
  return bookingsInSelectedPeriod.value.filter(
    (booking) => booking.status === "completed",
  ).length;
});

const activeBookingsCount = computed(() => {
  return bookingsInSelectedPeriod.value.filter(
    (booking) => booking.status === "active",
  ).length;
});

const profitPoints = computed(() =>
  buildChartPointsFromLedger(ledgerEntries.value, selectedPeriod.value),
);
const revenuePoints = computed(() =>
  buildChartPointsFromBookings(
    bookingsInSelectedPeriod.value,
    selectedPeriod.value,
  ),
);

const totalRealizedProfit = computed(() => {
  return profitPoints.value.reduce((sum, point) => sum + point.amount, 0);
});

const totalGrossRevenue = computed(() => {
  return revenuePoints.value.reduce((sum, point) => sum + point.amount, 0);
});

const revenueChartPoints = computed(() => {
  const points = revenuePoints.value;
  if (points.length === 0) {
    return [];
  }

  const width = 720;
  const height = 240;
  const paddingX = 24;
  const paddingY = 20;
  const availableWidth = width - paddingX * 2;
  const availableHeight = height - paddingY * 2;
  const maxAmount = Math.max(...points.map((point) => point.amount), 1);

  return points.map((point, index) => {
    const x =
      paddingX + (availableWidth / Math.max(points.length - 1, 1)) * index;
    const y = height - paddingY - (point.amount / maxAmount) * availableHeight;
    return { ...point, x, y };
  });
});

const revenueLinePath = computed(() => {
  if (revenueChartPoints.value.length === 0) {
    return "";
  }

  return revenueChartPoints.value
    .map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`)
    .join(" ");
});

const revenueAreaPath = computed(() => {
  if (revenueChartPoints.value.length === 0) {
    return "";
  }

  const baseline = 220;
  const start = revenueChartPoints.value[0]!;
  const end = revenueChartPoints.value[revenueChartPoints.value.length - 1]!;

  return [
    `M ${start.x} ${baseline}`,
    ...revenueChartPoints.value.map((point) => `L ${point.x} ${point.y}`),
    `L ${end.x} ${baseline}`,
    "Z",
  ].join(" ");
});

onMounted(async () => {
  await loadDashboard();
});

async function loadDashboard() {
  loading.value = true;
  errorMessage.value = "";

  try {
    const [walletResult, ledgerResult, bookingsResult, carsResult] =
      await Promise.all([
        safeLoadWallet(),
        getMyPartnerLedger(200),
        getMyPartnerBookings(),
        getMyPartnerCars(),
      ]);

    wallet.value = walletResult;
    ledgerEntries.value = ledgerResult;
    bookings.value = bookingsResult;
    cars.value = carsResult;
  } catch (e) {
    console.error("Failed to load partner dashboard", e);
    errorMessage.value = resolveErrorMessage(
      e,
      "Не удалось загрузить аналитику партнёра.",
    );
    error(errorMessage.value);
  } finally {
    loading.value = false;
  }
}

async function safeLoadWallet(): Promise<PartnerWallet> {
  try {
    return await getMyPartnerWallet();
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 404) {
      return createEmptyWallet();
    }

    throw e;
  }
}

function createEmptyWallet(): PartnerWallet {
  return {
    partnerUserId: "",
    currency: "KZT",
    pendingAmount: 0,
    availableAmount: 0,
    reservedAmount: 0,
    updatedAt: new Date().toISOString(),
  };
}

function buildPeriodStartDate(days: number) {
  const date = new Date();
  date.setHours(0, 0, 0, 0);
  date.setDate(date.getDate() - (days - 1));
  return date;
}

function isOnOrAfterDate(value: string, threshold: Date) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return false;
  }

  return date.getTime() >= threshold.getTime();
}

function buildChartPointsFromLedger(
  entries: PartnerLedgerEntry[],
  days: number,
): ChartPoint[] {
  const dateBuckets = buildRecentDateBuckets(days);
  const groupedAmounts = new Map<string, number>();

  for (const entry of entries) {
    if (
      entry.entryType !== "BookingAvailableCredit" ||
      entry.amountDelta <= 0
    ) {
      continue;
    }

    const dateKey = toDateKey(entry.createdAt);
    groupedAmounts.set(
      dateKey,
      (groupedAmounts.get(dateKey) ?? 0) + entry.amountDelta,
    );
  }

  const rawPoints = dateBuckets.map((bucket) => ({
    ...bucket,
    amount: groupedAmounts.get(bucket.isoDate) ?? 0,
    bookingsCount: 0,
  }));

  const maxAmount = Math.max(...rawPoints.map((point) => point.amount), 1);

  return rawPoints.map((point) => ({
    ...point,
    heightPercent:
      point.amount > 0 ? Math.max((point.amount / maxAmount) * 100, 8) : 0,
  }));
}

function buildChartPointsFromBookings(
  items: PartnerBooking[],
  days: number,
): ChartPoint[] {
  const dateBuckets = buildRecentDateBuckets(days);
  const groupedAmounts = new Map<string, number>();
  const groupedCounts = new Map<string, number>();

  for (const booking of items) {
    if (booking.status === "canceled") {
      continue;
    }

    const dateKey = toDateKey(booking.createdAt);
    groupedAmounts.set(
      dateKey,
      (groupedAmounts.get(dateKey) ?? 0) + (booking.totalPrice ?? 0),
    );
    groupedCounts.set(dateKey, (groupedCounts.get(dateKey) ?? 0) + 1);
  }

  const rawPoints = dateBuckets.map((bucket) => ({
    ...bucket,
    amount: groupedAmounts.get(bucket.isoDate) ?? 0,
    bookingsCount: groupedCounts.get(bucket.isoDate) ?? 0,
  }));

  const maxAmount = Math.max(...rawPoints.map((point) => point.amount), 1);

  return rawPoints.map((point) => ({
    ...point,
    heightPercent:
      point.amount > 0 ? Math.max((point.amount / maxAmount) * 100, 8) : 0,
  }));
}

function buildRecentDateBuckets(days: number): Array<{
  isoDate: string;
  label: string;
  shortLabel: string;
}> {
  const formatter = new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
  });

  return Array.from({ length: days }, (_, index) => {
    const date = new Date();
    date.setHours(0, 0, 0, 0);
    date.setDate(date.getDate() - (days - index - 1));

    const formatted = formatter.format(date);

    return {
      isoDate: toDateKey(date),
      label: formatted,
      shortLabel: formatted.split(" ")[0] ?? formatted,
    };
  });
}

function toDateKey(value: string | Date) {
  const date =
    value instanceof Date ? new Date(value.getTime()) : new Date(value);
  if (Number.isNaN(date.getTime())) {
    return typeof value === "string" ? value.slice(0, 10) : "";
  }

  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function resolveCarName(booking: PartnerBooking) {
  const car = carsById.value.get(booking.partnerCarId);
  if (car?.modelDisplayName) {
    return car.modelDisplayName;
  }

  const fallback = `${booking.carBrand} ${booking.carModel}`.trim();
  return fallback || `Машина #${booking.partnerCarId}`;
}

function resolveLicensePlate(booking: PartnerBooking) {
  return (
    carsById.value.get(booking.partnerCarId)?.licensePlate || "Номер не найден"
  );
}

function exportFilteredBookingsCsv() {
  if (filteredBookings.value.length === 0) {
    return;
  }

  const rows = [
    [
      "Booking ID",
      "Status",
      "Car",
      "License Plate",
      "Created At",
      "Start Time",
      "End Time",
      "Total Price",
      "Price Per Hour",
    ],
    ...filteredBookings.value.map((booking) => [
      String(booking.id),
      getBookingStatusLabel(booking.status),
      resolveCarName(booking),
      resolveLicensePlate(booking),
      formatDateTime(booking.createdAt),
      formatDateTime(booking.startTime),
      formatDateTime(booking.endTime),
      String(booking.totalPrice ?? 0),
      booking.priceHour != null ? String(booking.priceHour) : "",
    ]),
  ];

  const csvContent = rows
    .map((row) => row.map((cell) => escapeCsvCell(cell)).join(","))
    .join("\r\n");

  const blob = new Blob([`\uFEFF${csvContent}`], {
    type: "text/csv;charset=utf-8;",
  });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `partner-bookings-${selectedPeriod.value}d-${statusFilter.value}.csv`;
  document.body.append(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

function escapeCsvCell(value: string) {
  const escaped = value.split('"').join('""');
  return `"${escaped}"`;
}

function formatMoney(
  amount: number,
  currency = wallet.value.currency || "KZT",
) {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function formatCompactMoney(amount: number) {
  if (amount === 0) {
    return "0";
  }

  return new Intl.NumberFormat("ru-RU", {
    notation: "compact",
    maximumFractionDigits: 1,
  }).format(amount);
}

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function getBookingStatusLabel(status: BookingStatus) {
  switch (status) {
    case "pending":
      return "Pending";
    case "confirmed":
      return "Confirmed";
    case "active":
      return "Active";
    case "awaitingReview":
      return "Awaiting Review";
    case "completed":
      return "Completed";
    case "canceled":
      return "Canceled";
    default:
      return status;
  }
}

function getBookingStatusClass(status: BookingStatus) {
  switch (status) {
    case "pending":
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case "confirmed":
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case "active":
      return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
    case "awaitingReview":
      return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
    case "completed":
      return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
    case "canceled":
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
}

function hasPendingPartnerCancellation(booking: PartnerBooking) {
  return Boolean(
    booking.partnerCancellationTicketId &&
      booking.status !== "canceled" &&
      booking.status !== "completed",
  );
}

function resolveErrorMessage(value: unknown, fallback: string) {
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

function openPartnerCancelModal(booking: PartnerBooking) {
  if (cancelingId.value !== null || hasPendingPartnerCancellation(booking)) {
    return;
  }

  cancelBookingDraft.value = booking;
  cancelReason.value = "";
  showCancelModal.value = true;
}

function closePartnerCancelModal(force = false) {
  if (!force && cancelingId.value !== null) return;
  showCancelModal.value = false;
  cancelBookingDraft.value = null;
  cancelReason.value = "";
}

async function submitPartnerCancelRequest() {
  const booking = cancelBookingDraft.value;
  if (!booking || cancelingId.value !== null) return;

  if (!cancelReason.value.trim()) {
    error("Укажите причину отмены.");
    return;
  }

  cancelingId.value = booking.id;
  try {
    const result = await cancelPartnerBooking(booking.id, cancelReason.value.trim());
    success(
      result.alreadyPending
        ? "Запрос уже находится на рассмотрении"
        : "Запрос на отмену отправлен менеджеру",
    );
    closePartnerCancelModal(true);
    await loadDashboard();
  } catch (e: any) {
    error(
      e?.response?.data?.detail ??
        e?.response?.data?.error ??
        "Не удалось отправить запрос на отмену",
    );
  } finally {
    cancelingId.value = null;
  }
}
</script>
