<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-center gap-6">
        <router-link
          to="/clients"
          class="inline-flex items-center gap-2 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-blue-500 transition-colors shrink-0"
        >
          ← Назад
        </router-link>

        <template v-if="!loading && client">
          <!-- Avatar -->
          <div
            class="h-14 w-14 rounded-full bg-blue-100 dark:bg-blue-500/20 flex items-center justify-center text-blue-700 dark:text-blue-300 font-extrabold text-xl shrink-0 select-none"
          >
            {{ clientInitials }}
          </div>

          <!-- Title + status dot -->
          <div class="flex-1 min-w-0 space-y-1">
            <p class="text-xs font-bold uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400">
              Data Management · Клиент #{{ client.id }}
            </p>
            <div class="flex items-center gap-3 flex-wrap">
              <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
                {{ client.firstName }} {{ client.lastName }}
              </h1>
              <!-- Status dot -->
              <span class="flex items-center gap-1.5">
                <span
                  :class="[
                    'h-2.5 w-2.5 rounded-full',
                    client.bookingActionsBlocked
                      ? 'bg-red-500 shadow-[0_0_6px_rgba(239,68,68,0.7)]'
                      : 'bg-emerald-500 shadow-[0_0_6px_rgba(16,185,129,0.7)]',
                  ]"
                />
                <span
                  :class="[
                    'text-xs font-semibold',
                    client.bookingActionsBlocked
                      ? 'text-red-600 dark:text-red-400'
                      : 'text-emerald-600 dark:text-emerald-400',
                  ]"
                >
                  {{ client.bookingActionsBlocked ? "Заблокирован" : "Активен" }}
                </span>
              </span>
            </div>
          </div>
        </template>

        <template v-else-if="loading">
          <div class="space-y-1">
            <p class="text-xs font-bold uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400">
              Data Management
            </p>
            <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">Загрузка...</h1>
          </div>
        </template>
      </div>
    </header>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка данных клиента...
    </div>

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Клиент не найден.
    </div>

    <template v-else-if="client">
      <!-- Block status banner -->
      <div
        v-if="client.bookingActionsBlocked"
        class="rounded-2xl border border-red-200 dark:border-red-800/50 bg-red-50 dark:bg-red-900/20 px-6 py-4 flex flex-col sm:flex-row sm:items-center justify-between gap-4"
      >
        <div class="flex items-start gap-3">
          <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3m0 3h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z" />
          </svg>
          <div>
            <p class="text-sm font-bold text-red-700 dark:text-red-300">Бронирования заблокированы</p>
            <p v-if="client.bookingBlockReason" class="text-sm text-red-600 dark:text-red-400 mt-0.5">
              Причина: {{ client.bookingBlockReason }}
            </p>
            <p v-if="client.bookingBlockedAt" class="text-xs text-red-500 dark:text-red-500 mt-1">
              Заблокирован: {{ formatDateTime(client.bookingBlockedAt) }}
            </p>
          </div>
        </div>
        <button
          v-if="canBlock"
          @click="promptUnblock"
          :disabled="blockingInProgress"
          class="shrink-0 px-5 py-2 rounded-xl border border-red-300 dark:border-red-600/50 text-red-700 dark:text-red-300 text-sm font-semibold hover:bg-red-100 dark:hover:bg-red-800/30 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {{ blockingInProgress ? "Обработка..." : "Разблокировать" }}
        </button>
      </div>

      <!-- Quick actions bar -->
      <div class="flex flex-wrap items-start gap-3">
        <!-- Block action -->
        <template v-if="canBlock">
          <div v-if="!client.bookingActionsBlocked" class="flex flex-col gap-2">
            <div v-if="showBlockForm" class="flex items-center gap-2 flex-wrap">
              <input
                v-model="blockReason"
                type="text"
                placeholder="Причина блокировки (необязательно)"
                class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-red-400 focus:ring-2 focus:ring-red-400/20 transition-colors w-72"
              />
              <button
                @click="promptBlock"
                :disabled="blockingInProgress"
                class="px-4 py-2 rounded-xl bg-red-600 hover:bg-red-700 text-white text-sm font-semibold transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ blockingInProgress ? "Блокировка..." : "Заблокировать" }}
              </button>
              <button
                @click="showBlockForm = false; blockReason = ''"
                class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-600 dark:text-gray-400 text-sm font-semibold hover:border-gray-400 transition-colors"
              >
                Отмена
              </button>
            </div>
            <button
              v-else
              @click="showBlockForm = true"
              class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-500/40 text-red-600 dark:text-red-400 text-sm font-semibold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
            >
              Заблокировать
            </button>
          </div>

          <button
            v-else
            @click="promptUnblock"
            :disabled="blockingInProgress"
            class="px-5 py-2.5 rounded-2xl border border-emerald-300 dark:border-emerald-500/40 text-emerald-600 dark:text-emerald-400 text-sm font-semibold hover:bg-emerald-50 dark:hover:bg-emerald-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ blockingInProgress ? "Обработка..." : "Разблокировать" }}
          </button>
        </template>

        <!-- Edit profile toggle -->
        <button
          v-if="canUpdate && !editMode"
          @click="startEdit"
          class="px-5 py-2.5 rounded-2xl border border-blue-300 dark:border-blue-500/40 text-blue-600 dark:text-blue-400 text-sm font-semibold hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors"
        >
          Редактировать профиль
        </button>
      </div>

      <!-- Profile section -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Профиль</h2>
          <div v-if="editMode" class="flex items-center gap-2">
            <button
              @click="onSave"
              :disabled="saving"
              class="px-5 py-2 rounded-xl bg-blue-600 hover:bg-blue-700 text-white text-sm font-semibold transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {{ saving ? "Сохранение..." : "Сохранить" }}
            </button>
            <button
              @click="cancelEdit"
              class="px-5 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-600 dark:text-gray-400 text-sm font-semibold hover:border-gray-400 transition-colors"
            >
              Отмена
            </button>
          </div>
        </div>

        <!-- View mode -->
        <dl v-if="!editMode" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Имя</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ client.firstName }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Фамилия</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ client.lastName }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Телефон</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ client.phoneNumber || "—" }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Дата рождения</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ formatDate(client.birthDate) }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Дата регистрации</dt>
            <dd class="text-gray-600 dark:text-gray-400 text-sm">
              {{ formatDateTime(client.createdOn) }}
              <span class="text-gray-400 dark:text-gray-500 ml-1">({{ relativeTime(client.createdOn) }})</span>
            </dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">User ID</dt>
            <dd class="font-mono text-xs text-gray-500 dark:text-gray-400 break-all">{{ client.relatedUserId || "—" }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Документы</dt>
            <dd class="flex flex-wrap gap-1.5 mt-1">
              <span
                v-if="client.identityDocumentFileName"
                class="px-2 py-0.5 rounded-full bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300 text-xs font-semibold"
              >
                Паспорт
              </span>
              <span
                v-if="client.driverLicenseFileName"
                class="px-2 py-0.5 rounded-full bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300 text-xs font-semibold"
              >
                Права
              </span>
              <span
                v-if="!client.identityDocumentFileName && !client.driverLicenseFileName"
                class="text-gray-400 dark:text-gray-500 text-sm"
              >
                Не загружены
              </span>
            </dd>
          </div>
          <div v-if="client.avatarUrl">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Аватар</dt>
            <dd>
              <img
                :src="client.avatarUrl"
                alt="Аватар"
                class="h-12 w-12 rounded-full object-cover border border-gray-200 dark:border-gray-700"
              />
            </dd>
          </div>
        </dl>

        <!-- Edit mode form -->
        <form v-else @submit.prevent="onSave" class="grid grid-cols-1 sm:grid-cols-2 gap-5">
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Имя</label>
            <input
              v-model="editForm.firstName"
              type="text"
              required
              class="w-full px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm"
            />
          </div>
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Фамилия</label>
            <input
              v-model="editForm.lastName"
              type="text"
              required
              class="w-full px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm"
            />
          </div>
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Телефон</label>
            <input
              v-model="editForm.phoneNumber"
              type="tel"
              required
              class="w-full px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm"
            />
          </div>
          <div>
            <label class="block text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1.5">Дата рождения</label>
            <input
              v-model="editForm.birthDate"
              type="date"
              required
              class="w-full px-4 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm"
            />
          </div>
        </form>
      </div>

      <!-- Related bookings -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Бронирования</h2>
          <span
            v-if="!bookingsLoading"
            class="px-3 py-1 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold"
          >
            {{ clientBookings.length }}
          </span>
        </div>

        <div
          v-if="bookingsLoading"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 text-gray-500 dark:text-gray-400 text-sm font-medium"
        >
          Загрузка бронирований...
        </div>

        <div
          v-else-if="clientBookings.length === 0"
          class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-10 text-center text-gray-500 dark:text-gray-400 font-medium"
        >
          Бронирования не найдены.
        </div>

        <div
          v-else
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
        >
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-200 dark:border-gray-800">
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Автомобиль</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Период</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="booking in clientBookings"
                :key="booking.id"
                @click="$router.push(`/bookings/${booking.id}`)"
                class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
              >
                <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                  #{{ booking.id }}
                </td>
                <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">
                  {{ booking.carBrand }} {{ booking.carModel }}
                </td>
                <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ formatDate(booking.startTime) }} — {{ formatDate(booking.endTime) }}
                </td>
                <td class="px-5 py-3 text-gray-900 dark:text-white font-semibold">
                  {{ booking.totalPrice ? formatPrice(booking.totalPrice) : "—" }}
                </td>
                <td class="px-5 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', bookingStatusBadge(booking.status)]">
                    {{ bookingStatusLabel(booking.status) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Related tickets -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Заявки</h2>
          <span
            v-if="!ticketsLoading"
            class="px-3 py-1 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold"
          >
            {{ clientTickets.length }}
          </span>
        </div>

        <div
          v-if="ticketsLoading"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-6 text-gray-500 dark:text-gray-400 text-sm font-medium"
        >
          Загрузка заявок...
        </div>

        <div
          v-else-if="clientTickets.length === 0"
          class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-10 text-center text-gray-500 dark:text-gray-400 font-medium"
        >
          Заявки не найдены.
        </div>

        <div
          v-else
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
        >
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-200 dark:border-gray-800">
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Тип</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Создана</th>
                <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Рассмотрена</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="ticket in clientTickets"
                :key="ticket.id"
                class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors"
              >
                <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400 max-w-[120px] truncate" :title="ticket.id">
                  {{ ticket.id.slice(0, 8) }}…
                </td>
                <td class="px-5 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', ticketTypeBadge(ticket.ticketType)]">
                    {{ ticketTypeLabel(ticket.ticketType) }}
                  </span>
                </td>
                <td class="px-5 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', ticketStatusBadge(ticket.status)]">
                    {{ ticketStatusLabel(ticket.status) }}
                  </span>
                </td>
                <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ formatDateTime(ticket.createdAt) }}
                </td>
                <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                  {{ ticket.reviewedAt ? formatDateTime(ticket.reviewedAt) : "—" }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- Confirm modal -->
    <ConfirmModal
      :show="confirmModal.show"
      :title="confirmModal.title"
      :message="confirmModal.message"
      :variant="confirmModal.variant"
      :confirm-text="confirmModal.confirmText"
      @confirm="confirmModal.onConfirm(); confirmModal.show = false"
      @cancel="confirmModal.show = false"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getClient,
  updateClient,
  blockClient,
  unblockClient,
  type ClientDto,
  type ClientUpdatePayload,
} from "../api/clients";
import { getAllBookings, type BookingDto } from "../api/bookings";
import { getAllTickets } from "../api/tickets";
import type { Ticket } from "../types/Ticket";
import { formatDate, formatDateTime, formatPrice, relativeTime } from "../utils/formatters";
import {
  bookingStatusLabel,
  bookingStatusBadge,
  ticketStatusLabel,
  ticketStatusBadge,
} from "../utils/statusMaps";
import { useToast } from "../composables/useToast";
import { auth } from "../store/auth";
import EntityLink from "../components/EntityLink.vue";
import ConfirmModal from "../components/ConfirmModal.vue";

