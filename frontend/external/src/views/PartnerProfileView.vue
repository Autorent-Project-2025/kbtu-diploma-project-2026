<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto space-y-8">
      <!-- ── Hero Header ─────────────────────────────────────────────────── -->
      <header
        class="relative overflow-hidden rounded-[32px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8 sm:p-10"
      >
        <div class="flex flex-col sm:flex-row sm:items-center gap-6">
          <!-- Avatar with tie badge -->
          <div class="relative shrink-0">
            <div
              class="w-24 h-24 rounded-3xl bg-gradient-to-br from-emerald-500 to-emerald-700 flex items-center justify-center shadow-xl border-2 border-emerald-400/40"
            >
              <span class="text-white text-3xl font-extrabold">
                {{ initials }}
              </span>
            </div>
            <!-- Tie/business badge -->
            <div
              class="absolute -bottom-2 -right-2 w-9 h-9 rounded-xl bg-white dark:bg-gray-900 border-2 border-emerald-400/60 flex items-center justify-center shadow-lg"
              title="Партнёр"
            >
              <!-- Tie icon SVG -->
              <svg
                class="w-5 h-5 text-emerald-600 dark:text-emerald-400"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 2L9.5 7l1 3L9 22h6l-1.5-12 1-3z" opacity="0.85" />
                <path d="M9.5 7l-1.5-5h8l-1.5 5z" />
              </svg>
            </div>
          </div>

          <!-- Name & partner badge -->
          <div class="space-y-2 flex-1">
            <div class="flex flex-wrap items-center gap-3">
              <p
                class="text-sm font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
              >
                Профиль партнёра
              </p>
              <span
                class="px-3 py-0.5 rounded-full bg-emerald-100 dark:bg-emerald-900/40 text-emerald-700 dark:text-emerald-300 text-xs font-bold uppercase tracking-wide border border-emerald-200 dark:border-emerald-700/50"
              >
                ✦ Партнёр
              </span>
            </div>
            <h1
              class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
            >
              <span v-if="partner"
                >{{ partner.ownerFirstName }} {{ partner.ownerLastName }}</span
              >
              <span v-else class="opacity-40">Загрузка...</span>
            </h1>
            <p
              v-if="partner"
              class="text-base text-gray-500 dark:text-gray-400"
            >
              {{ partner.phoneNumber }} · Партнёр с
              {{ formatDate(partner.registrationDate) }}
            </p>
          </div>

          <!-- Quick nav buttons -->
          <div class="flex flex-wrap gap-3 shrink-0">
            <router-link
              to="/partner/bookings"
              class="inline-flex items-center gap-2 px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 text-white font-semibold shadow-lg shadow-emerald-500/20 transition-colors"
            >
              Финансы и брони
            </router-link>
            <router-link
              to="/partner/cars"
              class="inline-flex items-center gap-2 px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-emerald-500 transition-colors"
            >
              Мои машины
            </router-link>
          </div>
        </div>
      </header>

      <!-- ── Loading / Error ─────────────────────────────────────────────── -->
      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-10 text-center text-gray-500 dark:text-gray-400"
      >
        Загружаем профиль партнёра...
      </div>

      <div
        v-else-if="errorMessage"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-8 text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </div>

      <template v-else-if="partner">
        <!-- ── Wallet stats strip ───────────────────────────────────────── -->
        <section class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
          <article
            class="rounded-3xl border border-emerald-200/70 dark:border-emerald-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-400 font-bold"
            >
              Доступный баланс
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.availableAmount) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Можно выводить
            </p>
          </article>

          <article
            class="rounded-3xl border border-amber-200/70 dark:border-amber-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-amber-600 dark:text-amber-400 font-bold"
            >
              Pending
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.pendingAmount) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Ещё не доступно
            </p>
          </article>

          <article
            class="rounded-3xl border border-blue-200/70 dark:border-blue-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-blue-600 dark:text-blue-400 font-bold"
            >
              Reserved
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.reservedAmount) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">Под выплату</p>
          </article>

          <article
            class="rounded-3xl border border-violet-200/70 dark:border-violet-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-violet-600 dark:text-violet-400 font-bold"
            >
              Партнёрство до
            </p>
            <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
              {{ formatDate(partner.partnershipEndDate) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Дата окончания
            </p>
          </article>
        </section>

        <!-- ── Partner info + documents ───────────────────────────────── -->
        <div class="grid lg:grid-cols-2 gap-6">
          <!-- Info card -->
          <section
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6"
          >
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              Данные партнёра
            </h2>

            <div class="grid grid-cols-2 gap-x-6 gap-y-5">
              <div>
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  Имя
                </p>
                <p class="mt-1 font-semibold text-gray-900 dark:text-white">
                  {{ partner.ownerFirstName }}
                </p>
              </div>
              <div>
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  Фамилия
                </p>
                <p class="mt-1 font-semibold text-gray-900 dark:text-white">
                  {{ partner.ownerLastName }}
                </p>
              </div>
              <div>
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  Телефон
                </p>
                <p class="mt-1 font-semibold text-gray-900 dark:text-white">
                  {{ partner.phoneNumber }}
                </p>
              </div>
              <div>
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  Дата регистрации
                </p>
                <p class="mt-1 font-semibold text-gray-900 dark:text-white">
                  {{ formatDate(partner.registrationDate) }}
                </p>
              </div>
              <div class="col-span-2">
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  RelatedUserId
                </p>
                <p
                  class="mt-1 text-xs font-mono text-gray-600 dark:text-gray-400 break-all"
                >
                  {{ partner.relatedUserId }}
                </p>
              </div>
              <div>
                <p
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-400 dark:text-gray-500"
                >
                  Создан
                </p>
                <p class="mt-1 font-semibold text-gray-900 dark:text-white">
                  {{ formatDateTime(partner.createdOn) }}
                </p>
              </div>
            </div>
          </section>

          <!-- Documents card -->
          <section
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6"
          >
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              Документы
            </h2>

            <div class="space-y-4">
              <!-- Identity doc -->
              <div
                class="flex items-center justify-between p-4 rounded-2xl bg-gray-50 dark:bg-gray-800/50 border border-gray-100 dark:border-gray-800"
              >
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-xl bg-gray-200 dark:bg-gray-700 flex items-center justify-center"
                  >
                    <svg
                      class="w-5 h-5 text-gray-500"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2"
                      />
                    </svg>
                  </div>
                  <span
                    class="font-medium text-gray-900 dark:text-white text-sm"
                    >Удостоверение владельца</span
                  >
                </div>
                <button
                  :disabled="openingIdentityDocument"
                  @click="
                    openDocument(partner.ownerIdentityFileName, 'identity')
                  "
                  class="px-3 py-1.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition-colors disabled:opacity-60"
                >
                  {{ openingIdentityDocument ? "Открытие..." : "Открыть" }}
                </button>
              </div>

              <!-- Contract -->
              <div
                class="flex items-center justify-between p-4 rounded-2xl bg-gray-50 dark:bg-gray-800/50 border border-gray-100 dark:border-gray-800"
              >
                <div class="flex items-center gap-3">
                  <div
                    class="w-9 h-9 rounded-xl bg-gray-200 dark:bg-gray-700 flex items-center justify-center"
                  >
                    <svg
                      class="w-5 h-5 text-gray-500"
                      fill="none"
                      stroke="currentColor"
                      viewBox="0 0 24 24"
                    >
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                      />
                    </svg>
                  </div>
                  <span
                    class="font-medium text-gray-900 dark:text-white text-sm"
                    >Файл контракта</span
                  >
                </div>
                <button
                  v-if="partner.contractFileName"
                  :disabled="openingContractDocument"
                  @click="openDocument(partner.contractFileName, 'contract')"
                  class="px-3 py-1.5 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold transition-colors disabled:opacity-60"
                >
                  {{ openingContractDocument ? "Открытие..." : "Открыть" }}
                </button>
                <span
                  v-else
                  class="px-3 py-1.5 rounded-xl bg-gray-100 dark:bg-gray-800 text-gray-400 text-xs font-bold"
                >
                  Не загружен
                </span>
              </div>
            </div>
          </section>
        </div>

        <!-- ── Recent bookings ─────────────────────────────────────────── -->
        <section
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
        >
          <div
            class="p-6 sm:p-8 border-b border-gray-100 dark:border-gray-800 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4"
          >
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              Последние бронирования клиентов
            </h2>
            <router-link
              to="/partner/bookings"
              class="text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:underline"
            >
              Все бронирования →
            </router-link>
          </div>

          <div
            v-if="bookingsLoading"
            class="p-8 text-center text-gray-500 dark:text-gray-400"
          >
            Загружаем бронирования...
          </div>

          <div
            v-else-if="recentBookings.length === 0"
            class="p-8 text-center text-gray-400 dark:text-gray-500 border-2 border-dashed border-gray-200 dark:border-gray-800 m-6 rounded-2xl"
          >
            Бронирований пока нет
          </div>

          <table v-else class="w-full text-sm">
            <thead class="bg-gray-50 dark:bg-gray-800/50">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Автомобиль
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden sm:table-cell"
                >
                  Период
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden md:table-cell"
                >
                  Сумма
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Статус
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="b in recentBookings"
                :key="b.id"
                class="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
              >
                <td class="px-6 py-4 font-medium text-gray-900 dark:text-white">
                  {{ b.carBrand }} {{ b.carModel }}
                </td>
                <td
                  class="px-6 py-4 text-gray-600 dark:text-gray-400 hidden sm:table-cell"
                >
                  {{ formatDateShort(b.startTime) }} —
                  {{ formatDateShort(b.endTime) }}
                </td>
                <td
                  class="px-6 py-4 font-semibold text-gray-900 dark:text-white hidden md:table-cell"
                >
                  {{ b.totalPrice != null ? formatMoney(b.totalPrice) : "—" }}
                </td>
                <td class="px-6 py-4">
                  <span
                    :class="statusClass(b.status)"
                    class="px-2.5 py-1 rounded-full text-xs font-bold uppercase tracking-wide"
                  >
                    {{ statusLabel(b.status) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- ── Recent ledger ───────────────────────────────────────────── -->
        <section
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
        >
          <div
            class="p-6 sm:p-8 border-b border-gray-100 dark:border-gray-800 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4"
          >
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              Последние транзакции
            </h2>
            <router-link
              to="/partner/bookings"
              class="text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:underline"
            >
              Все транзакции →
            </router-link>
          </div>

          <div
            v-if="ledgerLoading"
            class="p-8 text-center text-gray-500 dark:text-gray-400"
          >
            Загружаем транзакции...
          </div>

          <div
            v-else-if="recentLedger.length === 0"
            class="p-8 text-center text-gray-400 dark:text-gray-500 border-2 border-dashed border-gray-200 dark:border-gray-800 m-6 rounded-2xl"
          >
            Транзакций пока нет
          </div>

          <table v-else class="w-full text-sm">
            <thead class="bg-gray-50 dark:bg-gray-800/50">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Дата
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden sm:table-cell"
                >
                  Описание
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Сумма
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden md:table-cell"
                >
                  Тип
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="entry in recentLedger"
                :key="entry.id"
                class="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
              >
                <td class="px-6 py-4 text-gray-600 dark:text-gray-400">
                  {{ formatDateShort(entry.createdAt) }}
                </td>
                <td
                  class="px-6 py-4 text-gray-700 dark:text-gray-300 hidden sm:table-cell"
                >
                  {{ entry.description ?? entry.entryType }}
                </td>
                <td
                  class="px-6 py-4 font-bold"
                  :class="
                    entry.amountDelta >= 0
                      ? 'text-emerald-600 dark:text-emerald-400'
                      : 'text-red-600 dark:text-red-400'
                  "
                >
                  {{ entry.amountDelta >= 0 ? "+" : ""
                  }}{{ formatMoney(entry.amountDelta) }}
                </td>
                <td class="px-6 py-4 hidden md:table-cell">
                  <span
                    :class="bucketClass(entry.bucket)"
                    class="px-2.5 py-1 rounded-full text-xs font-bold uppercase tracking-wide"
                  >
                    {{ entry.bucket }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from "axios";
import { computed, onMounted, ref } from "vue";
import {
  getMyPartner,
  getMyPartnerWallet,
  getMyPartnerBookings,
  getMyPartnerLedger,
  getMyPartnerFileTemporaryLink,
} from "../api/partners";
import { useToast } from "../composables/useToast";
import type {
  Partner,
  PartnerBooking,
  PartnerLedgerEntry,
  PartnerWallet,
} from "../types/Partner";

const { error } = useToast();

// ─── State ────────────────────────────────────────────────────────────────────
const loading = ref(true);
const bookingsLoading = ref(true);
const ledgerLoading = ref(true);
const errorMessage = ref("");

const partner = ref<Partner | null>(null);
const wallet = ref<PartnerWallet>(emptyWallet());
const recentBookings = ref<PartnerBooking[]>([]);
const recentLedger = ref<PartnerLedgerEntry[]>([]);
const openingIdentityDocument = ref(false);
const openingContractDocument = ref(false);

// ─── Computed ─────────────────────────────────────────────────────────────────
const initials = computed(() => {
  if (!partner.value) return "P";
  return (
    (partner.value.ownerFirstName[0] ?? "") +
    (partner.value.ownerLastName[0] ?? "")
  ).toUpperCase();
});

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  loading.value = true;
  try {
    const [partnerResult] = await Promise.all([getMyPartner(), loadWallet()]);
    partner.value = partnerResult;
  } catch (e: any) {
    errorMessage.value =
      e?.response?.status === 404
        ? "Партнёрский профиль не найден."
        : "Не удалось загрузить профиль партнёра.";
  } finally {
    loading.value = false;
  }

  await Promise.all([loadBookings(), loadLedger()]);
});

// ─── Loaders ──────────────────────────────────────────────────────────────────
async function loadWallet() {
  try {
    wallet.value = await getMyPartnerWallet();
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 404) return;
    throw e;
  }
}

async function loadBookings() {
  try {
    bookingsLoading.value = true;
    const all = await getMyPartnerBookings();
    recentBookings.value = all.slice(0, 5);
  } catch {
    // non-critical
  } finally {
    bookingsLoading.value = false;
  }
}

async function loadLedger() {
  try {
    ledgerLoading.value = true;
    const all = await getMyPartnerLedger(5);
    recentLedger.value = all;
  } catch {
    // non-critical
  } finally {
    ledgerLoading.value = false;
  }
}

// ─── Document open ────────────────────────────────────────────────────────────
async function openDocument(
  fileName: string | null | undefined,
  type: "identity" | "contract",
) {
  const name = (fileName ?? "").trim();
  if (!name) {
    error("Документ не найден.");
    return;
  }

  if (type === "identity") openingIdentityDocument.value = true;
  else openingContractDocument.value = true;

  try {
    const link = await getMyPartnerFileTemporaryLink(name);
    const w = window.open(link.url, "_blank", "noopener,noreferrer");
    if (!w) window.location.href = link.url;
  } catch (e: any) {
    error(e?.response?.data?.error ?? "Не удалось открыть документ.");
  } finally {
    if (type === "identity") openingIdentityDocument.value = false;
    else openingContractDocument.value = false;
  }
}

// ─── Formatting ───────────────────────────────────────────────────────────────
function emptyWallet(): PartnerWallet {
  return {
    partnerUserId: "",
    currency: "KZT",
    pendingAmount: 0,
    availableAmount: 0,
    reservedAmount: 0,
    updatedAt: "",
  };
}

function formatMoney(amount: number) {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: wallet.value.currency || "KZT",
    maximumFractionDigits: 0,
  }).format(amount);
}

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
    });
  } catch {
    return iso;
  }
}

function formatDateTime(iso: string) {
  try {
    return new Date(iso).toLocaleString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  } catch {
    return iso;
  }
}

function formatDateShort(iso: string) {
  try {
    return new Date(iso).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "2-digit",
    });
  } catch {
    return iso;
  }
}

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    pending: "Ожидает",
    confirmed: "Подтверждён",
    active: "Активен",
    completed: "Завершён",
    canceled: "Отменён",
  };
  return map[status] ?? status;
}

function statusClass(status: string): string {
  const map: Record<string, string> = {
    pending:
      "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400",
    confirmed:
      "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400",
    active:
      "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400",
    completed: "bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400",
    canceled: "bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-600";
}

function bucketClass(bucket: string): string {
  const b = bucket?.toLowerCase();
  if (b === "available")
    return "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400";
  if (b === "pending")
    return "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400";
  if (b === "reserved")
    return "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400";
  return "bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400";
}
</script>
