<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 pt-24 pb-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300">
    <div class="max-w-6xl mx-auto space-y-8">
      <header class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-3">
        <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">Мои машины</h1>
        <p class="text-gray-600 dark:text-gray-400">
          Добавление новой машины выполняется через заявку и согласование менеджером.
        </p>
      </header>

      <section class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-6">
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Новая заявка на машину</h2>

        <div
          v-if="submitted"
          class="rounded-xl border border-green-300/80 bg-green-50 p-4 text-green-800 dark:border-green-500/30 dark:bg-green-900/20 dark:text-green-300"
        >
          Заявка отправлена. После решения менеджера вы получите уведомление на email.
        </div>

        <form class="grid md:grid-cols-2 gap-4" @submit.prevent="submitTicket">
          <div class="space-y-2 md:col-span-2">
            <label for="partnerEmail" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">Email партнера</label>
            <input
              id="partnerEmail"
              v-model="form.email"
              type="email"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2 md:col-span-2">
            <label for="carModel" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">Марка и модель</label>
            <select
              id="carModel"
              v-model.number="form.modelId"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            >
              <option :value="0" disabled>Выберите модель</option>
              <option v-for="model in carModels" :key="model.id" :value="model.id">
                {{ model.brand }} {{ model.model }} {{ model.year }}
              </option>
            </select>
          </div>

          <div class="space-y-2">
            <label for="licensePlate" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">Гос номер</label>
            <input
              id="licensePlate"
              v-model="form.licensePlate"
              type="text"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2">
            <label for="ownershipFile" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              Подтверждение собственности (PDF)
            </label>
            <input
              id="ownershipFile"
              type="file"
              accept="application/pdf,.pdf"
              required
              @change="onOwnershipFileChange"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2 md:col-span-2">
            <label for="carImages" class="block text-sm font-semibold text-gray-700 dark:text-gray-300">Фото машины</label>
            <input
              id="carImages"
              type="file"
              accept="image/*"
              multiple
              required
              @change="onCarImagesChange"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
            <p class="text-xs text-gray-500 dark:text-gray-400">Минимум 1 фото, максимум 12.</p>
          </div>

          <div class="md:col-span-2">
            <button
              type="submit"
              :disabled="submitting"
              class="w-full px-6 py-3 rounded-xl bg-primary-600 hover:bg-primary-700 text-white font-semibold transition-colors disabled:opacity-70"
            >
              {{ submitting ? "Отправка..." : "Отправить заявку" }}
            </button>
          </div>
        </form>
      </section>

      <section class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-2xl font-bold text-gray-900 dark:text-white">Мои активные машины</h2>
          <button class="btn-premium px-4 py-2 rounded-xl" @click="loadCars" :disabled="loadingCars">
            {{ loadingCars ? "Обновление..." : "Обновить" }}
          </button>
        </div>

        <div v-if="loadingCars" class="text-gray-600 dark:text-gray-400">Загрузка...</div>
        <div v-else-if="cars.length === 0" class="text-gray-600 dark:text-gray-400">
          Пока нет машин. Создайте заявку выше.
        </div>

        <div v-else class="grid md:grid-cols-2 gap-4">
          <article
            v-for="car in cars"
            :key="car.id"
            class="rounded-2xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-900 p-5 space-y-3"
          >
            <div class="flex items-center justify-between gap-3">
              <h3 class="font-bold text-gray-900 dark:text-white">{{ car.modelDisplayName }}</h3>
              <span class="text-xs rounded-full bg-gray-100 dark:bg-gray-800 px-3 py-1 text-gray-600 dark:text-gray-300">
                #{{ car.id }}
              </span>
            </div>
            <p class="text-sm text-gray-600 dark:text-gray-400">Гос номер: {{ car.licensePlate }}</p>
            <p class="text-sm text-gray-600 dark:text-gray-400">
              Рейтинг: {{ car.rating ?? "нет" }} · Бронирований: {{ car.bookingCount }}
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
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from "vue";
import { createPartnerCarTicket } from "../api/tickets";
import {
  getCarModels,
  getMyPartnerCars,
  type CarModelOption,
  type PartnerCarSummary,
} from "../api/partnerCars";
import { useToast } from "../composables/useToast";

const { error, success } = useToast();

const cars = ref<PartnerCarSummary[]>([]);
const carModels = ref<CarModelOption[]>([]);
const loadingCars = ref(false);
const submitting = ref(false);
const submitted = ref(false);

const form = reactive({
  email: "",
  modelId: 0,
  licensePlate: "",
  ownershipDocumentFile: null as File | null,
  carImageFiles: [] as File[],
});

function isPdf(file: File): boolean {
  return file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf");
}

function onOwnershipFileChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0] ?? null;
  if (file && !isPdf(file)) {
    input.value = "";
    form.ownershipDocumentFile = null;
    error("Файл собственности должен быть PDF.");
    return;
  }

  form.ownershipDocumentFile = file;
}

function onCarImagesChange(event: Event) {
  const input = event.target as HTMLInputElement;
  const files = Array.from(input.files ?? []);

  if (files.length === 0) {
    form.carImageFiles = [];
    return;
  }

  if (files.length > 12) {
    input.value = "";
    form.carImageFiles = [];
    error("Можно загрузить не более 12 фотографий.");
    return;
  }

  const invalidFile = files.find((file) => !file.type.startsWith("image/"));
  if (invalidFile) {
    input.value = "";
    form.carImageFiles = [];
    error("Все файлы в фотографиях должны быть изображениями.");
    return;
  }

  form.carImageFiles = files;
}

async function loadCars() {
  loadingCars.value = true;
  try {
    cars.value = await getMyPartnerCars();
  } catch (e: any) {
    error(e?.response?.data?.error || "Не удалось загрузить список машин.");
  } finally {
    loadingCars.value = false;
  }
}

async function loadModels() {
  try {
    carModels.value = await getCarModels();
  } catch (e: any) {
    error(e?.response?.data?.error || "Не удалось загрузить список моделей.");
  }
}

function resetForm() {
  form.modelId = 0;
  form.licensePlate = "";
  form.ownershipDocumentFile = null;
  form.carImageFiles = [];
}

async function submitTicket() {
  if (submitting.value) return;
  if (!form.email.trim()) {
    error("Укажите email партнера.");
    return;
  }

  const selectedModel = carModels.value.find((model) => model.id === form.modelId);
  if (!selectedModel) {
    error("Выберите марку и модель машины.");
    return;
  }

  if (!form.licensePlate.trim()) {
    error("Укажите гос номер.");
    return;
  }

  if (!form.ownershipDocumentFile) {
    error("Загрузите файл подтверждения собственности.");
    return;
  }

  if (form.carImageFiles.length === 0) {
    error("Добавьте хотя бы одну фотографию машины.");
    return;
  }

  submitting.value = true;
  try {
    await createPartnerCarTicket({
      email: form.email.trim(),
      carBrand: selectedModel.brand,
      carModel: selectedModel.model,
      licensePlate: form.licensePlate.trim(),
      ownershipDocumentFile: form.ownershipDocumentFile,
      carImageFiles: form.carImageFiles,
    });
    submitted.value = true;
    success("Заявка успешно отправлена.");
    resetForm();
  } catch (e: any) {
    error(e?.response?.data?.error || "Не удалось отправить заявку.");
  } finally {
    submitting.value = false;
  }
}

onMounted(async () => {
  await Promise.all([loadCars(), loadModels()]);
});
</script>
