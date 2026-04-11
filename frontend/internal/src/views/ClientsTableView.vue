<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Клиенты</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {{ loading ? "Загрузка..." : `${filtered.length} из ${clients.length} клиентов` }}
            </p>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="p-2.5 rounded-xl border border-gray-200 dark:border-gray-700 text-gray-500 hover:text-gray-900 dark:hover:text-white hover:border-gray-300 transition-colors"
          >
            <svg :class="['w-4 h-4', loading && 'animate-spin']" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
          </button>
        </div>

        <!-- Search + Filters -->
        <div class="mt-4 flex flex-col sm:flex-row gap-3">
          <div class="flex-1 relative">
            <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
            <input
              v-model="search"
              type="text"
              placeholder="Имя, телефон, ID..."
              class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-colors"
            />
          </div>
          <div class="flex gap-2">
            <button
              v-for="f in statusFilters"
              :key="f.value"
              @click="statusFilter = f.value"
              :class="[
                'px-3 py-2 rounded-xl text-xs font-semibold border transition-colors whitespace-nowrap',
                statusFilter === f.value
                  ? 'bg-emerald-600 text-white border-emerald-600'
                  : 'bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400 border-gray-200 dark:border-gray-700 hover:border-gray-300',
              ]"
            >
              {{ f.label }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 6" :key="i" class="h-16 bg-white dark:bg-gray-900 rounded-xl animate-pulse" />
      </div>

      <!-- Empty -->
      <div v-else-if="filtered.length === 0" class="text-center py-20">
        <svg class="w-12 h-12 mx-auto text-gray-300 dark:text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" />
        </svg>
        <p class="text-gray-500 dark:text-gray-400 font-medium">Клиенты не найдены</p>
        <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">Попробуйте изменить параметры поиска</p>
      </div>

      <!-- Table -->
      <div v-else class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
        <table class="w-full">
          <thead>
            <tr class="border-b border-gray-100 dark:border-gray-800">
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Клиент</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Телефон</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Дата рождения</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Документы</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Статус</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Регистрация</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
            <tr
              v-for="c in filtered"
              :key="c.id"
              @click="$router.push(`/clients/${c.id}`)"
              class="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
            >
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-full bg-blue-100 dark:bg-blue-900/30 flex items-center justify-center text-blue-700 dark:text-blue-400 text-xs font-bold flex-shrink-0">
                    {{ (c.firstName?.[0] ?? "") + (c.lastName?.[0] ?? "") }}
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ c.firstName }} {{ c.lastName }}</p>
                    <p class="text-xs text-gray-400">ID: {{ c.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ c.phoneNumber || "—" }}</td>
              <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ formatDate(c.birthDate) }}</td>
              <td class="px-6 py-4">
                <div class="flex gap-1.5">
                  <span v-if="c.identityDocumentFileName" class="px-2 py-0.5 text-xs font-medium rounded-full bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400">Паспорт</span>
                  <span v-if="c.driverLicenseFileName" class="px-2 py-0.5 text-xs font-medium rounded-full bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400">Права</span>
                  <span v-if="!c.identityDocumentFileName && !c.driverLicenseFileName" class="text-xs text-gray-400">—</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <span
                  :class="[
                    'px-2.5 py-1 text-xs font-semibold rounded-full',
                    c.bookingActionsBlocked
                      ? 'bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400'
                      : 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
                  ]"
                >
                  {{ c.bookingActionsBlocked ? "Заблокирован" : "Активен" }}
                </span>
              </td>
              <td class="px-6 py-4 text-sm text-gray-400">{{ formatDate(c.createdOn) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { getClients, type ClientDto } from "../api/clients";
import { formatDate } from "../utils/formatters";
import { useToast } from "../composables/useToast";

const toast = useToast();
const clients = ref<ClientDto[]>([]);
const loading = ref(true);
const search = ref("");
const statusFilter = ref("all");

const statusFilters = [
  { value: "all", label: "Все" },
  { value: "active", label: "Активные" },
  { value: "blocked", label: "Заблокированные" },
];

const filtered = computed(() => {
  let result = clients.value;

  if (statusFilter.value === "active") {
    result = result.filter((c) => !c.bookingActionsBlocked);
  } else if (statusFilter.value === "blocked") {
    result = result.filter((c) => c.bookingActionsBlocked);
  }

  if (search.value.trim()) {
    const q = search.value.toLowerCase();
    result = result.filter(
      (c) =>
        c.firstName?.toLowerCase().includes(q) ||
        c.lastName?.toLowerCase().includes(q) ||
        c.phoneNumber?.toLowerCase().includes(q) ||
        String(c.id).includes(q) ||
        c.relatedUserId?.toLowerCase().includes(q),
    );
  }

  return result;
});

async function reload() {
  loading.value = true;
  try {
    clients.value = await getClients();
  } catch (e: any) {
    toast.error("Ошибка загрузки клиентов: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

onMounted(reload);
</script>
