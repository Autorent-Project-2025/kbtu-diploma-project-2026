<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-7xl mx-auto">

      <!-- Back link -->
      <router-link
        to="/complaints"
        class="inline-flex items-center gap-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors mb-6"
      >
        <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
        </svg>
        Все обращения
      </router-link>

      <!-- Loading -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-4">
          <svg class="w-10 h-10 animate-spin text-primary-600" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
          <p class="text-gray-500 dark:text-gray-400">Загрузка обращения...</p>
        </div>
      </div>

      <!-- Not found -->
      <div v-else-if="!complaint" class="text-center py-32">
        <p class="text-gray-500 dark:text-gray-400 text-lg">Обращение не найдено</p>
      </div>

      <template v-else>
        <!-- Two-column layout -->
        <div class="grid grid-cols-1 lg:grid-cols-5 gap-6">

          <!-- Left column: complaint info -->
          <div class="lg:col-span-2 space-y-5">

            <!-- Header card -->
            <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-lg overflow-hidden">
              <div class="px-6 py-5">
                <h1 class="text-xl font-extrabold text-gray-900 dark:text-white leading-tight mb-3">{{ complaint.subject }}</h1>
                <div class="flex flex-wrap items-center gap-2">
                  <span
                    :class="getStatusClass(complaint.status)"
                    class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide shadow-sm"
                  >
                    <span :class="getStatusDotClass(complaint.status)" class="w-2 h-2 rounded-full animate-pulse"></span>
                    {{ statusLabels[complaint.status] || 'Неизвестно' }}
                  </span>
                  <span class="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-bold bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 ring-1 ring-gray-200 dark:ring-gray-700">
                    {{ categoryLabels[complaint.category] || 'Другое' }}
                  </span>
                </div>
              </div>
              <div class="px-6 py-3 bg-gray-50 dark:bg-gray-800/50 border-t border-gray-100 dark:border-gray-800">
                <span class="text-xs text-gray-500 dark:text-gray-400">Создано: {{ formatDate(complaint.createdAt) }}</span>
              </div>
            </div>

            <!-- Booking Snapshot -->
            <div v-if="complaint.snapshotData" class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div class="px-5 py-3 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                <h2 class="text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Бронирование</h2>
              </div>
              <div class="px-5 py-4 space-y-2.5 text-sm">
                <div class="flex items-start justify-between">
                  <span class="text-gray-500 dark:text-gray-400 shrink-0">Автомобиль</span>
                  <span class="font-medium text-gray-900 dark:text-white text-right">{{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}</span>
                </div>
                <div v-if="complaint.snapshotData.partnerName" class="flex items-start justify-between">
                  <span class="text-gray-500 dark:text-gray-400 shrink-0">Партнер</span>
                  <span class="font-medium text-gray-900 dark:text-white text-right">{{ complaint.snapshotData.partnerName }}</span>
                </div>
                <div class="flex items-start justify-between">
                  <span class="text-gray-500 dark:text-gray-400 shrink-0">Начало</span>
                  <span class="font-medium text-gray-900 dark:text-white text-right">{{ formatDate(complaint.snapshotData.startTime) }}</span>
                </div>
                <div class="flex items-start justify-between">
                  <span class="text-gray-500 dark:text-gray-400 shrink-0">Окончание</span>
                  <span class="font-medium text-gray-900 dark:text-white text-right">{{ formatDate(complaint.snapshotData.endTime) }}</span>
                </div>
                <div v-if="complaint.snapshotData.totalPrice" class="flex items-start justify-between pt-2 border-t border-gray-100 dark:border-gray-800">
                  <span class="text-gray-500 dark:text-gray-400 shrink-0">Стоимость</span>
                  <span class="font-bold text-gray-900 dark:text-white text-right">{{ formatMoney(complaint.snapshotData.totalPrice) }}</span>
                </div>
              </div>
            </div>

            <!-- Description -->
            <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div class="px-5 py-3 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                <h2 class="text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Описание</h2>
              </div>
              <div class="px-5 py-4">
                <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap text-sm leading-relaxed">{{ complaint.description }}</p>
              </div>
            </div>

            <!-- Attachments -->
            <div v-if="complaint.attachments && complaint.attachments.length > 0" class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div class="px-5 py-3 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                <h2 class="text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Вложения ({{ complaint.attachments.length }})</h2>
              </div>
              <div class="px-5 py-4 space-y-4">
                <div v-if="imageAttachments.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  <button
                    v-for="attachment in imageAttachments"
                    :key="attachment.id"
                    type="button"
                    @click="openAttachmentPreview(attachment)"
                    class="overflow-hidden rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-left transition-all hover:border-primary-300 hover:shadow-md dark:hover:border-primary-500/40"
                  >
                    <img
                      v-if="attachmentPreviewUrls[attachment.id]"
                      :src="attachmentPreviewUrls[attachment.id]"
                      :alt="attachment.originalFileName"
                      class="h-44 w-full object-cover"
                      loading="lazy"
                    />
                    <div
                      v-else
                      class="h-44 w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 text-xs font-medium text-gray-400 dark:text-gray-500"
                    >
                      Загрузка изображения...
                    </div>
                    <div class="flex items-center justify-between gap-3 px-3 py-2.5">
                      <span class="truncate text-sm font-medium text-gray-700 dark:text-gray-300">{{ attachment.originalFileName }}</span>
                      <span class="text-[11px] font-semibold uppercase tracking-wide text-primary-600 dark:text-primary-400">Открыть</span>
                    </div>
                  </button>
                </div>
                <div v-if="fileAttachments.length > 0" class="flex flex-wrap gap-2">
                  <button
                    v-for="attachment in fileAttachments"
                    :key="attachment.id"
                    @click="downloadAttachment(attachment.id, attachment.originalFileName)"
                    class="inline-flex items-center gap-2 px-3 py-2 rounded-lg border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm font-medium text-gray-700 dark:text-gray-300 hover:bg-primary-50 hover:border-primary-300 hover:text-primary-700 dark:hover:bg-primary-900/20 dark:hover:border-primary-500/40 dark:hover:text-primary-300 transition-all"
                  >
                    <svg class="w-4 h-4 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                    </svg>
                    <span class="truncate max-w-[180px]">{{ attachment.originalFileName }}</span>
                  </button>
                </div>
              </div>
            </div>

            <!-- Resolution -->
            <div v-if="complaint.status === 4" class="rounded-2xl border-2 border-green-300 dark:border-green-500/40 bg-green-50 dark:bg-green-950/30 overflow-hidden">
              <div class="px-5 py-3 bg-green-100/60 dark:bg-green-900/30 border-b border-green-200 dark:border-green-500/30 flex items-center gap-2">
                <svg class="w-4 h-4 text-green-600 dark:text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <h2 class="text-xs font-bold text-green-700 dark:text-green-300 uppercase tracking-wider">Решение</h2>
              </div>
              <div class="px-5 py-4 space-y-2">
                <p v-if="complaint.resolutionType" class="text-sm font-semibold text-green-800 dark:text-green-200">{{ resolutionTypeLabels[complaint.resolutionType] || 'Неизвестно' }}</p>
                <p v-if="complaint.resolutionNote" class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap text-sm leading-relaxed">{{ complaint.resolutionNote }}</p>
                <p v-if="complaint.resolvedAt" class="text-xs text-green-600 dark:text-green-400 pt-1">{{ formatDate(complaint.resolvedAt) }}</p>
              </div>
            </div>

            <!-- Rejection -->
            <div v-if="complaint.status === 5" class="rounded-2xl border-2 border-red-300 dark:border-red-500/40 bg-red-50 dark:bg-red-950/30 overflow-hidden">
              <div class="px-5 py-3 bg-red-100/60 dark:bg-red-900/30 border-b border-red-200 dark:border-red-500/30 flex items-center gap-2">
                <svg class="w-4 h-4 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <h2 class="text-xs font-bold text-red-700 dark:text-red-300 uppercase tracking-wider">Отклонено</h2>
              </div>
              <div class="px-5 py-4 space-y-2">
                <p v-if="complaint.rejectionReason" class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap text-sm leading-relaxed">{{ complaint.rejectionReason }}</p>
                <p v-if="complaint.rejectedAt" class="text-xs text-red-600 dark:text-red-400 pt-1">{{ formatDate(complaint.rejectedAt) }}</p>
              </div>
            </div>

            <!-- Reopen Request Section (for closed complaints) -->
            <div v-if="isClosed" class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-sm overflow-hidden">
              <div class="px-5 py-3 bg-gray-50 dark:bg-gray-800/50 border-b border-gray-100 dark:border-gray-800">
                <h2 class="text-xs font-bold text-gray-500 dark:text-gray-400 uppercase tracking-wider">Переоткрытие</h2>
              </div>
              <div class="px-5 py-4 space-y-4">
                <!-- Existing reopen requests -->
                <div v-if="reopenRequests.length > 0" class="space-y-3">
                  <div
                    v-for="req in reopenRequests"
                    :key="req.id"
                    :class="[
                      'rounded-xl border p-4 text-sm',
                      req.status === 1 ? 'border-amber-200 bg-amber-50 dark:border-amber-500/30 dark:bg-amber-950/20' :
                      req.status === 2 ? 'border-green-200 bg-green-50 dark:border-green-500/30 dark:bg-green-950/20' :
                      'border-red-200 bg-red-50 dark:border-red-500/30 dark:bg-red-950/20'
                    ]"
                  >
                    <div class="flex items-center gap-2 mb-1">
                      <span :class="[
                        'px-2 py-0.5 rounded-full text-[10px] font-bold uppercase',
                        req.status === 1 ? 'bg-amber-100 text-amber-700 dark:bg-amber-900/40 dark:text-amber-300' :
                        req.status === 2 ? 'bg-green-100 text-green-700 dark:bg-green-900/40 dark:text-green-300' :
                        'bg-red-100 text-red-700 dark:bg-red-900/40 dark:text-red-300'
                      ]">
                        {{ req.status === 1 ? 'На рассмотрении' : req.status === 2 ? 'Одобрено' : 'Отклонено' }}
                      </span>
                      <span class="text-xs text-gray-400">{{ formatDate(req.createdAt) }}</span>
                    </div>
                    <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ req.reason }}</p>
                    <p v-if="req.decisionNote" class="mt-2 text-xs text-gray-500 dark:text-gray-400 italic">{{ req.decisionNote }}</p>
                  </div>
                </div>

                <!-- Create reopen request form -->
                <div v-if="!hasPendingReopen">
                  <div v-if="!showReopenForm">
                    <button
                      @click="showReopenForm = true"
                      class="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-white bg-primary-600 hover:bg-primary-700 transition-all shadow-lg shadow-primary-500/30"
                    >
                      <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                      </svg>
                      Запросить переоткрытие
                    </button>
                  </div>
                  <form v-else @submit.prevent="handleReopenSubmit" class="space-y-4">
                    <textarea
                      v-model="reopenReason"
                      rows="3"
                      class="w-full px-4 py-3 rounded-xl border-2 border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:ring-2 focus:ring-primary-500 focus:border-primary-500 transition-all resize-none text-sm"
                      placeholder="Опишите причину, по которой вы хотите переоткрыть обращение..."
                    ></textarea>
                    <div class="flex items-center gap-3">
                      <button
                        type="submit"
                        :disabled="reopenSubmitting || !reopenReason.trim()"
                        class="px-5 py-2.5 rounded-xl font-semibold text-white bg-primary-600 hover:bg-primary-700 transition-all shadow-lg shadow-primary-500/30 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                      >
                        {{ reopenSubmitting ? 'Отправка...' : 'Отправить запрос' }}
                      </button>
                      <button
                        type="button"
                        @click="showReopenForm = false; reopenReason = ''"
                        class="px-5 py-2.5 rounded-xl font-semibold text-gray-700 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors text-sm"
                      >
                        Отмена
                      </button>
                    </div>
                  </form>
                </div>
                <p v-else class="text-sm text-amber-600 dark:text-amber-400 font-medium">
                  Ваш запрос на переоткрытие находится на рассмотрении
                </p>
              </div>
            </div>

          </div>

          <!-- Right column: chat (sticky on desktop) -->
          <div class="lg:col-span-3">
            <div class="lg:sticky lg:top-24">
              <ChatPanel
                :context-type="'complaint'"
                :context-id="complaint.id"
                height="600px"
                :refresh-context="refreshComplaint"
              />
            </div>
          </div>

        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useRoute } from "vue-router";
