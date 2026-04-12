<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto space-y-8">
      <button
        type="button"
        class="inline-flex items-center gap-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-primary-600 dark:hover:text-primary-400 transition-colors"
        @click="router.push('/partner/cars')"
      >
        <span>&larr;</span>
        <span>К списку машин</span>
      </button>

      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-center text-gray-500 dark:text-gray-400"
      >
        Загружаем детали машины...
      </div>

      <div
        v-else-if="errorMessage"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-6 text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </div>

      <template v-else-if="car">
        <section class="rounded-[32px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
          <div class="grid gap-6 lg:grid-cols-[360px_minmax(0,1fr)]">
            <div class="space-y-3 bg-gray-100 dark:bg-gray-800 p-4">
              <div class="relative overflow-hidden rounded-3xl bg-gray-200 dark:bg-gray-900 aspect-[4/3]">
                <img
                  v-if="currentImage"
                  :src="currentImage.imageUrl"
                  :alt="carTitle"
                  class="h-full w-full object-cover"
                />
                <div
                  v-else
                  class="flex h-full items-center justify-center text-gray-500 dark:text-gray-400"
                >
                  Нет фотографий
                </div>
                <span
                  v-if="currentImage"
                  class="absolute left-4 top-4 rounded-full bg-black/70 px-3 py-1 text-xs font-semibold text-white"
                >
                  {{ getCarImageTypeLabel(currentImage.imageType) }}
                </span>
              </div>

              <div
                v-if="car.images.length > 1"
                class="grid grid-cols-4 gap-3"
              >
                <button
                  v-for="(image, index) in car.images.slice(0, 8)"
                  :key="`${image.id}-${index}`"
                  type="button"
                  :class="[
                    'relative overflow-hidden rounded-2xl border transition-all',
                    currentImageIndex === index
                      ? 'border-primary-500 ring-2 ring-primary-500/30'
                      : 'border-gray-200 dark:border-gray-700',
                  ]"
                  @click="currentImageIndex = index"
                >
                  <img
                    :src="image.imageUrl"
                    :alt="carTitle"
                    class="h-20 w-full object-cover"
                  />
                </button>
              </div>
            </div>

            <div class="p-6 sm:p-8 space-y-6">
              <div class="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
                <div class="space-y-3">
                  <div class="flex flex-wrap items-center gap-3">
                    <span class="rounded-full bg-primary-100 px-3 py-1 text-xs font-bold uppercase tracking-[0.22em] text-primary-700 dark:bg-primary-900/30 dark:text-primary-300">
                      Машина партнера
                    </span>
                    <span class="rounded-full border border-gray-200 dark:border-gray-700 px-3 py-1 text-xs font-semibold text-gray-700 dark:text-gray-300">
                      {{ statusLabel(car.status) }}
                    </span>
                    <span
                      :class="car.isActive ? 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300' : 'bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-300'"
                      class="rounded-full px-3 py-1 text-xs font-semibold"
                    >
                      {{ car.isActive ? "Активна" : "Неактивна" }}
                    </span>
                  </div>

                  <div>
                    <h1 class="text-3xl sm:text-4xl font-extrabold text-gray-900 dark:text-white">
                      {{ carTitle }}
                    </h1>
                    <p class="mt-2 text-base font-medium text-gray-600 dark:text-gray-400">
                      Гос. номер: {{ car.licensePlate }}
                    </p>
                  </div>
                </div>
              </div>

              <div class="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Цвет</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ car.color || "Не указан" }}</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Цена / час</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ formatPrice(car.priceHour) }}</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Рейтинг</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ formatRating(car.rating) }}</p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-4 space-y-1.5">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Бронирований</p>
                  <p class="text-base font-bold text-gray-900 dark:text-white">{{ car.bookings.length }}</p>
                </article>
              </div>

              <div class="grid gap-4 md:grid-cols-2">
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 space-y-2">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Описание</p>
                  <p class="text-sm leading-6 text-gray-700 dark:text-gray-200">
                    {{ car.description || "Описание отсутствует." }}
                  </p>
                </article>
                <article class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 space-y-2">
                  <p class="text-xs font-bold uppercase tracking-[0.18em] text-gray-500 dark:text-gray-400">Характеристики</p>
                  <dl class="grid grid-cols-2 gap-y-2 text-sm text-gray-600 dark:text-gray-300">
                    <div>Двигатель: <span class="font-semibold text-gray-900 dark:text-white">{{ car.engine || "—" }}</span></div>
                    <div>Трансмиссия: <span class="font-semibold text-gray-900 dark:text-white">{{ car.transmission || "—" }}</span></div>
                    <div>Топливо: <span class="font-semibold text-gray-900 dark:text-white">{{ car.fuelType || "—" }}</span></div>
                    <div>Мест: <span class="font-semibold text-gray-900 dark:text-white">{{ car.seats ?? "—" }}</span></div>
                    <div>Дверей: <span class="font-semibold text-gray-900 dark:text-white">{{ car.doors ?? "—" }}</span></div>
                    <div>Кузов: <span class="font-semibold text-gray-900 dark:text-white">{{ car.bodyType || "—" }}</span></div>
                  </dl>
                </article>
              </div>
            </div>
          </div>
        </section>

        <section class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6">
          <div class="space-y-2">
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Запрос на изменение</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Партнер не меняет машину напрямую. Изменения уходят консультанту на согласование.
            </p>
            <p
              class="rounded-2xl border border-amber-200 dark:border-amber-500/30 bg-amber-50 dark:bg-amber-500/10 px-4 py-3 text-sm text-amber-800 dark:text-amber-200"
            >
              Если после согласования машина станет неактивной, все связанные бронирования будут автоматически отменены.
            </p>
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <label class="space-y-1.5">
              <span class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">Госномер</span>
              <input
                v-model="form.licensePlate"
                type="text"
                class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-3 text-sm text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
              />
            </label>

            <label class="space-y-1.5">
              <span class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">Цвет</span>
              <input
                v-model="form.color"
                type="text"
                class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-3 text-sm text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
              />
            </label>

            <label class="space-y-1.5">
              <span class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">Статус</span>
              <select
                v-model.number="form.requestedStatus"
                class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 px-4 py-3 text-sm text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
              >
                <option
                  v-for="option in partnerCarStatusOptions"
                  :key="option.value"
                  :value="option.value"
                >
                  {{ option.label }}
                </option>
              </select>
            </label>

            <div class="rounded-2xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/60 px-4 py-3 flex items-center justify-between gap-4">
              <div>
                <p class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">Активность</p>
                <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                  Машина скрывается из выдачи и больше не участвует в бронировании.
                </p>
              </div>
              <label class="inline-flex items-center gap-3 text-sm font-semibold text-gray-900 dark:text-white">
                <input
                  v-model="form.isActive"
                  type="checkbox"
                  class="h-4 w-4 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                />
                <span>{{ form.isActive ? "Активна" : "Неактивна" }}</span>
              </label>
            </div>
          </div>

          <div class="space-y-4">
            <div>
              <h3 class="text-lg font-bold text-gray-900 dark:text-white">Новые фотографии</h3>
              <p class="text-sm text-gray-500 dark:text-gray-400">
                Текущие фото останутся на машине. Новые изображения будут добавлены после согласования.
              </p>
            </div>

            <div
              class="relative flex flex-col items-center justify-center gap-2 rounded-2xl border-2 border-dashed px-6 py-8 transition-colors cursor-pointer"
              :class="imageDragging ? 'border-primary-500 bg-primary-50 dark:bg-primary-500/10' : 'border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 hover:border-gray-300 dark:hover:border-gray-600'"
              @dragover.prevent="imageDragging = true"
              @dragleave.prevent="imageDragging = false"
              @drop.prevent="onImageDrop"
              @click="imageInputRef?.click()"
            >
              <p class="text-sm text-gray-500 dark:text-gray-400">
                <span class="font-semibold text-primary-600 dark:text-primary-400">Нажмите</span>
                или перетащите фото сюда
              </p>
              <p class="text-xs text-gray-400 dark:text-gray-500">
                Можно загрузить дополнительные фотографии с типом кадра.
              </p>
              <input
                ref="imageInputRef"
                type="file"
                accept="image/*"
                multiple
                class="hidden"
                @change="onCarImagesChange"
              />
            </div>

            <div
              v-if="form.newImages.length > 0"
              class="grid gap-3 md:grid-cols-2 xl:grid-cols-3"
            >
              <div
                v-for="(image, index) in form.newImages"
                :key="`${image.file.name}-${index}`"
                class="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 overflow-hidden"
              >
                <img
                  :src="image.previewUrl"
                  :alt="`Новое фото ${index + 1}`"
                  class="h-44 w-full object-cover"
                />
                <div class="p-4 space-y-3">
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                      <p class="text-sm font-semibold text-gray-900 dark:text-white">
                        Фото {{ index + 1 }}
                      </p>
                      <p class="text-xs text-gray-500 dark:text-gray-400 truncate">
                        {{ image.file.name }}
                      </p>
                    </div>
                    <button
                      type="button"
                      class="rounded-full p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors"
                      @click="removeNewImage(index)"
                    >
                      ✕
                    </button>
                  </div>

                  <label class="space-y-1.5 block">
                    <span class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">Тип фото</span>
                    <select
                      v-model="image.imageType"
                      class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 px-3 py-2.5 text-sm text-gray-900 dark:text-white focus:outline-none focus:border-primary-500 focus:ring-2 focus:ring-primary-500/20"
                    >
                      <option
                        v-for="option in partnerCarImageTypeOptions"
                        :key="option.value"
                        :value="option.value"
                      >
                        {{ option.label }}
                      </option>
                    </select>
                  </label>
                </div>
              </div>
            </div>
          </div>

          <button
            type="button"
            :disabled="submitting"
            class="w-full rounded-2xl bg-primary-600 hover:bg-primary-700 disabled:opacity-60 px-6 py-4 text-white font-bold transition-colors"
            @click="submitUpdateRequest"
          >
            {{ submitting ? "Отправка..." : "Отправить консультанту" }}
          </button>
        </section>

        <section class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-4">
          <div>
            <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Связанные бронирования</h2>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Все бронирования, привязанные к этой машине.
            </p>
          </div>

          <div
            v-if="car.bookings.length === 0"
            class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-6 text-center text-gray-500 dark:text-gray-400"
          >
            По этой машине пока нет бронирований.
          </div>

          <div v-else class="space-y-4">
            <article
              v-for="booking in car.bookings"
              :key="booking.id"
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/60 p-5 flex flex-col md:flex-row md:items-center md:justify-between gap-4"
            >
              <div class="space-y-2">
                <p class="text-sm font-bold text-gray-900 dark:text-white">
                  Бронирование #{{ booking.id }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  {{ formatDateRange(booking.startDate, booking.endDate) }}
                </p>
                <p class="text-sm text-gray-600 dark:text-gray-400">
                  Стоимость: {{ formatPrice(booking.price) }}
                </p>
              </div>
              <span
                class="inline-flex items-center rounded-full px-3 py-1 text-sm font-semibold"
                :class="bookingStatusClass(booking.status)"
              >
                {{ bookingStatusLabel(booking.status) }}
              </span>
            </article>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getMyPartnerCarDetails, type PartnerCarDetails } from "../api/partnerCars";
