<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
    <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">{{ label }}</p>
    <p class="text-sm font-bold text-gray-900 dark:text-white">{{ name }}</p>
    <p class="text-xs text-gray-500 dark:text-gray-400">Тип: {{ typeLabel }}</p>
    <div>
      <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">User ID</p>
      <p class="font-mono text-xs text-gray-700 dark:text-gray-300 break-all leading-relaxed">
        {{ userId }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string;
  name: string;
  typeLabel: string;
  userId: string;
}>();
</script>