const route = useRoute();
const router = useRouter();
const toast = useToast();

// ── State ───────────────────────────────────────────────────────────────────

const loading = ref(false);
const notFound = ref(false);
const client = ref<ClientDto | null>(null);

const editMode = ref(false);
const saving = ref(false);

const bookingsLoading = ref(false);
const allBookings = ref<BookingDto[]>([]);

const ticketsLoading = ref(false);
const allTickets = ref<Ticket[]>([]);

const blockingInProgress = ref(false);
const showBlockForm = ref(false);
const blockReason = ref("");

// ── Permissions ─────────────────────────────────────────────────────────────

const canUpdate = computed(() => auth.hasPermission("Client.Update"));
const canBlock = computed(() => auth.hasPermission("Client.Block"));

// ── Edit form ───────────────────────────────────────────────────────────────

const editForm = reactive({
  firstName: "",
  lastName: "",
  phoneNumber: "",
  birthDate: "",
});

function startEdit() {
  if (!client.value) return;
  editForm.firstName = client.value.firstName ?? "";
  editForm.lastName = client.value.lastName ?? "";
  editForm.phoneNumber = client.value.phoneNumber ?? "";
  editForm.birthDate = client.value.birthDate
    ? (client.value.birthDate.split("T")[0] ?? "")
    : "";
  editMode.value = true;
}

