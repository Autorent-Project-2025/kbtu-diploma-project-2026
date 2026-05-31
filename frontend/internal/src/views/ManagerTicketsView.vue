<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800 rounded-2xl">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
          <div class="space-y-1">
            <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
              Operations CRM
            </p>
            <h1 class="text-2xl font-extrabold text-gray-900 dark:text-white">
              Рабочая очередь
            </h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Проверяйте новые регистрации, открывайте документы и принимайте решение по каждой заявке.
            </p>
          </div>

          <!-- Stats strip -->
          <div class="flex flex-wrap gap-3 items-center">
            <div
              class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden"
            >
              <div
                v-for="(stat, i) in statsStrip"
                :key="stat.label"
                :class="[
                  'px-5 py-3 text-center',
                  i > 0 ? 'border-l border-gray-200 dark:border-gray-800' : '',
                ]"
              >
                <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
                  {{ stat.value }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">
                  {{ stat.label }}
                </p>
              </div>
            </div>
            <button
              @click="loadPending"
              :disabled="loading"
              class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-emerald-500 transition-colors disabled:opacity-60"
            >
              Обновить
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка заявок...
    </div>

    <!-- Empty -->
    <div
      v-else-if="tickets.length === 0"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Сейчас нет заявок на рассмотрении.
    </div>

    <!-- Main review layout -->
    <div v-else class="grid xl:grid-cols-[340px,1fr] gap-6 items-start">
      <TicketQueueSidebar
        :tickets="tickets"
        :selected-ticket-id="selectedTicketId"
        :last-updated-at="lastUpdatedAt"
        @select="selectTicket"
      />

      <!-- Detail panel -->
      <div
        v-if="selectedTicket"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
      >
        <!-- Header -->
        <div
          class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 pb-6 border-b border-gray-100 dark:border-gray-800"
        >
          <div class="flex items-start gap-4">
            <div
              class="w-12 h-12 flex-shrink-0 rounded-2xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 flex items-center justify-center text-base font-extrabold"
            >
              {{ userInitials(selectedTicket.fullName) }}
            </div>
            <div>
              <h2 class="text-2xl font-extrabold text-gray-900 dark:text-white">
                {{ selectedTicket.fullName }}
              </h2>
              <p class="text-gray-500 dark:text-gray-400 mt-1">
                {{ ticketTypeLabel(selectedTicket.ticketType) }} ·
                {{ selectedTicket.email }}
              </p>
            </div>
          </div>
          <div class="text-sm text-gray-500 dark:text-gray-400 space-y-1 text-right">
            <p>
              Статус:
              <span class="font-semibold text-gray-700 dark:text-gray-300">{{
                statusLabel(selectedTicket.status)
              }}</span>
            </p>
            <p>Создана: {{ formatDateTime(selectedTicket.createdAt) }}</p>
          </div>
        </div>

        <div class="grid xl:grid-cols-[1fr,300px] gap-6 items-start">
          <!-- Left: data + docs -->
          <div class="space-y-6">
            <TicketBasicFields :ticket="selectedTicket" />

            <PartnerCarReviewForm
              v-if="isPartnerCarTicket(selectedTicket)"
              :ticket="selectedTicket"
              :form="partnerCarForm"
            />

            <PartnerBookingCancellationCard
              v-if="isPartnerBookingCancellationTicket(selectedTicket)"
              :ticket="selectedTicket"
              :data="partnerBookingCancellationData"
            />

            <TicketDocuments
              :ticket="selectedTicket"
              :completion-photos="completionTicketPhotos"
              :partner-car-images="partnerCarImages"
              :ai-assessment="aiAssessment"
              :has-selected-documents="hasSelectedDocuments"
              :action-loading="actionLoading"
              @open-document="openDocument"
              @open-image="openImage"
            />
          </div>

          <!-- Right: summary + decision -->
          <div class="space-y-4">
            <TicketSummaryCard :rows="summaryRows" />

            <TicketDecisionPanel
              :ticket="selectedTicket"
              :action-loading="actionLoading"
              :can-approve="canApproveSelected"
              v-model:reject-reason="rejectReason"
              v-model:fine-amount="fineAmount"
              v-model:fine-comment="fineComment"
              @approve="approveSelected"
              @reject="rejectSelected"
              @issue-fine="issueFineSelected"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useManagerTickets } from "../composables/useManagerTickets";
import { formatDateTime, userInitials } from "../utils/formatters";
import {
  isPartnerBookingCancellationTicket,
  isPartnerCarTicket,
  statusLabel,
  ticketTypeLabel,
} from "../utils/ticketLabels";
import TicketQueueSidebar from "../components/tickets/TicketQueueSidebar.vue";
import TicketBasicFields from "../components/tickets/TicketBasicFields.vue";
import PartnerCarReviewForm from "../components/tickets/PartnerCarReviewForm.vue";
import PartnerBookingCancellationCard from "../components/tickets/PartnerBookingCancellationCard.vue";
import TicketDocuments from "../components/tickets/TicketDocuments.vue";
import TicketSummaryCard from "../components/tickets/TicketSummaryCard.vue";
import TicketDecisionPanel from "../components/tickets/TicketDecisionPanel.vue";

const {
  // state
  tickets,
  selectedTicket,
  selectedTicketId,
  rejectReason,
  fineAmount,
  fineComment,
  loading,
  actionLoading,
  lastUpdatedAt,
  partnerCarForm,
  // computed
  partnerCarImages,
  completionTicketPhotos,
  aiAssessment,
  partnerBookingCancellationData,
  statsStrip,
  hasSelectedDocuments,
  summaryRows,
  canApproveSelected,
  // actions
  loadPending,
  selectTicket,
  approveSelected,
  rejectSelected,
  issueFineSelected,
  openImage,
  openDocument,
} = useManagerTickets();

onMounted(loadPending);
</script>
