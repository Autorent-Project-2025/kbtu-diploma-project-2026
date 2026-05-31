<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
    <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Запросы на повторное открытие</h2>
    <div class="space-y-3">
      <div
        v-for="req in requests"
        :key="req.id"
        class="rounded-xl border p-4"
        :class="{
          'border-amber-200 dark:border-amber-800/50 bg-amber-50 dark:bg-amber-900/10': req.status === 1,
          'border-emerald-200 dark:border-emerald-800/50 bg-emerald-50 dark:bg-emerald-900/10': req.status === 2,
          'border-red-200 dark:border-red-800/50 bg-red-50 dark:bg-red-900/10': req.status === 3,
        }"
      >
        <div class="flex items-start justify-between gap-3 mb-2">
          <div>
            <span
              class="inline-flex items-center px-2 py-0.5 rounded-lg text-xs font-bold uppercase tracking-wide"
              :class="{
                'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400': req.status === 1,
                'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400': req.status === 2,
                'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400': req.status === 3,
              }"
            >
              {{ reopenStatusLabels[req.status] ?? '—' }}
            </span>
            <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ formatDateTime(req.createdAt) }}</span>
          </div>
          <!-- Approve/Reject buttons for pending requests -->
          <div v-if="req.status === 1" class="flex gap-2 shrink-0">
            <button
              @click="emit('approve', req.id)"
              :disabled="actionLoading"
              class="px-3 py-1.5 rounded-lg text-xs font-bold text-white bg-emerald-600 hover:bg-emerald-700 transition-colors disabled:opacity-60"
            >
              Одобрить
            </button>
            <button
              @click="emit('reject', req.id)"
              :disabled="actionLoading"
              class="px-3 py-1.5 rounded-lg text-xs font-bold text-red-600 dark:text-red-400 border border-red-200 dark:border-red-800 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60"
            >
              Отклонить
            </button>
          </div>
        </div>
        <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ req.reason }}</p>
        <p v-if="req.decisionNote" class="text-xs text-gray-500 dark:text-gray-400 mt-2 italic">{{ req.decisionNote }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { ReopenRequest } from "../../types/Complaint";
import { formatDateTime } from "../../utils/formatters";
import { reopenStatusLabels } from "../../utils/complaintLabels";

defineProps<{
  requests: ReopenRequest[];
  actionLoading: boolean;
}>();

const emit = defineEmits<{
  approve: [requestId: string];
  reject: [requestId: string];
}>();
</script>