import {
  createPartnerCarUpdateTicket,
  type PartnerCarImageType,
} from "../api/tickets";
import { useToast } from "../composables/useToast";
import { getCarImageTypeLabel } from "../utils/carImageType";
import { formatMoney } from "../utils/formatMoney";

interface LocalPartnerCarImage {
  file: File;
  previewUrl: string;
  imageType: PartnerCarImageType;
}

const route = useRoute();
const router = useRouter();
const { success, error } = useToast();

const loading = ref(true);
const submitting = ref(false);
const errorMessage = ref("");
const car = ref<PartnerCarDetails | null>(null);
const currentImageIndex = ref(0);
const imageDragging = ref(false);
const imageInputRef = ref<HTMLInputElement | null>(null);

const partnerCarImageTypeOptions: Array<{
  value: PartnerCarImageType;
  label: string;
}> = [
  { value: "front", label: "Спереди" },
  { value: "back", label: "Сзади" },
  { value: "side", label: "Сбоку" },
  { value: "interior", label: "Салон" },
  { value: "general", label: "Общий вид" },
];

const partnerCarStatusOptions = [
  { value: 0, label: "Доступна" },
  { value: 1, label: "Забронирована" },
  { value: 2, label: "В поездке" },
  { value: 3, label: "На обслуживании" },
];

