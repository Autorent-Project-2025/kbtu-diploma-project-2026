<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Машины</h1>
            <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
              {{ loading ? "Загрузка..." : `${totalCount} машин` }}
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

        <!-- Filters -->
        <div class="mt-4 flex flex-wrap gap-2">
          <button
            v-for="f in statusFilters"
            :key="f.value"
            @click="setStatusFilter(f.value)"
            :class="[
              'px-3 py-2 rounded-xl text-xs font-semibold border transition-colors',
              activeStatusFilter === f.value
                ? 'bg-emerald-600 text-white border-emerald-600'
                : 'bg-white dark:bg-gray-800 text-gray-600 dark:text-gray-400 border-gray-200 dark:border-gray-700 hover:border-gray-300',
            ]"
          >
            {{ f.label }}
          </button>
        </div>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-3">
        <div v-for="i in 6" :key="i" class="h-16 bg-white dark:bg-gray-900 rounded-xl animate-pulse" />
      </div>

      <!-- Empty -->
      <div v-else-if="cars.length === 0" class="text-center py-20">
        <svg class="w-12 h-12 mx-auto text-gray-300 dark:text-gray-600 mb-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 17h.01M16 17h.01M3 11l1.5-5A2 2 0 016.4 4h11.2a2 2 0 011.9 1.4L21 11M3 11v6a1 1 0 001 1h1m16-7v6a1 1 0 01-1 1h-1M3 11h18" />
        </svg>
        <p class="text-gray-500 dark:text-gray-400 font-medium">Машины не найдены</p>
      </div>

      <!-- Table -->
      <template v-else>
        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
          <table class="w-full">
            <thead>
              <tr class="border-b border-gray-100 dark:border-gray-800">
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">ID</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Марка / Модель</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Гос. номер</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Год</th>
                <th class="text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Цена/ч</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Рейтинг</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Статус</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Теги</th>
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Добавлена</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              <tr
                v-for="car in cars"
                :key="car.id"
                @click="$router.push(`/cars/${car.id}`)"
                class="hover:bg-gray-50 dark:hover:bg-gray-800/50 cursor-pointer transition-colors"
              >
                <td class="px-6 py-4 text-sm font-medium text-gray-900 dark:text-white">{{ car.id }}</td>
                <td class="px-6 py-4">
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ car.modelBrand }} {{ car.modelName }}</p>
                </td>
                <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400 font-mono">{{ car.licensePlate }}</td>
                <td class="px-6 py-4 text-sm text-gray-600 dark:text-gray-400">{{ car.modelYear }}</td>
                <td class="px-6 py-4 text-sm font-semibold text-gray-900 dark:text-white text-right">{{ formatPrice(car.priceHour) }}</td>
                <td class="px-6 py-4">
                  <div v-if="car.rating" class="flex items-center gap-1">
                    <svg class="w-3.5 h-3.5 text-amber-500" fill="currentColor" viewBox="0 0 20 20"><path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"/></svg>
                    <span class="text-sm text-gray-700 dark:text-gray-300">{{ car.rating.toFixed(1) }}</span>
                    <span class="text-xs text-gray-400">({{ car.ratingsCount }})</span>
                  </div>
                  <span v-else class="text-xs text-gray-400">—</span>
                </td>
                <td class="px-6 py-4">
                  <span :class="['px-2.5 py-1 text-xs font-semibold rounded-full', carStatusBadge(car.status)]">
                    {{ carStatusLabel(car.status) }}
                  </span>
                </td>
                <td class="px-6 py-4">
                  <div class="flex flex-wrap gap-1">
                    <span
                      v-for="tag in car.commercialBadgeKeys"
                      :key="tag"
                      class="px-2 py-0.5 text-xs font-medium rounded-full bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400"
                    >
                      {{ tag }}
                    </span>
                    <span v-if="!car.commercialBadgeKeys?.length" class="text-xs text-gray-400">—</span>
                  </div>
                </td>
                <td class="px-6 py-4 text-sm text-gray-400">{{ formatDateTime(car.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div class="mt-4 flex items-center justify-between">
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Страница {{ page }} из {{ totalPages }}
          </p>
          <div class="flex gap-2">
            <button
              @click="goToPage(page - 1)"
              :disabled="page <= 1"
              class="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:border-gray-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Назад
            </button>
            <button
              @click="goToPage(page + 1)"
              :disabled="page >= totalPages"
              class="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:border-gray-300 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
            >
              Вперёд
            </button>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from "vue";
import { getPartnerCars, type PartnerCarDto } from "../api/cars";
import { formatDateTime, formatPrice } from "../utils/formatters";
import { carStatusLabel, carStatusBadge } from "../utils/statusMaps";
import { useToast } from "../composables/useToast";

const toast = useToast();
const cars = ref<PartnerCarDto[]>([]);
const loading = ref(true);
const page = ref(1);
const totalCount = ref(0);
const totalPages = ref(1);
const pageSize = 20;
const activeStatusFilter = ref("all");

const statusFilters = [
  { value: "all", label: "Все" },
  { value: "0", label: "На модерации" },
  { value: "1", label: "Активные" },
  { value: "2", label: "Неактивные" },
  { value: "3", label: "Заблокированные" },
];

function setStatusFilter(value: string) {
  activeStatusFilter.value = value;
  page.value = 1;
  reload();
}

async function reload() {
  loading.value = true;
  try {
    const status = activeStatusFilter.value === "all" ? undefined : Number(activeStatusFilter.value);
    const result = await getPartnerCars({ page: page.value, pageSize, status });
    cars.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;
  } catch (e: any) {
    toast.error("Ошибка загрузки: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

function goToPage(p: number) {
  if (p < 1 || p > totalPages.value) return;
  page.value = p;
  reload();
}

onMounted(reload);
</script>
