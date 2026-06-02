<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <ComplaintHeader :complaint="complaint" :loading="loading" />

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-500 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Жалоба не найдена.
    </div>

    <template v-else-if="complaint">
      <!-- Two-column layout: info left, chat right -->
      <div class="flex flex-col lg:flex-row gap-6 items-start">
        <!-- LEFT COLUMN: complaint info -->
        <div class="w-full lg:w-1/2 space-y-6 min-w-0">
          <!-- Context cards -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <ComplaintBookingCard
              :complaint="complaint"
              :has-booking-view="hasBookingView"
              :is-assigned-manager="isAssignedManager"
              :access-request="accessRequest"
              :is-grant-expired="isGrantExpired"
              @request-access="showAccessRequestModal = true"
            />
            <ComplaintPartyCard
              label="Заявитель"
              :name="complaint.snapshotData.reporterFullName"
              :type-label="reporterLabels[complaint.reporterActorType] ?? '—'"
              :user-id="complaint.createdByUserId"
            />
          </div>

          <!-- Counterparty (full width in left col) -->
          <ComplaintPartyCard
            label="Контрагент"
            :name="complaint.snapshotData.counterpartyName"
            :type-label="targetLabels[complaint.targetType] ?? '—'"
            :user-id="complaint.snapshotData.counterpartyUserId"
          />

          <!-- Charges / Payments -->
          <ComplaintChargesTable
            v-if="hasPaymentView && bookingCharges.length > 0"
            :charges="bookingCharges"
          />

          <!-- Description -->
          <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
            <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Описание</h2>
            <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">{{ complaint.description }}</p>
          </div>

          <!-- Attachments (creation phase) -->
          <ComplaintAttachments
            v-if="creationAttachments.length > 0"
            :image-attachments="creationImageAttachments"
            :file-attachments="creationFileAttachments"
            :preview-urls="attachmentPreviewUrls"
            @open-preview="openComplaintAttachmentPreview"
            @download="downloadAttachment"
          />

          <!-- Actions panel -->
          <ComplaintActionsPanel
            v-if="complaint.status === 1 || complaint.status === 2 || complaint.status === 3"
            :status="complaint.status"
            :action-loading="actionLoading"
            @take="onTake"
            @open-resolve="showResolveModal = true"
            @open-reject="showRejectModal = true"
          />

          <!-- Manager Actions -->
          <ComplaintManagerActions
            v-if="complaint.assignedToManagerId && complaint.status !== 4 && complaint.status !== 5"
            :complaint="complaint"
            :can-cancel-booking="canCancelBooking"
            :booking-not-cancelable="bookingNotCancelable"
            :booking-not-cancelable-reason="bookingNotCancelableReason"
            @cancel-booking="showCancelBookingModal = true"
            @waive-charge="showWaiveChargeModal = true"
            @refund-charge="showRefundChargeModal = true"
            @escalate="showEscalateModal = true"
          />

          <!-- Resolution / Rejection -->
          <ComplaintOutcome :complaint="complaint" />

          <!-- Reopen Requests -->
          <ComplaintReopenRequests
            v-if="reopenRequests.length > 0"
            :requests="reopenRequests"
            :action-loading="actionLoading"
            @approve="onApproveReopen"
            @reject="startRejectReopen"
          />
        </div>

        <!-- RIGHT COLUMN: chat (sticky on desktop) -->
        <div class="w-full lg:w-1/2 lg:sticky lg:top-8 min-w-0">
          <ChatPanel
            :context-type="'complaint'"
            :context-id="complaint.id"
            height="calc(100vh - 120px)"
            :complaint-state="complaintState"
            :refresh-context="refreshComplaintForChat"
          />
        </div>
      </div>
    </template>

    <!-- Modals -->
    <ReasonPromptModal
      v-model:show="showRejectReopenModal"
      v-model="rejectReopenNote"
      title="Отклонить запрос на открытие"
      placeholder="Причина отклонения (необязательно)..."
      accent="red"
      confirm-label="Отклонить"
      loading-label="Отправка..."
      :loading="actionLoading"
      @confirm="onRejectReopen"
    />

    <ReasonPromptModal
      v-model:show="showResolveModal"
      v-model="resolveNote"
      title="Решить жалобу"
      placeholder="Комментарий к закрытию..."
      accent="emerald"
      required
      confirm-label="Решить"
      loading-label="Сохранение..."
      :loading="actionLoading"
      @confirm="onResolve"
    />

    <ReasonPromptModal
      v-model:show="showRejectModal"
      v-model="rejectReason"
      title="Отклонить жалобу"
      placeholder="Причина отклонения..."
      accent="red"
      required
      :rows="4"
      confirm-label="Отклонить"
      loading-label="Отправка..."
      :loading="actionLoading"
      @confirm="onReject"
    />

    <ReasonPromptModal
      v-model:show="showAccessRequestModal"
      v-model="accessRequestReason"
      title="Запросить доступ к бронированию"
      :description="`Укажите причину, по которой вам необходим доступ к данным бронирования #${complaint?.bookingId}. Запрос будет рассмотрен супер-менеджером.`"
      placeholder="Причина запроса доступа..."
      accent="amber"
      required
      :rows="4"
      confirm-label="Отправить запрос"
      loading-label="Отправка..."
      :loading="actionLoading"
      @confirm="onRequestAccess"
    />

    <ReasonPromptModal
      v-model:show="showCancelBookingModal"
      v-model="cancelBookingReason"
      title="Отменить бронирование"
      :description="`Бронирование #${complaint?.bookingId} будет отменено. Это действие необратимо.`"
      placeholder="Причина отмены бронирования..."
      accent="red"
      required
      confirm-label="Отменить бронирование"
      loading-label="Обработка..."
      :loading="actionLoading"
      @confirm="onCancelBooking"
    />

    <ReasonPromptModal
      v-model:show="showWaiveChargeModal"
      v-model="waiveChargeReason"
      title="Аннулировать начисление"
      :description="`Начисление #${complaint?.chargeId} будет аннулировано. Аннулировать можно только pending-начисления.`"
      placeholder="Причина аннулирования..."
      accent="amber"
      required
      confirm-label="Аннулировать"
      loading-label="Обработка..."
      :loading="actionLoading"
      @confirm="onWaiveCharge"
    />

    <ReasonPromptModal
      v-model:show="showEscalateModal"
      v-model="escalateReason"
      title="Эскалировать жалобу"
      :description="escalateDescription"
      placeholder="Причина эскалации..."
      accent="purple"
      required
      confirm-label="Эскалировать"
      loading-label="Обработка..."
      :loading="actionLoading"
      @confirm="onEscalate"
    />

    <ReasonPromptModal
      v-model:show="showRefundChargeModal"
      v-model="refundChargeReason"
      title="Возврат средств по начислению"
      description="Средства будут возвращены клиенту. Доля партнёра будет списана из кошелька."
      placeholder="Причина возврата..."
      accent="rose"
      required
      confirm-label="Вернуть средства"
      loading-label="Обработка..."
      :loading="actionLoading"
      @confirm="onRefundCharge"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useComplaintDetail } from "../composables/useComplaintDetail";
