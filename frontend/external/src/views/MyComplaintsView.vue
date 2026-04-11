<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-6xl mx-auto">
      <!-- Header -->
      <div class="mb-12 space-y-5 animate-slide-up">
        <h1
          class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
        >
          Мои обращения
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Отслеживайте статус ваших жалоб и обращений
        </p>

        <!-- Filters -->
        <div
          class="flex flex-wrap gap-3 rounded-3xl border border-white/10 bg-white/60 p-3 shadow-xl backdrop-blur-md dark:bg-gray-900/70 dark:border-gray-800"
        >
          <button
            v-for="filter in filters"
            :key="filter.value"
            @click="currentFilter = filter.value"
            :class="[
              'px-5 py-2.5 rounded-xl font-extrabold text-sm transition-all',
              currentFilter === filter.value
                ? 'bg-primary-600 text-white shadow-lg shadow-primary-500/50'
                : 'bg-white dark:bg-gray-800 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700 border border-gray-200 dark:border-gray-700',
            ]"
          >
            {{ filter.label }}
            <span
              v-if="filter.count > 0"
              :class="[
                'ml-2 px-2 py-0.5 rounded-full text-xs font-extrabold',
                currentFilter === filter.value
                  ? 'bg-white/20'
                  : 'bg-gray-100 dark:bg-gray-700',
              ]"
            >
              {{ filter.count }}
            </span>
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-4">
          <svg
            class="w-10 h-10 animate-spin text-primary-600"
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
          <p class="text-gray-500 dark:text-gray-400">Загрузка обращений...</p>
        </div>
      </div>

      <!-- Complaints List -->
      <div v-else-if="filteredComplaints.length > 0" class="space-y-6">
        <div
          v-for="complaint in filteredComplaints"
          :key="complaint.id"
          class="group relative overflow-hidden rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-md hover:shadow-xl transition-all duration-300"
        >
          <!-- Card Header -->
          <div
            class="p-5 cursor-pointer"
            @click="toggleExpand(complaint.id)"
          >
            <div class="flex flex-col sm:flex-row sm:items-start gap-4">
              <!-- Left: Info -->
              <div class="flex-1 min-w-0">
                <div class="flex items-start justify-between gap-3 mb-2">
                  <h3
                    class="text-lg font-bold text-gray-900 dark:text-white leading-tight truncate"
                  >
                    {{ complaint.subject }}
                  </h3>
                  <span
                    :class="getStatusClass(complaint.status)"
                    class="inline-flex items-center gap-1.5 px-3 py-1 rounded-xl text-xs font-bold uppercase tracking-wide flex-shrink-0"
                  >
                    <span
                      :class="getStatusDotClass(complaint.status)"
                      class="w-1.5 h-1.5 rounded-full"
                    ></span>
                    {{ statusLabels[complaint.status] || 'Неизвестно' }}
                  </span>
                </div>

                <div class="flex flex-wrap items-center gap-3 text-sm">
                  <span
                    class="inline-flex items-center px-2.5 py-1 rounded-lg text-xs font-bold bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300"
                  >
                    {{ categoryLabels[complaint.category] || 'Другое' }}
                  </span>
                  <span
                    v-if="complaint.snapshotData"
                    class="text-gray-600 dark:text-gray-400 font-medium"
                  >
                    {{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}
                  </span>
                  <span class="text-gray-400 dark:text-gray-500 text-xs">
                    {{ formatDate(complaint.createdAt) }}
                  </span>
                </div>
              </div>

              <!-- Expand indicator -->
              <div class="flex-shrink-0 hidden sm:block">
                <svg
                  :class="[
                    'w-5 h-5 text-gray-400 transition-transform duration-200',
                    expandedId === complaint.id ? 'rotate-180' : '',
                  ]"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </div>
            </div>
          </div>

          <!-- Expanded Detail -->
          <Transition name="expand">
            <div
              v-if="expandedId === complaint.id"
              class="border-t border-gray-200 dark:border-gray-800"
            >
              <div class="p-5 space-y-5">
                <!-- Description -->
                <div>
                  <h4 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">
                    Описание
                  </h4>
                  <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                    {{ complaint.description }}
                  </p>
                </div>

                <!-- Booking Snapshot -->
                <div
                  v-if="complaint.snapshotData"
                  class="rounded-2xl bg-gray-50 dark:bg-gray-800/50 p-4"
                >
                  <h4 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-3">
                    Бронирование
                  </h4>
                  <div class="grid grid-cols-2 gap-3 text-sm">
                    <div>
                      <span class="text-gray-500 dark:text-gray-400">Автомобиль:</span>
                      <span class="ml-1 font-medium text-gray-900 dark:text-white">
                        {{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}
                      </span>
                    </div>
                    <div v-if="complaint.snapshotData.partnerName">
                      <span class="text-gray-500 dark:text-gray-400">Партнер:</span>
                      <span class="ml-1 font-medium text-gray-900 dark:text-white">
                        {{ complaint.snapshotData.partnerName }}
                      </span>
                    </div>
                    <div>
                      <span class="text-gray-500 dark:text-gray-400">Начало:</span>
                      <span class="ml-1 font-medium text-gray-900 dark:text-white">
                        {{ formatDate(complaint.snapshotData.startTime) }}
                      </span>
                    </div>
                    <div>
                      <span class="text-gray-500 dark:text-gray-400">Окончание:</span>
                      <span class="ml-1 font-medium text-gray-900 dark:text-white">
                        {{ formatDate(complaint.snapshotData.endTime) }}
                      </span>
                    </div>
                    <div v-if="complaint.snapshotData.totalPrice">
                      <span class="text-gray-500 dark:text-gray-400">Стоимость:</span>
                      <span class="ml-1 font-medium text-gray-900 dark:text-white">
                        {{ formatMoney(complaint.snapshotData.totalPrice) }}
                      </span>
                    </div>
                  </div>
                </div>

                <!-- Attachments -->
                <div v-if="complaint.attachments && complaint.attachments.length > 0">
                  <h4 class="text-sm font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-2">
                    Вложения
                  </h4>
                  <div class="flex flex-wrap gap-2">
                    <button
                      v-for="attachment in complaint.attachments"
                      :key="attachment.id"
                      @click.stop="downloadAttachment(complaint.id, attachment.id, attachment.originalFileName)"
                      class="inline-flex items-center gap-2 px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
                    >
                      <svg
                        class="w-4 h-4 text-gray-400"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                      >
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                        />
                      </svg>
                      {{ attachment.originalFileName }}
                    </button>
                  </div>
                </div>

                <!-- Info Request from Manager -->
                <div
                  v-if="complaint.infoRequestText"
                  class="rounded-2xl border border-amber-200 bg-amber-50 p-4 dark:border-amber-500/30 dark:bg-amber-950/20"
                >
                  <h4 class="text-sm font-semibold text-amber-700 dark:text-amber-300 uppercase tracking-wide mb-2">
                    Запрос от менеджера
                  </h4>
                  <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                    {{ complaint.infoRequestText }}
                  </p>
                  <p
                    v-if="complaint.infoRequestAt"
                    class="mt-2 text-xs text-amber-600 dark:text-amber-400"
                  >
                    {{ formatDate(complaint.infoRequestAt) }}
                  </p>
                </div>

                <!-- Info Response -->
                <div
                  v-if="complaint.infoResponseText"
                  class="rounded-2xl border border-blue-200 bg-blue-50 p-4 dark:border-blue-500/30 dark:bg-blue-950/20"
                >
                  <h4 class="text-sm font-semibold text-blue-700 dark:text-blue-300 uppercase tracking-wide mb-2">
                    Ваш ответ
                  </h4>
                  <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                    {{ complaint.infoResponseText }}
                  </p>
                  <p
                    v-if="complaint.infoResponseAt"
                    class="mt-2 text-xs text-blue-600 dark:text-blue-400"
                  >
                    {{ formatDate(complaint.infoResponseAt) }}
                  </p>
                </div>

                <!-- Resolution -->
                <div
                  v-if="complaint.status === 4 && complaint.resolutionType"
                  class="rounded-2xl border border-green-200 bg-green-50 p-4 dark:border-green-500/30 dark:bg-green-950/20"
                >
                  <h4 class="text-sm font-semibold text-green-700 dark:text-green-300 uppercase tracking-wide mb-2">
                    Решение
                  </h4>
                  <p class="text-sm font-medium text-gray-900 dark:text-white mb-1">
                    {{ resolutionTypeLabels[complaint.resolutionType] || 'Неизвестно' }}
                  </p>
                  <p
                    v-if="complaint.resolutionNote"
                    class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap"
                  >
                    {{ complaint.resolutionNote }}
                  </p>
                  <p
                    v-if="complaint.resolvedAt"
                    class="mt-2 text-xs text-green-600 dark:text-green-400"
                  >
                    {{ formatDate(complaint.resolvedAt) }}
                  </p>
                </div>

                <!-- Rejection -->
                <div
                  v-if="complaint.status === 5"
                  class="rounded-2xl border border-red-200 bg-red-50 p-4 dark:border-red-500/30 dark:bg-red-950/20"
                >
                  <h4 class="text-sm font-semibold text-red-700 dark:text-red-300 uppercase tracking-wide mb-2">
                    Отклонено
                  </h4>
                  <p
                    v-if="complaint.rejectionReason"
                    class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap"
                  >
                    {{ complaint.rejectionReason }}
                  </p>
                  <p
                    v-if="complaint.rejectedAt"
                    class="mt-2 text-xs text-red-600 dark:text-red-400"
                  >
                    {{ formatDate(complaint.rejectedAt) }}
                  </p>
                </div>

                <!-- Respond to Info Request (status === 3: AwaitingResponse) -->
                <div
                  v-if="complaint.status === 3 && !complaint.infoResponseText"
                  class="rounded-2xl border-2 border-amber-300 bg-amber-50 p-5 dark:border-amber-500/40 dark:bg-amber-950/30"
                >
                  <h4 class="text-sm font-bold text-amber-700 dark:text-amber-300 uppercase tracking-wide mb-3">
                    Ответить на запрос
                  </h4>
                  <form @submit.prevent="handleRespondSubmit(complaint.id)" class="space-y-4">
                    <textarea
                      v-model="respondText"
                      rows="4"
                      class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:ring-2 focus:ring-amber-500 focus:border-amber-500 transition-all resize-none"
                      placeholder="Ваш ответ на запрос менеджера..."
                    ></textarea>
                    <div>
                      <label
                        class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"
                      >
                        Дополнительные файлы
                        <span class="text-gray-400 font-normal">(необязательно)</span>
                      </label>
                      <input
                        ref="respondFileInput"
                        type="file"
                        multiple
                        accept="image/*,.pdf,.doc,.docx"
                        class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white transition-all file:mr-4 file:py-1 file:px-3 file:rounded-lg file:border-0 file:text-sm file:font-semibold file:bg-amber-50 file:text-amber-700 dark:file:bg-amber-900/30 dark:file:text-amber-300 hover:file:bg-amber-100 dark:hover:file:bg-amber-900/50"
                        @change="handleRespondFileChange"
                      />
                    </div>
                    <div class="flex justify-end">
                      <button
                        type="submit"
                        :disabled="respondSubmitting || !respondText.trim()"
                        class="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-white bg-amber-600 hover:bg-amber-700 transition-all shadow-lg shadow-amber-500/30 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <span v-if="!respondSubmitting">Отправить ответ</span>
                        <span v-else class="flex items-center gap-2">
                          <svg
                            class="w-4 h-4 animate-spin"
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
            </div>
          </Transition>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else-if="!loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6 max-w-md mx-auto">
          <div
            class="w-24 h-24 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
          >
            <svg
              class="w-12 h-12 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          <div class="space-y-2">
            <h3 class="text-2xl font-bold text-gray-900 dark:text-white">
              {{ getEmptyStateTitle() }}
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              {{ getEmptyStateDescription() }}
            </p>
          </div>
          <router-link
            to="/bookings"
            class="btn-premium inline-flex items-center gap-2"
          >
            <span>К бронированиям</span>
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
                d="M13 7l5 5m0 0l-5 5m5-5H6"
              />
            </svg>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { getMyComplaints, respondToInfoRequest, getAttachmentLink } from "../api/complaints";
import type { Complaint } from "../types/Complaint";
import { useToast } from "../composables/useToast";

const complaints = ref<Complaint[]>([]);
const loading = ref(true);
const currentFilter = ref<"all" | "active" | "closed">("all");
const expandedId = ref<string | null>(null);
const respondText = ref("");
const respondFiles = ref<File[]>([]);
const respondSubmitting = ref(false);
const respondFileInput = ref<HTMLInputElement | null>(null);
const { success, error } = useToast();

const statusLabels: Record<number, string> = {
  1: "Новая",
  2: "На рассмотрении",
  3: "Ожидает вашего ответа",
  4: "Решена",
  5: "Отклонена",
};

const categoryLabels: Record<number, string> = {
  1: "Состояние авто",
  2: "Задержка передачи",
  3: "Качество сервиса",
  4: "Безопасность",
  5: "Поведение клиента",
  99: "Другое",
};

const resolutionTypeLabels: Record<number, string> = {
  1: "В пользу заявителя",
  2: "В пользу контрагента",
  3: "Компромисс",
  4: "Действий не требуется",
};

const filters = computed(() => {
  const all = complaints.value.length;
  const active = complaints.value.filter(
    (c) => c.status === 1 || c.status === 2 || c.status === 3,
  ).length;
  const closed = complaints.value.filter(
    (c) => c.status === 4 || c.status === 5,
  ).length;

  return [
    { label: "Все", value: "all" as const, count: all },
    { label: "Активные", value: "active" as const, count: active },
    { label: "Закрытые", value: "closed" as const, count: closed },
  ];
});

const filteredComplaints = computed(() => {
  if (currentFilter.value === "all") {
    return complaints.value;
  }
  if (currentFilter.value === "active") {
    return complaints.value.filter(
      (c) => c.status === 1 || c.status === 2 || c.status === 3,
    );
  }
  return complaints.value.filter(
    (c) => c.status === 4 || c.status === 5,
  );
});

function toggleExpand(id: string) {
  expandedId.value = expandedId.value === id ? null : id;
  respondText.value = "";
  respondFiles.value = [];
}

function handleRespondFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  respondFiles.value = Array.from(input.files ?? []);
}