function cancelEdit() {
  editMode.value = false;
}

// ── Confirm modal ────────────────────────────────────────────────────────────

const confirmModal = reactive({
  show: false,
  title: "",
  message: "",
  variant: "primary" as "primary" | "danger",
  confirmText: "Подтвердить",
  onConfirm: () => {},
});

// ── Derived ─────────────────────────────────────────────────────────────────

const clientInitials = computed(() => {
  if (!client.value) return "";
  const f = client.value.firstName?.charAt(0)?.toUpperCase() ?? "";
  const l = client.value.lastName?.charAt(0)?.toUpperCase() ?? "";
  return f + l;
});

const clientBookings = computed(() => {
  if (!client.value?.relatedUserId) return allBookings.value;
  return allBookings.value.filter(
    (b) => b.userId === client.value!.relatedUserId
  );
});

const clientTickets = computed(() => {
  if (!client.value) return [];
  const userId = client.value.relatedUserId;
  const phone = client.value.phoneNumber;
  return allTickets.value.filter((t) => {
    if (userId && (t.data as Record<string, unknown>)?.relatedUserId === userId) return true;
    if (userId && t.reviewedByManagerId === userId) return false;
    // ticketType 1 = client ticket; match by phone or relatedUserId stored in ticket root fields
    if (t.ticketType === 1) {
      if (phone && t.phoneNumber === phone) return true;
    }
    return false;
  });
});

