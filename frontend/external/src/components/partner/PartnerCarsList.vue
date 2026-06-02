<template>
  <section
    class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-4"
  >
    <div class="flex items-center justify-between">
      <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
        Мои активные машины
      </h2>
      <button
        class="btn-premium px-4 py-2 rounded-xl"
        :disabled="loading"
        @click="emit('refresh')"
      >
        {{ loading ? "Обновление..." : "Обновить" }}
      </button>
    </div>

    <div v-if="loading" class="text-gray-600 dark:text-gray-400">
      Загрузка...
    </div>
    <div
      v-else-if="cars.length === 0"
      class="text-gray-600 dark:text-gray-400"
    >
      Пока нет машин. Создайте заявку выше.
    </div>

    <div v-else class="grid md:grid-cols-2 gap-4">
      <article
        v-for="car in cars"
        :key="car.id"
        class="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 space-y-3"
      >
        <div class="flex items-center justify-between gap-3">
          <h3 class="font-bold text-gray-900 dark:text-white">
            {{ car.modelDisplayName }}
          </h3>
          <span
            class="text-xs rounded-full bg-gray-100 dark:bg-gray-800 px-3 py-1 text-gray-600 dark:text-gray-300"
          >
            #{{ car.id }}
          </span>
        </div>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Гос номер: {{ car.licensePlate }}
        </p>
        <p class="text-sm text-gray-600 dark:text-gray-400">
          Рейтинг: {{ car.rating ?? "нет" }} · Бронирований:
          {{ car.bookingCount }}
        </p>
        <router-link
          :to="`/partner/cars/${car.id}`"
          class="inline-flex items-center gap-2 text-primary-600 dark:text-primary-400 font-semibold"
        >
          Детали машины
        </router-link>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { PartnerCarSummary } from "../../api/partnerCars";

defineProps<{
  cars: PartnerCarSummary[];
  loading: boolean;
}>();

const emit = defineEmits<{
  refresh: [];
}>();
</script>
