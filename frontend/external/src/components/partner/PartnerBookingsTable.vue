<template>
  <section
    class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
  >
    <div
      class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4"
    >
      <div>
        <p
          class="text-sm uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 font-bold"
        >
          Клиентские бронирования
        </p>
        <h2 class="mt-2 text-3xl font-bold text-gray-900 dark:text-white">
          Все брони по вашим машинам
        </h2>
        <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
          Показаны бронирования, созданные за последние
          {{ selectedPeriod }} дней.
        </p>
      </div>

      <div class="flex flex-col sm:flex-row sm:items-center gap-3">
        <button
          type="button"
          @click="$emit('export-csv')"
          :disabled="bookings.length === 0"
          class="inline-flex items-center justify-center px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:bg-gray-300 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-semibold shadow-lg shadow-emerald-500/20"
        >
          Экспорт CSV
        </button>

        <div class="relative">
          <button
            type="button"
            @click="statusDropdownOpen = !statusDropdownOpen"
            class="inline-flex items-center gap-3 px-5 py-3 rounded-2xl bg-gray-900 dark:bg-white text-white dark:text-gray-900 font-bold text-sm shadow-lg transition-colors min-w-[180px] justify-between"
          >
            <span>
              {{ selectedStatusFilter?.label }}
              <span class="ml-1 opacity-60">
                {{ selectedStatusFilter?.count }}
              </span>
            </span>
            <svg
              class="w-4 h-4 transition-transform"
              :class="statusDropdownOpen ? 'rotate-180' : ''"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 9l-7 7-7-7"
              />
            </svg>
          </button>

          <Transition
            enter-active-class="transition ease-out duration-150"
            enter-from-class="opacity-0 translate-y-1 scale-95"
            enter-to-class="opacity-100 translate-y-0 scale-100"
            leave-active-class="transition ease-in duration-100"
            leave-from-class="opacity-100 translate-y-0 scale-100"
            leave-to-class="opacity-0 translate-y-1 scale-95"
          >
            <div
              v-if="statusDropdownOpen"
              class="absolute right-0 mt-2 w-52 z-30 rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-2xl overflow-hidden"
            >
              <button
                v-for="filter in statusFilters"
                :key="filter.value"
                type="button"
                @click="selectStatusFilter(filter.value)"
                :class="[
                  'w-full flex items-center justify-between px-4 py-2.5 text-sm font-semibold transition-colors',
                  statusFilter === filter.value
                    ? 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-white'
                    : 'text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800/60',
                ]"
              >
                <span>{{ filter.label }}</span>
                <span
                  class="text-xs font-bold px-2 py-0.5 rounded-full bg-gray-200 dark:bg-gray-700 text-gray-600 dark:text-gray-300"
                >
                  {{ filter.count }}
                </span>
              </button>
            </div>
          </Transition>
        </div>
      </div>
    </div>

    <div
      v-if="bookings.length === 0"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-10 text-center"
    >
      <div
        class="w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center mx-auto mb-3"
      >
        <svg
          class="w-6 h-6 text-gray-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="1.5"
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
          />
        </svg>
      </div>
      <p class="text-sm font-semibold text-gray-500 dark:text-gray-400">
        Нет бронирований
      </p>
      <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
        Попробуйте изменить фильтр или период
      </p>
    </div>

    <div v-else class="overflow-x-auto">
      <table class="min-w-full text-sm">
        <thead>
          <tr
            class="text-left border-b border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50"
          >
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Бронь
            </th>
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Машина
            </th>
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Период
            </th>
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Сумма
            </th>
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Статус
            </th>
            <th
              class="px-4 py-3 text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400 hidden lg:table-cell"
            >
              Создано
            </th>
            <th class="px-4 py-3"></th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="booking in bookings"
            :key="booking.id"
            class="border-b border-gray-100 dark:border-gray-800/80"
          >
            <td class="px-4 py-4 align-top">
              <p class="font-bold text-gray-900 dark:text-white">
                #{{ booking.id }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                carId: {{ booking.partnerCarId }}
              </p>
            </td>
            <td class="px-4 py-4 align-top">
              <p class="font-semibold text-gray-900 dark:text-white">
                {{ resolveCarName(booking) }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{ resolveLicensePlate(booking) }}
              </p>
            </td>
            <td class="px-4 py-4 align-top">
              <p class="font-medium text-gray-900 dark:text-white">
                {{ formatDateTime(booking.startTime) }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                до {{ formatDateTime(booking.endTime) }}
              </p>
            </td>
            <td class="px-4 py-4 align-top">
              <p class="font-bold text-gray-900 dark:text-white">
                {{ formatMoney(booking.totalPrice ?? 0) }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400">
                {{
                  booking.priceHour != null
                    ? `${formatMoney(booking.priceHour)}/час`
                    : "Ставка не указана"
                }}
              </p>
            </td>
            <td class="px-4 py-4 align-top">
              <div class="flex flex-col items-start gap-2">
                <span
                  :class="getBookingStatusClass(booking.status)"
                  class="inline-flex px-3 py-1 rounded-full text-xs font-bold uppercase tracking-[0.12em]"
                >
                  {{ getBookingStatusLabel(booking.status) }}
                </span>
                <span
                  v-if="hasPendingPartnerCancellation(booking)"
                  class="inline-flex px-3 py-1 rounded-full text-[11px] font-bold tracking-[0.08em] bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300"
                >
                  Запрос на отмену отправлен
                </span>
              </div>
            </td>
            <td class="px-4 py-4 align-top hidden lg:table-cell">
              <p class="font-medium text-gray-900 dark:text-white">
                {{ formatDateTime(booking.createdAt) }}
              </p>
            </td>
            <td class="px-4 py-4 align-top">
              <button
                v-if="
                  (booking.status === 'pending' ||
                    booking.status === 'confirmed') &&
                  !hasPendingPartnerCancellation(booking)
                "
                @click="$emit('request-cancel', booking)"
                :disabled="cancelingId === booking.id"
                class="px-3 py-1.5 rounded-xl border border-red-300 dark:border-red-700 text-red-600 dark:text-red-400 text-xs font-bold hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-50 transition-colors"
              >
                {{
                  cancelingId === booking.id
                    ? "Отправка..."
                    : "Запросить отмену"
                }}
              </button>
              <p
                v-else-if="hasPendingPartnerCancellation(booking)"
                class="text-xs text-amber-700 dark:text-amber-300 leading-5 max-w-[180px]"
              >
                На рассмотрении менеджера
                <span v-if="booking.partnerCancellationRequestedAt">
                  с {{ formatDateTime(booking.partnerCancellationRequestedAt) }}
                </span>
              </p>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import type { BookingStatus } from "../../types/Booking";
import type { PartnerBooking } from "../../types/Partner";

type BookingFilter = "all" | BookingStatus;

interface BookingStatusFilter {
  label: string;
  value: BookingFilter;
  count: number;
}

const props = defineProps<{
  bookings: PartnerBooking[];
  selectedPeriod: number;
  statusFilter: BookingFilter;
  statusFilters: BookingStatusFilter[];
  cancelingId: number | null;
  formatMoney: (amount: number) => string;
  formatDateTime: (value: string) => string;
  getBookingStatusLabel: (status: BookingStatus) => string;
  getBookingStatusClass: (status: BookingStatus) => string;
  hasPendingPartnerCancellation: (booking: PartnerBooking) => boolean;
  resolveCarName: (booking: PartnerBooking) => string;
  resolveLicensePlate: (booking: PartnerBooking) => string;
}>();

const emit = defineEmits<{
  "update:statusFilter": [value: BookingFilter];
  "export-csv": [];
  "request-cancel": [booking: PartnerBooking];
}>();

const statusDropdownOpen = ref(false);
const selectedStatusFilter = computed(() =>
  props.statusFilters.find((filter) => filter.value === props.statusFilter),
);

function selectStatusFilter(value: BookingFilter) {
  emit("update:statusFilter", value);
  statusDropdownOpen.value = false;
}
</script>
