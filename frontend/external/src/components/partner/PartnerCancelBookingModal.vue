<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="open"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/45 backdrop-blur-sm px-4"
        @click.self="$emit('close')"
      >
        <div
          class="w-full max-w-lg rounded-3xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 shadow-2xl p-6 space-y-5"
        >
          <div>
            <p
              class="text-xs font-bold uppercase tracking-[0.22em] text-red-600 dark:text-red-400"
            >
              Partner Cancellation Review
            </p>
            <h3
              class="mt-2 text-2xl font-extrabold text-gray-900 dark:text-white"
            >
              Запрос на отмену бронирования
            </h3>
            <p class="mt-2 text-sm text-gray-500 dark:text-gray-400">
              Заявка уйдет менеджеру на проверку. Бронирование не отменится
              мгновенно.
            </p>
          </div>

          <div
            v-if="booking"
            class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-4"
          >
            <p class="text-sm font-bold text-gray-900 dark:text-white">
              {{ resolveCarName(booking) }}
            </p>
            <p class="mt-1 text-xs text-gray-500 dark:text-gray-400">
              Бронь #{{ booking.id }} · {{ formatDateTime(booking.startTime) }}
              - {{ formatDateTime(booking.endTime) }}
            </p>
          </div>

          <div class="space-y-2">
            <label
              for="partnerCancelReason"
              class="block text-xs font-bold uppercase tracking-[0.14em] text-gray-500 dark:text-gray-400"
            >
              Причина отмены
            </label>
            <textarea
              id="partnerCancelReason"
              :value="reason"
              @input="
                $emit(
                  'update:reason',
                  ($event.target as HTMLTextAreaElement).value,
                )
              "
              placeholder="Опишите, почему это бронирование нужно отменить"
              class="w-full min-h-[140px] rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-3 text-sm text-gray-900 dark:text-white resize-y focus:outline-none focus:ring-2 focus:ring-red-500/20 focus:border-red-500 transition-colors placeholder-gray-400"
            />
          </div>

          <div class="flex flex-col sm:flex-row gap-3 sm:justify-end">
            <button
              type="button"
              @click="$emit('close')"
              :disabled="submitting"
              class="px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-gray-400 transition-colors disabled:opacity-60"
            >
              Закрыть
            </button>
            <button
              type="button"
              @click="$emit('submit')"
              :disabled="submitting"
              class="px-4 py-2.5 rounded-2xl bg-red-600 hover:bg-red-700 text-sm font-bold text-white transition-colors disabled:opacity-60"
            >
              {{ submitting ? "Отправка..." : "Отправить на проверку" }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import type { PartnerBooking } from "../../types/Partner";

defineProps<{
  open: boolean;
  booking: PartnerBooking | null;
  reason: string;
  submitting: boolean;
  resolveCarName: (booking: PartnerBooking) => string;
  formatDateTime: (value: string) => string;
}>();

defineEmits<{
  "update:reason": [value: string];
  close: [];
  submit: [];
}>();
</script>
