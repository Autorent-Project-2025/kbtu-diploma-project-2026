<template>
  <div class="rounded-2xl border border-indigo-200 dark:border-indigo-800/50 bg-white dark:bg-gray-900 shadow-xl p-8">
    <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Действия менеджера</h2>
    <div class="space-y-3">
      <!-- Cancel Booking -->
      <div v-if="canCancelBooking" class="flex items-center justify-between gap-3">
        <div>
          <p class="text-sm font-semibold text-gray-900 dark:text-white">Отменить бронирование</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">Бронирование #{{ complaint.bookingId }} ({{ complaint.snapshotData.status }})</p>
        </div>
        <button
          @click="emit('cancel-booking')"
          class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors shrink-0"
        >
          Отменить
        </button>
      </div>
      <div v-else-if="bookingNotCancelable" class="flex items-center gap-3">
        <div>
          <p class="text-sm font-semibold text-gray-400 dark:text-gray-500">Отменить бронирование</p>
          <p class="text-xs text-gray-400 dark:text-gray-500">Бронирование уже {{ bookingNotCancelableReason }}</p>
        </div>
      </div>

      <!-- Waive Charge -->
      <div v-if="complaint.chargeId" class="flex items-center justify-between gap-3">
        <div>
          <p class="text-sm font-semibold text-gray-900 dark:text-white">Аннулировать начисление</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">Начисление #{{ complaint.chargeId }}</p>
        </div>
        <button
          @click="emit('waive-charge')"
          class="px-4 py-2 text-sm font-semibold text-amber-600 dark:text-amber-400 border border-amber-300 dark:border-amber-700 hover:bg-amber-50 dark:hover:bg-amber-900/20 rounded-xl transition-colors shrink-0"
        >
          Аннулировать
        </button>
      </div>

      <!-- Refund Charge -->
      <div v-if="complaint.chargeId" class="flex items-center justify-between gap-3">
        <div>
          <p class="text-sm font-semibold text-gray-900 dark:text-white">Возврат средств</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">Возврат оплаченного начисления #{{ complaint.chargeId }}</p>
        </div>
        <button
          @click="emit('refund-charge')"
          class="px-4 py-2 text-sm font-semibold text-rose-600 dark:text-rose-400 border border-rose-300 dark:border-rose-700 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-xl transition-colors shrink-0"
        >
          Вернуть
        </button>
      </div>

      <!-- Escalate -->
      <div v-if="!complaint.isEscalated" class="flex items-center justify-between gap-3">
        <div>
          <p class="text-sm font-semibold text-gray-900 dark:text-white">Эскалировать</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">Передать жалобу суперменеджеру</p>
        </div>
        <button
          @click="emit('escalate')"
          class="px-4 py-2 text-sm font-semibold text-purple-600 dark:text-purple-400 border border-purple-300 dark:border-purple-700 hover:bg-purple-50 dark:hover:bg-purple-900/20 rounded-xl transition-colors shrink-0"
        >
          Эскалировать
        </button>
      </div>
      <div v-else class="flex items-center gap-3">
        <div>
          <p class="text-sm font-semibold text-purple-600 dark:text-purple-400">Эскалирована</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">
            {{ complaint.escalationReason }}
            <span v-if="complaint.escalatedAt" class="ml-1 text-gray-400">
              ({{ formatDateTime(complaint.escalatedAt) }})
            </span>
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { Complaint } from "../../types/Complaint";
import { formatDateTime } from "../../utils/formatters";

defineProps<{
  complaint: Complaint;
  canCancelBooking: boolean;
  bookingNotCancelable: boolean;
  bookingNotCancelableReason: string;
}>();

const emit = defineEmits<{
  "cancel-booking": [];
  "waive-charge": [];
  "refund-charge": [];
  escalate: [];
}>();
</script>
