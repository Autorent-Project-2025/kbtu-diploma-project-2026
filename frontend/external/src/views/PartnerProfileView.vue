<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-4xl mx-auto space-y-8">
      <div class="space-y-3 animate-slide-up">
        <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white">Профиль партнера</h1>
        <p class="text-lg text-gray-600 dark:text-gray-400">
          Информация о вашем партнерском аккаунте
        </p>
      </div>

      <div
        v-if="loading"
        class="glass p-6 rounded-2xl border border-gray-200 dark:border-gray-800 text-gray-700 dark:text-gray-300"
      >
        Загрузка...
      </div>

      <div
        v-else-if="errorMessage"
        class="glass p-6 rounded-2xl border border-red-300/70 dark:border-red-500/30 text-red-700 dark:text-red-300 space-y-4"
      >
        <p>{{ errorMessage }}</p>
        <router-link
          to="/cars"
          class="inline-flex items-center gap-2 text-primary-600 dark:text-primary-400 font-semibold"
        >
          Перейти к автомобилям
        </router-link>
      </div>

      <div
        v-else-if="partner"
        class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-6"
      >
        <div class="grid md:grid-cols-[1.1fr,0.9fr] gap-6">
          <div class="rounded-3xl border border-emerald-200/70 dark:border-emerald-700/40 bg-white dark:bg-gray-900 p-6 shadow-lg space-y-3">
            <p class="text-sm uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-400 font-bold">
              Доступный баланс
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(wallet.availableAmount) }}
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Pending: {{ formatMoney(wallet.pendingAmount) }} · Reserved: {{ formatMoney(wallet.reservedAmount) }}
            </p>
          </div>

          <div class="flex flex-wrap items-start justify-end gap-3">
            <router-link
              to="/partner/bookings"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-emerald-600 hover:bg-emerald-700 text-white font-semibold transition-colors"
            >
              Финансы и бронирования
            </router-link>
            <router-link
              to="/partner/cars"
              class="inline-flex items-center gap-2 px-4 py-2 rounded-xl bg-primary-600 hover:bg-primary-700 text-white font-semibold transition-colors"
            >
              Мои машины
            </router-link>
          </div>
        </div>

        <div class="grid sm:grid-cols-2 gap-6">
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Имя владельца</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ partner.ownerFirstName }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Фамилия владельца</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ partner.ownerLastName }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Телефон</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ partner.phoneNumber }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">RelatedUserId</p>
            <p class="text-sm font-mono text-gray-900 dark:text-white break-all">{{ partner.relatedUserId }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Дата регистрации</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ formatDate(partner.registrationDate) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Дата окончания партнерства</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ formatDate(partner.partnershipEndDate) }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Удостоверение владельца</p>
            <button
              type="button"
              class="inline-flex items-center gap-2 px-3 py-2 rounded-lg bg-primary-600 hover:bg-primary-700 text-white text-sm font-semibold transition-colors disabled:opacity-60"
              :disabled="openingIdentityDocument"
              @click="openDocument(partner.ownerIdentityFileName, 'identity')"
            >
              {{ openingIdentityDocument ? "Открытие..." : "Посмотреть документ" }}
            </button>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Файл контракта</p>
            <button
              v-if="partner.contractFileName"
              type="button"
              class="inline-flex items-center gap-2 px-3 py-2 rounded-lg bg-primary-600 hover:bg-primary-700 text-white text-sm font-semibold transition-colors disabled:opacity-60"
              :disabled="openingContractDocument"
              @click="openDocument(partner.contractFileName, 'contract')"
            >
              {{ openingContractDocument ? "Открытие..." : "Посмотреть документ" }}
            </button>
            <p v-else class="text-lg font-semibold text-gray-900 dark:text-white">Не загружен</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Создано</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ formatDateTime(partner.createdOn) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import axios from "axios";
import { onMounted, ref } from "vue";
import {
  getMyPartner,
  getMyPartnerFileTemporaryLink,
  getMyPartnerWallet,
} from "../api/partners";
import { useToast } from "../composables/useToast";
import type { Partner, PartnerWallet } from "../types/Partner";

const { error } = useToast();
const loading = ref(true);
const errorMessage = ref("");
const partner = ref<Partner | null>(null);
const wallet = ref<PartnerWallet>(createEmptyWallet());
const openingIdentityDocument = ref(false);
const openingContractDocument = ref(false);

function createEmptyWallet(): PartnerWallet {
  return {
    partnerUserId: "",
    currency: "KZT",
    pendingAmount: 0,
    availableAmount: 0,
    reservedAmount: 0,
    updatedAt: new Date().toISOString(),
  };
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat("ru-RU", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).format(date);
}

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat("ru-RU", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function formatMoney(amount: number, currency = wallet.value.currency || "KZT") {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    maximumFractionDigits: 2,
  }).format(amount);
}

function openTemporaryLink(url: string) {
  const openedWindow = window.open(url, "_blank", "noopener,noreferrer");
  if (!openedWindow) {
    window.location.href = url;
  }
}

async function openDocument(fileName: string | null | undefined, documentType: "identity" | "contract") {
  const normalizedFileName = (fileName ?? "").trim();
  if (!normalizedFileName) {
    error("Документ не найден.");
    return;
  }

  const isIdentity = documentType === "identity";
  if (isIdentity) {
    openingIdentityDocument.value = true;
  } else {
    openingContractDocument.value = true;
  }

  try {
    const temporaryLink = await getMyPartnerFileTemporaryLink(normalizedFileName);
    openTemporaryLink(temporaryLink.url);
  } catch (e: any) {
    error(
      e?.response?.data?.error ||
      e?.response?.data?.message ||
      e?.response?.data?.detail ||
      "Не удалось открыть документ."
    );
  } finally {
    if (isIdentity) {
      openingIdentityDocument.value = false;
    } else {
      openingContractDocument.value = false;
    }
  }
}

async function loadWallet() {
  try {
    wallet.value = await getMyPartnerWallet();
  } catch (e) {
    if (axios.isAxiosError(e) && e.response?.status === 404) {
      wallet.value = createEmptyWallet();
      return;
    }

    throw e;
  }
}

onMounted(async () => {
  loading.value = true;
  errorMessage.value = "";

  try {
    const [partnerResult] = await Promise.all([
      getMyPartner(),
      loadWallet(),
    ]);

    partner.value = partnerResult;
  } catch (error: any) {
    if (error?.response?.status === 404) {
      errorMessage.value = "Партнерский профиль для текущего пользователя не найден.";
    } else {
      errorMessage.value = "Не удалось загрузить профиль партнера.";
    }
  } finally {
    loading.value = false;
  }
});
</script>
