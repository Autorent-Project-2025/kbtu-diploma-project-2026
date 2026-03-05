<template>
  <div class="container manager-page">
    <section class="card manager-hero">
      <div>
        <p class="manager-hero__eyebrow">Manager Panel</p>
        <h1>Заявки на согласование</h1>
        <p class="manager-hero__subtitle">Проверяйте данные и принимайте решение по каждой заявке.</p>
      </div>
      <div class="manager-hero__actions">
        <span class="hero-stat">Pending: {{ tickets.length }}</span>
        <button class="btn btn-outline" @click="loadPending" :disabled="loading">Обновить</button>
      </div>
    </section>

    <div v-if="errorMessage" class="error">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success">{{ successMessage }}</div>

    <div v-if="loading" class="card state-card">Загрузка заявок...</div>
    <div v-else-if="tickets.length === 0" class="card state-card">Сейчас нет заявок в статусе Pending.</div>

    <div v-else class="tickets-layout">
      <aside class="card ticket-list-card">
        <div class="ticket-list-head">
          <h2>Очередь заявок</h2>
          <span>{{ tickets.length }} шт.</span>
        </div>

        <ul class="ticket-list">
          <li v-for="ticket in tickets" :key="ticket.id">
            <button
              class="ticket-item"
              :class="{ 'ticket-item--active': selectedTicketId === ticket.id }"
              @click="selectTicket(ticket.id)"
            >
              <div class="ticket-item__top">
                <strong>{{ ticket.fullName }}</strong>
                <span class="type-pill" :class="ticketTypeClass(ticket.ticketType)">
                  {{ ticketTypeLabel(ticket.ticketType) }}
                </span>
              </div>
              <p>{{ ticket.email }}</p>
              <div class="ticket-item__meta">
                <span>{{ formatDate(ticket.createdAt) }}</span>
                <span class="status-pill" :class="statusClass(ticket.status)">
                  {{ statusLabel(ticket.status) }}
                </span>
              </div>
            </button>
          </li>
        </ul>
      </aside>

      <section class="card ticket-details" v-if="selectedTicket">
        <header class="ticket-details__header">
          <div>
            <h2>{{ selectedTicket.fullName }}</h2>
            <p>{{ selectedTicket.email }}</p>
          </div>
          <span class="status-pill" :class="statusClass(selectedTicket.status)">
            {{ statusLabel(selectedTicket.status) }}
          </span>
        </header>

        <div class="details-grid">
          <article class="detail-item">
            <span class="detail-label">Ticket ID</span>
            <span class="detail-value detail-value--mono">{{ selectedTicket.id }}</span>
          </article>
          <article class="detail-item">
            <span class="detail-label">Тип заявки</span>
            <span class="detail-value">{{ ticketTypeLabel(selectedTicket.ticketType) }}</span>
          </article>
          <article class="detail-item">
            <span class="detail-label">Телефон</span>
            <span class="detail-value">{{ selectedTicket.phoneNumber }}</span>
          </article>
          <article class="detail-item" v-if="isClientTicket(selectedTicket)">
            <span class="detail-label">Дата рождения</span>
            <span class="detail-value">{{ selectedTicket.birthDate || "Не указана" }}</span>
          </article>
          <article class="detail-item">
            <span class="detail-label">Создана</span>
            <span class="detail-value">{{ formatDate(selectedTicket.createdAt) }}</span>
          </article>
        </div>

        <section class="docs-block" v-if="isPartnerCarTicket(selectedTicket)">
          <h3>Данные машины (редактируются перед решением)</h3>
          <div class="details-grid">
            <article class="detail-item">
              <label class="label" for="carBrand">Марка</label>
              <input id="carBrand" class="input" v-model="partnerCarForm.carBrand" />
            </article>
            <article class="detail-item">
              <label class="label" for="carModel">Модель</label>
              <input id="carModel" class="input" v-model="partnerCarForm.carModel" />
            </article>
            <article class="detail-item">
              <label class="label" for="carYear">Год</label>
              <input
                id="carYear"
                class="input"
                type="number"
                min="1886"
                :max="maxAllowedCarYear"
                v-model.number="partnerCarForm.carYear"
              />
            </article>
            <article class="detail-item">
              <label class="label" for="licensePlate">Гос номер</label>
              <input id="licensePlate" class="input" v-model="partnerCarForm.licensePlate" />
            </article>
            <article class="detail-item">
              <label class="label" for="contactEmail">Email партнера</label>
              <input id="contactEmail" class="input" type="email" v-model="partnerCarForm.email" />
            </article>
          </div>
        </section>

        <section class="docs-block">
          <h3>Проверка документов</h3>
          <div class="doc-actions">
            <button
              v-if="selectedTicket.identityDocumentFileName"
              class="btn btn-outline"
              @click="openDocument('identity')"
              :disabled="actionLoading"
            >
              {{ isPartnerTicket(selectedTicket) ? "Открыть удостоверение владельца" : "Открыть документ личности" }}
            </button>
            <button
              v-if="isClientTicket(selectedTicket)"
              class="btn btn-outline"
              @click="openDocument('license')"
              :disabled="actionLoading || !selectedTicket.driverLicenseFileName"
            >
              Открыть водительские права
            </button>
            <button
              v-if="isPartnerCarTicket(selectedTicket)"
              class="btn btn-outline"
              @click="openDocument('ownership')"
              :disabled="actionLoading || !selectedTicket.ownershipDocumentFileName"
            >
              Открыть файл собственности
            </button>
          </div>

          <div v-if="isPartnerCarTicket(selectedTicket) && partnerCarImages.length > 0" class="doc-actions">
            <button
              v-for="(image, index) in partnerCarImages"
              :key="`${image.imageId}-${index}`"
              class="btn btn-outline"
              @click="openImage(image.imageUrl)"
            >
              Фото машины #{{ index + 1 }}
            </button>
          </div>
        </section>

        <section class="decision-block">
          <label class="label" for="rejectReason">Причина отказа</label>
          <textarea
            id="rejectReason"
            class="textarea"
            v-model="rejectReason"
            placeholder="Укажите причину, если заявка отклоняется"
          />

          <div class="decision-actions">
            <button class="btn btn-primary" @click="approveSelected" :disabled="actionLoading">
              {{ actionLoading ? "Обработка..." : "Одобрить" }}
            </button>
            <button class="btn btn-danger" @click="rejectSelected" :disabled="actionLoading">
              {{ actionLoading ? "Обработка..." : "Отклонить" }}
            </button>
          </div>
        </section>
      </section>
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
import type { PartnerCarTicketData, PartnerCarTicketImageData, Ticket } from "../types/Ticket";

