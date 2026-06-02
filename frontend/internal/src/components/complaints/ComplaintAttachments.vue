<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
    <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Вложения</h2>
    <div v-if="imageAttachments.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
      <button
        v-for="att in imageAttachments"
        :key="att.id"
        type="button"
        @click="emit('open-preview', att)"
        class="overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-left transition-all hover:border-emerald-300 hover:shadow-md dark:hover:border-emerald-500/40"
      >
        <img
          v-if="previewUrls[att.id]"
          :src="previewUrls[att.id]"
          :alt="att.originalFileName"
          class="h-48 w-full object-cover"
          loading="lazy"
        />
        <div
          v-else
          class="h-48 w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 text-xs font-medium text-gray-400 dark:text-gray-500"
        >
          Загрузка изображения...
        </div>
        <div class="flex items-center justify-between gap-3 px-4 py-3">
          <span class="truncate text-sm font-medium text-gray-700 dark:text-gray-300">{{ att.originalFileName }}</span>
          <span class="text-[11px] font-semibold uppercase tracking-wide text-emerald-600 dark:text-emerald-400">Открыть</span>
        </div>
      </button>
    </div>
    <ul class="space-y-2">
      <li v-for="att in fileAttachments" :key="att.id" class="flex items-center gap-3">
        <svg class="w-4 h-4 text-gray-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
        </svg>
        <button
          @click="emit('download', att.id, att.originalFileName)"
          class="text-sm text-emerald-600 dark:text-emerald-400 hover:underline font-medium"
        >
          {{ att.originalFileName }}
        </button>
        <span class="text-xs text-gray-400">{{ att.fileType }}</span>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import type { ComplaintAttachment } from "../../types/Complaint";

defineProps<{
  imageAttachments: ComplaintAttachment[];
  fileAttachments: ComplaintAttachment[];
  previewUrls: Record<string, string>;
}>();

const emit = defineEmits<{
  "open-preview": [attachment: ComplaintAttachment];
  download: [attachmentId: string, fileName: string];
}>();
</script>
