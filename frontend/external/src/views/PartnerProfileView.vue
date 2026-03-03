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
            <p class="text-lg font-semibold text-gray-900 dark:text-white">{{ partner.ownerIdentityFileName }}</p>
          </div>
          <div>
            <p class="text-sm text-gray-500 dark:text-gray-400">Файл контракта</p>
            <p class="text-lg font-semibold text-gray-900 dark:text-white">
              {{ partner.contractFileName || "Не загружен" }}
            </p>
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
import { onMounted, ref } from "vue";
import { getMyPartner } from "../api/partners";
import type { Partner } from "../types/Partner";

const loading = ref(true);
const errorMessage = ref("");
const partner = ref<Partner | null>(null);

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

onMounted(async () => {
  loading.value = true;
  errorMessage.value = "";

  try {
    partner.value = await getMyPartner();
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