const tickets = ref<Ticket[]>([]);
const selectedTicket = ref<Ticket | null>(null);
const selectedTicketId = ref<string>("");
const rejectReason = ref("");
const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");
const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

const partnerCarForm = reactive({
  carBrand: "",
  carModel: "",
  carYear: null as number | null,
  licensePlate: "",
  email: "",
});

const partnerCarImages = computed<PartnerCarTicketImageData[]>(() => {
  if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value)) {
    return [];
  }

  if (Array.isArray(selectedTicket.value.carImages) && selectedTicket.value.carImages.length > 0) {
    return selectedTicket.value.carImages;
  }

  const data = selectedTicket.value.data;
  if (data && (data as PartnerCarTicketData).$type === "partner-car") {
    return (data as PartnerCarTicketData).carImages ?? [];
  }

  return [];
});

function statusLabel(status: number) {
  if (status === 1) return "Pending";
  if (status === 2) return "Approved";
  if (status === 3) return "Rejected";
  return "Unknown";
}

function statusClass(status: number) {
  if (status === 1) return "status-pill--pending";
  if (status === 2) return "status-pill--approved";
  if (status === 3) return "status-pill--rejected";
  return "";
}

function ticketTypeLabel(ticketType: number) {
  if (ticketType === 2) return "Партнер";
  if (ticketType === 3) return "Машина партнера";
  return "Клиент";
}

