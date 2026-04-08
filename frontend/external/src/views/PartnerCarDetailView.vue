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
          <div v-if="carTags.length > 0" class="flex flex-wrap gap-2">
            <span
              v-for="tag in carTags"
              :key="`${car.id}-${tag}`"
              class="px-3 py-1.5 rounded-full bg-white/80 dark:bg-gray-900/70 border border-gray-200 dark:border-gray-800 text-sm font-semibold text-gray-700 dark:text-gray-300"
            >
              {{ tag }}
            </span>
          </div>
          <div class="grid md:grid-cols-2 gap-4 text-gray-700 dark:text-gray-300">
            <p>Гос номер: <b>{{ car.licensePlate }}</b></p>
            <p>Статус: <b>{{ statusLabel(car.status) }}</b></p>
            <p>Цена/час: <b>{{ formatMoney(car.priceHour) }}</b></p>
            <p>Цена/день: <b>{{ formatMoney(car.priceDay) }}</b></p>
            <p>Рейтинг: <b>{{ car.rating ?? "нет" }}</b></p>
            <p class="flex items-center gap-2">
              Файл собственности:
              <button
                v-if="car.ownershipFileName"
                type="button"
                class="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg bg-primary-600 hover:bg-primary-700 text-white text-xs font-semibold transition-colors disabled:opacity-60"
                :disabled="openingOwnershipDocument"
                @click="openOwnershipDocument"
              >
                {{ openingOwnershipDocument ? "Открытие..." : "Посмотреть документ" }}
              </button>
              <b v-else>не указан</b>
            </p>
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
              class="relative block overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-700"
            >
              <img :src="image.imageUrl" alt="car image" class="w-full h-48 object-cover" />
              <span
                class="absolute bottom-3 left-3 rounded-full bg-black/70 px-3 py-1 text-xs font-semibold text-white"
              >
                {{ getCarImageTypeLabel(image.imageType) }}
              </span>
            </a>
          </div>
        </section>

        <section class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-5">
          <div class="flex items-center justify-between gap-4">
            <div>
              <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Отзывы</h2>
            </div>
            <div class="text-sm font-semibold text-gray-600 dark:text-gray-300">
              {{ car.comments.length }} отзыв{{ reviewSuffix(car.comments.length) }}
            </div>
          </div>

          <div
            v-if="car.comments.length === 0"
            class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-6 text-gray-600 dark:text-gray-400"
          >
            Для этой машины ещё не оставляли комментарии.
          </div>

          <div v-else class="space-y-4">
            <article
              v-for="comment in car.comments"
              :key="comment.id"
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white/80 dark:bg-gray-900/70 p-5 space-y-4"
            >
              <div class="flex items-start justify-between gap-4">
                <div class="flex items-start gap-4 min-w-0">
                  <div
                    class="w-12 h-12 shrink-0 rounded-2xl overflow-hidden bg-gradient-to-br from-primary-500 to-primary-700 flex items-center justify-center text-white font-bold shadow-lg"
                  >
                    <img
                      v-if="comment.avatarUrl"
                      :src="comment.avatarUrl"
                      :alt="comment.userName"
                      class="w-full h-full object-cover"
                    />
                    <span v-else>{{ getInitials(comment.userName) }}</span>
                  </div>

                  <div class="min-w-0 space-y-1">
                    <p class="font-semibold text-gray-900 dark:text-white">
                      {{ comment.userName }}
                    </p>
                    <p class="text-sm text-gray-500 dark:text-gray-400">
                      {{ formatDateTime(comment.createdOn) }}
                    </p>
                  </div>
                </div>

                <div class="flex items-center gap-1 shrink-0">
                  <svg
                    v-for="n in 5"
                    :key="n"
                    :class="[
                      'w-5 h-5',
                      n <= comment.rating
                        ? 'text-amber-400 fill-current'
                        : 'text-gray-300 dark:text-gray-600',
                    ]"
                    viewBox="0 0 20 20"
                  >
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                    />
                  </svg>
                </div>
              </div>

              <p class="text-gray-700 dark:text-gray-300 leading-relaxed">
                {{ comment.content }}
              </p>
            </article>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRoute } from "vue-router";
import { getMyPartnerCarDetails, type PartnerCarDetails } from "../api/partnerCars";
import { getMyPartnerFileTemporaryLink } from "../api/partners";
import { useToast } from "../composables/useToast";
import { getCarImageTypeLabel } from "../utils/carImageType";
import { formatMoney } from "../utils/formatMoney";
import { buildCarTags } from "../utils/carTags";

const route = useRoute();
const { error } = useToast();
const loading = ref(true);
const errorMessage = ref("");
const car = ref<PartnerCarDetails | null>(null);
const openingOwnershipDocument = ref(false);
const carTags = computed(() =>
  car.value
    ? buildCarTags(
        {
          engine: car.value.engine,
          transmission: car.value.transmission,
          fuelType: car.value.fuelType,
          seats: car.value.seats,
          doors: car.value.doors,
        },
        8,
      )
    : [],
);

function statusLabel(status: number): string {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Неизвестно";
}

function openTemporaryLink(url: string) {
  const openedWindow = window.open(url, "_blank", "noopener,noreferrer");
  if (!openedWindow) {
    window.location.href = url;
  }
}

async function openOwnershipDocument() {
  if (!car.value?.ownershipFileName) {
    error("Документ собственности не найден.");
    return;
  }

  openingOwnershipDocument.value = true;
  try {
    const link = await getMyPartnerFileTemporaryLink(car.value.ownershipFileName);
    openTemporaryLink(link.url);
  } catch (e: any) {
    error(
      e?.response?.data?.error ||
      e?.response?.data?.message ||
      e?.response?.data?.detail ||
      "Не удалось открыть документ собственности."
    );
  } finally {
    openingOwnershipDocument.value = false;
  }
}

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

function getInitials(name: string): string {
  const parts = (name ?? "")
    .trim()
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2);

  if (parts.length === 0) {
    return "?";
  }

  return parts.map((part) => part[0]?.toUpperCase() ?? "").join("");
}

function reviewSuffix(count: number): string {
  const mod10 = count % 10;
  const mod100 = count % 100;

  if (mod10 === 1 && mod100 !== 11) {
    return "";
  }

  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) {
    return "а";
  }

  return "ов";
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
