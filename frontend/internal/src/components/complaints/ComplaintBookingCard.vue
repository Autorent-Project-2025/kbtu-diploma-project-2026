<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
    <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Бронирование</p>
    <div class="flex gap-3">
      <div
        v-if="complaint.snapshotData.coverImageUrl"
        class="shrink-0 w-20 h-14 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
      >
        <img :src="complaint.snapshotData.coverImageUrl" class="w-full h-full object-cover" />
      </div>
      <div class="min-w-0">
        <p class="text-sm font-bold text-gray-900 dark:text-white">
          {{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}
        </p>
        <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
          {{ formatDateTime(complaint.snapshotData.startTime) }} → {{ formatDateTime(complaint.snapshotData.endTime) }}
        </p>
        <p v-if="complaint.snapshotData.totalPrice != null" class="text-xs font-semibold text-gray-700 dark:text-gray-300 mt-0.5">
          {{ formatPrice(complaint.snapshotData.totalPrice) }}
        </p>
      </div>
    </div>
    <!-- Booking access link -->
    <template v-if="hasBookingView">
      <EntityLink :to="`/bookings/${complaint.bookingId}`">
        Бронирование #{{ complaint.bookingId }}
      </EntityLink>
    </template>
    <!-- Assigned manager gets auto-read access to booking review -->
    <template v-else-if="isAssignedManager">
      <router-link
        :to="`/complaints/${complaint.id}/booking-review`"
        class="inline-flex items-center gap-1.5 text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
      >
        Просмотр бронирования #{{ complaint.bookingId }}
        <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
        </svg>
      </router-link>
      <!-- Edit access request for higher-risk actions -->
      <div v-if="!accessRequest || accessRequest.status === 3 || accessRequest.status === 5" class="mt-1">
        <button
          @click="emit('request-access')"
          class="text-xs font-medium text-amber-600 dark:text-amber-400 hover:text-amber-700 dark:hover:text-amber-300 transition-colors"
        >
          Запросить доступ на редактирование
        </button>
      </div>
      <p v-else-if="accessRequest.status === 1" class="text-xs font-medium text-blue-600 dark:text-blue-400 mt-1">
        Запрос на доступ к редактированию отправлен
      </p>
      <p v-else-if="accessRequest.status === 2 && !isGrantExpired" class="text-xs font-medium text-emerald-600 dark:text-emerald-400 mt-1">
        Доступ на редактирование одобрен
      </p>
    </template>
    <template v-else>
      <div class="space-y-2">
        <p class="text-xs text-gray-500 dark:text-gray-400">
          Бронирование #{{ complaint.bookingId }}
        </p>

        <!-- No request yet -->
        <button
          v-if="!accessRequest"
          @click="emit('request-access')"
          class="text-sm font-semibold text-amber-600 dark:text-amber-400 hover:text-amber-700 dark:hover:text-amber-300 transition-colors"
        >
          Запросить доступ к бронированию
        </button>

        <!-- Pending -->
        <p
          v-else-if="accessRequest.status === 1"
          class="text-sm font-semibold text-blue-600 dark:text-blue-400"
        >
          Запрос на доступ отправлен
        </p>

        <!-- Approved -->
        <router-link
          v-else-if="accessRequest.status === 2 && !isGrantExpired"
          :to="`/complaints/${complaint.id}/booking-review`"
          class="inline-flex items-center gap-1.5 text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
        >
          Открыть review бронирования
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
          </svg>
        </router-link>

        <!-- Expired -->
        <p
          v-else-if="accessRequest.status === 2 && isGrantExpired"
          class="text-sm font-semibold text-gray-500 dark:text-gray-400"
        >
          Срок доступа истёк
        </p>

        <!-- Rejected -->
        <p
          v-else-if="accessRequest.status === 3"
          class="text-sm font-semibold text-red-600 dark:text-red-400"
        >
          Доступ отклонён
          <span v-if="accessRequest.decisionNote" class="font-normal text-xs block mt-0.5 text-gray-500 dark:text-gray-400">
            {{ accessRequest.decisionNote }}
          </span>
        </p>

        <!-- Revoked -->
        <p
          v-else-if="accessRequest.status === 5"
          class="text-sm font-semibold text-gray-500 dark:text-gray-400"
        >
          Доступ отозван
        </p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { Complaint } from "../../types/Complaint";
import type { AccessRequest } from "../../types/AccessRequest";
import { formatDateTime, formatPrice } from "../../utils/formatters";
import EntityLink from "../EntityLink.vue";

defineProps<{
  complaint: Complaint;
  hasBookingView: boolean;
  isAssignedManager: boolean;
  accessRequest: AccessRequest | null;
  isGrantExpired: boolean;
}>();

const emit = defineEmits<{
  "request-access": [];
}>();
</script>