function ticketTypeClass(ticketType: number) {
  if (ticketType === 2) return "type-pill--partner";
  if (ticketType === 3) return "type-pill--partner-car";
  return "type-pill--client";
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

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function syncPartnerCarForm(ticket: Ticket | null) {
  if (!ticket || !isPartnerCarTicket(ticket)) {
    partnerCarForm.carBrand = "";
    partnerCarForm.carModel = "";
    partnerCarForm.carYear = null;
    partnerCarForm.licensePlate = "";
    partnerCarForm.email = "";
    return;
  }

  const data = (ticket.data as PartnerCarTicketData | undefined) ?? undefined;
  partnerCarForm.carBrand = (ticket.carBrand ?? data?.carBrand ?? "").trim();
  partnerCarForm.carModel = (ticket.carModel ?? data?.carModel ?? "").trim();
  const rawCarYear = ticket.carYear ?? data?.carYear ?? null;
  partnerCarForm.carYear = Number.isInteger(rawCarYear) ? Number(rawCarYear) : null;
  partnerCarForm.licensePlate = (ticket.licensePlate ?? data?.licensePlate ?? "").trim();
  partnerCarForm.email = (ticket.email ?? "").trim();
}

function buildPartnerCarPayload(): PartnerCarReviewPayload | null | undefined {
  if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value)) {
    return undefined;
  }

  const carBrand = partnerCarForm.carBrand.trim();
  const carModel = partnerCarForm.carModel.trim();
  const carYear = Number(partnerCarForm.carYear);
  const licensePlate = partnerCarForm.licensePlate.trim();
  const email = partnerCarForm.email.trim();

  if (!carBrand || !carModel || !licensePlate || !email || !Number.isInteger(carYear)) {
    errorMessage.value = "Для заявки на машину партнера заполните марку, модель, год, гос номер и email.";
    return null;
  }

  if (carYear < 1886 || carYear > maxAllowedCarYear) {
    errorMessage.value = `Год машины должен быть в диапазоне 1886-${maxAllowedCarYear}.`;
    return null;
  }

  return {
    carBrand,
    carModel,
    carYear,
    licensePlate,
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

    if (data.length === 0) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      syncPartnerCarForm(null);
      return;
    }

    const fallbackTicket = data[0];
    if (!fallbackTicket) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      syncPartnerCarForm(null);
      return;
    }

    const nextId = data.some((ticket) => ticket.id === selectedTicketId.value)
      ? selectedTicketId.value
      : fallbackTicket.id;

    await selectTicket(nextId);
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось получить список заявок.";
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
    errorMessage.value = e?.response?.data?.error || "Не удалось загрузить заявку.";
  }
}

async function approveSelected() {
  if (!selectedTicket.value || actionLoading.value) return;

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const partnerCarPayload = buildPartnerCarPayload();
    if (partnerCarPayload === null) {
      return;
    }

    await approveTicket(selectedTicket.value.id, partnerCarPayload);
    successMessage.value = "Заявка одобрена.";
    await loadPending();
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось одобрить заявку.";
  } finally {
    actionLoading.value = false;
  }
}

function openImage(url: string) {
  if (!url) return;
  window.open(url, "_blank", "noopener,noreferrer");
}

async function openDocument(documentType: "identity" | "license" | "ownership") {
  if (!selectedTicket.value || actionLoading.value) return;

  actionLoading.value = true;
  errorMessage.value = "";
  successMessage.value = "";

  try {
    const link = await getTicketDocumentTemporaryLink(selectedTicket.value.id, documentType);
    window.open(link.url, "_blank", "noopener,noreferrer");
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось получить временную ссылку на документ.";
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
    const partnerCarPayload = buildPartnerCarPayload();
    if (partnerCarPayload === null) {
      return;
    }

    await rejectTicket(selectedTicket.value.id, rejectReason.value.trim(), partnerCarPayload);
    successMessage.value = "Заявка отклонена.";
    await loadPending();
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось отклонить заявку.";
  } finally {
    actionLoading.value = false;
  }
}

onMounted(async () => {
  await loadPending();
});
</script>