async function handleRespondSubmit(complaintId: string) {
  if (!respondText.value.trim()) return;

  respondSubmitting.value = true;

  try {
    const formData = new FormData();
    formData.append("message", respondText.value.trim());

    for (const file of respondFiles.value) {
      formData.append("attachments", file);
    }

    const updated = await respondToInfoRequest(complaintId, formData);

    const index = complaints.value.findIndex((c) => c.id === complaintId);
    if (index !== -1) {
      complaints.value[index] = updated;
    }

    respondText.value = "";
    respondFiles.value = [];
    if (respondFileInput.value) {
      respondFileInput.value.value = "";
    }

    success("Ответ успешно отправлен");
  } catch (e) {
    console.error("Failed to respond to info request:", e);
    error(
      (e as any)?.response?.data?.detail ||
        (e as any)?.response?.data?.error ||
        "Не удалось отправить ответ",
    );
  } finally {
    respondSubmitting.value = false;
  }
}

async function downloadAttachment(
  complaintId: string,
  attachmentId: string,
  fileName: string,
) {
  try {
    const url = await getAttachmentLink(complaintId, attachmentId);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.target = "_blank";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  } catch (e) {
    console.error("Failed to download attachment:", e);
    error("Не удалось скачать файл");
  }
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatMoney(amount: number | null | undefined): string {
  if (amount == null) return "";
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "KZT",
    maximumFractionDigits: 2,
  }).format(amount);
}

