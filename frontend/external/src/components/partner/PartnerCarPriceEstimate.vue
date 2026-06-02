<template>
  <div class="space-y-3 md:col-span-2">
    <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
      Примерная стоимость аренды
    </label>
    <div class="flex items-center gap-3 flex-wrap">
      <button
        type="button"
        :disabled="!canEstimatePrice || estimating"
        class="inline-flex items-center gap-2 rounded-xl border border-gray-300 bg-white px-4 py-2.5 text-sm font-medium text-gray-700 transition-colors hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-40 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-200 dark:hover:bg-gray-700"
        @click="emit('estimate')"
      >
        <svg
          class="h-4 w-4"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            d="M9 7h6m0 10v-3m-3 3h.01M9 17h.01M9 11h.01M12 11h.01M15 11h.01M4 19h16a2 2 0 002-2V7a2 2 0 00-2-2H4a2 2 0 00-2 2v10a2 2 0 002 2z"
          />
        </svg>
        {{ estimating ? "Запрос..." : "Рассчитать" }}
      </button>
      <p v-if="!canEstimatePrice" class="text-xs text-gray-400 dark:text-gray-500">
        Заполните марку, модель и год
      </p>
      <div
        v-if="priceEstimate"
        class="flex items-center gap-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-2.5 dark:border-emerald-500/30 dark:bg-emerald-500/10"
      >
        <span class="text-sm font-semibold text-emerald-800 dark:text-emerald-300">
          {{ priceEstimate.priceHour.toLocaleString("ru-RU") }} ₸/час
        </span>
        <span class="text-xs text-emerald-600 dark:text-emerald-400">
          · {{ priceEstimate.priceDay.toLocaleString("ru-RU") }} ₸/сут
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface PartnerCarPriceEstimateResult {
  priceHour: number;
  priceDay: number;
}

defineProps<{
  canEstimatePrice: boolean;
  estimating: boolean;
  priceEstimate: PartnerCarPriceEstimateResult | null;
}>();

const emit = defineEmits<{
  estimate: [];
}>();
</script>
