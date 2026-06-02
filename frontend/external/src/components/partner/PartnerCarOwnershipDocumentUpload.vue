<template>
  <div class="space-y-3 md:col-span-2">
    <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
      Подтверждение собственности
    </label>
    <div
      class="relative flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed px-6 py-6 transition-colors cursor-pointer"
      :class="
        dragging
          ? 'border-primary-400 bg-primary-50 dark:border-primary-500 dark:bg-primary-500/10'
          : file
            ? 'border-emerald-300 bg-emerald-50 dark:border-emerald-500/40 dark:bg-emerald-500/10'
            : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 hover:border-gray-300 dark:hover:border-gray-600'
      "
      @click="inputRef?.click()"
      @dragover.prevent="dragging = true"
      @dragleave.prevent="dragging = false"
      @drop.prevent="onDrop"
    >
      <template v-if="file">
        <svg
          class="h-7 w-7 text-emerald-500 dark:text-emerald-400 shrink-0"
          fill="none"
          stroke="currentColor"
          stroke-width="1.5"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"
          />
        </svg>
        <p
          class="text-sm font-medium text-emerald-700 dark:text-emerald-300 text-center break-all"
        >
          {{ file.name }}
        </p>
        <button
          type="button"
          class="text-xs text-gray-400 hover:text-red-500 dark:hover:text-red-400 transition-colors"
          @click.stop="emit('remove')"
        >
          Удалить
        </button>
      </template>
      <template v-else>
        <svg
          class="h-7 w-7 text-gray-400 dark:text-gray-500"
          fill="none"
          stroke="currentColor"
          stroke-width="1.5"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m2.25 0H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z"
          />
        </svg>
        <p class="text-sm text-gray-500 dark:text-gray-400">
          <span class="font-semibold text-primary-600 dark:text-primary-400">
            Нажмите
          </span>
          или перетащите PDF сюда
        </p>
      </template>
      <input
        ref="inputRef"
        type="file"
        accept="application/pdf,.pdf"
        class="hidden"
        @change="onFileChange"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";

defineProps<{
  file: File | null;
}>();

const emit = defineEmits<{
  "file-selected": [file: File];
  remove: [];
}>();

const inputRef = ref<HTMLInputElement | null>(null);
const dragging = ref(false);

function emitSelectedFile(file: File | null) {
  if (file) {
    emit("file-selected", file);
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0] ?? null;
  input.value = "";
  emitSelectedFile(file);
}

function onDrop(event: DragEvent) {
  dragging.value = false;
  emitSelectedFile(event.dataTransfer?.files?.[0] ?? null);
}
</script>