// ── Ticket display helpers ───────────────────────────────────────────────────

const ticketTypeLabels: Record<number, string> = {
  1: "Клиент",
  2: "Партнёр",
  3: "Авто партнёра",
  4: "Завершение поездки",
  5: "Отмена бронирования",
};

const ticketTypeBadgeMap: Record<number, string> = {
  1: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300",
  2: "bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300",
  3: "bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-300",
  4: "bg-teal-100 text-teal-700 dark:bg-teal-500/20 dark:text-teal-300",
  5: "bg-rose-100 text-rose-700 dark:bg-rose-500/20 dark:text-rose-300",
};

function ticketTypeLabel(type: number): string {
  return ticketTypeLabels[type] ?? `Тип ${type}`;
}

function ticketTypeBadge(type: number): string {
  return ticketTypeBadgeMap[type] ?? "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300";
}

// ── Data loaders ─────────────────────────────────────────────────────────────

async function loadClient() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }
  loading.value = true;
  try {
    client.value = await getClient(id);
  } catch {
    notFound.value = true;
  } finally {
    loading.value = false;
  }
}

async function loadBookings() {
  if (!client.value?.relatedUserId) return;
  bookingsLoading.value = true;
  try {
    const result = await getAllBookings({
      userId: client.value.relatedUserId,
      page: 1,
      pageSize: 20,
    });
    allBookings.value = result.items ?? [];
  } catch {
    allBookings.value = [];
  } finally {
    bookingsLoading.value = false;
  }
}

