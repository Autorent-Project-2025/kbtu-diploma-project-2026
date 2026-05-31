<template>
  <section
    class="rounded-2xl border border-rose-100 dark:border-rose-900/40 p-5 space-y-4"
  >
    <div>
      <h3
        class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
      >
        Запрос на отмену бронирования
      </h3>
      <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
        Одобрение этого тикета отправит команду на отмену брони в booking-service.
      </p>
    </div>

    <dl class="grid sm:grid-cols-2 gap-4">
      <div class="rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
        <dt class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
          Бронирование
        </dt>
        <dd class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
          #{{ ticket.bookingId }}
        </dd>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {{ ticket.carBrand }} {{ ticket.carModel }}
        </p>
      </div>

      <div class="rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
        <dt class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
          Статус на момент запроса
        </dt>
        <dd class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
          {{ partnerBookingStatusLabel(data?.bookingStatus) }}
        </dd>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {{ formatDateTime(data?.bookingStartTime || "") }}
          -
          {{ formatDateTime(data?.bookingEndTime || "") }}
        </p>
      </div>
    </dl>

    <div class="rounded-2xl border border-rose-200 dark:border-rose-500/30 bg-rose-50/70 dark:bg-rose-500/10 p-4">
      <p class="text-xs font-bold uppercase tracking-[0.14em] text-rose-700 dark:text-rose-300">
        Причина партнёра
      </p>
      <p class="mt-3 text-sm leading-6 text-gray-700 dark:text-gray-200 whitespace-pre-line">
        {{ data?.partnerReason }}
      </p>
    </div>
  </section>
</template>

<script setup lang="ts">
import type {
  PartnerBookingCancellationTicketData,
  Ticket,
} from "../../types/Ticket";
import { formatDateTime } from "../../utils/formatters";
import { partnerBookingStatusLabel } from "../../utils/ticketLabels";

defineProps<{
  ticket: Ticket;
  data: PartnerBookingCancellationTicketData | null;
}>();
</script>