function getStatusClass(status: number): string {
  switch (status) {
    case 1:
      return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
    case 2:
      return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
    case 3:
      return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
    case 4:
      return "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300";
    case 5:
      return "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300";
    default:
      return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
}

function getStatusDotClass(status: number): string {
  switch (status) {
    case 1:
      return "bg-blue-600 dark:bg-blue-400";
    case 2:
      return "bg-violet-600 dark:bg-violet-400";
    case 3:
      return "bg-amber-600 dark:bg-amber-400";
    case 4:
      return "bg-green-600 dark:bg-green-400";
    case 5:
      return "bg-red-600 dark:bg-red-400";
    default:
      return "bg-gray-600 dark:bg-gray-400";
  }
}

function getEmptyStateTitle(): string {
  if (currentFilter.value === "all") {
    return "Нет обращений";
  }
  if (currentFilter.value === "active") {
    return "Нет активных обращений";
  }
  return "Нет закрытых обращений";
}

function getEmptyStateDescription(): string {
  if (currentFilter.value === "all") {
    return "У вас пока нет обращений. Вы можете подать жалобу из раздела бронирований.";
  }
  return "Попробуйте выбрать другой фильтр";
}

onMounted(async () => {
  try {
    complaints.value = await getMyComplaints();
  } catch (e) {
    console.error("Failed to load complaints:", e);
    error("Не удалось загрузить обращения");
    complaints.value = [];
  } finally {
    loading.value = false;
  }
});
</script>

<style scoped>
.expand-enter-active,
.expand-leave-active {
  transition: all 0.3s ease;
  overflow: hidden;
}

.expand-enter-from,
.expand-leave-to {
  opacity: 0;
  max-height: 0;
}

.expand-enter-to,
.expand-leave-from {
  opacity: 1;
  max-height: 2000px;
}
</style>
