<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
    <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Действия</h2>

    <!-- Status=New -->
    <div v-if="status === 1">
      <button
        @click="emit('take')"
        :disabled="actionLoading"
        class="px-5 py-2.5 rounded-2xl bg-emerald-600 text-white font-semibold hover:bg-emerald-700 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
      >
        {{ actionLoading ? "Обработка..." : "Взять в работу" }}
      </button>
    </div>

    <!-- Status=InReview -->
    <div v-if="status === 2" class="flex flex-wrap gap-3">
      <button
        @click="emit('open-resolve')"
        class="px-5 py-2.5 rounded-2xl bg-emerald-600 text-white font-semibold hover:bg-emerald-700 transition-colors"
      >
        Решить
      </button>
      <button
        @click="emit('open-reject')"
        class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-semibold bg-white/60 dark:bg-transparent hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
      >
        Отклонить
      </button>
    </div>

    <!-- Status=AwaitingResponse -->
    <div v-if="status === 3">
      <p class="text-sm text-orange-600 dark:text-orange-400 font-semibold">
        Ожидание ответа от заявителя
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  status: number;
  actionLoading: boolean;
}>();

const emit = defineEmits<{
  take: [];
  "open-resolve": [];
  "open-reject": [];
}>();
</script>
