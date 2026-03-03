<template>
  <div
    class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-950 py-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="w-full max-w-md space-y-8">
      <div class="text-center space-y-2 animate-slide-up">
        <h2 class="text-4xl font-extrabold text-gray-900 dark:text-white">Заявка на регистрацию</h2>
        <p class="text-gray-600 dark:text-gray-400">Заполните данные, и менеджер рассмотрит заявку</p>
      </div>

      <div
        class="glass p-8 rounded-3xl shadow-2xl border border-gray-200 dark:border-gray-800 space-y-6 animate-slide-up"
        style="animation-delay: 0.1s"
      >
        <div
          v-if="submitted"
          class="rounded-xl border border-green-300/80 bg-green-50 p-4 text-green-800 dark:border-green-500/30 dark:bg-green-900/20 dark:text-green-300"
        >
          Ваша заявка отправлена
        </div>

        <form v-else @submit.prevent="onSubmit" class="space-y-6">
          <div class="space-y-2">
            <label for="firstName" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Имя
            </label>
            <input
              id="firstName"
              v-model="firstName"
              type="text"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="lastName" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Фамилия
            </label>
            <input
              id="lastName"
              v-model="lastName"
              type="text"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="email" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Email
            </label>
            <input
              id="email"
              v-model="email"
              type="email"
              required
              class="w-full px-4 py-3.5 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-primary-500 dark:focus:border-primary-400 focus:ring-4 focus:ring-primary-500/10 transition-all duration-200"
            />
          </div>

          <div class="space-y-2">
            <label for="identityDocumentFile" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Удостоверение личности (PDF)
            </label>
            <input
              id="identityDocumentFile"
              type="file"
              accept="application/pdf,.pdf"
              required
              @change="onIdentityFileChange"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white file:mr-3 file:rounded-lg file:border-0 file:bg-primary-600 file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-primary-700"
            />
          </div>

          <div class="space-y-2">
            <label for="driverLicenseFile" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Водительские права (PDF)
            </label>
            <input
              id="driverLicenseFile"
              type="file"
              accept="application/pdf,.pdf"
              required
              @change="onDriverLicenseFileChange"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white file:mr-3 file:rounded-lg file:border-0 file:bg-primary-600 file:px-3 file:py-2 file:text-sm file:font-semibold file:text-white hover:file:bg-primary-700"
            />
          </div>

          <div class="space-y-2">
            <label for="birthDate" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Дата рождения
            </label>
            <input
              id="birthDate"
              v-model="birthDate"
              type="date"
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
              Уже есть аккаунт?
            </span>
          </div>
        </div>

        <div class="text-center">
          <router-link
            to="/login"
            class="text-primary-600 dark:text-primary-400 hover:text-primary-700 dark:hover:text-primary-300 font-semibold transition-colors"
          >
            Войти
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { createTicket } from "../api/tickets";
import { useToast } from "../composables/useToast";

const firstName = ref("");
const lastName = ref("");
const email = ref("");
const birthDate = ref("");
const phoneNumber = ref("");
const identityDocumentFile = ref<File | null>(null);
const driverLicenseFile = ref<File | null>(null);
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
    identityDocumentFile.value = null;
    error("Файл удостоверения должен быть PDF.");
    return;
  }

  identityDocumentFile.value = file;
}

function onDriverLicenseFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0] ?? null;

  if (file && !isPdfFile(file)) {
    input.value = "";
    driverLicenseFile.value = null;
    error("Файл прав должен быть PDF.");
    return;
  }

  driverLicenseFile.value = file;
}

async function onSubmit() {
  if (loading.value || submitted.value) return;
  if (!identityDocumentFile.value || !driverLicenseFile.value) {
    error("Загрузите оба документа в формате PDF.");
    return;
  }

  loading.value = true;
  try {
    await createTicket({
      firstName: firstName.value.trim(),
      lastName: lastName.value.trim(),
      email: email.value.trim(),
      birthDate: birthDate.value,
      phoneNumber: phoneNumber.value.trim(),
      identityDocumentFile: identityDocumentFile.value,
      driverLicenseFile: driverLicenseFile.value
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
