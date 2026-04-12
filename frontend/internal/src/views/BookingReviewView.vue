<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">

    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
        <div class="flex items-start gap-4">
          <router-link
            :to="`/complaints/${complaintId}`"
            class="mt-1 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-blue-500 transition-colors shrink-0"
          >
            ← Назад к жалобе
          </router-link>
          <div class="space-y-2">
            <div class="flex items-center gap-2">
              <p class="text-xs font-bold uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400">
                Обзор бронирования
              </p>
              <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400">
                Read-only
              </span>
            </div>
            <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
              {{ loading ? "Загрузка..." : booking ? `${booking.carBrand} ${booking.carModel}` : "Не найдено" }}
            </h1>
            <p v-if="booking" class="text-sm text-gray-500 dark:text-gray-400">
              Бронирование #{{ booking.bookingId }} — доступ по жалобе
            </p>
          </div>
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-500 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Access denied -->
    <div
      v-else-if="accessDenied"
      class="rounded-2xl border border-dashed border-red-300 dark:border-red-700 p-12 text-center space-y-3"
    >
      <p class="text-red-600 dark:text-red-400 font-semibold">Доступ запрещён</p>
      <p class="text-sm text-gray-500 dark:text-gray-400">
        У вас нет активного доступа к данному бронированию. Запросите доступ чере�� страницу жалобы.
      </p>
      <router-link
        :to="`/complaints/${complaintId}`"
        class="inline-block mt-2 px-5 py-2.5 rounded-xl bg-blue-600 text-white font-semibold hover:bg-blue-700 transition-colors"
      >
        Вернуться к жалобе
      </router-link>
    </div>

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Бронирование не найдено.
    </div>

    <!-- Booking data -->
    <template v-else-if="booking">

      <!-- Status card -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-6">
        <div class="flex items-center gap-3 mb-4">
          <span class="px-3 py-1 rounded-full text-sm font-bold bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">
            {{ booking.status }}
          </span>
          <span class="text-sm text-gray-500 dark:text-gray-400">
            По жалобе: {{ booking.complaintSubject }}
          </span>
        </div>
      </div>

      <!-- Info cards -->
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">

        <!-- Car -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Автомобиль</p>
          <div class="flex gap-3">
            <div
              v-if="booking.coverImageUrl"
              class="shrink-0 w-20 h-14 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
            >
              <img :src="booking.coverImageUrl" class="w-full h-full object-cover" />
            </div>
            <div>
              <p class="text-sm font-bold text-gray-900 dark:text-white">
                {{ booking.carBrand }} {{ booking.carModel }}
              </p>
              <p v-if="booking.partnerName" class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                Партнёр: {{ booking.partnerName }}
              </p>
            </div>
          </div>
        </div>

        <!-- Period -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Период</p>
          <div class="space-y-1">
            <p class="text-sm text-gray-900 dark:text-white">
              <span class="font-semibold">Начало:</span> {{ formatDateTime(booking.startTime) }}
            </p>
            <p class="text-sm text-gray-900 dark:text-white">
              <span class="font-semibold">Конец:</span> {{ formatDateTime(booking.endTime) }}
            </p>
            <p v-if="booking.tripStartedAt" class="text-sm text-gray-900 dark:text-white">
              <span class="font-semibold">Поездка начата:</span> {{ formatDateTime(booking.tripStartedAt) }}
            </p>
          </div>
        </div>

        <!-- Price -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Стоимость</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ formatPrice(booking.totalPrice) }}
          </p>
        </div>

      </div>

      <!-- Complaint link -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-6">
        <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-3">Связанная жалоба</p>
        <router-link
          :to="`/complaints/${booking.complaintId}`"
          class="inline-flex items-center gap-1.5 text-sm font-medium text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
        >
          {{ booking.complaintSubject }}
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
          </svg>
        </router-link>
      </div>

    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getBookingReview } from "../api/accessRequests";
import type { BookingReview } from "../types/AccessRequest";
import { formatDateTime, formatPrice } from "../utils/formatters";

const route = useRoute();
const complaintId = route.params.complaintId as string;

const loading = ref(false);
const notFound = ref(false);
const accessDenied = ref(false);
const booking = ref<BookingReview | null>(null);

async function loadBooking() {
  if (!complaintId) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    booking.value = await getBookingReview(complaintId);
  } catch (error: unknown) {
    const status = (error as { response?: { status?: number } })?.response?.status;
    if (status === 403 || status === 401) {
      accessDenied.value = true;
    } else {
      notFound.value = true;
    }
  } finally {
    loading.value = false;
  }
}

onMounted(loadBooking);
</script>
