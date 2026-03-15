<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Hero header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div
        class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-6"
      >
        <div class="space-y-3">
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
          >
            Internal Panel
          </p>
          <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">
            Рабочая очередь
          </h1>
          <p class="text-gray-600 dark:text-gray-400">
            Проверяйте новые регистрации, открывайте документы и принимайте
            решение по каждой заявке.
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
              <p
                class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5"
              >
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
    </header>

    <!-- Error / success banners -->
    <div
      v-if="errorMessage"
      class="rounded-2xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 px-5 py-4 text-red-700 dark:text-red-300 font-medium"
    >
      {{ errorMessage }}
    </div>
    <div
      v-if="successMessage"
      class="rounded-2xl border border-emerald-300/70 dark:border-emerald-500/30 bg-emerald-50 dark:bg-emerald-900/20 px-5 py-4 text-emerald-700 dark:text-emerald-300 font-medium"
    >
      {{ successMessage }}
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
      <!-- Queue sidebar -->
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
            Обновлено {{ formatDate(lastUpdatedAt) }}
          </p>
        </div>

        <ul
          class="divide-y divide-gray-100 dark:divide-gray-800 max-h-[70vh] overflow-y-auto"
        >
          <li v-for="ticket in tickets" :key="ticket.id">
            <button
              @click="selectTicket(ticket.id)"
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
                    {{ getInitials(ticket.fullName) }}
                  </div>
                  <div class="min-w-0">
                    <p
                      class="font-bold text-gray-900 dark:text-white text-sm truncate"
                    >
                      {{ ticket.fullName }}
                    </p>
                    <p
                      class="text-xs text-gray-500 dark:text-gray-400 truncate"
                    >
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
                <span>{{ formatDate(ticket.createdAt) }}</span>
              </div>
            </button>
          </li>
        </ul>
      </div>

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
              {{ getInitials(selectedTicket.fullName) }}
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
          <div
            class="text-sm text-gray-500 dark:text-gray-400 space-y-1 text-right"
          >
            <p>
              Статус:
              <span class="font-semibold text-gray-700 dark:text-gray-300">{{
                statusLabel(selectedTicket.status)
              }}</span>
            </p>
            <p>Создана: {{ formatDate(selectedTicket.createdAt) }}</p>
          </div>
        </div>

        <div class="grid xl:grid-cols-[1fr,300px] gap-6 items-start">
          <!-- Left: data + docs -->
          <div class="space-y-6">
            <!-- Basic fields -->
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
                    {{ selectedTicket.id }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Тип
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ ticketTypeLabel(selectedTicket.ticketType) }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Email
                  </dt>
                  <dd
                    class="font-semibold text-gray-900 dark:text-white break-all"
                  >
                    {{ selectedTicket.email }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Телефон
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ selectedTicket.phoneNumber }}
                  </dd>
                </div>
                <div v-if="isClientTicket(selectedTicket)" class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Дата рождения
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ selectedTicket.birthDate || "Не указана" }}
                  </dd>
                </div>
              </dl>
            </section>

            <!-- Partner car form -->
            <section
              v-if="isPartnerCarTicket(selectedTicket)"
              class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
            >
              <div>
                <h3
                  class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Данные автомобиля
                </h3>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  При необходимости скорректируйте перед решением.
                </p>
              </div>
              <div class="grid sm:grid-cols-2 gap-4">
                <div
                  v-for="field in carFormFields"
                  :key="field.id"
                  class="space-y-1.5"
                >
                  <label
                    :for="field.id"
                    class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                    >{{ field.label }}</label
                  >
                  <input
                    :id="field.id"
                    v-model="
                      partnerCarForm[field.key as keyof typeof partnerCarForm]
                    "
                    :type="field.type || 'text'"
                    :min="field.min"
                    :max="field.max"
                    :step="field.step"
                    class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
                  />
                </div>
              </div>
            </section>

            <!-- Documents -->
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
                  v-if="selectedTicket.identityDocumentFileName"
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      {{
                        isPartnerTicket(selectedTicket)
                          ? "Документ владельца"
                          : "Документ личности"
                      }}
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.identityDocumentFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('identity')"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
                <li
                  v-if="
                    isClientTicket(selectedTicket) &&
                    selectedTicket.driverLicenseFileName
                  "
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      Водительские права
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.driverLicenseFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('license')"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
                <li
                  v-if="
                    isPartnerCarTicket(selectedTicket) &&
                    selectedTicket.ownershipDocumentFileName
                  "
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      Документ собственности
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.ownershipDocumentFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('ownership')"
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

              <div
                v-if="
                  isPartnerCarTicket(selectedTicket) &&
                  partnerCarImages.length > 0
                "
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
                    @click="openImage(image.imageUrl)"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors"
                  >
                    Фото {{ index + 1 }}
                  </button>
                </div>
              </div>
            </section>
          </div>

          <!-- Right: summary + decision -->
          <div class="space-y-4">
            <!-- Summary card -->
            <div
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-0 divide-y divide-gray-200 dark:divide-gray-800"
            >
              <h3
                class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 pb-3"
              >
                Сводка
              </h3>
              <div
                v-for="row in summaryRows"
                :key="row.label"
                class="flex justify-between items-center py-3"
              >
                <dt class="text-sm text-gray-500 dark:text-gray-400">
                  {{ row.label }}
                </dt>
                <dd class="text-sm font-bold text-gray-900 dark:text-white">
                  {{ row.value }}
                </dd>
              </div>
            </div>

            <!-- Decision card -->
            <div
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-4"
            >
              <div>
                <h3
                  class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Решение
                </h3>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  Причину нужно указать только для отказа.
                </p>
              </div>

              <div class="space-y-1.5">
                <label
                  for="rejectReason"
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                  >Причина отказа</label
                >
                <textarea
                  id="rejectReason"
                  v-model="rejectReason"
                  placeholder="Укажите причину, если заявка отклоняется"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm min-h-[100px] resize-y focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors placeholder-gray-400"
                />
              </div>

              <div class="flex flex-col gap-3">
                <button
                  @click="approveSelected"
                  :disabled="actionLoading"
                  class="w-full px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
                >
                  {{ actionLoading ? "Обработка..." : "✓ Одобрить" }}
                </button>
                <button
                  @click="rejectSelected"
                  :disabled="actionLoading"
                  class="w-full px-5 py-3 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 disabled:cursor-not-allowed font-bold transition-colors"
                >
                  {{ actionLoading ? "Обработка..." : "✕ Отклонить" }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import {
  approveTicket,
  getPendingTickets,
  getTicketById,
  getTicketDocumentTemporaryLink,
  rejectTicket,
  type PartnerCarReviewPayload,
} from "../api/tickets";
import type {
  PartnerCarTicketData,
  PartnerCarTicketImageData,
  Ticket,
} from "../types/Ticket";

const tickets = ref<Ticket[]>([]);
const selectedTicket = ref<Ticket | null>(null);
const selectedTicketId = ref<string>("");
const rejectReason = ref("");
const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");
const lastUpdatedAt = ref<string>("");
const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

const partnerCarForm = reactive({
  carBrand: "",
  carModel: "",
  carYear: null as number | null,
  licensePlate: "",
  email: "",
  priceHour: null as number | null,
  priceDay: null as number | null,
});

const carFormFields = [
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
  { id: "contactEmail", key: "email", label: "Email партнёра", type: "email" },
  {
    id: "priceHour",
    key: "priceHour",
    label: "Цена за час",
    type: "number",
    min: "0.01",
    max: "1000000",
    step: "0.01",
  },
  {
    id: "priceDay",
    key: "priceDay",
    label: "Цена за день",
    type: "number",
    min: "0.01",
    max: "1000000",
    step: "0.01",
  },
];

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

const ticketStats = computed(() => {
  let client = 0,
    partner = 0,
    partnerCar = 0;
  for (const t of tickets.value) {
    if (t.ticketType === 2) partner++;
    else if (t.ticketType === 3) partnerCar++;
    else client++;
  }
  return { client, partner, partnerCar };
});

const statsStrip = computed(() => [
  { label: "В очереди", value: tickets.value.length },
  { label: "Клиенты", value: ticketStats.value.client },
  { label: "Партнёры", value: ticketStats.value.partner },
  { label: "Авто", value: ticketStats.value.partnerCar },
]);

const hasSelectedDocuments = computed(() => {
  if (!selectedTicket.value) return false;
  return Boolean(
    selectedTicket.value.identityDocumentFileName ||
    (isClientTicket(selectedTicket.value) &&
      selectedTicket.value.driverLicenseFileName) ||
    (isPartnerCarTicket(selectedTicket.value) &&
      selectedTicket.value.ownershipDocumentFileName),
  );
});

const selectedDocumentCount = computed(() => {
  if (!selectedTicket.value) return 0;
  let count = 0;
  if (selectedTicket.value.identityDocumentFileName) count++;
  if (
    isClientTicket(selectedTicket.value) &&
    selectedTicket.value.driverLicenseFileName
  )
    count++;
  if (
    isPartnerCarTicket(selectedTicket.value) &&
    selectedTicket.value.ownershipDocumentFileName
  )
    count++;
  return count;
});

const summaryRows = computed(() => {
  if (!selectedTicket.value) return [];
  const rows = [
    { label: "Статус", value: statusLabel(selectedTicket.value.status) },
    { label: "Тип", value: ticketTypeLabel(selectedTicket.value.ticketType) },
    { label: "Документы", value: String(selectedDocumentCount.value) },
  ];
  if (isPartnerCarTicket(selectedTicket.value))
    rows.push({
      label: "Фотографии",
      value: String(partnerCarImages.value.length),
    });
  return rows;
});

function statusLabel(status: number) {
  if (status === 1) return "На рассмотрении";
  if (status === 2) return "Одобрена";
  if (status === 3) return "Отклонена";
  return "Неизвестно";
}

function ticketTypeLabel(ticketType: number) {
  if (ticketType === 2) return "Партнёр";
  if (ticketType === 3) return "Авто партнёра";
  return "Клиент";
}

function getTicketTypeBadgeClass(ticketType: number) {
  if (ticketType === 2)
    return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
  if (ticketType === 3)
    return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
  return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
}

function getInitials(value: string) {
  const parts = value.trim().split(/\s+/).filter(Boolean).slice(0, 2);
  if (parts.length === 0) return "AR";
  return parts.map((p) => p[0]?.toUpperCase() ?? "").join("");
}

function isClientTicket(ticket: Ticket) {
  return ticket.ticketType === 1;
}
function isPartnerTicket(ticket: Ticket) {
  return ticket.ticketType === 2;
}
function isPartnerCarTicket(ticket: Ticket) {
  return ticket.ticketType === 3;
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function syncPartnerCarForm(ticket: Ticket | null) {
  if (!ticket || !isPartnerCarTicket(ticket)) {
    Object.assign(partnerCarForm, {
      carBrand: "",
      carModel: "",
      carYear: null,
      licensePlate: "",
      email: "",
      priceHour: null,
      priceDay: null,
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
  partnerCarForm.email = (ticket.email ?? "").trim();
  const rph = ticket.priceHour ?? data?.priceHour ?? null;
  const rpd = ticket.priceDay ?? data?.priceDay ?? null;
  partnerCarForm.priceHour = rph === null ? null : Number(rph);
  partnerCarForm.priceDay = rpd === null ? null : Number(rpd);
}

function buildPartnerCarPayload(): PartnerCarReviewPayload | null | undefined {
  if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value))
    return undefined;
  const carBrand = partnerCarForm.carBrand.trim();
  const carModel = partnerCarForm.carModel.trim();
  const carYear = Number(partnerCarForm.carYear);
  const licensePlate = partnerCarForm.licensePlate.trim();
  const email = partnerCarForm.email.trim();
  const priceHour = Number(partnerCarForm.priceHour);
  const priceDay = Number(partnerCarForm.priceDay);

  if (
    !carBrand ||
    !carModel ||
    !licensePlate ||
    !email ||
    !Number.isInteger(carYear)
  ) {
    errorMessage.value = "Заполните марку, модель, год, госномер и email.";
    return null;
  }
  if (carYear < 1886 || carYear > maxAllowedCarYear) {
    errorMessage.value = `Год машины должен быть в диапазоне 1886-${maxAllowedCarYear}.`;
    return null;
  }
  if (
    !Number.isFinite(priceHour) ||
    !Number.isFinite(priceDay) ||
    priceHour <= 0 ||
    priceDay <= 0
  ) {
    errorMessage.value = "Укажите корректные значения цен за час и за день.";
    return null;
  }
  return {
    carBrand,
    carModel,
    carYear,
    licensePlate,
    priceHour,
    priceDay,
    email,
  };
}

async function loadPending() {
  loading.value = true;
  errorMessage.value = "";
  successMessage.value = "";
  try {
    const data = await getPendingTickets();
    tickets.value = data;
    lastUpdatedAt.value = new Date().toISOString();
    if (data.length === 0) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      syncPartnerCarForm(null);
      return;
    }
    const fallback = data[0];
    if (!fallback) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      syncPartnerCarForm(null);
      return;
    }
    const nextId = data.some((t) => t.id === selectedTicketId.value)
      ? selectedTicketId.value
      : fallback.id;
    await selectTicket(nextId);
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось получить список заявок.";
  } finally {
    loading.value = false;
  }
}

async function selectTicket(ticketId: string) {
  selectedTicketId.value = ticketId;
  rejectReason.value = "";
  errorMessage.value = "";
  successMessage.value = "";
  try {
    selectedTicket.value = await getTicketById(ticketId);
    syncPartnerCarForm(selectedTicket.value);
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось загрузить заявку.";
  }
}

async function approveSelected() {
  if (!selectedTicket.value || actionLoading.value) return;
  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";
  try {
    const payload = buildPartnerCarPayload();
    if (payload === null) return;
    await approveTicket(selectedTicket.value.id, payload);
    successMessage.value = "Заявка одобрена.";
    await loadPending();
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось одобрить заявку.";
  } finally {
    actionLoading.value = false;
  }
}

async function rejectSelected() {
  if (!selectedTicket.value || actionLoading.value) return;
  if (!rejectReason.value.trim()) {
    errorMessage.value = "Укажите причину отказа.";
    return;
  }
  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";
  try {
    const payload = buildPartnerCarPayload();
    if (payload === null) return;
    await rejectTicket(
      selectedTicket.value.id,
      rejectReason.value.trim(),
      payload,
    );
    successMessage.value = "Заявка отклонена.";
    await loadPending();
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось отклонить заявку.";
  } finally {
    actionLoading.value = false;
  }
}

function openImage(url: string) {
  if (!url) return;
  window.open(url, "_blank", "noopener,noreferrer");
}

async function openDocument(
  documentType: "identity" | "license" | "ownership",
) {
  if (!selectedTicket.value || actionLoading.value) return;
  actionLoading.value = true;
  errorMessage.value = "";
  try {
    const link = await getTicketDocumentTemporaryLink(
      selectedTicket.value.id,
      documentType,
    );
    window.open(link.url, "_blank", "noopener,noreferrer");
  } catch (e: any) {
    errorMessage.value =
      e?.response?.data?.error || "Не удалось получить ссылку на документ.";
  } finally {
    actionLoading.value = false;
  }
}

onMounted(async () => {
  await loadPending();
});
</script>
