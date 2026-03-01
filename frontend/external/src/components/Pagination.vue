<template>
  <div class="flex items-center justify-center gap-2 flex-wrap">
    <!-- Previous Button -->
    <button
      @click="goToPage(currentPage - 1)"
      :disabled="currentPage === 1"
      class="px-4 py-2 rounded-xl font-semibold transition-all disabled:opacity-40 disabled:cursor-not-allowed bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 disabled:hover:border-gray-200 dark:disabled:hover:border-gray-700 disabled:hover:text-gray-700 dark:disabled:hover:text-gray-300"
    >
      <svg
        class="w-5 h-5"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15 19l-7-7 7-7"
        />
      </svg>
    </button>

    <!-- First Page -->
    <button
      v-if="showFirstPage"
      @click="goToPage(1)"
      :class="[
        'px-4 py-2 rounded-xl font-semibold transition-all',
        currentPage === 1
          ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/50'
          : 'bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400',
      ]"
    >
      1
    </button>

    <!-- Left Ellipsis -->
    <span v-if="showLeftEllipsis" class="px-2 text-gray-500 dark:text-gray-400">
      ...
    </span>

    <!-- Page Numbers -->
    <button
      v-for="page in displayPages"
      :key="page"
      @click="goToPage(page)"
      :class="[
        'px-4 py-2 rounded-xl font-semibold transition-all',
        currentPage === page
          ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/50'
          : 'bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400',
      ]"
    >
      {{ page }}
    </button>

    <!-- Right Ellipsis -->
    <span
      v-if="showRightEllipsis"
      class="px-2 text-gray-500 dark:text-gray-400"
    >
      ...
    </span>

    <!-- Last Page -->
    <button
      v-if="showLastPage"
      @click="goToPage(totalPages)"
      :class="[
        'px-4 py-2 rounded-xl font-semibold transition-all',
        currentPage === totalPages
          ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/50'
          : 'bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400',
      ]"
    >
      {{ totalPages }}
    </button>

    <!-- Next Button -->
    <button
      @click="goToPage(currentPage + 1)"
      :disabled="currentPage === totalPages"
      class="px-4 py-2 rounded-xl font-semibold transition-all disabled:opacity-40 disabled:cursor-not-allowed bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 hover:border-primary-500 dark:hover:border-primary-500 text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 disabled:hover:border-gray-200 dark:disabled:hover:border-gray-700 disabled:hover:text-gray-700 dark:disabled:hover:text-gray-300"
    >
      <svg
        class="w-5 h-5"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M9 5l7 7-7 7"
        />
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";

interface Props {
  currentPage: number;
  totalPages: number;
  maxDisplayPages?: number;
}

const props = withDefaults(defineProps<Props>(), {
  maxDisplayPages: 5,
});

const emit = defineEmits<{
  (e: "page-change", page: number): void;
}>();

// Вычисляем какие страницы показывать
const displayPages = computed(() => {
  const pages: number[] = [];
  const { currentPage, totalPages, maxDisplayPages } = props;

  if (totalPages <= maxDisplayPages + 2) {
    // Если страниц мало, показываем все
    for (let i = 2; i < totalPages; i++) {
      pages.push(i);
    }
  } else {
    // Показываем страницы вокруг текущей
    let start = Math.max(2, currentPage - Math.floor(maxDisplayPages / 2));
    let end = Math.min(totalPages - 1, start + maxDisplayPages - 1);

    // Корректируем если уперлись в конец
    if (end === totalPages - 1) {
      start = Math.max(2, end - maxDisplayPages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
  }

  return pages;
});

const showFirstPage = computed(() => {
  return props.totalPages > 1;
});

const showLastPage = computed(() => {
  return (
    props.totalPages > 1 &&
    displayPages.value[displayPages.value.length - 1] !== props.totalPages
  );
});

const showLeftEllipsis = computed(() => {
  return displayPages.value.length > 0 && displayPages.value[0] > 2;
});

const showRightEllipsis = computed(() => {
  return (
    displayPages.value.length > 0 &&
    displayPages.value[displayPages.value.length - 1] < props.totalPages - 1
  );
});

function goToPage(page: number) {
  if (page < 1 || page > props.totalPages || page === props.currentPage) {
    return;
  }
  emit("page-change", page);
}
</script>