import { reporterLabels, targetLabels } from "../utils/complaintLabels";
import ChatPanel from "../components/ChatPanel.vue";
import ReasonPromptModal from "../components/ReasonPromptModal.vue";
import ComplaintHeader from "../components/complaints/ComplaintHeader.vue";
import ComplaintBookingCard from "../components/complaints/ComplaintBookingCard.vue";
import ComplaintPartyCard from "../components/complaints/ComplaintPartyCard.vue";
import ComplaintChargesTable from "../components/complaints/ComplaintChargesTable.vue";
import ComplaintAttachments from "../components/complaints/ComplaintAttachments.vue";
import ComplaintActionsPanel from "../components/complaints/ComplaintActionsPanel.vue";
import ComplaintManagerActions from "../components/complaints/ComplaintManagerActions.vue";
import ComplaintOutcome from "../components/complaints/ComplaintOutcome.vue";
import ComplaintReopenRequests from "../components/complaints/ComplaintReopenRequests.vue";

const escalateDescription =
  'Жалоба будет передана суперменеджеру. Приоритет будет повышен до "Срочный".';

const {
  // state
  loading,
  notFound,
  actionLoading,
  complaint,
  reopenRequests,
  accessRequest,
  bookingCharges,
  // modal state + forms
  showRejectReopenModal,
  rejectReopenNote,
  showResolveModal,
  showRejectModal,
  resolveNote,
  rejectReason,
  showCancelBookingModal,
  showWaiveChargeModal,
  showEscalateModal,
  showRefundChargeModal,
  cancelBookingReason,
  waiveChargeReason,
  escalateReason,
  refundChargeReason,
  showAccessRequestModal,
  accessRequestReason,
  // computed
  hasBookingView,
  hasPaymentView,
  complaintState,
  canCancelBooking,
  bookingNotCancelable,
  bookingNotCancelableReason,
  isAssignedManager,
  isGrantExpired,
  creationAttachments,
  creationImageAttachments,
  creationFileAttachments,
  // data loading
  loadComplaint,
  refreshComplaintForChat,
  // actions
  onTake,
  onResolve,
  onReject,
  onRequestAccess,
  onApproveReopen,
  startRejectReopen,
  onRejectReopen,
  onCancelBooking,
  onWaiveCharge,
  onEscalate,
  onRefundCharge,
  // attachments
  attachmentPreviewUrls,
  downloadAttachment,
  openComplaintAttachmentPreview,
} = useComplaintDetail();

onMounted(loadComplaint);
</script>
