<template>
  <!-- Resolution -->
  <div
    v-if="complaint.status === 4"
    class="rounded-2xl border border-emerald-200 dark:border-emerald-800/50 bg-emerald-50 dark:bg-emerald-900/20 shadow-xl p-8"
  >
    <h2 class="text-lg font-bold text-emerald-700 dark:text-emerald-400 mb-4">Решение</h2>
    <p v-if="complaint.resolutionType != null" class="text-sm font-semibold text-gray-900 dark:text-white mb-2">
      {{ resolutionLabels[complaint.resolutionType] ?? "—" }}
    </p>
    <p v-if="complaint.resolutionNote" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.resolutionNote }}</p>
    <p v-if="complaint.resolvedAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.resolvedAt) }}</p>
  </div>

  <!-- Rejection -->
  <div
    v-else-if="complaint.status === 5"
    class="rounded-2xl border border-red-200 dark:border-red-800/50 bg-red-50 dark:bg-red-900/20 shadow-xl p-8"
  >
    <h2 class="text-lg font-bold text-red-700 dark:text-red-400 mb-4">Отклонена</h2>
    <p v-if="complaint.rejectionReason" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.rejectionReason }}</p>
    <p v-if="complaint.rejectedAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.rejectedAt) }}</p>
  </div>
</template>

<script setup lang="ts">
import type { Complaint } from "../../types/Complaint";
import { formatDateTime } from "../../utils/formatters";
import { resolutionLabels } from "../../utils/complaintLabels";

defineProps<{
  complaint: Complaint;
}>();
</script>
