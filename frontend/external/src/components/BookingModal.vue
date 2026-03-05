<template>
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="isOpen"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm"
        @click.self="closeModal"
      >
        <div
          class="relative w-full max-w-md bg-white dark:bg-gray-900 rounded-3xl shadow-2xl overflow-hidden transform transition-all"
          @click.stop
        >
          <!-- Header -->
          <div
            class="relative bg-gradient-to-br from-primary-600 to-primary-700 px-6 py-8 text-white"
          >
            <button
              @click="closeModal"
              class="absolute top-4 right-4 p-2 rounded-full hover:bg-white/20 transition-colors"
              aria-label="Закрыть"
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

            <div class="space-y-2">
              <h2 class="text-2xl font-bold">Выберите даты</h2>
              <p class="text-primary-100 text-sm">
                {{ car.brand }} {{ car.model }}
              </p>
            </div>
          </div>

          <!-- Content -->
          <div class="p-6 space-y-6">
            <!-- Date Start -->
            <div class="space-y-2">
              <label
                for="start-date"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Дата начала
              </label>
              <div class="relative">
                <input
                  id="start-date"
                  v-model="startDate"
                  type="datetime-local"
                  :min="minDate"
                  class="w-full px-4 py-3 pr-12 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                  required
                />
                <div
                  class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none"
                >
                  <svg
                    class="w-5 h-5 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Date End -->
            <div class="space-y-2">
              <label
                for="end-date"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >
                Дата окончания
              </label>
              <div class="relative">
                <input
                  id="end-date"
                  v-model="endDate"
                  type="datetime-local"
                  :min="startDate || minDate"
                  class="w-full px-4 py-3 pr-12 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent transition-all"
                  required
                />
                <div
                  class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none"
                >
                  <svg
                    class="w-5 h-5 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                    />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Duration Info -->
            <div
              v-if="duration"
              class="flex items-center gap-3 p-4 bg-primary-50 dark:bg-primary-900/20 rounded-xl"
            >
              <div
                class="flex-shrink-0 w-10 h-10 bg-primary-100 dark:bg-primary-900/40 rounded-full flex items-center justify-center"
              >
                <svg
                  class="w-5 h-5 text-primary-600 dark:text-primary-400"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </div>
              <div class="flex-1">
                <p class="text-sm font-semibold text-gray-900 dark:text-white">
                  Продолжительность аренды
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ duration.days > 0 ? `${duration.days} дн. ` : ""
                  }}{{ duration.hours }} ч.
                  {{ duration.minutes > 0 ? `${duration.minutes} мин.` : "" }}
                </p>
              </div>
            </div>

            <!-- Price Estimation -->
            <div
              v-if="estimatedPrice"
              class="flex items-center justify-between p-4 bg-green-50 dark:bg-green-900/20 rounded-xl"
            >
              <span
                class="text-sm font-semibold text-gray-700 dark:text-gray-300"
                >Примерная стоимость:</span
              >
              <span
                class="text-2xl font-bold text-green-600 dark:text-green-400"
                >${{ estimatedPrice }}</span
              >
            </div>

            <!-- Error Message -->
            <div
              v-if="displayError"
              class="flex items-center gap-3 p-4 bg-red-50 dark:bg-red-900/20 rounded-xl"
            >
              <svg
                class="w-5 h-5 text-red-600 dark:text-red-400 flex-shrink-0"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
              <p class="text-sm text-red-600 dark:text-red-400">
                {{ displayError }}
              </p>
            </div>

            <div
              v-if="(suggestedDates ?? []).length > 0"
              class="space-y-3 p-4 rounded-xl border border-amber-300/80 bg-amber-50 dark:border-amber-500/40 dark:bg-amber-900/20"
            >
              <p class="text-sm font-semibold text-amber-800 dark:text-amber-200">
                На выбранные даты все машины заняты. Ближайшие доступные:
              </p>
              <div class="flex flex-wrap gap-2">
                <button
                  v-for="date in suggestedDates"
                  :key="date"
                  type="button"
                  @click="applySuggestedDate(date)"
                  class="px-3 py-2 rounded-lg border border-amber-300/80 dark:border-amber-500/40 bg-white dark:bg-gray-900 text-xs font-semibold text-amber-800 dark:text-amber-200"
                >
                  {{ formatSuggestionDate(date) }}
                </button>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div
            class="flex gap-3 px-6 py-4 bg-gray-50 dark:bg-gray-800/50 border-t border-gray-200 dark:border-gray-700"
          >
            <button
              @click="closeModal"
              class="flex-1 px-6 py-3 bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 font-semibold rounded-xl border-2 border-gray-300 dark:border-gray-600 hover:bg-gray-50 dark:hover:bg-gray-700 transition-all active:scale-95"
            >
              Отмена
            </button>
            <button
              @click="confirmBooking"
              :disabled="!isValid || isLoading"
              class="flex-1 px-6 py-3 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-semibold rounded-xl transition-all hover:shadow-lg active:scale-95 disabled:cursor-not-allowed disabled:hover:shadow-none flex items-center justify-center gap-2"
            >
              <span v-if="!isLoading">Забронировать</span>
              <span v-else>Загрузка...</span>
              <svg
                v-if="!isLoading"
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
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import type { Car } from "../types/Car";

