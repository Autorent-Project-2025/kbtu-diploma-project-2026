<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="w-full max-w-md space-y-8">
      <div class="text-center space-y-2 animate-slide-up">
        <h2 class="text-4xl font-extrabold text-gray-900 dark:text-white">Заявка партнера</h2>
        <p class="text-gray-600 dark:text-gray-400">Заполните данные владельца, и менеджер рассмотрит заявку</p>
      </div>

      <div
        class="glass p-8 rounded-3xl shadow-2xl border border-gray-200 dark:border-gray-800 space-y-6 animate-slide-up"
        style="animation-delay: 0.1s"
      >
        <div
          v-if="submitted"
          class="rounded-xl border border-green-300/80 bg-green-50 p-4 text-green-800 dark:border-green-500/30 dark:bg-green-900/20 dark:text-green-300"
        >
          Ваша партнерская заявка отправлена
        </div>

        <form v-else @submit.prevent="onSubmit" class="space-y-6">
          <div class="space-y-2">
            <label for="ownerFirstName" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Имя владельца
            </label>
            <input
              id="ownerFirstName"
              v-model="ownerFirstName"
              type="text"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="ownerLastName" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Фамилия владельца
            </label>
            <input
              id="ownerLastName"
              v-model="ownerLastName"
              type="text"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="ownerEmail" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Email владельца
            </label>
            <input
              id="ownerEmail"
              v-model="ownerEmail"
              type="email"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="phoneNumber" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Номер телефона
            </label>
            <input
              id="phoneNumber"
              v-model="phoneNumber"
              type="tel"
              required
              placeholder="+77011234567"
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="ownerIdentityFile" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Удостоверение владельца (PDF)
            </label>
            <input
              id="ownerIdentityFile"
              type="file"
              accept="application/pdf,.pdf"
              required
              @change="onIdentityFileChange"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white file:mr-3 file:rounded-lg file:border-0 file:bg-primary-600 file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-primary-700"
            />
          </div>

          <button
            type="submit"
            :disabled="loading"
            class="w-full btn-premium flex items-center justify-center gap-2"
          >
            <span v-if="!loading">Отправить заявку</span>
            <span v-else>Отправка...</span>
          </button>
        </form>

        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-200 dark:border-gray-700"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-4 bg-white dark:bg-gray-900 text-gray-500 dark:text-gray-400">
              Уже зарегистрированы?
            </span>
          </div>
        </div>

        <div class="text-center">
          <router-link
            to="/login"
            class="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 font-semibold transition-colors"
          >
            Я уже зарегистрирован
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { createPartnerTicket } from "../api/tickets";
import { useToast } from "../composables/useToast";

const ownerFirstName = ref("");
const ownerLastName = ref("");
const ownerEmail = ref("");
const phoneNumber = ref("");
const ownerIdentityFile = ref<File | null>(null);
const loading = ref(false);
const submitted = ref(false);
const { error } = useToast();

function isPdfFile(file: File): boolean {
  const normalizedName = file.name.toLowerCase();
  return file.type === "application/pdf" || normalizedName.endsWith(".pdf");
}

function onIdentityFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0] ?? null;

  if (file && !isPdfFile(file)) {
    input.value = "";
    ownerIdentityFile.value = null;
    error("Файл удостоверения должен быть PDF.");
    return;
  }

  ownerIdentityFile.value = file;
}

async function onSubmit() {
  if (loading.value || submitted.value) return;
  if (!ownerIdentityFile.value) {
    error("Загрузите удостоверение в формате PDF.");
    return;
  }

  loading.value = true;
  try {
    await createPartnerTicket({
      ownerFirstName: ownerFirstName.value.trim(),
      ownerLastName: ownerLastName.value.trim(),
      ownerEmail: ownerEmail.value.trim(),
      phoneNumber: phoneNumber.value.trim(),
      ownerIdentityFile: ownerIdentityFile.value,
    });
    submitted.value = true;
  } catch (e: any) {
    const errorMsg =
      e?.response?.data?.error || e?.response?.data?.message || "Не удалось отправить заявку.";
    error(errorMsg);
  } finally {
    loading.value = false;
  }
}
</script>
