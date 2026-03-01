<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
        @click.self="handleClose"
      >
        <div
          class="relative w-full max-w-lg bg-white dark:bg-gray-900 rounded-3xl shadow-2xl overflow-hidden"
          @click.stop
        >
          <!-- Header -->
          <div
            class="relative p-6 bg-gradient-to-r from-primary-600 to-primary-700 dark:from-primary-700 dark:to-primary-800"
          >
            <button
              @click="handleClose"
              class="absolute top-4 right-4 w-10 h-10 flex items-center justify-center rounded-full bg-white/10 hover:bg-white/20 text-white transition-colors"
            >
              <svg
                class="w-6 h-6"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </button>

            <div class="pr-12">
              <h2 class="text-2xl font-bold text-white mb-2">Оставить отзыв</h2>
              <p class="text-primary-100">
                {{ car.brand }} {{ car.model }} ({{ car.year }})
              </p>
            </div>
          </div>

          <!-- Content -->
          <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
            <!-- Rating -->
            <div>
              <label
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Ваша оценка
                <span class="text-red-500">*</span>
              </label>
              <div class="flex items-center gap-2">
                <button
                  v-for="star in 5"
                  :key="star"
                  type="button"
                  @click="rating = star"
                  @mouseenter="hoverRating = star"
                  @mouseleave="hoverRating = 0"
                  class="transition-all duration-200 hover:scale-110 focus:outline-none focus:scale-110"
                >
                  <svg
                    :class="[
                      'w-10 h-10 transition-colors',
                      star <= (hoverRating || rating)
                        ? 'text-yellow-400 fill-current'
                        : 'text-gray-300 dark:text-gray-600',
                    ]"
                    viewBox="0 0 20 20"
                  >
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                    />
                  </svg>
                </button>
                <span
                  v-if="rating > 0"
                  class="ml-3 text-lg font-semibold text-gray-900 dark:text-white"
                >
                  {{ ratingText }}
                </span>
              </div>
              <p
                v-if="errors.rating"
                class="mt-2 text-sm text-red-600 dark:text-red-400"
              >
                {{ errors.rating }}
              </p>
            </div>

            <!-- Comment -->
            <div>
              <label
                for="review-content"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Ваш отзыв
                <span class="text-red-500">*</span>
              </label>
              <textarea
                id="review-content"
                v-model="content"
                rows="5"
                class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all resize-none"
                placeholder="Поделитесь своими впечатлениями об автомобиле. Что вам понравилось? Что можно улучшить?"
                :class="{
                  'border-red-500 dark:border-red-500': errors.content,
                }"
              ></textarea>
              <div class="mt-2 flex items-center justify-between">
                <p
                  v-if="errors.content"
                  class="text-sm text-red-600 dark:text-red-400"
                >
                  {{ errors.content }}
                </p>
                <p
                  class="text-sm text-gray-500 dark:text-gray-400 ml-auto"
                  :class="{
                    'text-red-500 dark:text-red-400': content.length > 500,
                  }"
                >
                  {{ content.length }}/500
                </p>
              </div>
            </div>

            <!-- Actions -->
            <div class="flex gap-3 pt-4">
              <button
                type="button"
                @click="handleClose"
                class="flex-1 px-6 py-3 rounded-xl font-semibold text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors"
              >
                Отмена
              </button>
              <button
                type="submit"
                :disabled="submitting"
                class="flex-1 btn-premium py-3 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span
                  v-if="!submitting"
                  class="flex items-center justify-center gap-2"
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
                      d="M5 13l4 4L19 7"
                    />
                  </svg>
                  Отправить
                </span>
                <span v-else class="flex items-center justify-center gap-2">
                  <svg
                    class="w-5 h-5 animate-spin"
                    fill="none"
                    viewBox="0 0 24 24"
                  >
                    <circle
                      class="opacity-25"
                      cx="12"
                      cy="12"
                      r="10"
                      stroke="currentColor"
                      stroke-width="4"
                    ></circle>
                    <path
                      class="opacity-75"
                      fill="currentColor"
                      d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    ></path>
                  </svg>
                  Отправка...
                </span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import type { CarDetails } from "../types/Car";

interface Props {
  isOpen: boolean;
  car: CarDetails;
}

interface Emits {
  (e: "close"): void;
  (e: "submit", rating: number, content: string): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const rating = ref(0);
const hoverRating = ref(0);
const content = ref("");
const submitting = ref(false);
const errors = ref({
  rating: "",
  content: "",
});

const ratingText = computed(() => {
  const texts = ["", "Ужасно", "Плохо", "Нормально", "Хорошо", "Отлично"];
  return texts[rating.value] || "";
});

function validateForm(): boolean {
  errors.value = { rating: "", content: "" };
  let isValid = true;

  if (rating.value === 0) {
    errors.value.rating = "Пожалуйста, выберите оценку";
    isValid = false;
  }

  if (!content.value.trim()) {
    errors.value.content = "Пожалуйста, напишите отзыв";
    isValid = false;
  } else if (content.value.length < 10) {
    errors.value.content = "Отзыв должен содержать минимум 10 символов";
    isValid = false;
  } else if (content.value.length > 500) {
    errors.value.content = "Отзыв не должен превышать 500 символов";
    isValid = false;
  }

  return isValid;
}

async function handleSubmit() {
  if (!validateForm()) return;

  submitting.value = true;
  emit("submit", rating.value, content.value);
}

function handleClose() {
  if (!submitting.value) {
    resetForm();
    emit("close");
  }
}

function resetForm() {
  rating.value = 0;
  hoverRating.value = 0;
  content.value = "";
  errors.value = { rating: "", content: "" };
}

// Reset form when modal opens
watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      resetForm();
    }
  }
);

// Expose submitting state control for parent
defineExpose({
  setSubmitting: (value: boolean) => {
    submitting.value = value;
  },
});
</script>

<style scoped>
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-active .relative,
.modal-leave-active .relative {
  transition: transform 0.3s ease;
}

.modal-enter-from .relative,
.modal-leave-to .relative {
  transform: scale(0.9);
}
</style>
