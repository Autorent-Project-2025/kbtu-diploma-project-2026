<template>
  <div
    class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-4"
  >
    <div>
      <h3
        class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
      >
        Решение
      </h3>
      <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
        Причину нужно указать только для отказа. Для завершения поездки
        вынесите решение в отдельном блоке: либо одобрение, либо штраф с
        комментарием.
      </p>
    </div>

    <div v-if="!isBookingCompletionTicket(ticket)" class="space-y-1.5">
      <label
        for="rejectReason"
        class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
        >Причина отказа</label
      >
      <textarea
        id="rejectReason"
        v-model="rejectReasonModel"
        placeholder="Укажите причину, если заявка отклоняется"
        class="w-full px-4 py-3 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm min-h-[100px] resize-y focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors placeholder-gray-400"
      />
    </div>

    <div v-else class="space-y-4">
      <div class="rounded-2xl border border-emerald-200 dark:border-emerald-500/30 bg-emerald-50/70 dark:bg-emerald-500/10 p-4 space-y-3">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.14em] text-emerald-700 dark:text-emerald-300">
            Одобрение без штрафа
          </p>
          <p class="text-xs text-emerald-700/80 dark:text-emerald-200/80 mt-1">
            Кнопка активна только когда блок штрафа пустой.
          </p>
        </div>
        <button
          @click="emit('approve')"
          :disabled="actionLoading || !canApprove"
          class="w-full px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
        >
          {{ actionLoading ? "Обработка..." : "✓ Одобрить завершение" }}
        </button>
      </div>

      <div class="rounded-2xl border border-red-200 dark:border-red-500/30 bg-red-50/70 dark:bg-red-500/10 p-4 space-y-4">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.14em] text-red-700 dark:text-red-300">
            Выставление штрафа
          </p>
          <p class="text-xs text-red-700/80 dark:text-red-200/80 mt-1">
            Укажите сумму и обязательно добавьте комментарий, чтобы клиент видел причину начисления.
          </p>
        </div>

        <div class="space-y-1.5">
          <label
            for="fineAmount"
            class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
            >Сумма штрафа</label
          >
          <input
            id="fineAmount"
            v-model="fineAmountModel"
            type="number"
            min="0.01"
            step="0.01"
            placeholder="Например 15000"
            class="w-full px-4 py-3 rounded-xl border border-red-200 dark:border-red-500/30 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-500/20 transition-colors placeholder-gray-400"
          />
        </div>

        <div class="space-y-1.5">
          <label
            for="fineComment"
            class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
            >Комментарий к штрафу</label
          >
          <textarea
            id="fineComment"
            v-model="fineCommentModel"
            placeholder="Опишите повреждение, недостающие элементы или иную причину начисления"
            class="w-full px-4 py-3 rounded-xl border border-red-200 dark:border-red-500/30 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm min-h-[110px] resize-y focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-500/20 transition-colors placeholder-gray-400"
          />
        </div>

        <button
          @click="emit('issue-fine')"
          :disabled="actionLoading"
          class="w-full px-5 py-3 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 disabled:cursor-not-allowed font-bold transition-colors"
        >
          {{ actionLoading ? "Обработка..." : "Выставить штраф" }}
        </button>
      </div>
    </div>

    <div v-if="!isBookingCompletionTicket(ticket)" class="flex flex-col gap-3">
      <button
        @click="emit('approve')"
        :disabled="actionLoading"
        class="w-full px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
      >
        {{
          actionLoading
            ? "Обработка..."
            : isPartnerBookingCancellationTicket(ticket)
              ? "✓ Одобрить отмену"
              : "✓ Одобрить"
        }}
      </button>
      <button
        @click="emit('reject')"
        :disabled="actionLoading"
        class="w-full px-5 py-3 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 disabled:cursor-not-allowed font-bold transition-colors"
      >
        {{
          actionLoading
            ? "Обработка..."
            : isPartnerBookingCancellationTicket(ticket)
              ? "✕ Отклонить запрос"
              : "✕ Отклонить"
        }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import type { Ticket } from "../../types/Ticket";
import {
  isBookingCompletionTicket,
  isPartnerBookingCancellationTicket,
} from "../../utils/ticketLabels";

const props = defineProps<{
  ticket: Ticket;
  actionLoading: boolean;
  canApprove: boolean;
  rejectReason: string;
  fineAmount: string;
  fineComment: string;
}>();

const emit = defineEmits<{
  "update:rejectReason": [value: string];
  "update:fineAmount": [value: string];
  "update:fineComment": [value: string];
  approve: [];
  reject: [];
  "issue-fine": [];
}>();

const rejectReasonModel = computed({
  get: () => props.rejectReason,
  set: (v: string) => emit("update:rejectReason", v),
});
const fineAmountModel = computed({
  get: () => props.fineAmount,
  set: (v: string) => emit("update:fineAmount", v),
});
const fineCommentModel = computed({
  get: () => props.fineComment,
  set: (v: string) => emit("update:fineComment", v),
});
</script>