import {
  getMyComplaintById,
  getAttachmentLink,
  createReopenRequest,
  getReopenRequests,
} from "../api/complaints";
import type { Complaint, ComplaintAttachment, ReopenRequest } from "../types/Complaint";
import { useToast } from "../composables/useToast";
import ChatPanel from "../components/ChatPanel.vue";
import { isImageMimeType, resolveAttachmentPreviewUrl } from "../utils/attachmentPreview";

const route = useRoute();
const { success, error: toastError } = useToast();

const complaint = ref<Complaint | null>(null);
const reopenRequests = ref<ReopenRequest[]>([]);
const loading = ref(true);
const showReopenForm = ref(false);
const reopenReason = ref("");
const reopenSubmitting = ref(false);
const attachmentPreviewUrls = ref<Record<string, string>>({});

const isClosed = computed(() => complaint.value?.status === 4 || complaint.value?.status === 5);
const hasPendingReopen = computed(() => reopenRequests.value.some((r) => r.status === 1));
const imageAttachments = computed(() =>
  complaint.value?.attachments.filter((attachment) => isImageMimeType(attachment.fileType)) ?? [],
);
const fileAttachments = computed(() =>
  complaint.value?.attachments.filter((attachment) => !isImageMimeType(attachment.fileType)) ?? [],
);

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

