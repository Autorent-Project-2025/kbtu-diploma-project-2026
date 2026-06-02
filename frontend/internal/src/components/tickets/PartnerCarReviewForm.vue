<template>
  <section
    class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
  >
    <div>
      <h3
        class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
      >
        Данные автомобиля
      </h3>
      <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
        При необходимости скорректируйте характеристики перед принятием решения.
      </p>
    </div>

    <div
      class="rounded-2xl border border-violet-100 dark:border-violet-900/40 bg-violet-50/70 dark:bg-violet-500/10 p-4"
    >
      <p class="text-xs font-bold uppercase tracking-[0.14em] text-violet-700 dark:text-violet-300">
        {{ partnerCarRequestKindLabel(resolvePartnerCarRequestKind(ticket)) }}
      </p>
      <p class="mt-2 text-sm text-gray-700 dark:text-gray-200">
        <template v-if="ticket.partnerCarId">
          Изменения будут применены к машине #{{ ticket.partnerCarId }} после одобрения.
        </template>
        <template v-else>
          После одобрения будет создана новая машина партнера.
        </template>
      </p>
    </div>

    <div class="grid sm:grid-cols-2 gap-4">
      <div v-for="field in carFormFields" :key="field.id" class="space-y-1.5">
        <label
          :for="field.id"
          class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
          >{{ field.label }}</label
        >
        <input
          :id="field.id"
          v-model="form[field.key]"
          :type="field.type || 'text'"
          :min="field.min"
          :max="field.max"
          :step="field.step"
          class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
        />
      </div>
    </div>

    <div class="grid sm:grid-cols-2 gap-4">
      <div class="space-y-1.5">
        <label
          for="requestedStatus"
          class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
        >
          Статус машины
        </label>
        <select
          id="requestedStatus"
          v-model.number="form.requestedStatus"
          class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
        >
          <option
            v-for="option in partnerCarStatusOptions"
            :key="option.value"
            :value="option.value"
          >
            {{ option.label }}
          </option>
        </select>
      </div>

      <div class="rounded-2xl border border-gray-100 dark:border-gray-800 px-4 py-3 flex items-center justify-between gap-4">
        <div>
          <p class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
            Активность
          </p>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Если машина будет деактивирована, связанные бронирования отменятся.
          </p>
        </div>
        <label class="inline-flex items-center gap-3 text-sm font-semibold text-gray-900 dark:text-white">
          <input
            v-model="form.isActive"
            type="checkbox"
            class="h-4 w-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
          />
          <span>{{ form.isActive ? "Активна" : "Неактивна" }}</span>
        </label>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { Ticket } from "../../types/Ticket";
import type { PartnerCarFormState } from "../../composables/useManagerTickets";
import {
  partnerCarRequestKindLabel,
  resolvePartnerCarRequestKind,
} from "../../utils/ticketLabels";

defineProps<{
  ticket: Ticket;
  form: PartnerCarFormState;
}>();

type PartnerCarFormField = {
  id: string;
  key: "carBrand" | "carModel" | "carYear" | "licensePlate" | "color";
  label: string;
  type?: string;
  min?: string;
  max?: string;
  step?: string;
};

const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

const carFormFields: PartnerCarFormField[] = [
  { id: "carBrand", key: "carBrand", label: "Марка" },
  { id: "carModel", key: "carModel", label: "Модель" },
  {
    id: "carYear",
    key: "carYear",
    label: "Год",
    type: "number",
    min: "1886",
    max: String(maxAllowedCarYear),
  },
  { id: "licensePlate", key: "licensePlate", label: "Госномер" },
  { id: "color", key: "color", label: "Цвет" },
];

const partnerCarStatusOptions = [
  { value: 0, label: "Доступна" },
  { value: 1, label: "Забронирована" },
  { value: 2, label: "В поездке" },
  { value: 3, label: "На обслуживании" },
];
</script>