interface Props {
  isOpen: boolean;
  car: Car;
  bookingError?: string;
  suggestedDates?: string[];
}

interface Emits {
  (e: "close"): void;
  (e: "confirm", startDate: string, endDate: string): void;
  (e: "suggestion-click", value: string): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const isLoading = ref(false);
const startDate = ref("");
const endDate = ref("");
const validationError = ref("");

// Минимальная дата - текущий момент
const minDate = computed(() => {
  const now = new Date();
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
  return now.toISOString().slice(0, 16);
});

// Инициализация дат по умолчанию
watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      const now = new Date();
      now.setMinutes(now.getMinutes() - now.getTimezoneOffset());
      startDate.value = now.toISOString().slice(0, 16);

      const tomorrow = new Date(now.getTime() + 24 * 60 * 60 * 1000);
      endDate.value = tomorrow.toISOString().slice(0, 16);

      validationError.value = "";
    }
  }
);

const displayError = computed(() => {
  return props.bookingError?.trim() || validationError.value;
});

// Вычисление продолжительности
const duration = computed(() => {
  if (!startDate.value || !endDate.value) return null;

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const diffMs = end.getTime() - start.getTime();

  if (diffMs <= 0) return null;

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  return { days, hours, minutes, totalMinutes };
});

// Примерная цена
const estimatedPrice = computed(() => {
  if (!duration.value || !props.car.priceHour) return null;

  const totalHours = duration.value.totalMinutes / 60;
  return Math.round(totalHours * props.car.priceHour);
});

// Валидация
const isValid = computed(() => {
  if (!startDate.value || !endDate.value) {
    validationError.value = "Заполните обе даты";
    return false;
  }

  const start = new Date(startDate.value);
  const end = new Date(endDate.value);
  const now = new Date();

  if (start < now) {
    validationError.value = "Дата начала не может быть в прошлом";
    return false;
  }

  if (end <= start) {
    validationError.value = "Дата окончания должна быть позже даты начала";
    return false;
  }

  const diffMs = end.getTime() - start.getTime();
  const minDuration = 60 * 60 * 1000; // 1 час

  if (diffMs < minDuration) {
    validationError.value = "Минимальная продолжительность аренды - 1 час";
    return false;
  }

  validationError.value = "";
  return true;
});

function formatSuggestionDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function applySuggestedDate(value: string) {
  const start = new Date(value);
  if (Number.isNaN(start.getTime())) {
    return;
  }

  const end = new Date(start.getTime() + 3 * 60 * 60 * 1000);
  const localStart = new Date(start.getTime() - start.getTimezoneOffset() * 60000);
  const localEnd = new Date(end.getTime() - end.getTimezoneOffset() * 60000);

  startDate.value = localStart.toISOString().slice(0, 16);
  endDate.value = localEnd.toISOString().slice(0, 16);
  emit("suggestion-click", value);
}

function closeModal() {
  emit("close");
}

function confirmBooking() {
  if (!isValid.value) return;

  const start = new Date(startDate.value).toISOString();
  const end = new Date(endDate.value).toISOString();

  emit("confirm", start, end);
}
</script>

<style scoped>
/* Modal transitions */
.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.3s ease;
}

.modal-enter-active > div,
.modal-leave-active > div {
  transition: transform 0.3s ease, opacity 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from > div,
.modal-leave-to > div {
  transform: scale(0.9);
  opacity: 0;
}

/* Custom datetime input styling */
input[type="datetime-local"]::-webkit-calendar-picker-indicator {
  opacity: 0;
  position: absolute;
  right: 0;
  width: 100%;
  height: 100%;
  cursor: pointer;
}
</style>