async function loadTickets() {
  ticketsLoading.value = true;
  try {
    allTickets.value = await getAllTickets();
  } catch {
    allTickets.value = [];
  } finally {
    ticketsLoading.value = false;
  }
}

// ── Actions ──────────────────────────────────────────────────────────────────

async function onSave() {
  if (saving.value || !client.value) return;
  saving.value = true;
  try {
    const payload: ClientUpdatePayload = {
      firstName: editForm.firstName,
      lastName: editForm.lastName,
      phoneNumber: editForm.phoneNumber,
      birthDate: editForm.birthDate,
      relatedUserId: client.value.relatedUserId,
      identityDocumentFileName: client.value.identityDocumentFileName,
      driverLicenseFileName: client.value.driverLicenseFileName,
      avatarUrl: client.value.avatarUrl,
      avatarImageId: client.value.avatarImageId,
    };
    const updated = await updateClient(client.value.id, payload);
    client.value = updated;
    editMode.value = false;
    toast.success("Профиль клиента обновлён");
  } catch {
    toast.error("Ошибка при сохранении профиля");
  } finally {
    saving.value = false;
  }
}

function promptBlock() {
  confirmModal.title = "Заблокировать клиента";
  confirmModal.message = blockReason.value
    ? `Клиент будет заблокирован. Причина: "${blockReason.value}". Продолжить?`
    : "Вы уверены, что хотите заблокировать этого клиента?";
  confirmModal.variant = "danger";
  confirmModal.confirmText = "Заблокировать";
  confirmModal.onConfirm = doBlock;
  confirmModal.show = true;
}

function promptUnblock() {
  confirmModal.title = "Разблокировать клиента";
  confirmModal.message = "Клиент снова получит доступ к бронированиям. Продолжить?";
  confirmModal.variant = "primary";
  confirmModal.confirmText = "Разблокировать";
  confirmModal.onConfirm = doUnblock;
  confirmModal.show = true;
}

async function doBlock() {
  if (!client.value?.relatedUserId) return;
  blockingInProgress.value = true;
  try {
    await blockClient(client.value.relatedUserId, blockReason.value || undefined);
    toast.success("Клиент заблокирован");
    showBlockForm.value = false;
    blockReason.value = "";
    await loadClient();
  } catch {
    toast.error("Ошибка при блокировке клиента");
  } finally {
    blockingInProgress.value = false;
  }
}

async function doUnblock() {
  if (!client.value?.relatedUserId) return;
  blockingInProgress.value = true;
  try {
    await unblockClient(client.value.relatedUserId);
    toast.success("Клиент разблокирован");
    await loadClient();
  } catch {
    toast.error("Ошибка при разблокировке клиента");
  } finally {
    blockingInProgress.value = false;
  }
}

// ── Lifecycle ────────────────────────────────────────────────────────────────

onMounted(async () => {
  await loadClient();
  if (client.value) {
    await Promise.all([loadBookings(), loadTickets()]);
  }
});
</script>
