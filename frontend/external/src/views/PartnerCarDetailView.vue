<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 pt-24 pb-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-5xl mx-auto space-y-6">
      <router-link
        to="/partner/cars"
        class="inline-flex items-center gap-2 text-primary-600 dark:text-primary-400 font-semibold"
      >
        ← Назад к моим машинам
      </router-link>

      <section v-if="loading" class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl">
        Загрузка...
      </section>

      <section
        v-else-if="errorMessage"
        class="glass p-8 rounded-3xl border border-red-300/70 dark:border-red-500/30 shadow-xl text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </section>

      <template v-else-if="car">
        <section class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-4">
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            {{ car.brand }} {{ car.model }} {{ car.year }}
          </h1>
          <div class="grid md:grid-cols-2 gap-4 text-gray-700 dark:text-gray-300">
            <p>Гос номер: <b>{{ car.licensePlate }}</b></p>
            <p>Статус: <b>{{ statusLabel(car.status) }}</b></p>
            <p>Цена/час: <b>{{ car.priceHour ?? "—" }}</b></p>
            <p>Цена/день: <b>{{ car.priceDay ?? "—" }}</b></p>
            <p>Рейтинг: <b>{{ car.rating ?? "нет" }}</b></p>
            <p>Файл собственности: <b>{{ car.ownershipFileName || "не указан" }}</b></p>
          </div>
          <p v-if="car.description" class="text-gray-600 dark:text-gray-400">{{ car.description }}</p>
        </section>

        <section class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-4">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Фотографии</h2>
          <div v-if="car.images.length === 0" class="text-gray-600 dark:text-gray-400">Фотографии отсутствуют.</div>
          <div v-else class="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <a
              v-for="image in car.images"
              :key="image.id"
              :href="image.imageUrl"
              target="_blank"
              rel="noopener noreferrer"
              class="block overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-700"
            >
              <img :src="image.imageUrl" alt="car image" class="w-full h-48 object-cover" />
            </a>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getMyPartnerCarDetails, type PartnerCarDetails } from "../api/partnerCars";

const route = useRoute();
const loading = ref(true);
const errorMessage = ref("");
const car = ref<PartnerCarDetails | null>(null);

function statusLabel(status: number): string {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Неизвестно";
}

onMounted(async () => {
  loading.value = true;
  errorMessage.value = "";

  const id = Number(route.params.id);
  if (!Number.isFinite(id) || id <= 0) {
    errorMessage.value = "Некорректный id машины.";
    loading.value = false;
    return;
  }

  try {
    car.value = await getMyPartnerCarDetails(id);
  } catch (e: any) {
    errorMessage.value = e?.response?.data?.error || "Не удалось загрузить детали машины.";
  } finally {
    loading.value = false;
  }
});
</script>
