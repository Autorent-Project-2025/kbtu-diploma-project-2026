<template>
  <section
    class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
  >
    <h3
      class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
    >
      Основные данные
    </h3>
    <dl
      class="grid sm:grid-cols-2 gap-x-6 gap-y-0 divide-y divide-gray-100 dark:divide-gray-800"
    >
      <div class="py-3">
        <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
          ID заявки
        </dt>
        <dd
          class="font-mono text-xs font-semibold text-gray-900 dark:text-white break-all"
        >
          {{ ticket.id }}
        </dd>
      </div>
      <div class="py-3">
        <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">Тип</dt>
        <dd class="font-semibold text-gray-900 dark:text-white">
          {{ ticketTypeLabel(ticket.ticketType) }}
        </dd>
      </div>
      <div class="py-3">
        <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">Email</dt>
        <dd class="font-semibold text-gray-900 dark:text-white break-all">
          {{ ticket.email }}
        </dd>
      </div>
      <div class="py-3">
        <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">Телефон</dt>
        <dd class="font-semibold text-gray-900 dark:text-white">
          {{ ticket.phoneNumber }}
        </dd>
      </div>
      <div v-if="isClientTicket(ticket)" class="py-3">
        <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
          Дата рождения
        </dt>
        <dd class="font-semibold text-gray-900 dark:text-white">
          {{ ticket.birthDate || "Не указана" }}
        </dd>
      </div>
    </dl>
  </section>
</template>

<script setup lang="ts">
import type { Ticket } from "../../types/Ticket";
import { isClientTicket, ticketTypeLabel } from "../../utils/ticketLabels";

defineProps<{
  ticket: Ticket;
}>();
</script>
