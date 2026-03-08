<template>
  <div class="page">
    <header class="page-header">
      <div>
        <h1>Рабочая очередь</h1>
        <p>Проверяйте новые регистрации, открывайте документы и принимайте решение по каждой заявке.</p>
      </div>

      <div class="page-header__actions">
        <div class="overview-strip">
          <div class="overview-strip__item">
            <strong>{{ tickets.length }}</strong>
            <span>В очереди</span>
          </div>
          <div class="overview-strip__item">
            <strong>{{ ticketStats.client }}</strong>
            <span>Клиенты</span>
          </div>
          <div class="overview-strip__item">
            <strong>{{ ticketStats.partner }}</strong>
            <span>Партнёры</span>
          </div>
          <div class="overview-strip__item">
            <strong>{{ ticketStats.partnerCar }}</strong>
            <span>Авто</span>
          </div>
        </div>

        <button class="btn btn-secondary" @click="loadPending" :disabled="loading">Обновить</button>
      </div>
    </header>

    <div v-if="errorMessage" class="error">{{ errorMessage }}</div>
    <div v-if="successMessage" class="success">{{ successMessage }}</div>

    <section v-if="loading" class="panel state-panel">Загрузка заявок...</section>
    <section v-else-if="tickets.length === 0" class="panel state-panel">Сейчас нет заявок на рассмотрении.</section>

    <div v-else class="review-layout">
      <aside class="panel queue-panel">
        <div class="panel-head">
          <h2>Очередь</h2>
          <p v-if="lastUpdatedAt">Обновлено {{ formatDate(lastUpdatedAt) }}</p>
          <p v-else>{{ tickets.length }} заявок ожидают решения.</p>
        </div>

        <ul class="ticket-list">
          <li v-for="ticket in tickets" :key="ticket.id">
            <button
              class="ticket-item"
              :class="{ 'ticket-item--active': selectedTicketId === ticket.id }"
              @click="selectTicket(ticket.id)"
            >
              <div class="ticket-item__row">
                <div class="ticket-item__identity">
                  <span class="avatar">{{ getInitials(ticket.fullName) }}</span>
                  <div class="ticket-item__copy">
                    <strong>{{ ticket.fullName }}</strong>
                    <p>{{ ticket.email }}</p>
                  </div>
                </div>

                <span class="ticket-item__type">{{ ticketTypeLabel(ticket.ticketType) }}</span>
              </div>

              <div class="ticket-item__meta">
                <span>{{ ticket.phoneNumber }}</span>
                <span>{{ formatDate(ticket.createdAt) }}</span>
              </div>
            </button>
          </li>
        </ul>
      </aside>

      <section v-if="selectedTicket" class="panel detail-panel">
        <header class="detail-header">
          <div class="detail-header__identity">
            <span class="avatar avatar--large">{{ getInitials(selectedTicket.fullName) }}</span>
            <div>
              <h2>{{ selectedTicket.fullName }}</h2>
              <p>{{ ticketTypeLabel(selectedTicket.ticketType) }} · {{ selectedTicket.email }}</p>
            </div>
          </div>

          <div class="detail-header__meta">
            <span>Статус: {{ statusLabel(selectedTicket.status) }}</span>
            <span>Создана: {{ formatDate(selectedTicket.createdAt) }}</span>
          </div>
        </header>

        <div class="detail-layout">
          <div class="detail-main">
            <section class="section-block">
              <h3>Основные данные</h3>

              <dl class="field-grid">
                <div class="field">
                  <dt>ID заявки</dt>
                  <dd class="field-value field-value--mono">{{ selectedTicket.id }}</dd>
                </div>
                <div class="field">
                  <dt>Тип заявки</dt>
                  <dd class="field-value">{{ ticketTypeLabel(selectedTicket.ticketType) }}</dd>
                </div>
                <div class="field">
                  <dt>Email</dt>
                  <dd class="field-value">{{ selectedTicket.email }}</dd>
                </div>
                <div class="field">
                  <dt>Телефон</dt>
                  <dd class="field-value">{{ selectedTicket.phoneNumber }}</dd>
                </div>
                <div v-if="isClientTicket(selectedTicket)" class="field">
                  <dt>Дата рождения</dt>
                  <dd class="field-value">{{ selectedTicket.birthDate || "Не указана" }}</dd>
                </div>
                <div class="field">
                  <dt>Создана</dt>
                  <dd class="field-value">{{ formatDate(selectedTicket.createdAt) }}</dd>
                </div>
              </dl>
            </section>

            <section v-if="isPartnerCarTicket(selectedTicket)" class="section-block">
              <div class="section-block__header">
                <h3>Данные автомобиля</h3>
                <p>При необходимости скорректируйте карточку перед финальным решением.</p>
              </div>

              <div class="form-grid">
                <div class="form-field">
                  <label class="label" for="carBrand">Марка</label>
                  <input id="carBrand" class="input" v-model="partnerCarForm.carBrand" />
                </div>
                <div class="form-field">
                  <label class="label" for="carModel">Модель</label>
                  <input id="carModel" class="input" v-model="partnerCarForm.carModel" />
                </div>
                <div class="form-field">
                  <label class="label" for="carYear">Год</label>
                  <input
                    id="carYear"
                    class="input"
                    type="number"
                    min="1886"
                    :max="maxAllowedCarYear"
                    v-model.number="partnerCarForm.carYear"
                  />
                </div>
                <div class="form-field">
                  <label class="label" for="licensePlate">Госномер</label>
                  <input id="licensePlate" class="input" v-model="partnerCarForm.licensePlate" />
                </div>
                <div class="form-field">
                  <label class="label" for="contactEmail">Email партнёра</label>
                  <input id="contactEmail" class="input" type="email" v-model="partnerCarForm.email" />
                </div>
                <div class="form-field">
                  <label class="label" for="priceHour">Цена за час</label>
                  <input
                    id="priceHour"
                    class="input"
                    type="number"
                    min="0.01"
                    max="1000000"
                    step="0.01"
                    v-model.number="partnerCarForm.priceHour"
                  />
                </div>
                <div class="form-field">
                  <label class="label" for="priceDay">Цена за день</label>
                  <input
                    id="priceDay"
                    class="input"
                    type="number"
                    min="0.01"
                    max="1000000"
                    step="0.01"
                    v-model.number="partnerCarForm.priceDay"
                  />
                </div>
              </div>
            </section>

            <section class="section-block">
              <div class="section-block__header">
                <h3>Документы</h3>
                <p>Проверьте приложенные файлы перед принятием решения.</p>
              </div>

              <ul v-if="hasSelectedDocuments" class="document-list">
                <li v-if="selectedTicket.identityDocumentFileName" class="document-item">
                  <div>
                    <strong>{{ isPartnerTicket(selectedTicket) ? "Документ владельца" : "Документ личности" }}</strong>
                    <span>{{ selectedTicket.identityDocumentFileName }}</span>
                  </div>

                  <button class="btn btn-secondary" @click="openDocument('identity')" :disabled="actionLoading">
                    Открыть
                  </button>
                </li>

                <li v-if="isClientTicket(selectedTicket) && selectedTicket.driverLicenseFileName" class="document-item">
                  <div>
                    <strong>Водительские права</strong>
                    <span>{{ selectedTicket.driverLicenseFileName }}</span>
                  </div>

                  <button class="btn btn-secondary" @click="openDocument('license')" :disabled="actionLoading">
                    Открыть
                  </button>
                </li>

                <li
                  v-if="isPartnerCarTicket(selectedTicket) && selectedTicket.ownershipDocumentFileName"
                  class="document-item"
                >
                  <div>
                    <strong>Документ собственности</strong>
                    <span>{{ selectedTicket.ownershipDocumentFileName }}</span>
                  </div>

                  <button class="btn btn-secondary" @click="openDocument('ownership')" :disabled="actionLoading">
                    Открыть
                  </button>
                </li>
              </ul>

              <p v-else class="section-empty">К заявке пока не прикреплены документы.</p>

              <div v-if="isPartnerCarTicket(selectedTicket) && partnerCarImages.length > 0" class="section-block__subsection">
                <h3>Фотографии автомобиля</h3>
                <div class="image-list">
                  <button
                    v-for="(image, index) in partnerCarImages"
                    :key="`${image.imageId}-${index}`"
                    class="btn btn-secondary"
                    @click="openImage(image.imageUrl)"
                  >
                    Фото {{ index + 1 }}
                  </button>
                </div>
              </div>
            </section>
          </div>

          <aside class="detail-side">
            <section class="section-block section-block--aside">
              <h3>Сводка</h3>

              <dl class="summary-list">
                <div class="summary-list__item">
                  <dt>Статус</dt>
                  <dd>{{ statusLabel(selectedTicket.status) }}</dd>
                </div>
                <div class="summary-list__item">
                  <dt>Тип</dt>
                  <dd>{{ ticketTypeLabel(selectedTicket.ticketType) }}</dd>
                </div>
                <div class="summary-list__item">
                  <dt>Документы</dt>
                  <dd>{{ selectedDocumentCount }}</dd>
                </div>
                <div class="summary-list__item" v-if="isPartnerCarTicket(selectedTicket)">
                  <dt>Фотографии</dt>
                  <dd>{{ partnerCarImages.length }}</dd>
                </div>
              </dl>
            </section>

            <section class="section-block section-block--aside">
              <div class="section-block__header">
                <h3>Решение</h3>
                <p>Причину нужно указать только для отказа.</p>
              </div>

              <div>
                <label class="label" for="rejectReason">Причина отказа</label>
                <textarea
                  id="rejectReason"
                  class="textarea"
                  v-model="rejectReason"
                  placeholder="Укажите причину, если заявка отклоняется"
                />
              </div>

              <div class="decision-actions">
                <button class="btn btn-primary" @click="approveSelected" :disabled="actionLoading">
                  {{ actionLoading ? "Обработка..." : "Одобрить" }}
                </button>
                <button class="btn btn-danger" @click="rejectSelected" :disabled="actionLoading">
                  {{ actionLoading ? "Обработка..." : "Отклонить" }}
                </button>
              </div>
            </section>
          </aside>
        </div>
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

