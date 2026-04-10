<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(249,115,22,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(249,115,22,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="space-y-2">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-orange-600 dark:text-orange-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">Машины партнёров</h1>
          <p class="text-gray-600 dark:text-gray-400">
            Автомобили, зарегистрированные партнёрами для аренды.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <div class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden">
            <div class="px-5 py-3 text-center">
              <p class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ totalCount }}</p>
              <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">Всего</p>
            </div>
          </div>
          <button
            @click="reload"
            :disabled="loading"
            class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-orange-500 transition-colors disabled:opacity-60"
          >
            Обновить
          </button>
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

    <!-- Empty -->
    <div
      v-else-if="cars.length === 0"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Машины не найдены.
    </div>

    <template v-else>
      <!-- Table -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-gray-200 dark:border-gray-800">
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Марка / Модель</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Гос. номер</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Год</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Рейтинг</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Теги</th>
              <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Добавлена</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="car in cars"
              :key="car.id"
              @click="router.push(`/cars/${car.id}`)"
              class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
            >
              <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                {{ car.id }}
              </td>
              <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">
                {{ car.modelBrand }} {{ car.modelName }}
              </td>
              <td class="px-5 py-3 font-mono text-gray-700 dark:text-gray-300">
                {{ car.licensePlate }}
              </td>
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400">
                {{ car.modelYear }}
              </td>
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400 whitespace-nowrap">
                <template v-if="car.priceHour">{{ formatPrice(car.priceHour) }}/ч</template>
                <template v-else>—</template>
              </td>
              <td class="px-5 py-3">
                <span v-if="car.rating" class="text-amber-600 dark:text-amber-400 font-semibold">
                  {{ car.rating.toFixed(1) }}
                </span>
                <span v-else class="text-gray-400 dark:text-gray-600">—</span>
                <span v-if="car.ratingsCount" class="text-xs text-gray-400 dark:text-gray-500 ml-1">
                  ({{ car.ratingsCount }})
                </span>
              </td>
              <td class="px-5 py-3">
                <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', carStatusBadge(car.status)]">
                  {{ carStatusLabel(car.status) }}
                </span>
              </td>
              <td class="px-5 py-3">
                <div class="flex flex-wrap gap-1">
                  <span
                    v-for="tag in car.commercialBadgeKeys"
                    :key="tag"
                    class="px-2 py-0.5 rounded-full bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300 text-xs font-semibold"
                  >
                    {{ tag }}
                  </span>
                </div>
              </td>
              <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                {{ formatDateTime(car.createdAt) }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="flex items-center justify-center gap-2">
        <button
          @click="goToPage(page - 1)"
          :disabled="page <= 1"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-orange-500 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Назад
        </button>
        <span class="text-sm text-gray-500 dark:text-gray-400 font-medium">
          {{ page }} / {{ totalPages }}
        </span>
        <button
          @click="goToPage(page + 1)"
          :disabled="page >= totalPages"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-orange-500 transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          Вперёд
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getPartnerCars, type PartnerCarDto } from "../api/cars";

const router = useRouter();
const cars = ref<PartnerCarDto[]>([]);
const loading = ref(false);
const page = ref(1);
const totalCount = ref(0);
const totalPages = ref(0);
const pageSize = 20;

const carStatusMap: Record<number, { label: string; css: string }> = {
  0: { label: "На модерации", css: "bg-amber-100 text-amber-700 dark:bg-amber-500/20 dark:text-amber-300" },
  1: { label: "Активна", css: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300" },
  2: { label: "Неактивна", css: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400" },
  3: { label: "Заблокирована", css: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300" },
};

function carStatusLabel(status: number): string {
  return carStatusMap[status]?.label ?? String(status);
}

function carStatusBadge(status: number): string {
  return carStatusMap[status]?.css ?? "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400";
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("ru-RU", { style: "currency", currency: "KZT", maximumFractionDigits: 0 }).format(value);
}

function formatDateTime(dateStr: string): string {
  if (!dateStr) return "—";
  return new Date(dateStr).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

async function reload() {
  loading.value = true;
  try {
    const result = await getPartnerCars(page.value, pageSize);
    cars.value = result.items;
    totalCount.value = result.totalCount;
    totalPages.value = result.totalPages;
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
