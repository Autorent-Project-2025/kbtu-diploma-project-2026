import { computed, reactive, ref } from "vue";
import {
  approveTicket,
  getPendingTickets,
  getTicketById,
  getTicketDocumentTemporaryLink,
  issueTicketFine,
  rejectTicket,
  type PartnerCarReviewPayload,
} from "../api/tickets";
import type {
  BookingCompletionTicketData,
  BookingCompletionTicketPhotoData,
  PartnerBookingCancellationTicketData,
  PartnerCarTicketData,
  PartnerCarTicketImageData,
  Ticket,
} from "../types/Ticket";
import {
  isBookingCompletionTicket,
  isPartnerBookingCancellationTicket,
  isPartnerCarTicket,
  partnerBookingStatusLabel,
  partnerCarRequestKindLabel,
  statusLabel,
  ticketTypeLabel,
} from "../utils/ticketLabels";
import { useToast } from "./useToast";

type DocumentType =
  | "identity"
  | "license"
  | "ownership"
  | "front"
  | "back"
  | "side_left"
  | "side_right"
  | "interior";

export interface PartnerCarFormState {
  carBrand: string;
  carModel: string;
  carYear: number | null;
  licensePlate: string;
  color: string;
  requestedStatus: number | null;
  isActive: boolean;
}

/**
 * Single source of truth for the manager review queue: pending tickets,
 * selection, the editable partner-car form, the decision form (reject / fine)
 * and every server action. Pure labels/guards live in `utils/ticketLabels`.
 */