async function refreshComplaint(): Promise<void> {
  const id = route.params.id as string;
  if (!id) return;
  try {
    complaint.value = await getMyComplaintById(id);
    void preloadAttachmentPreviews(complaint.value);
  } catch { /* ignore */ }
}

async function downloadAttachment(attachmentId: string, fileName: string) {
  if (!complaint.value) return;
  try {
    const url = await getAttachmentLink(complaint.value.id, attachmentId);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.target = "_blank";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  } catch {
    toastError("Не удалось скачать файл");
  }
}

async function ensureAttachmentPreview(attachment: ComplaintAttachment): Promise<string | null> {
  if (!complaint.value || !isImageMimeType(attachment.fileType)) {
    return null;
  }

  const existing = attachmentPreviewUrls.value[attachment.id];
  if (existing) {
    return existing;
  }

  try {
    const url = await getAttachmentLink(complaint.value.id, attachment.id);
    const resolvedUrl = resolveAttachmentPreviewUrl(url);
    if (!resolvedUrl) {
      return null;
    }

    attachmentPreviewUrls.value = {
      ...attachmentPreviewUrls.value,
      [attachment.id]: resolvedUrl,
    };

    return resolvedUrl;
  } catch {
    return null;
  }
}

async function preloadAttachmentPreviews(targetComplaint: Complaint | null): Promise<void> {
  if (!targetComplaint) {
    return;
  }

  await Promise.all(
    targetComplaint.attachments
      .filter((attachment) => isImageMimeType(attachment.fileType))
      .map((attachment) => ensureAttachmentPreview(attachment)),
  );
}

