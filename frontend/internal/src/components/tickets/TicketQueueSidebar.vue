<template>
  <div
    class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
  >
    <div class="px-5 py-4 border-b border-gray-100 dark:border-gray-800">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">
          Очередь
        </h2>
        <span
          class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 px-2.5 py-1 rounded-full"
          >{{ tickets.length }}</span
        >
      </div>
      <p
        v-if="lastUpdatedAt"
        class="text-xs text-gray-400 dark:text-gray-500 mt-1"
      >
        Обновлено {{ formatDateTime(lastUpdatedAt) }}
      </p>
    </div>

    <ul
      class="divide-y divide-gray-100 dark:divide-gray-800 max-h-[70vh] overflow-y-auto"
    >
      <li v-for="ticket in tickets" :key="ticket.id">
        <button
          @click="emit('select', ticket.id)"
          :class="[
            'w-full px-5 py-4 text-left transition-colors',
            selectedTicketId === ticket.id
              ? 'bg-emerald-50 dark:bg-emerald-900/20 border-l-4 border-emerald-500'
              : 'hover:bg-gray-50 dark:hover:bg-gray-800/60 border-l-4 border-transparent',
          ]"
        >
          <div class="flex items-start justify-between gap-3">
            <div class="flex items-center gap-3 min-w-0">
              <div
                class="w-9 h-9 flex-shrink-0 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 flex items-center justify-center text-xs font-bold"
              >
                {{ userInitials(ticket.fullName) }}
              </div>
              <div class="min-w-0">
                <p
                  class="font-bold text-gray-900 dark:text-white text-sm truncate"
                >
                  {{ ticket.fullName }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                  {{ ticket.email }}
                </p>
              </div>
            </div>
            <span
              :class="getTicketTypeBadgeClass(ticket.ticketType)"
              class="inline-flex px-2 py-0.5 rounded-full text-xs font-bold uppercase tracking-wide flex-shrink-0"
            >
              {{ ticketTypeLabel(ticket.ticketType) }}
            </span>
          </div>
          <div
            class="flex justify-between mt-2 pl-12 text-xs text-gray-400 dark:text-gray-500"
          >
            <span>{{ ticket.phoneNumber }}</span>
            <span>{{ formatDateTime(ticket.createdAt) }}</span>
          </div>
        </button>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import type { Ticket } from "../../types/Ticket";
import { formatDateTime, userInitials } from "../../utils/formatters";
import {
  getTicketTypeBadgeClass,
  ticketTypeLabel,
} from "../../utils/ticketLabels";

defineProps<{
  tickets: Ticket[];
  selectedTicketId: string;
  lastUpdatedAt: string;
}>();

const emit = defineEmits<{
  select: [ticketId: string];
}>();
</script>
