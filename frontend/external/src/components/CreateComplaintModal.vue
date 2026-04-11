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
            class="relative p-6 bg-gradient-to-r from-red-600 to-red-700 dark:from-red-700 dark:to-red-800"
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
              <h2 class="text-2xl font-bold text-white mb-2">Подать жалобу</h2>
              <p class="text-red-100">
                Бронирование #{{ bookingId }}
              </p>
            </div>
          </div>

          <!-- Content -->
          <form @submit.prevent="handleSubmit" class="p-6 space-y-6">
            <!-- Category -->
            <div>
              <label
                for="complaint-category"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Категория
                <span class="text-red-500">*</span>
              </label>
              <select
                id="complaint-category"
                v-model="category"
                class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all"
                :class="{
                  'border-red-500 dark:border-red-500': errors.category,
                }"
              >
                <option value="" disabled>Выберите категорию</option>
                <option
                  v-for="cat in availableCategories"
                  :key="cat.value"
                  :value="cat.value"
                >
                  {{ cat.label }}
                </option>
              </select>
              <p
                v-if="errors.category"
                class="mt-2 text-sm text-red-600 dark:text-red-400"
              >
                {{ errors.category }}
              </p>
            </div>

            <!-- Subject -->
            <div>
              <label
                for="complaint-subject"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Тема
                <span class="text-red-500">*</span>
              </label>
              <input
                id="complaint-subject"
                v-model="subject"
                type="text"
                maxlength="200"
                class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all"
                placeholder="Кратко опишите проблему"
                :class="{
                  'border-red-500 dark:border-red-500': errors.subject,
                }"
              />
              <div class="mt-2 flex items-center justify-between">
                <p
                  v-if="errors.subject"
                  class="text-sm text-red-600 dark:text-red-400"
                >
                  {{ errors.subject }}
                </p>
                <p
                  class="text-sm text-gray-500 dark:text-gray-400 ml-auto"
                  :class="{
                    'text-red-500 dark:text-red-400': subject.length > 200,
                  }"
                >
                  {{ subject.length }}/200
                </p>
              </div>
            </div>

            <!-- Description -->
            <div>
              <label
                for="complaint-description"
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Описание
                <span class="text-red-500">*</span>
              </label>
              <textarea
                id="complaint-description"
                v-model="description"
                rows="5"
                maxlength="4000"
                class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-red-500 focus:border-red-500 transition-all resize-none"
                placeholder="Подробно опишите вашу проблему"
                :class="{
                  'border-red-500 dark:border-red-500': errors.description,
                }"
              ></textarea>
              <div class="mt-2 flex items-center justify-between">
                <p
                  v-if="errors.description"
                  class="text-sm text-red-600 dark:text-red-400"
                >
                  {{ errors.description }}
                </p>
                <p
                  class="text-sm text-gray-500 dark:text-gray-400 ml-auto"
                  :class="{
                    'text-red-500 dark:text-red-400': description.length > 4000,
                  }"
                >
                  {{ description.length }}/4000
                </p>
              </div>
            </div>

            <!-- Attachments -->
            <div>
              <label
                class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-3"
              >
                Вложения
                <span class="text-gray-400 font-normal">(необязательно, до 5 файлов)</span>
              </label>
              <input
                ref="fileInput"
                type="file"
                multiple
                accept="image/*,.pdf,.doc,.docx"
                class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white transition-all file:mr-4 file:py-1 file:px-3 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-red-50 file:text-red-700 dark:file:bg-red-900/30 dark:file:text-red-300 hover:file:bg-red-100 dark:hover:file:bg-red-900/50"
                :class="{
                  'border-red-500 dark:border-red-500': errors.attachments,
                }"
                @change="handleFileChange"
              />
              <p
                v-if="errors.attachments"
                class="mt-2 text-sm text-red-600 dark:text-red-400"
              >
                {{ errors.attachments }}
              </p>
              <p
                v-if="selectedFiles.length > 0"
                class="mt-2 text-sm text-gray-500 dark:text-gray-400"
              >
                Выбрано файлов: {{ selectedFiles.length }}
              </p>
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
                :disabled="isSubmitting"
                class="flex-1 px-6 py-3 rounded-xl font-semibold text-white bg-red-600 hover:bg-red-700 transition-all shadow-lg shadow-red-500/30 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <span
                  v-if="!isSubmitting"
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
                      d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z"
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

