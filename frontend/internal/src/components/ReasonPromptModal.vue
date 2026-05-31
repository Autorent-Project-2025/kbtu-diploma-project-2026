<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div
        v-if="show"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
        @click.self="emit('update:show', false)"
      >
        <div
          class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4"
        >
          <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">
            {{ title }}
          </h3>
          <p
            v-if="description"
            class="text-sm text-gray-500 dark:text-gray-400 mb-4"
          >
            {{ description }}
          </p>
          <textarea
            v-model="text"
            :rows="rows"
            :placeholder="placeholder"
            :class="[
              'w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 resize-none',
              ringClass,
            ]"
          />
          <div class="flex justify-end gap-3 mt-4">
            <button
              @click="emit('update:show', false)"
              class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
            >
              {{ cancelLabel }}
            </button>
            <button
              @click="emit('confirm')"
              :disabled="loading || (required && !text.trim())"
              :class="[
                'px-4 py-2 text-sm font-semibold text-white rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed',
                buttonClass,
              ]"
            >
              {{ loading ? loadingLabel : confirmLabel }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed } from "vue";

type Accent = "emerald" | "red" | "amber" | "purple" | "rose";

const props = withDefaults(
  defineProps<{
    show: boolean;
    modelValue: string;
    title: string;
    description?: string;
    placeholder?: string;
    confirmLabel?: string;
    loadingLabel?: string;
    cancelLabel?: string;
    accent?: Accent;
    required?: boolean;
    loading?: boolean;
    rows?: number;
  }>(),
  {
    description: "",
    placeholder: "",
    confirmLabel: "Подтвердить",
    loadingLabel: "Обработка...",
    cancelLabel: "Отмена",
    accent: "emerald",
    required: false,
    loading: false,
    rows: 3,
  },
);

const emit = defineEmits<{
  "update:show": [value: boolean];
  "update:modelValue": [value: string];
  confirm: [];
}>();

const text = computed({
  get: () => props.modelValue,
  set: (v: string) => emit("update:modelValue", v),
});

const ringClass = computed(
  () =>
    ({
      emerald: "focus:ring-emerald-500",
      red: "focus:ring-red-500",
      amber: "focus:ring-amber-500",
      purple: "focus:ring-purple-500",
      rose: "focus:ring-rose-500",
    })[props.accent],
);

const buttonClass = computed(
  () =>
    ({
      emerald: "bg-emerald-600 hover:bg-emerald-700",
      red: "bg-red-600 hover:bg-red-700",
      amber: "bg-amber-600 hover:bg-amber-700",
      purple: "bg-purple-600 hover:bg-purple-700",
      rose: "bg-rose-600 hover:bg-rose-700",
    })[props.accent],
);
</script>