const form = reactive({
  licensePlate: "",
  color: "",
  requestedStatus: 0,
  isActive: true,
  newImages: [] as LocalPartnerCarImage[],
});

const carId = computed(() => Number(route.params.id));
const carTitle = computed(() => {
  if (!car.value) {
    return "";
  }

  return `${car.value.brand} ${car.value.model} ${car.value.year}`;
});

const currentImage = computed(() => {
  if (!car.value || car.value.images.length === 0) {
    return null;
  }

  return car.value.images[currentImageIndex.value] ?? car.value.images[0] ?? null;
});

onMounted(async () => {
  await loadCar();
});

onUnmounted(() => {
  for (const image of form.newImages) {
    URL.revokeObjectURL(image.previewUrl);
  }
});

async function loadCar() {
  loading.value = true;
  errorMessage.value = "";

  try {
    if (!Number.isFinite(carId.value) || carId.value <= 0) {
      throw new Error("Некорректный идентификатор машины.");
    }

    car.value = await getMyPartnerCarDetails(carId.value);
    currentImageIndex.value = 0;
    syncFormWithCar();
  } catch (loadCause) {
    console.error("Failed to load my partner car details", loadCause);
    car.value = null;
    errorMessage.value = "Не удалось загрузить детали машины.";
  } finally {
    loading.value = false;
  }
}