interface Props {
  isOpen: boolean;
  bookingId: number;
  isPartner: boolean;
}

interface Emits {
  (e: "close"): void;
  (e: "submit"): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const category = ref("");
const subject = ref("");
const description = ref("");
const selectedFiles = ref<File[]>([]);
const fileInput = ref<HTMLInputElement | null>(null);
const isSubmitting = ref(false);
const errors = ref({
  category: "",
  subject: "",
  description: "",
  attachments: "",
});

const clientCategories = [
  { value: "car_condition", label: "Состояние авто" },
  { value: "late_handover", label: "Задержка передачи" },
  { value: "service_quality", label: "Качество сервиса" },
  { value: "safety_issue", label: "Безопасность" },
  { value: "other", label: "Другое" },
];

const partnerCategories = [
  { value: "safety_issue", label: "Безопасность" },
  { value: "client_misbehavior", label: "Поведение клиента" },
  { value: "other", label: "Другое" },
];

const availableCategories = computed(() =>
  props.isPartner ? partnerCategories : clientCategories,
);

const targetType = computed(() => (props.isPartner ? "client" : "partner"));

function handleFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const files = Array.from(input.files ?? []);

  if (files.length > 5) {
    errors.value.attachments = "Максимум 5 файлов";
    selectedFiles.value = files.slice(0, 5);
  } else {
    errors.value.attachments = "";
    selectedFiles.value = files;
  }
}

function validateForm(): boolean {
  errors.value = { category: "", subject: "", description: "", attachments: "" };
  let isValid = true;

  if (!category.value) {
    errors.value.category = "Выберите категорию";
    isValid = false;
  }

  if (!subject.value.trim()) {
    errors.value.subject = "Укажите тему";
    isValid = false;
  } else if (subject.value.length > 200) {
    errors.value.subject = "Тема не должна превышать 200 символов";
    isValid = false;
  }

  if (!description.value.trim()) {
    errors.value.description = "Заполните описание";
    isValid = false;
  } else if (description.value.length < 10) {
    errors.value.description = "Описание должно содержать минимум 10 символов";
    isValid = false;
  } else if (description.value.length > 4000) {
    errors.value.description = "Описание не должно превышать 4000 символов";
    isValid = false;
  }

  if (selectedFiles.value.length > 5) {
    errors.value.attachments = "Максимум 5 файлов";
    isValid = false;
  }

  return isValid;
}

async function handleSubmit() {
  if (!validateForm()) return;

  isSubmitting.value = true;

  try {
    const { createComplaint } = await import("../api/complaints");

    const formData = new FormData();
    formData.append("bookingId", props.bookingId.toString());
    formData.append("targetType", targetType.value);
    formData.append("category", category.value);
    formData.append("subject", subject.value.trim());
    formData.append("description", description.value.trim());

    for (const file of selectedFiles.value) {
      formData.append("attachments", file);
    }

    await createComplaint(formData);
    emit("submit");
  } catch (error) {
    console.error("Failed to create complaint:", error);
  } finally {
    isSubmitting.value = false;
  }
}

function handleClose() {
  if (!isSubmitting.value) {
    resetForm();
    emit("close");
  }
}

function resetForm() {
  category.value = "";
  subject.value = "";
  description.value = "";
  selectedFiles.value = [];
  errors.value = { category: "", subject: "", description: "", attachments: "" };
  if (fileInput.value) {
    fileInput.value.value = "";
  }
}

watch(
  () => props.isOpen,
  (newVal) => {
    if (newVal) {
      resetForm();
    }
  },
);
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
