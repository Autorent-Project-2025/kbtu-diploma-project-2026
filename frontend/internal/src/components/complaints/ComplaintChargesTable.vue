<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6">
    <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Начисления</h2>
    <div class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="text-left text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
            <th class="pb-2 pr-4">ID</th>
            <th class="pb-2 pr-4">Тип</th>
            <th class="pb-2 pr-4">Сумма</th>
            <th class="pb-2 pr-4">Статус</th>
            <th class="pb-2">Дата</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
          <tr v-for="charge in charges" :key="charge.id">
            <td class="py-2 pr-4 font-mono text-xs text-gray-600 dark:text-gray-400">#{{ charge.id }}</td>
            <td class="py-2 pr-4 text-gray-900 dark:text-white">{{ chargeTypeLabels[charge.chargeType] ?? charge.chargeType }}</td>
            <td class="py-2 pr-4 font-semibold text-gray-900 dark:text-white">{{ formatPrice(charge.amount) }}</td>
            <td class="py-2 pr-4">
              <span :class="chargeStatusClass(charge.status)">{{ chargeStatusLabel(charge.status) }}</span>
            </td>
            <td class="py-2 text-xs text-gray-500 dark:text-gray-400">{{ formatDateTime(charge.createdAt) }}</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BookingCharge } from "../../api/payments";
import { formatDateTime, formatPrice } from "../../utils/formatters";
import {
  chargeStatusClass,
  chargeStatusLabel,
  chargeTypeLabels,
} from "../../utils/complaintLabels";

defineProps<{
  charges: BookingCharge[];
}>();
</script>