function syncFormWithCar() {
  if (!car.value) {
    return;
  }

  form.licensePlate = car.value.licensePlate ?? "";
  form.color = car.value.color ?? "";
  form.requestedStatus = car.value.status ?? 0;
  form.isActive = car.value.isActive;

  for (const image of form.newImages) {
    URL.revokeObjectURL(image.previewUrl);
  }
  form.newImages = [];
}

function getDefaultPartnerCarImageType(index: number): PartnerCarImageType {
  if (index === 0) return "front";
  if (index === 1) return "back";
  if (index === 2) return "side";
  if (index === 3) return "interior";
  return "general";
}

function appendImages(files: FileList | File[] | null | undefined) {
  if (!files || files.length === 0) {
    return;
  }

  const currentCount = form.newImages.length;
  const acceptedFiles = Array.from(files).filter((file) =>
    file.type.startsWith("image/"),
  );

  for (const [index, file] of acceptedFiles.entries()) {
    form.newImages.push({
      file,
      previewUrl: URL.createObjectURL(file),
      imageType: getDefaultPartnerCarImageType(currentCount + index),
    });
  }
}

function onCarImagesChange(event: Event) {
  const target = event.target as HTMLInputElement | null;
  appendImages(target?.files);
  if (target) {
    target.value = "";
  }
}

function onImageDrop(event: DragEvent) {
  imageDragging.value = false;
  appendImages(event.dataTransfer?.files);
}

function removeNewImage(index: number) {
  const [removed] = form.newImages.splice(index, 1);
  if (removed) {
    URL.revokeObjectURL(removed.previewUrl);
  }
}

function hasPendingChanges() {
  if (!car.value) {
    return false;
  }

  return (
    form.licensePlate.trim() !== (car.value.licensePlate ?? "").trim() ||
    form.color.trim() !== (car.value.color ?? "").trim() ||
    form.requestedStatus !== car.value.status ||
    form.isActive !== car.value.isActive ||
    form.newImages.length > 0
  );
}

async function submitUpdateRequest() {
  if (!car.value || submitting.value) {
    return;
  }

  const normalizedLicensePlate = form.licensePlate.trim();
  if (!normalizedLicensePlate) {
    error("Укажите госномер.");
    return;
  }

  if (!hasPendingChanges()) {
    error("Изменений для отправки нет.");
    return;
  }

  submitting.value = true;
  try {
    await createPartnerCarUpdateTicket({
      partnerCarId: car.value.id,
      carBrand: car.value.brand,
      carModel: car.value.model,
      carYear: car.value.year,
      licensePlate: normalizedLicensePlate,
      color: form.color.trim() || null,
      requestedStatus: form.requestedStatus,
      isActive: form.isActive,
      transmission: car.value.transmission ?? null,
      fuelType: car.value.fuelType ?? null,
      seats: car.value.seats ?? null,
      doors: car.value.doors ?? null,
      bodyType: car.value.bodyType ?? null,
      horsepower: car.value.horsepower ?? null,
      newImages: form.newImages.map((image) => ({
        file: image.file,
        imageType: image.imageType,
      })),
    });

    success("Заявка на изменение машины отправлена консультанту.");
    syncFormWithCar();
  } catch (submitCause: any) {
    console.error("Failed to submit partner car update request", submitCause);
    error(
      submitCause?.response?.data?.detail ||
        submitCause?.response?.data?.error ||
        "Не удалось отправить заявку на изменение машины.",
    );
  } finally {
    submitting.value = false;
  }
}

function statusLabel(status: number): string {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Недоступна";
}

function bookingStatusLabel(status?: string | null): string {
  const normalized = (status ?? "").trim().toLowerCase();
  if (normalized === "pending") return "Ожидает оплаты";
  if (normalized === "confirmed") return "Подтверждено";
  if (normalized === "active") return "В поездке";
  if (normalized === "awaitingreview") return "На проверке";
  if (normalized === "completed") return "Завершено";
  if (normalized === "canceled") return "Отменено";
  return status || "Неизвестно";
}

function bookingStatusClass(status?: string | null): string {
  const normalized = (status ?? "").trim().toLowerCase();
  if (normalized === "pending") {
    return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
  }
  if (normalized === "confirmed") {
    return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
  }
  if (normalized === "active") {
    return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
  }
  if (normalized === "awaitingreview") {
    return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
  }
  if (normalized === "completed") {
    return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-300";
  }
  return "bg-rose-100 text-rose-800 dark:bg-rose-900/30 dark:text-rose-300";
}

function formatPrice(value: number | null | undefined): string {
  if (value == null) {
    return "Не указана";
  }

  return formatMoney(value);
}

function formatRating(value: number | null | undefined): string {
  if (value == null) {
    return "Без рейтинга";
  }

  return `${value.toFixed(1)} / 5`;
}

function formatDate(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function formatDateRange(start: string, end: string): string {
  return `${formatDate(start)} - ${formatDate(end)}`;
}
</script>
