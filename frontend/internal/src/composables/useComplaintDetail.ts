import { computed, ref } from "vue";
import { useRoute } from "vue-router";
import {
  cancelComplaintBooking,
  escalateComplaint,
  getComplaintById,
  getReopenRequests,
  approveReopenRequest,
  refundComplaintCharge,
  rejectComplaint,
  rejectReopenRequest,
  resolveComplaint,
  takeComplaint,
  waiveComplaintCharge,
} from "../api/complaints";
import { createAccessRequest, getMyAccessRequest } from "../api/accessRequests";
import { getBookingCharges, type BookingCharge } from "../api/payments";
import type { Complaint, ReopenRequest } from "../types/Complaint";
import type { AccessRequest } from "../types/AccessRequest";
import { can } from "../accessControl";
import { auth } from "../store/auth";
import { isImageMimeType } from "../utils/attachmentPreview";
import { useToast } from "./useToast";
import { useComplaintAttachments } from "./useComplaintAttachments";

/**
 * Single source of truth for the complaint detail screen: data loading,
 * permission-derived computed flags, modal state + form models, and every
 * server action. Attachment preview/download is delegated to
 * `useComplaintAttachments`, re-exposed from here so the view needs one call.
 */
export function useComplaintDetail() {
  const route = useRoute();
  const toast = useToast();

  const loading = ref(false);
  const notFound = ref(false);
  const actionLoading = ref(false);
  const complaint = ref<Complaint | null>(null);

  const reopenRequests = ref<ReopenRequest[]>([]);
  const accessRequest = ref<AccessRequest | null>(null);
  const bookingCharges = ref<BookingCharge[]>([]);

  const attachments = useComplaintAttachments(complaint);

  // ── Modal state + form models ──────────────────────────────────────────────
  const showRejectReopenModal = ref(false);
  const rejectReopenNote = ref("");
  const rejectReopenTargetId = ref<string | null>(null);

  const showResolveModal = ref(false);
  const showRejectModal = ref(false);
  const resolveNote = ref("");
  const rejectReason = ref("");

  const showCancelBookingModal = ref(false);
  const showWaiveChargeModal = ref(false);
  const showEscalateModal = ref(false);
  const showRefundChargeModal = ref(false);
  const cancelBookingReason = ref("");
  const waiveChargeReason = ref("");
  const escalateReason = ref("");
  const refundChargeReason = ref("");

  const showAccessRequestModal = ref(false);
  const accessRequestReason = ref("");

  // ── Permission-derived flags ───────────────────────────────────────────────
  const hasBookingView = computed(() => can("Booking.View"));
  const hasPaymentView = computed(() => can("Payment.View"));

  // ── Computed ────────────────────────────────────────────────────────────────
  const complaintState = computed<"not-taken" | "taken" | "closed">(() => {
    if (!complaint.value) return "not-taken";
    if (complaint.value.status === 4 || complaint.value.status === 5)
      return "closed";
    if (complaint.value.status === 1) return "not-taken";
    return "taken";
  });

  // Pending/Confirmed: always allowed. Active/AwaitingReview: allowed (server
  // checks edit access).
  const canCancelBooking = computed(() => {
    if (!complaint.value) return false;
    const status = complaint.value.snapshotData.status?.toLowerCase();
    return (
      status === "pending" ||
      status === "confirmed" ||
      status === "active" ||
      status === "awaitingreview"
    );
  });
  const bookingNotCancelable = computed(() => {
    if (!complaint.value) return false;
    return !canCancelBooking.value;
  });
  const bookingNotCancelableReason = computed(() => {
    if (!complaint.value) return "";
    const status = complaint.value.snapshotData.status?.toLowerCase();
    if (status === "completed") return "завершено";
    if (status === "canceled") return "отменено";
    return "";
  });

  const isAssignedManager = computed(() => {
    if (!complaint.value) return false;
    const userId = auth.getUserId();
    return !!userId && complaint.value.assignedToManagerId === userId;
  });
  const isGrantExpired = computed(() => {
    if (!accessRequest.value?.expiresAt) return true;
    return new Date(accessRequest.value.expiresAt) <= new Date();
  });

  const creationAttachments = computed(
    () =>
      complaint.value?.attachments.filter((a) => a.attachmentPhase === 1) ?? [],
  );
  const creationImageAttachments = computed(() =>
    creationAttachments.value.filter((attachment) =>
      isImageMimeType(attachment.fileType),
    ),
  );
  const creationFileAttachments = computed(() =>
    creationAttachments.value.filter(
      (attachment) => !isImageMimeType(attachment.fileType),
    ),
  );

  // ── Data loading ──────────────────────────────────────────────────────────
  async function loadComplaint() {
    const id = route.params.id as string;
    if (!id) {
      notFound.value = true;
      return;
    }

    loading.value = true;
    try {
      complaint.value = await getComplaintById(id);
      void attachments.preloadPreviews(complaint.value);

      // Load reopen requests and access request in parallel
      const promises: Promise<void>[] = [];

      promises.push(
        getReopenRequests(id)
          .then((r) => {
            reopenRequests.value = r;
          })
          .catch(() => {}),
      );

      if (!hasBookingView.value) {
        promises.push(
          getMyAccessRequest(id)
            .then((r) => {
              accessRequest.value = r;
            })
            .catch(() => {}),
        );
      }

      if (hasPaymentView.value && complaint.value) {
        promises.push(
          getBookingCharges(complaint.value.bookingId)
            .then((c) => {
              bookingCharges.value = c;
            })
            .catch(() => {}),
        );
      }

      await Promise.all(promises);
    } catch {
      notFound.value = true;
    } finally {
      loading.value = false;
    }
  }

  // Re-fetch complaint to trigger backend EnsureConversationExists
  async function refreshComplaintForChat(): Promise<void> {
    const id = route.params.id as string;
    if (!id) return;
    try {
      complaint.value = await getComplaintById(id);
      void attachments.preloadPreviews(complaint.value);
    } catch {
      /* ignore */
    }
  }

  // ── Actions ───────────────────────────────────────────────────────────────
  async function onTake() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await takeComplaint(complaint.value.id);
      toast.success("Жалоба взята в работу");
    } catch {
      toast.error("Ошибка при взятии жалобы в работу");
    } finally {
      actionLoading.value = false;
    }
  }

  async function onResolve() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await resolveComplaint(
        complaint.value.id,
        resolveNote.value.trim(),
      );
      showResolveModal.value = false;
      resolveNote.value = "";
      toast.success("Жалоба решена");
    } catch {
      toast.error("Ошибка при решении жалобы");
    } finally {
      actionLoading.value = false;
    }
  }

  async function onReject() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await rejectComplaint(
        complaint.value.id,
        rejectReason.value.trim(),
      );
      showRejectModal.value = false;
      rejectReason.value = "";
      toast.success("Жалоба отклонена");
    } catch {
      toast.error("Ошибка при отклонении жалобы");
    } finally {
      actionLoading.value = false;
    }
  }

  async function onRequestAccess() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      accessRequest.value = await createAccessRequest(
        complaint.value.id,
        accessRequestReason.value.trim(),
      );
      showAccessRequestModal.value = false;
      accessRequestReason.value = "";
      toast.success("Запрос на доступ отправлен");
    } catch {
      toast.error("Ошибка при отправке запроса на доступ");
    } finally {
      actionLoading.value = false;
    }
  }

  async function onApproveReopen(requestId: string) {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      await approveReopenRequest(requestId);
      // Reload complaint (status changes to InReview) and reopen requests
      complaint.value = await getComplaintById(complaint.value.id);
      reopenRequests.value = await getReopenRequests(complaint.value.id);
      toast.success("Запрос одобрен, жалоба открыта повторно");
    } catch {
      toast.error("Ошибка при одобрении запроса");
    } finally {
      actionLoading.value = false;
    }
  }

  function startRejectReopen(requestId: string) {
    rejectReopenTargetId.value = requestId;
    rejectReopenNote.value = "";
    showRejectReopenModal.value = true;
  }

  async function onRejectReopen() {
    if (actionLoading.value || !rejectReopenTargetId.value || !complaint.value)
      return;
    actionLoading.value = true;
    try {
      await rejectReopenRequest(
        rejectReopenTargetId.value,
        rejectReopenNote.value.trim() || undefined,
      );
      reopenRequests.value = await getReopenRequests(complaint.value.id);
      showRejectReopenModal.value = false;
      rejectReopenTargetId.value = null;
      toast.success("Запрос отклонён");
    } catch {
      toast.error("Ошибка при отклонении запроса");
    } finally {
      actionLoading.value = false;
    }
  }

  async function onCancelBooking() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await cancelComplaintBooking(
        complaint.value.id,
        cancelBookingReason.value.trim(),
      );
      showCancelBookingModal.value = false;
      cancelBookingReason.value = "";
      toast.success("Бронирование отменено");
    } catch (e: any) {
      const msg =
        e?.response?.data?.error ||
        e?.response?.data?.message ||
        "Ошибка при отмене бронирования";
      toast.error(msg);
    } finally {
      actionLoading.value = false;
    }
  }

  async function onWaiveCharge() {
    if (actionLoading.value || !complaint.value || !complaint.value.chargeId)
      return;
    actionLoading.value = true;
    try {
      complaint.value = await waiveComplaintCharge(
        complaint.value.id,
        complaint.value.chargeId,
        waiveChargeReason.value.trim(),
      );
      showWaiveChargeModal.value = false;
      waiveChargeReason.value = "";
      toast.success("Начисление аннулировано");
    } catch (e: any) {
      const msg =
        e?.response?.data?.error ||
        e?.response?.data?.message ||
        "Ошибка при аннулировании начисления";
      toast.error(msg);
    } finally {
      actionLoading.value = false;
    }
  }

  async function onEscalate() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await escalateComplaint(
        complaint.value.id,
        escalateReason.value.trim(),
      );
      showEscalateModal.value = false;
      escalateReason.value = "";
      toast.success("Жалоба эскалирована суперменеджеру");
    } catch (e: any) {
      const msg =
        e?.response?.data?.error ||
        e?.response?.data?.message ||
        "Ошибка при эскалации жалобы";
      toast.error(msg);
    } finally {
      actionLoading.value = false;
    }
  }

  async function onRefundCharge() {
    if (actionLoading.value || !complaint.value) return;
    actionLoading.value = true;
    try {
      complaint.value = await refundComplaintCharge(
        complaint.value.id,
        complaint.value.chargeId!,
        refundChargeReason.value.trim(),
      );
      showRefundChargeModal.value = false;
      refundChargeReason.value = "";
      toast.success("Средства возвращены по начислению");
    } catch (e: any) {
      const msg =
        e?.response?.data?.error ||
        e?.response?.data?.message ||
        "Ошибка при возврате средств";
      toast.error(msg);
    } finally {
      actionLoading.value = false;
    }
  }

  return {
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
    // attachments (delegated)
    attachmentPreviewUrls: attachments.previewUrls,
    downloadAttachment: attachments.downloadAttachment,
    openComplaintAttachmentPreview: attachments.openPreview,
  };
}
