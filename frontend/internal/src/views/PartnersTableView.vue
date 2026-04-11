<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Партнёры</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {{ loading ? "Загрузка..." : `${partners.length} партнёров` }}
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

        <!-- Search -->
        <div class="mt-4 relative">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          <input
            v-model="search"
            type="text"
            placeholder="Поиск по имени, телефону..."
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-colors"
          />
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 5" :key="i" class="h-16 bg-white dark:bg-gray-900 rounded-xl animate-pulse" />
      </div>

      <!-- Empty -->
      <div v-else-if="filtered.length === 0" class="text-center py-20">
        <svg class="w-12 h-12 mx-auto text-gray-300 dark:text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
        </svg>
        <p class="text-gray-500 dark:text-gray-400 font-medium">Партнёры не найдены</p>
        <p class="text-sm text-gray-400 dark:text-gray-500 mt-1">Попробуйте изменить параметры поиска</p>
      </div>

      <!-- Table -->
      <div v-else class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
        <table class="w-full">
          <thead>
            <tr class="border-b border-gray-100 dark:border-gray-800">
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Партнёр</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Телефон</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Регистрация</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Окончание</th>
              <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Создан</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
            <tr
              v-for="p in filtered"
              :key="p.id"
              @click="$router.push(`/partners/${p.id}`)"
              class="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
            >
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="w-9 h-9 rounded-full bg-violet-100 dark:bg-violet-900/30 flex items-center justify-center text-violet-700 dark:text-violet-400 text-xs font-bold flex-shrink-0">
                    {{ initials(p) }}
                  </div>
                  <div>
                    <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ p.ownerFirstName }} {{ p.ownerLastName }}</p>
                    <p class="text-xs text-gray-400">ID: {{ p.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ p.phoneNumber || "—" }}</td>
              <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ formatDate(p.registrationDate) }}</td>
              <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ formatDate(p.partnershipEndDate) }}</td>
              <td class="px-6 py-4 text-sm text-gray-400">{{ formatDate(p.createdOn) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { getPartners, type PartnerDto } from "../api/partners";
import { formatDate } from "../utils/formatters";
import { useToast } from "../composables/useToast";

const toast = useToast();
const partners = ref<PartnerDto[]>([]);
const loading = ref(true);
const search = ref("");

const filtered = computed(() => {
  if (!search.value.trim()) return partners.value;
  const q = search.value.toLowerCase();
  return partners.value.filter(
    (p) =>
      p.ownerFirstName?.toLowerCase().includes(q) ||
      p.ownerLastName?.toLowerCase().includes(q) ||
      p.phoneNumber?.toLowerCase().includes(q) ||
      String(p.id).includes(q),
  );
});

function initials(p: PartnerDto): string {
  return ((p.ownerFirstName?.[0] ?? "") + (p.ownerLastName?.[0] ?? "")).toUpperCase();
}

async function reload() {
  loading.value = true;
  try {
    partners.value = await getPartners();
  } catch (e: any) {
    toast.error("Ошибка загрузки партнёров: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

onMounted(reload);
</script>
