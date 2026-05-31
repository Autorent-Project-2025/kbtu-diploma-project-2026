<template>
  <div
    class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-0 divide-y divide-gray-200 dark:divide-gray-800"
  >
    <h3
      class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 pb-3"
    >
      Сводка
    </h3>
    <div
      v-for="row in rows"
      :key="row.label"
      class="flex justify-between items-center py-3"
    >
      <dt class="text-sm text-gray-500 dark:text-gray-400">
        {{ row.label }}
      </dt>
      <dd class="text-sm font-bold text-gray-900 dark:text-white">
        {{ row.value }}
      </dd>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  rows: { label: string; value: string }[];
}>();
</script>
