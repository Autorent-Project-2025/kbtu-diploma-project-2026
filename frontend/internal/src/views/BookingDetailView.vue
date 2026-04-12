<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">

    <!-- ── Header ──────────────────────────────────────────────────── -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
        <div class="flex items-start gap-4">
          <router-link
            to="/bookings"
            class="mt-1 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-emerald-500 transition-colors shrink-0"
          >
            ← Назад
          </router-link>
          <div class="space-y-2">
            <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
              Data Management
            </p>
            <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
              {{ loading ? "Загрузка..." : `Бронирование #${booking?.id ?? route.params.id}` }}
            </h1>
            <div v-if="booking" class="flex flex-wrap items-center gap-2 pt-1">
              <span
                :class="['px-3 py-1 rounded-full text-sm font-bold', bookingStatusBadge(booking.status)]"
              >
                {{ bookingStatusLabel(booking.status) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Cancel action -->
        <div v-if="booking && canCancel" class="shrink-0 self-start sm:self-center">
          <button
            @click="showCancelModal = true"
            :disabled="cancelling"
            class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-semibold bg-white/60 dark:bg-transparent hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ cancelling ? "Отмена..." : "Отменить бронирование" }}
          </button>
        </div>
      </div>
    </header>

    <!-- ── Loading ─────────────────────────────────────────────────── -->
    <div
      v-if="loading"
      class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-500 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- ── Not found ───────────────────────────────────────────────── -->
    <div
      v-else-if="notFound"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Бронирование не найдено.
    </div>

    <template v-else-if="booking">

      <!-- ── Summary strip ───────────────────────────────────────────── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div
          class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1"
        >
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Итоговая сумма</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ formatPrice(booking.totalPrice) }}
          </p>
        </div>
        <div
          class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1"
        >
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Цена / час</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ formatPrice(booking.priceHour) }}
          </p>
        </div>
        <div
          class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1"
        >
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Длительность</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ durationLabel }}
          </p>
        </div>
        <div
          class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1"
        >
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Статус</p>
          <span
            :class="['self-start mt-0.5 px-3 py-1 rounded-full text-sm font-bold', bookingStatusBadge(booking.status)]"
          >
            {{ bookingStatusLabel(booking.status) }}
          </span>
        </div>
      </div>

      <!-- ── Timeline card ───────────────────────────────────────────── -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-6">Хронология</h2>
        <ol class="relative border-l-2 border-gray-200 dark:border-gray-700 ml-3 space-y-0">
          <li
            v-for="(evt, idx) in timelineEvents"
            :key="idx"
            class="ml-6 pb-8 last:pb-0"
          >
            <!-- dot -->
            <span
              :class="[
                'absolute -left-[11px] flex items-center justify-center w-5 h-5 rounded-full ring-4 ring-white dark:ring-gray-900',
                evt.active
                  ? 'bg-emerald-500'
                  : evt.done
                    ? 'bg-gray-400 dark:bg-gray-600'
                    : 'bg-gray-200 dark:bg-gray-700',
              ]"
            />
            <div class="flex flex-col sm:flex-row sm:items-baseline sm:gap-4">
              <p
                :class="[
                  'text-sm font-bold',
                  evt.active
                    ? 'text-emerald-600 dark:text-emerald-400'
                    : 'text-gray-900 dark:text-white',
                ]"
              >
                {{ evt.label }}
              </p>
              <template v-if="evt.time">
                <p class="text-sm text-gray-600 dark:text-gray-400">{{ formatDateTime(evt.time) }}</p>
                <p class="text-xs text-gray-400 dark:text-gray-500">{{ relativeTime(evt.time) }}</p>
              </template>
              <p v-else class="text-sm text-gray-400 dark:text-gray-500 italic">Ещё не наступило</p>
            </div>
          </li>
        </ol>
      </div>

      <!-- ── Participants card ───────────────────────────────────────── -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-6">Участники</h2>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <!-- Client -->
          <div class="space-y-3">
            <div class="flex items-center gap-2 mb-2">
              <span class="w-7 h-7 rounded-full bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center text-emerald-600 dark:text-emerald-400">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </span>
              <p class="text-sm font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Клиент</p>
            </div>
            <div>
              <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">User ID</p>
              <p class="font-mono text-xs text-gray-700 dark:text-gray-300 break-all leading-relaxed">
                {{ booking.userId }}
              </p>
            </div>
            <EntityLink :to="`/clients?userId=${booking.userId}`">
              Открыть клиента
            </EntityLink>
          </div>

          <!-- Partner -->
          <div class="space-y-3">
            <div class="flex items-center gap-2 mb-2">
              <span class="w-7 h-7 rounded-full bg-blue-100 dark:bg-blue-900/40 flex items-center justify-center text-blue-600 dark:text-blue-400">
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                </svg>
              </span>
              <p class="text-sm font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Партнёр</p>
            </div>
            <div v-if="booking.partnerName">
              <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">Имя</p>
              <p class="font-semibold text-gray-900 dark:text-white">{{ booking.partnerName }}</p>
            </div>
            <div>
              <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">Partner User ID</p>
              <p class="font-mono text-xs text-gray-700 dark:text-gray-300 break-all leading-relaxed">
                {{ booking.partnerUserId }}
              </p>
            </div>
            <EntityLink :to="`/partners?userId=${booking.partnerUserId}`">
              Открыть партнёра
            </EntityLink>
          </div>
        </div>
      </div>

      <!-- ── Vehicle card ────────────────────────────────────────────── -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-6">Автомобиль</h2>
        <div class="flex flex-col sm:flex-row gap-6">
          <!-- Cover image -->
          <div
            v-if="booking.coverImageUrl"
            class="shrink-0 w-full sm:w-56 h-36 rounded-xl overflow-hidden border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
          >
            <img
              :src="booking.coverImageUrl"
              :alt="`${booking.carBrand} ${booking.carModel}`"
              class="w-full h-full object-cover"
            />
          </div>
          <!-- Placeholder when no image -->
          <div
            v-else
            class="shrink-0 w-full sm:w-56 h-36 rounded-xl border border-dashed border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 flex items-center justify-center"
          >
            <svg class="w-10 h-10 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <!-- Info -->
          <div class="space-y-3">
            <div>
              <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">Марка и модель</p>
              <p class="text-xl font-bold text-gray-900 dark:text-white">
                {{ booking.carBrand }} {{ booking.carModel }}
              </p>
            </div>
            <div>
              <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">ID автомобиля</p>
              <p class="font-mono text-sm text-gray-700 dark:text-gray-300">{{ booking.partnerCarId }}</p>
            </div>
            <EntityLink :to="`/cars/${booking.partnerCarId}`">
              Открыть автомобиль
            </EntityLink>
          </div>
        </div>
      </div>

      <!-- ── Pricing breakdown card ──────────────────────────────────── -->
      <div
        v-if="booking.pricingBreakdown"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8"
      >
        <div class="flex items-center gap-3 mb-6">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Ценообразование</h2>
          <span
            v-if="booking.pricingBreakdown.isMarketValueStale"
            class="px-2.5 py-0.5 rounded-full bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400 text-xs font-bold"
          >
            Устаревшая рыночная стоимость
          </span>
          <span
            v-if="booking.pricingBreakdown.currency"
            class="px-2.5 py-0.5 rounded-full bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 text-xs font-bold"
          >
            {{ booking.pricingBreakdown.currency }}
          </span>
        </div>

        <!-- Quoted prices highlight -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
          <div class="rounded-xl bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800/50 p-4">
            <p class="text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400 mb-1">Цена/час (котировка)</p>
            <p class="text-2xl font-extrabold text-emerald-700 dark:text-emerald-300">
              {{ formatPrice(booking.pricingBreakdown.quotedPriceHour) }}
            </p>
          </div>
          <div class="rounded-xl bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800/50 p-4">
            <p class="text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400 mb-1">Итого (котировка)</p>
            <p class="text-2xl font-extrabold text-emerald-700 dark:text-emerald-300">
              {{ formatPrice(booking.pricingBreakdown.quotedTotalPrice) }}
            </p>
          </div>
        </div>

        <!-- Breakdown grid -->
        <dl class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-x-6 gap-y-5">
          <div v-if="booking.pricingBreakdown.marketValueKzt != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Рыночная стоимость</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">{{ formatPrice(booking.pricingBreakdown.marketValueKzt) }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.billableHours != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Оплачиваемые часы</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">{{ booking.pricingBreakdown.billableHours }} ч</dd>
          </div>
          <div v-if="booking.pricingBreakdown.ratingCoefficient != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">К-т рейтинга</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">× {{ booking.pricingBreakdown.ratingCoefficient }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.advanceBookingCoefficient != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">К-т предоплаты</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">× {{ booking.pricingBreakdown.advanceBookingCoefficient }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.availabilityCoefficient != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">К-т доступности</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">× {{ booking.pricingBreakdown.availabilityCoefficient }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.rating != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Рейтинг</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">{{ booking.pricingBreakdown.rating }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.daysBeforeBooking != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Дней до поездки</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">{{ booking.pricingBreakdown.daysBeforeBooking }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.currentAvailableCarsCount != null">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Доступно машин</dt>
            <dd class="text-gray-900 dark:text-white font-semibold">{{ booking.pricingBreakdown.currentAvailableCarsCount }}</dd>
          </div>
          <div v-if="booking.pricingBreakdown.quotedAtUtc">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-1">Котировка от</dt>
            <dd class="text-gray-900 dark:text-white font-semibold text-sm">{{ formatDateTime(booking.pricingBreakdown.quotedAtUtc) }}</dd>
          </div>
        </dl>
      </div>

      <!-- ── Charges section ────────────────────────────────────────── -->
      <div
        v-if="charges !== null"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <div class="px-8 py-6 border-b border-gray-100 dark:border-gray-800">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Платежи</h2>
          <p class="text-xs text-gray-400 dark:text-gray-500 mt-0.5">{{ charges.length }} записей</p>
        </div>

        <div v-if="charges.length === 0" class="px-8 py-10 text-center text-gray-400 dark:text-gray-500 text-sm">
          Платежи отсутствуют.
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-100 dark:border-gray-800">
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Тип</th>
                <th class="text-right px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Сумма</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Статус</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Описание</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Создан</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Оплачен</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="charge in charges"
                :key="charge.id"
                class="border-b border-gray-50 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
              >
                <td class="px-6 py-3 font-semibold text-gray-900 dark:text-white whitespace-nowrap">
                  {{ charge.chargeType }}
                </td>
                <td class="px-6 py-3 text-right font-bold text-gray-900 dark:text-white whitespace-nowrap">
                  {{ formatPrice(charge.amount) }}
                  <span class="text-xs text-gray-400 dark:text-gray-500 font-normal ml-1">{{ charge.currency }}</span>
                </td>
                <td class="px-6 py-3">
                  <span
                    :class="[
                      'px-2.5 py-0.5 rounded-full text-xs font-bold',
                      chargeStatusMap[charge.status]?.css ?? 'bg-gray-100 text-gray-500',
                    ]"
                  >
                    {{ chargeStatusMap[charge.status]?.label ?? charge.status }}
                  </span>
                </td>
                <td class="px-6 py-3 text-gray-600 dark:text-gray-400 max-w-xs truncate">
                  {{ charge.description ?? "—" }}
                </td>
                <td class="px-6 py-3 text-gray-500 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ formatDateTime(charge.createdAt) }}
                </td>
                <td class="px-6 py-3 text-gray-500 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ charge.paidAt ? formatDateTime(charge.paidAt) : "—" }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </template><!-- end booking template -->

    <!-- ── Confirm cancel modal ───────────────────────────────────── -->
    <ConfirmModal
      :show="showCancelModal"
      title="Отменить бронирование?"
      :message="`Бронирование #${booking?.id} будет отменено. Это действие нельзя отменить.`"
      confirm-text="Да, отменить"
      variant="danger"
      @confirm="onConfirmCancel"
      @cancel="showCancelModal = false"
    />

  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import {
  getBooking,
  cancelBooking,
  getBookingCharges,
  type BookingDto,
  type BookingChargeDto,
} from "../api/bookings";
import { formatDate, formatDateTime, formatPrice, relativeTime } from "../utils/formatters";
import { bookingStatusLabel, bookingStatusBadge, chargeStatusMap } from "../utils/statusMaps";
import { useToast } from "../composables/useToast";
import { auth } from "../store/auth";
import EntityLink from "../components/EntityLink.vue";
import ConfirmModal from "../components/ConfirmModal.vue";

// suppress unused-import warnings — formatDate is part of the required import list
void formatDate;

const route = useRoute();
const toast = useToast();

const loading = ref(false);
const cancelling = ref(false);
const notFound = ref(false);
const showCancelModal = ref(false);

const booking = ref<BookingDto | null>(null);
const charges = ref<BookingChargeDto[] | null>(null);

// ── Derived ──────────────────────────────────────────────────────────

const cancellableStatuses = new Set(["Pending", "PaymentPending", "Confirmed"]);

const canCancel = computed(
  () =>
    !!booking.value?.status &&
    cancellableStatuses.has(booking.value.status) &&
    auth.hasPermission("Booking.Update"),
);

/** Human-readable duration: "3 ч 30 мин" */
const durationLabel = computed(() => {
  if (!booking.value) return "—";
  const start = new Date(booking.value.startTime).getTime();
  const end = new Date(booking.value.endTime).getTime();
  if (isNaN(start) || isNaN(end)) return "—";
  const totalMins = Math.max(0, Math.round((end - start) / 60000));
  const h = Math.floor(totalMins / 60);
  const m = totalMins % 60;
  if (h === 0) return `${m} мин`;
  if (m === 0) return `${h} ч`;
  return `${h} ч ${m} мин`;
});

interface TimelineEvent {
  label: string;
  time?: string | null;
  done: boolean;
  active: boolean;
}

const timelineEvents = computed((): TimelineEvent[] => {
  const b = booking.value;
  if (!b) return [];

  const now = Date.now();
  const startMs = new Date(b.startTime).getTime();
  const endMs = new Date(b.endTime).getTime();

  return [
    {
      label: "Создано",
      time: b.createdAt,
      done: true,
      active: false,
    },
    {
      label: "Начало",
      time: b.startTime,
      done: now >= startMs,
      active: now >= startMs && now < endMs,
    },
    ...(b.tripStartedAt
      ? [
          {
            label: "Поездка началась",
            time: b.tripStartedAt,
            done: true,
            active: false,
          },
        ]
      : []),
    {
      label: "Окончание",
      time: b.endTime,
      done: now >= endMs,
      active: false,
    },
    ...(b.tripCompletedAt
      ? [
          {
            label: "Поездка завершена",
            time: b.tripCompletedAt,
            done: true,
            active: false,
          },
        ]
      : []),
  ];
});

// ── Data loading ──────────────────────────────────────────────────────

async function loadBooking() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    booking.value = await getBooking(id);
  } catch {
    notFound.value = true;
    loading.value = false;
    return;
  }
  loading.value = false;

  // Load charges separately — silently ignore errors (403/404 possible)
  try {
    charges.value = await getBookingCharges(id);
  } catch {
    charges.value = null;
  }
}

// ── Actions ───────────────────────────────────────────────────────────

async function onConfirmCancel() {
  showCancelModal.value = false;
  if (cancelling.value || !booking.value) return;

  cancelling.value = true;
  try {
    await cancelBooking(booking.value.id);
    toast.success("Бронирование успешно отменено");
    await loadBooking();
  } catch {
    toast.error("Ошибка при отмене бронирования");
  } finally {
    cancelling.value = false;
  }
}

onMounted(loadBooking);
</script>