export function useManagerTickets() {
  const toast = useToast();

  const tickets = ref<Ticket[]>([]);
  const selectedTicket = ref<Ticket | null>(null);
  const selectedTicketId = ref<string>("");
  const rejectReason = ref("");
  const fineAmount = ref("");
  const fineComment = ref("");
  const loading = ref(false);
  const actionLoading = ref(false);
  const lastUpdatedAt = ref<string>("");
  const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

  const partnerCarForm = reactive<PartnerCarFormState>({
    carBrand: "",
    carModel: "",
    carYear: null,
    licensePlate: "",
    color: "",
    requestedStatus: 0,
    isActive: true,
  });

  // ── Computed ────────────────────────────────────────────────────────────────
  const partnerCarImages = computed<PartnerCarTicketImageData[]>(() => {
    if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value))
      return [];
    if (
      Array.isArray(selectedTicket.value.carImages) &&
      selectedTicket.value.carImages.length > 0
    )
      return selectedTicket.value.carImages;
    const data = selectedTicket.value.data;
    if (data && (data as PartnerCarTicketData).$type === "partner-car")
      return (data as PartnerCarTicketData).carImages ?? [];
    return [];
  });

  const completionTicketPhotos = computed<BookingCompletionTicketPhotoData[]>(
    () => {
      if (
        !selectedTicket.value ||
        !isBookingCompletionTicket(selectedTicket.value)
      ) {
        return [];
      }

      if (
        Array.isArray(selectedTicket.value.completionPhotos) &&
        selectedTicket.value.completionPhotos.length > 0
      ) {
        return selectedTicket.value.completionPhotos;
      }

      const data = selectedTicket.value.data;
      if (
        data &&
        (data as BookingCompletionTicketData).$type === "booking-completion"
      ) {
        return (data as BookingCompletionTicketData).completionPhotos ?? [];
      }

      return [];
    },
  );

  const aiAssessment = computed(() => {
    if (
      !selectedTicket.value ||
      !isBookingCompletionTicket(selectedTicket.value)
    ) {
      return null;
    }

    const data = selectedTicket.value.data as
      | BookingCompletionTicketData
      | undefined;
    return data?.aiAssessment ?? null;
  });

  const partnerBookingCancellationData =
    computed<PartnerBookingCancellationTicketData | null>(() => {
      if (
        !selectedTicket.value ||
        !isPartnerBookingCancellationTicket(selectedTicket.value)
      ) {
        return null;
      }

      const data = selectedTicket.value.data;
      if (
        data &&
        (data as PartnerBookingCancellationTicketData).$type ===
          "partner-booking-cancellation"
      ) {
        return data as PartnerBookingCancellationTicketData;
      }

      return null;
    });

  const ticketStats = computed(() => {
    let client = 0,
      partner = 0,
      partnerCar = 0,
      bookingCompletion = 0;
    for (const t of tickets.value) {
      if (t.ticketType === 2) partner++;
      else if (t.ticketType === 3) partnerCar++;
      else if (t.ticketType === 4) bookingCompletion++;
      else client++;
    }
    return { client, partner, partnerCar, bookingCompletion };
  });

  const statsStrip = computed(() => [
    { label: "В очереди", value: tickets.value.length },
    { label: "Клиенты", value: ticketStats.value.client },
    { label: "Партнёры", value: ticketStats.value.partner },
    { label: "Авто", value: ticketStats.value.partnerCar },
    { label: "Поездки", value: ticketStats.value.bookingCompletion },
  ]);

  const hasSelectedDocuments = computed(() => {
    if (!selectedTicket.value) return false;
    return Boolean(
      selectedTicket.value.identityDocumentFileName ||
        (isClientTicketLocal(selectedTicket.value) &&
          selectedTicket.value.driverLicenseFileName) ||
        (isPartnerCarTicket(selectedTicket.value) &&
          selectedTicket.value.ownershipDocumentFileName) ||
        completionTicketPhotos.value.length > 0,
    );
  });

  const selectedDocumentCount = computed(() => {
    if (!selectedTicket.value) return 0;
    let count = 0;
    if (selectedTicket.value.identityDocumentFileName) count++;
    if (
      isClientTicketLocal(selectedTicket.value) &&
      selectedTicket.value.driverLicenseFileName
    )
      count++;
    if (
      isPartnerCarTicket(selectedTicket.value) &&
      selectedTicket.value.ownershipDocumentFileName
    )
      count++;
    count += completionTicketPhotos.value.length;
    return count;
  });

  const summaryRows = computed(() => {
    if (!selectedTicket.value) return [];
    const rows = [
      { label: "Статус", value: statusLabel(selectedTicket.value.status) },
      {
        label: "Тип",
        value: ticketTypeLabel(selectedTicket.value.ticketType),
      },
      { label: "Документы", value: String(selectedDocumentCount.value) },
    ];
    if (isPartnerCarTicket(selectedTicket.value))
      rows.push({
        label: "Фотографии",
        value: String(partnerCarImages.value.length),
      });
    if (isPartnerCarTicket(selectedTicket.value)) {
      rows.push({
        label: "Режим",
        value: partnerCarRequestKindLabel(
          selectedTicket.value.partnerCarRequestKind ??
            (selectedTicket.value.data as PartnerCarTicketData | undefined)
              ?.requestKind,
        ),
      });
      if (selectedTicket.value.partnerCarId) {
        rows.push({
          label: "Машина",
          value: `#${selectedTicket.value.partnerCarId}`,
        });
      }
    }
    if (isBookingCompletionTicket(selectedTicket.value)) {
      rows.push({
        label: "Фото после поездки",
        value: String(completionTicketPhotos.value.length),
      });
      rows.push({
        label: "Пеня за просрочку",
        value: selectedTicket.value.latePenaltyAmount
          ? `${selectedTicket.value.latePenaltyAmount.toFixed(2)} KZT`
          : "Нет",
      });
      rows.push({
        label: "Штраф за повреждение",
        value: selectedTicket.value.damageFineAmount
          ? `${selectedTicket.value.damageFineAmount.toFixed(2)} KZT`
          : "Не назначен",
      });
    }
    if (isPartnerBookingCancellationTicket(selectedTicket.value)) {
      rows.push({
        label: "Бронирование",
        value: `#${selectedTicket.value.bookingId ?? "?"}`,
      });
      rows.push({
        label: "Статус брони",
        value: partnerBookingStatusLabel(
          partnerBookingCancellationData.value?.bookingStatus,
        ),
      });
    }
    return rows;
  });

  const canApproveSelected = computed(() => {
    if (!isBookingCompletionTicket(selectedTicket.value)) {
      return true;
    }
    return !fineAmount.value.trim() && !fineComment.value.trim();
  });

  // ── Helpers ───────────────────────────────────────────────────────────────
  function isClientTicketLocal(ticket: Ticket): boolean {
    return ticket.ticketType === 1;
  }

  function syncPartnerCarForm(ticket: Ticket | null) {
    if (!ticket || !isPartnerCarTicket(ticket)) {
      Object.assign(partnerCarForm, {
        carBrand: "",
        carModel: "",
        carYear: null,
        licensePlate: "",
        color: "",
        requestedStatus: 0,
        isActive: true,
      });
      return;
    }
    const data = ticket.data as PartnerCarTicketData | undefined;
    partnerCarForm.carBrand = (ticket.carBrand ?? data?.carBrand ?? "").trim();
    partnerCarForm.carModel = (ticket.carModel ?? data?.carModel ?? "").trim();
    const rawYear = ticket.carYear ?? data?.carYear ?? null;
    partnerCarForm.carYear = Number.isInteger(rawYear) ? Number(rawYear) : null;
    partnerCarForm.licensePlate = (
      ticket.licensePlate ??
      data?.licensePlate ??
      ""
    ).trim();
    partnerCarForm.color = (ticket.color ?? data?.color ?? "").trim();
    partnerCarForm.requestedStatus =
      ticket.requestedPartnerCarStatus ?? data?.requestedStatus ?? 0;
    partnerCarForm.isActive = ticket.isActive ?? data?.isActive ?? true;
  }

  function buildPartnerCarPayload():
    | PartnerCarReviewPayload
    | null
    | undefined {
    if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value))
      return undefined;
    const carBrand = partnerCarForm.carBrand.trim();
    const carModel = partnerCarForm.carModel.trim();
    const carYear = Number(partnerCarForm.carYear);
    const licensePlate = partnerCarForm.licensePlate.trim();

    if (!carBrand || !carModel || !licensePlate || !Number.isInteger(carYear)) {
      toast.error("Заполните марку, модель, год и госномер.");
      return null;
    }
    if (carYear < 1886 || carYear > maxAllowedCarYear) {
      toast.error(
        `Год машины должен быть в диапазоне 1886-${maxAllowedCarYear}.`,
      );
      return null;
    }
    return {
      carBrand,
      carModel,
      carYear,
      licensePlate,
      color: partnerCarForm.color.trim() || null,
      requestedStatus: partnerCarForm.requestedStatus,
      isActive: Boolean(partnerCarForm.isActive),
    };
  }

  function resetDecisionForm() {
    rejectReason.value = "";
    fineAmount.value = "";
    fineComment.value = "";
  }

  // ── Data loading ──────────────────────────────────────────────────────────
  async function loadPending() {
    loading.value = true;
    try {
      const data = await getPendingTickets();
      tickets.value = data;
      lastUpdatedAt.value = new Date().toISOString();
      if (data.length === 0) {
        selectedTicket.value = null;
        selectedTicketId.value = "";
        resetDecisionForm();
        syncPartnerCarForm(null);
        return;
      }
      const fallback = data[0];
      if (!fallback) {
        selectedTicket.value = null;
        selectedTicketId.value = "";
        resetDecisionForm();
        syncPartnerCarForm(null);
        return;
      }
      const nextId = data.some((t) => t.id === selectedTicketId.value)
        ? selectedTicketId.value
        : fallback.id;
      await selectTicket(nextId);
    } catch (e: any) {
      toast.error(
        e?.response?.data?.error || "Не удалось получить список заявок.",
      );
    } finally {
      loading.value = false;
    }
  }

  async function selectTicket(ticketId: string) {
    selectedTicketId.value = ticketId;
    resetDecisionForm();
    try {
      selectedTicket.value = await getTicketById(ticketId);
      syncPartnerCarForm(selectedTicket.value);
    } catch (e: any) {
      toast.error(e?.response?.data?.error || "Не удалось загрузить заявку.");
    }
  }

  // ── Actions ───────────────────────────────────────────────────────────────
  async function approveSelected() {
    if (!selectedTicket.value || actionLoading.value) return;
    if (!canApproveSelected.value) {
      toast.error(
        "Очистите блок штрафа, если хотите одобрить завершение поездки без начислений.",
      );
      return;
    }
    actionLoading.value = true;
    try {
      const payload = buildPartnerCarPayload();
      if (payload === null) return;
      await approveTicket(selectedTicket.value.id, payload);
      toast.success("✓ Заявка одобрена");
      await loadPending();
    } catch (e: any) {
      toast.error(e?.response?.data?.error || "Не удалось одобрить заявку.");
    } finally {
      actionLoading.value = false;
    }
  }

  async function rejectSelected() {
    if (!selectedTicket.value || actionLoading.value) return;
    if (!rejectReason.value.trim()) {
      toast.error("Укажите причину отказа.");
      return;
    }
    actionLoading.value = true;
    try {
      const payload = buildPartnerCarPayload();
      if (payload === null) return;
      await rejectTicket(
        selectedTicket.value.id,
        rejectReason.value.trim(),
        payload,
      );
      toast.success("✕ Заявка отклонена", 4000);
      await loadPending();
    } catch (e: any) {
      toast.error(e?.response?.data?.error || "Не удалось отклонить заявку.");
    } finally {
      actionLoading.value = false;
    }
  }

  async function issueFineSelected() {
    if (!selectedTicket.value || actionLoading.value) return;
    const amount = Number(fineAmount.value);
    if (!Number.isFinite(amount) || amount <= 0) {
      toast.error("Укажите корректную сумму штрафа.");
      return;
    }
    if (!fineComment.value.trim()) {
      toast.error("Добавьте комментарий к штрафу.");
      return;
    }

    actionLoading.value = true;
    try {
      await issueTicketFine(
        selectedTicket.value.id,
        amount,
        fineComment.value.trim(),
      );
      toast.success("Штраф выставлен");
      fineAmount.value = "";
      fineComment.value = "";
      await loadPending();
    } catch (e: any) {
      toast.error(e?.response?.data?.error || "Не удалось выставить штраф.");
    } finally {
      actionLoading.value = false;
    }
  }

  function openImage(url: string) {
    if (!url) return;
    window.open(url, "_blank", "noopener,noreferrer");
  }

  async function openDocument(documentType: DocumentType) {
    if (!selectedTicket.value || actionLoading.value) return;
    actionLoading.value = true;
    try {
      const link = await getTicketDocumentTemporaryLink(
        selectedTicket.value.id,
        documentType,
      );
      window.open(link.url, "_blank", "noopener,noreferrer");
    } catch (e: any) {
      toast.error(
        e?.response?.data?.error || "Не удалось получить ссылку на документ.",
      );
    } finally {
      actionLoading.value = false;
    }
  }

  return {
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
    selectedDocumentCount,
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
  };
}
