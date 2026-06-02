<template>
  <section
    class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl px-6 py-4"
  >
    <div class="flex items-center justify-between gap-4">
      <div class="flex items-center gap-3">
        <p class="text-sm font-semibold text-gray-500 dark:text-gray-400">
          Период аналитики:
        </p>
        <span class="text-sm font-bold text-gray-900 dark:text-white">
          последние {{ selectedPeriod }} дней
        </span>
      </div>
      <div class="flex gap-1.5">
        <button
          v-for="period in periodOptions"
          :key="period"
          type="button"
          @click="$emit('update:selectedPeriod', period)"
          :class="[
            'px-3.5 py-1.5 rounded-xl text-sm font-bold transition-colors',
            selectedPeriod === period
              ? 'bg-gray-900 text-white dark:bg-white dark:text-gray-900'
              : 'bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700',
          ]"
        >
          {{ period }}д
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
type AnalyticsPeriod = 7 | 14 | 30;

defineProps<{
  selectedPeriod: AnalyticsPeriod;
  periodOptions: AnalyticsPeriod[];
}>();

defineEmits<{
  "update:selectedPeriod": [value: AnalyticsPeriod];
}>();
</script>
