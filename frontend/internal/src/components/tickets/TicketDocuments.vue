<template>
  <section
    class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
  >
    <h3
      class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
    >
      Документы
    </h3>

    <ul
      v-if="hasSelectedDocuments"
      class="divide-y divide-gray-100 dark:divide-gray-800"
    >
      <li
        v-if="ticket.identityDocumentFileName"
        class="flex items-center justify-between gap-4 py-3"
      >
        <div>
          <p class="font-semibold text-sm text-gray-900 dark:text-white">
            {{ isPartnerTicket(ticket) ? "Документ владельца" : "Документ личности" }}
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">
            {{ ticket.identityDocumentFileName }}
          </p>
        </div>
        <button
          @click="emit('open-document', 'identity')"
          :disabled="actionLoading"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
        >
          Открыть
        </button>
      </li>
      <li
        v-if="isClientTicket(ticket) && ticket.driverLicenseFileName"
        class="flex items-center justify-between gap-4 py-3"
      >
        <div>
          <p class="font-semibold text-sm text-gray-900 dark:text-white">
            Водительские права
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">
            {{ ticket.driverLicenseFileName }}
          </p>
        </div>
        <button
          @click="emit('open-document', 'license')"
          :disabled="actionLoading"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
        >
          Открыть
        </button>
      </li>
      <li
        v-if="isPartnerCarTicket(ticket) && ticket.ownershipDocumentFileName"
        class="flex items-center justify-between gap-4 py-3"
      >
        <div>
          <p class="font-semibold text-sm text-gray-900 dark:text-white">
            Документ собственности
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">
            {{ ticket.ownershipDocumentFileName }}
          </p>
        </div>
        <button
          @click="emit('open-document', 'ownership')"
          :disabled="actionLoading"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
        >
          Открыть
        </button>
      </li>
      <li
        v-for="photo in completionPhotos"
        :key="photo.slot"
        class="flex items-center justify-between gap-4 py-3"
      >
        <div>
          <p class="font-semibold text-sm text-gray-900 dark:text-white">
            Фото {{ completionPhotoLabel(photo.slot) }}
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">
            {{ photo.fileName }}
          </p>
        </div>
        <button
          @click="emit('open-document', photo.slot)"
          :disabled="actionLoading"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
        >
          Открыть
        </button>
      </li>
    </ul>
    <p v-else class="text-sm text-gray-400 dark:text-gray-500">
      К заявке не прикреплены документы.
    </p>

    <!-- AI damage assessment — advisory only. -->
    <AiDamageAssessment
      v-if="isBookingCompletionTicket(ticket) && aiAssessment"
      :assessment="aiAssessment"
    />

    <div
      v-if="isPartnerCarTicket(ticket) && partnerCarImages.length > 0"
      class="pt-4 border-t border-gray-100 dark:border-gray-800 space-y-3"
    >
      <h4
        class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
      >
        Фотографии авто
      </h4>
      <div class="flex flex-wrap gap-2">
        <button
          v-for="(image, index) in partnerCarImages"
          :key="`${image.imageId}-${index}`"
          @click="emit('open-image', image.imageUrl)"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors"
        >
          {{ partnerCarImageTypeLabel(image.imageType, index) }}
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type {
  BookingCompletionTicketData,
  BookingCompletionTicketPhotoData,
  PartnerCarTicketImageData,
  Ticket,
} from "../../types/Ticket";
import {
  completionPhotoLabel,
  isBookingCompletionTicket,
  isClientTicket,
  isPartnerCarTicket,
  isPartnerTicket,
  partnerCarImageTypeLabel,
} from "../../utils/ticketLabels";
import AiDamageAssessment from "./AiDamageAssessment.vue";

type AiAssessment = NonNullable<BookingCompletionTicketData["aiAssessment"]>;

type DocumentType =
  | "identity"
  | "license"
  | "ownership"
  | "front"
  | "back"
  | "side_left"
  | "side_right"
  | "interior";

defineProps<{
  ticket: Ticket;
  completionPhotos: BookingCompletionTicketPhotoData[];
  partnerCarImages: PartnerCarTicketImageData[];
  aiAssessment: AiAssessment | null;
  hasSelectedDocuments: boolean;
  actionLoading: boolean;
}>();

const emit = defineEmits<{
  "open-document": [documentType: DocumentType];
  "open-image": [url: string];
}>();
</script>
