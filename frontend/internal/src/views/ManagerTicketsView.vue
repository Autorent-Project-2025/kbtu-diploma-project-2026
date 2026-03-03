<template>
  <div class="container">
    <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px">
      <div>
        <h1 style="margin: 0">Manager Panel</h1>
        <p style="margin: 4px 0 0; color: #6b7280">Pending registration tickets</p>
      </div>
      <button class="btn btn-outline" @click="loadPending" :disabled="loading">Обновить</button>
    </div>

    <div v-if="errorMessage" class="error" style="margin-bottom: 12px">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success" style="margin-bottom: 12px">{{ successMessage }}</div>

    <div v-if="loading">Загрузка...</div>
    <div v-else-if="tickets.length === 0" class="card">Pending заявок нет.</div>

    <div v-else style="display: grid; gap: 16px; grid-template-columns: 320px 1fr">
      <aside class="card">
        <ul style="list-style: none; margin: 0; padding: 0; display: grid; gap: 8px">
          <li v-for="ticket in tickets" :key="ticket.id">
            <button
              class="btn btn-outline"
              style="width: 100%; text-align: left; padding: 12px"
              @click="selectTicket(ticket.id)"
            >
              <div style="font-weight: 600">{{ ticket.fullName }}</div>
              <div style="font-size: 13px; color: #6b7280">{{ ticket.email }}</div>
              <div style="font-size: 12px; color: #4b5563; margin-top: 2px">
                {{ ticketTypeLabel(ticket.ticketType) }}
              </div>
            </button>
          </li>
        </ul>
      </aside>

      <section class="card" v-if="selectedTicket">
        <div style="display: grid; gap: 10px; margin-bottom: 12px">
          <div><strong>Ticket ID:</strong> {{ selectedTicket.id }}</div>
          <div><strong>Тип заявки:</strong> {{ ticketTypeLabel(selectedTicket.ticketType) }}</div>
          <div><strong>ФИО:</strong> {{ selectedTicket.fullName }}</div>
          <div><strong>Email:</strong> {{ selectedTicket.email }}</div>
          <div v-if="isClientTicket(selectedTicket)"><strong>Дата рождения:</strong> {{ selectedTicket.birthDate }}</div>
          <div><strong>Телефон:</strong> {{ selectedTicket.phoneNumber }}</div>
          <div>
            <strong>{{ isPartnerTicket(selectedTicket) ? "Удостоверение владельца:" : "Документ личности:" }}</strong>
            {{ selectedTicket.identityDocumentFileName ? "Загружен" : "Нет" }}
          </div>
          <div v-if="isClientTicket(selectedTicket)">
            <strong>Водительские права:</strong>
            {{ selectedTicket.driverLicenseFileName ? "Загружены" : "Нет" }}
          </div>
          <div><strong>Статус:</strong> {{ statusLabel(selectedTicket.status) }}</div>
        </div>

        <div style="display: flex; gap: 8px; margin-bottom: 12px">
          <button
            class="btn btn-outline"
            @click="openDocument('identity')"
            :disabled="actionLoading || !selectedTicket.identityDocumentFileName"
          >
            {{ isPartnerTicket(selectedTicket) ? "Глянуть удостоверение владельца" : "Глянуть удостоверение" }}
          </button>
          <button
            v-if="isClientTicket(selectedTicket)"
            class="btn btn-outline"
            @click="openDocument('license')"
            :disabled="actionLoading || !selectedTicket.driverLicenseFileName"
          >
            Глянуть права
          </button>
        </div>

        <div style="margin-bottom: 12px">
          <label class="label" for="rejectReason">Причина отказа</label>
          <textarea
            id="rejectReason"
            class="textarea"
            v-model="rejectReason"
            placeholder="Укажите причину, если отклоняете заявку"
          />
        </div>

        <div style="display: flex; gap: 8px">
          <button class="btn btn-primary" @click="approveSelected" :disabled="actionLoading">
            Approve
          </button>
          <button class="btn btn-danger" @click="rejectSelected" :disabled="actionLoading">
            Reject
          </button>
        </div>
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

function ticketTypeLabel(ticketType: number) {
  if (ticketType === 2) return "Партнерская заявка";
  return "Клиентская заявка";
}

function isClientTicket(ticket: Ticket) {
  return ticket.ticketType !== 2;
}

function isPartnerTicket(ticket: Ticket) {
  return ticket.ticketType === 2;
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
