<template>
  <div class="container manager-page">
    <section class="card manager-hero">
      <div>
        <p class="manager-hero__eyebrow">Manager Panel</p>
        <h1>Заявки на регистрацию</h1>
        <p class="manager-hero__subtitle">Проверяйте документы и принимайте решение по каждой новой заявке.</p>
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

        <section class="docs-block">
          <h3>Проверка документов</h3>
          <div class="doc-actions">
            <button
              class="btn btn-outline"
              @click="openDocument('identity')"
              :disabled="actionLoading || !selectedTicket.identityDocumentFileName"
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
import { onMounted, ref } from "vue";
import {
  approveTicket,
  getPendingTickets,
  getTicketById,
  getTicketDocumentTemporaryLink,
  rejectTicket
} from "../api/tickets";
import type { Ticket } from "../types/Ticket";

const tickets = ref<Ticket[]>([]);
const selectedTicket = ref<Ticket | null>(null);
const selectedTicketId = ref<string>("");
const rejectReason = ref("");
const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const successMessage = ref("");

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
  return "Клиент";
}

function ticketTypeClass(ticketType: number) {
  return ticketType === 2 ? "type-pill--partner" : "type-pill--client";
}

function isClientTicket(ticket: Ticket) {
  return ticket.ticketType !== 2;
}

function isPartnerTicket(ticket: Ticket) {
  return ticket.ticketType === 2;
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
      return;
    }

    const fallbackTicket = data[0];
    if (!fallbackTicket) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
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
    await approveTicket(selectedTicket.value.id);
    successMessage.value = "Заявка одобрена.";
    await loadPending();
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось одобрить заявку.";
  } finally {
    actionLoading.value = false;
  }
}

async function openDocument(documentType: "identity" | "license") {
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
    await rejectTicket(selectedTicket.value.id, rejectReason.value.trim());
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