async function openAttachmentPreview(attachment: ComplaintAttachment): Promise<void> {
  const previewUrl = await ensureAttachmentPreview(attachment);

  if (previewUrl) {
    window.open(previewUrl, "_blank");
    return;
  }

  await downloadAttachment(attachment.id, attachment.originalFileName);
}

async function handleReopenSubmit() {
  if (!complaint.value || !reopenReason.value.trim()) return;
  reopenSubmitting.value = true;
  try {
    const req = await createReopenRequest(complaint.value.id, reopenReason.value.trim());
    reopenRequests.value.unshift(req);
    showReopenForm.value = false;
    reopenReason.value = "";
    success("Запрос на переоткрытие отправлен");
  } catch (e) {
    toastError(
      (e as any)?.response?.data?.detail ||
      "Не удалось отправить запрос на переоткрытие",
    );
  } finally {
    reopenSubmitting.value = false;
  }
}

function formatDate(dateString: string): string {
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";
  return new Intl.DateTimeFormat("ru-RU", { dateStyle: "medium", timeStyle: "short" }).format(date);
}

function formatMoney(amount: number | null | undefined): string {
  if (amount == null) return "";
  return new Intl.NumberFormat("ru-RU", { style: "currency", currency: "KZT", maximumFractionDigits: 2 }).format(amount);
}

function getStatusClass(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300",
    2: "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300",
    3: "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300",
    4: "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-300",
    5: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-300",
  };
  return map[status] ?? "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
}

function getStatusDotClass(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-600 dark:bg-blue-400",
    2: "bg-violet-600 dark:bg-violet-400",
    3: "bg-amber-600 dark:bg-amber-400",
    4: "bg-green-600 dark:bg-green-400",
    5: "bg-red-600 dark:bg-red-400",
  };
  return map[status] ?? "bg-gray-600 dark:bg-gray-400";
}

onMounted(async () => {
  const id = route.params.id as string;
  if (!id) {
    loading.value = false;
    return;
  }
  try {
    complaint.value = await getMyComplaintById(id);
    void preloadAttachmentPreviews(complaint.value);
    if (complaint.value && (complaint.value.status === 4 || complaint.value.status === 5)) {
      reopenRequests.value = await getReopenRequests(id);
    }
  } catch {
    toastError("Не удалось загрузить обращение");
  } finally {
    loading.value = false;
  }
});
</script>