const ticketStats = computed(() => {
  const stats = {
    client: 0,
    partner: 0,
    partnerCar: 0,
  };

  for (const ticket of tickets.value) {
    if (ticket.ticketType === 2) {
      stats.partner += 1;
      continue;
    }

    if (ticket.ticketType === 3) {
      stats.partnerCar += 1;
      continue;
    }

    stats.client += 1;
  }

  return stats;
});

const hasSelectedDocuments = computed(() => {
  if (!selectedTicket.value) {
    return false;
  }

  return Boolean(
    selectedTicket.value.identityDocumentFileName ||
      (isClientTicket(selectedTicket.value) && selectedTicket.value.driverLicenseFileName) ||
      (isPartnerCarTicket(selectedTicket.value) && selectedTicket.value.ownershipDocumentFileName),
  );
});

const selectedDocumentCount = computed(() => {
  if (!selectedTicket.value) {
    return 0;
  }

  let count = 0;
  if (selectedTicket.value.identityDocumentFileName) {
    count += 1;
  }

  if (isClientTicket(selectedTicket.value) && selectedTicket.value.driverLicenseFileName) {
    count += 1;
  }

  if (isPartnerCarTicket(selectedTicket.value) && selectedTicket.value.ownershipDocumentFileName) {
    count += 1;
  }

  return count;
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

function getInitials(value: string) {
  const parts = value
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2);

  if (parts.length === 0) {
    return "AR";
  }

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
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
    partnerCarForm.priceHour = null;
    partnerCarForm.priceDay = null;
    return;
  }

  const data = (ticket.data as PartnerCarTicketData | undefined) ?? undefined;
  partnerCarForm.carBrand = (ticket.carBrand ?? data?.carBrand ?? "").trim();
  partnerCarForm.carModel = (ticket.carModel ?? data?.carModel ?? "").trim();
  const rawCarYear = ticket.carYear ?? data?.carYear ?? null;
  partnerCarForm.carYear = Number.isInteger(rawCarYear) ? Number(rawCarYear) : null;
  partnerCarForm.licensePlate = (ticket.licensePlate ?? data?.licensePlate ?? "").trim();
  partnerCarForm.email = (ticket.email ?? "").trim();
  const rawPriceHour = ticket.priceHour ?? data?.priceHour ?? null;
  const rawPriceDay = ticket.priceDay ?? data?.priceDay ?? null;
  partnerCarForm.priceHour = rawPriceHour === null ? null : Number(rawPriceHour);
  partnerCarForm.priceDay = rawPriceDay === null ? null : Number(rawPriceDay);
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
  const priceHour = Number(partnerCarForm.priceHour);
  const priceDay = Number(partnerCarForm.priceDay);

  if (!carBrand || !carModel || !licensePlate || !email || !Number.isInteger(carYear)) {
    errorMessage.value = "Для заявки на авто партнёра заполните марку, модель, год, госномер и email.";
    return null;
  }

  if (carYear < 1886 || carYear > maxAllowedCarYear) {
    errorMessage.value = `Год машины должен быть в диапазоне 1886-${maxAllowedCarYear}.`;
    return null;
  }

  if (!Number.isFinite(priceHour) || !Number.isFinite(priceDay) || priceHour <= 0 || priceDay <= 0) {
    errorMessage.value = "Укажите корректные значения цен за час и за день (больше 0).";
    return null;
  }

  if (priceHour > 1_000_000 || priceDay > 1_000_000) {
    errorMessage.value = "Цена за час и за день должна быть не больше 1 000 000.";
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
