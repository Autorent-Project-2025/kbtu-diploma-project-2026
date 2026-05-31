<template>
  <div class="space-y-4 md:col-span-2">
    <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
      Фото машины
    </label>
    <div
      class="relative flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed px-6 py-8 transition-colors cursor-pointer"
      :class="
        dragging
          ? 'border-primary-400 bg-primary-50 dark:border-primary-500 dark:bg-primary-500/10'
          : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 hover:border-gray-300 dark:hover:border-gray-600'
      "
      @click="inputRef?.click()"
      @dragover.prevent="dragging = true"
      @dragleave.prevent="dragging = false"
      @drop.prevent="onDrop"
    >
      <svg
        class="h-8 w-8 text-gray-400 dark:text-gray-500"
        fill="none"
        stroke="currentColor"
        stroke-width="1.5"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M2.25 15.75l5.159-5.159a2.25 2.25 0 013.182 0l5.159 5.159m-1.5-1.5l1.409-1.409a2.25 2.25 0 013.182 0l2.909 2.909M3 10.5a7.5 7.5 0 1115 0 7.5 7.5 0 01-15 0z"
        />
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          d="M12 16.5v-9m-4.5 4.5h9"
        />
      </svg>
      <p class="text-sm text-gray-500 dark:text-gray-400">
        <span class="font-semibold text-primary-600 dark:text-primary-400">
          Нажмите
        </span>
        или перетащите фото сюда
      </p>
      <p class="text-xs text-gray-400 dark:text-gray-500">
        До 12 изображений. Для каждого фото выберите тип, чтобы менеджеру было
        проще проверить заявку.
      </p>
      <input
        ref="inputRef"
        type="file"
        accept="image/*"
        multiple
        class="hidden"
        @change="onFileChange"
      />
    </div>
    <div
      v-if="images.length > 0"
      class="grid gap-3 md:grid-cols-2 xl:grid-cols-3"
    >
      <div
        v-for="(image, index) in images"
        :key="index"
        class="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden shadow-sm"
      >
        <img
          :src="image.previewUrl"
          :alt="`Фото ${index + 1}`"
          class="h-44 w-full object-cover"
        />
        <div class="p-4 space-y-3">
          <div class="flex items-start justify-between gap-3">
            <div class="min-w-0">
              <p class="text-sm font-semibold text-gray-900 dark:text-white">
                Фото {{ index + 1 }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                {{ image.file.name }}
              </p>
            </div>
            <button
              type="button"
              class="shrink-0 rounded-full p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
              @click.stop="emit('remove-image', index)"
            >
              <svg
                class="h-4 w-4"
                fill="none"
                stroke="currentColor"
                stroke-width="2.5"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>
          </div>

          <div class="space-y-1.5">
            <label
              :for="`partner-car-image-type-${index}`"
              class="block text-xs font-bold uppercase tracking-[0.12em] text-gray-500 dark:text-gray-400"
            >
              Тип фото
            </label>
            <select
              :id="`partner-car-image-type-${index}`"
              :value="image.imageType"
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2.5 text-sm text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20 transition-colors"
              @change="onImageTypeChange(index, $event)"
            >
              <option
                v-for="option in imageTypeOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import type { PartnerCarImageType } from "../../api/tickets";

interface PartnerCarImagePreview {
  file: File;
  previewUrl: string;
  imageType: PartnerCarImageType;
}

defineProps<{
  images: PartnerCarImagePreview[];
  imageTypeOptions: Array<{
    value: PartnerCarImageType;
    label: string;
  }>;
}>();

const emit = defineEmits<{
  "add-files": [files: File[]];
  "remove-image": [index: number];
  "update-image-type": [
    payload: {
      index: number;
      imageType: PartnerCarImageType;
    },
  ];
}>();

const inputRef = ref<HTMLInputElement | null>(null);
const dragging = ref(false);

function emitFiles(files: File[]) {
  if (files.length > 0) {
    emit("add-files", files);
  }
}

function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const files = Array.from(input.files ?? []);
  input.value = "";
  emitFiles(files);
}

function onDrop(event: DragEvent) {
  dragging.value = false;
  emitFiles(Array.from(event.dataTransfer?.files ?? []));
}

function onImageTypeChange(index: number, event: Event) {
  const input = event.target as HTMLSelectElement;
  emit("update-image-type", {
    index,
    imageType: input.value as PartnerCarImageType,
  });
}
</script>
