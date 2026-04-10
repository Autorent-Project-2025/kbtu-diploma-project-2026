<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(59,130,246,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(16,185,129,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex items-center gap-4">
        <router-link
          to="/clients"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-blue-500 transition-colors"
        >
          Назад
        </router-link>
        <div class="space-y-1">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            {{ loading ? "Загрузка..." : `${form.firstName} ${form.lastName}` }}
          </h1>
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Клиент не найден.
    </div>

    <!-- Form -->
    <template v-else>
      <form @submit.prevent="onSave" class="space-y-6">
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Основная информация</h2>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Имя</label>
              <input
                v-model="form.firstName"
                type="text"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Фамилия</label>
              <input
                v-model="form.lastName"
                type="text"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Телефон</label>
              <input
                v-model="form.phoneNumber"
                type="tel"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Дата рождения</label>
              <input
                v-model="form.birthDate"
                type="date"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>
          </div>
        </div>

        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Документы и связь</h2>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">ID пользователя</label>
              <input
                v-model="form.relatedUserId"
                type="text"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors font-mono text-sm disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Паспорт (файл)</label>
              <input
                v-model="form.identityDocumentFileName"
                type="text"
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="Не загружен"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Водительские права (файл)</label>
              <input
                v-model="form.driverLicenseFileName"
                type="text"
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20 transition-colors text-sm disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="Не загружены"
              />
            </div>
          </div>
        </div>

        <!-- Status info (read-only) -->
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Статус</h2>
          <div class="flex items-center gap-4">
            <span
              :class="[
                'px-3 py-1 rounded-full text-sm font-semibold',
                originalClient?.bookingActionsBlocked
                  ? 'bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300'
                  : 'bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300',
              ]"
            >
              {{ originalClient?.bookingActionsBlocked ? "Заблокирован" : "Активен" }}
            </span>
            <span v-if="originalClient?.bookingBlockReason" class="text-sm text-gray-500 dark:text-gray-400">
              {{ originalClient.bookingBlockReason }}
            </span>
          </div>
          <p v-if="originalClient?.createdOn" class="text-sm text-gray-500 dark:text-gray-400">
            Дата регистрации: {{ formatDateTime(originalClient.createdOn) }}
          </p>
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-3">
          <button
            v-if="canUpdate"
            type="submit"
            :disabled="saving"
            class="px-6 py-3 rounded-2xl bg-blue-600 hover:bg-blue-700 disabled:bg-gray-300 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-blue-500/20 transition-colors"
          >
            {{ saving ? "Сохранение..." : "Сохранить" }}
          </button>

          <button
            v-if="canDelete"
            type="button"
            @click="onDelete"
            :disabled="deleting"
            class="px-6 py-3 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-bold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ deleting ? "Удаление..." : "Удалить" }}
          </button>
        </div>
      </form>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getClient, updateClient, deleteClient, type ClientDto } from "../api/clients";
import { auth } from "../store/auth";
import { useToast } from "../composables/useToast";

const route = useRoute();
const router = useRouter();
const toast = useToast();

const loading = ref(false);
const saving = ref(false);
const deleting = ref(false);
const notFound = ref(false);
const originalClient = ref<ClientDto | null>(null);

const canUpdate = computed(() => auth.hasPermission("Client.Update"));
const canDelete = computed(() => auth.hasPermission("Client.Delete"));

const form = reactive({
  firstName: "",
  lastName: "",
  phoneNumber: "",
  birthDate: "",
  relatedUserId: "",
  identityDocumentFileName: "",
  driverLicenseFileName: "",
});

function populateForm(client: ClientDto) {
  form.firstName = client.firstName ?? "";
  form.lastName = client.lastName ?? "";
  form.phoneNumber = client.phoneNumber ?? "";
  form.birthDate = client.birthDate ? client.birthDate.split("T")[0] : "";
  form.relatedUserId = client.relatedUserId ?? "";
  form.identityDocumentFileName = client.identityDocumentFileName ?? "";
  form.driverLicenseFileName = client.driverLicenseFileName ?? "";
}

function formatDateTime(dateStr: string): string {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

async function loadClient() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    const client = await getClient(id);
    originalClient.value = client;
    populateForm(client);
  } catch {
    notFound.value = true;
  } finally {
    loading.value = false;
  }
}

async function onSave() {
  if (saving.value || !originalClient.value) return;
  saving.value = true;

  try {
    const updated = await updateClient(originalClient.value.id, {
      firstName: form.firstName,
      lastName: form.lastName,
      phoneNumber: form.phoneNumber,
      birthDate: form.birthDate,
      relatedUserId: form.relatedUserId,
      identityDocumentFileName: form.identityDocumentFileName || undefined,
      driverLicenseFileName: form.driverLicenseFileName || undefined,
    });
    originalClient.value = updated;
    populateForm(updated);
    toast.success("Клиент успешно обновлён");
  } catch {
    toast.error("Ошибка при сохранении клиента");
  } finally {
    saving.value = false;
  }
}

async function onDelete() {
  if (deleting.value || !originalClient.value) return;
  if (!confirm("Вы уверены, что хотите удалить этого клиента?")) return;

  deleting.value = true;
  try {
    await deleteClient(originalClient.value.id);
    toast.success("Клиент удалён");
    router.push("/clients");
  } catch {
    toast.error("Ошибка при удалении клиента");
  } finally {
    deleting.value = false;
  }
}

onMounted(loadClient);
</script>
