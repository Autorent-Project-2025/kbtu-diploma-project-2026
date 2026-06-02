<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 pt-24 pb-12 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-6xl mx-auto space-y-8">
      <header
        class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-3"
      >
        <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
          Мои машины
        </h1>
        <p class="text-gray-600 dark:text-gray-400">
          Добавление новой машины выполняется через заявку и согласование
          менеджером.
        </p>
      </header>

      <section
        class="glass p-8 rounded-3xl border border-gray-200 dark:border-gray-800 shadow-xl space-y-6"
      >
        <h2 class="text-2xl font-bold text-gray-900 dark:text-white">
          Новая заявка на машину
        </h2>

        <div
          v-if="submitted"
          class="rounded-xl border border-green-300/80 bg-green-50 p-4 text-green-800 dark:border-green-500/30 dark:bg-green-900/20 dark:text-green-300"
        >
          Заявка отправлена. После решения менеджера вы получите уведомление на
          email.
        </div>

        <form class="grid md:grid-cols-2 gap-4" @submit.prevent="submitTicket">
          <div class="space-y-2">
            <label
              for="carBrand"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Марка</label
            >
            <select
              id="carBrand"
              v-model="form.brandSelection"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            >
              <option value="" disabled>Выберите марку</option>
              <option v-for="brand in brandOptions" :key="brand" :value="brand">
                {{ brand }}
              </option>
              <option :value="customOptionValue">Свой вариант</option>
            </select>
            <input
              v-if="isCustomBrandSelected"
              id="customCarBrand"
              v-model="form.customCarBrand"
              type="text"
              placeholder="Введите свою марку"
              autocomplete="off"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
            <p class="text-xs text-gray-500 dark:text-gray-400">
              Марки загружаются из каталога. При необходимости можно указать
              свою.
            </p>
          </div>

          <div class="space-y-2">
            <label
              for="carModel"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Модель</label
            >
            <select
              id="carModel"
              v-model="form.modelSelection"
              required
              :disabled="!form.brandSelection || loadingModels"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white disabled:cursor-not-allowed disabled:opacity-50"
            >
              <option value="" disabled>{{ modelPlaceholder }}</option>
              <option v-for="model in modelOptions" :key="model" :value="model">
                {{ model }}
              </option>
              <option :value="customOptionValue">Свой вариант</option>
            </select>
            <input
              v-if="isCustomModelSelected"
              id="customCarModel"
              v-model="form.customCarModel"
              type="text"
              placeholder="Введите свою модель"
              autocomplete="off"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
            <p class="text-xs text-gray-500 dark:text-gray-400">
              {{ modelHint }}
            </p>
          </div>

          <div class="space-y-2">
            <label
              for="licensePlate"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Гос номер</label
            >
            <input
              id="licensePlate"
              v-model="form.licensePlate"
              type="text"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2">
            <label
              for="carYear"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Год выпуска</label
            >
            <input
              id="carYear"
              v-model.number="form.carYear"
              type="number"
              min="1886"
              :max="maxAllowedCarYear"
              required
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2">
            <label
              for="transmission"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Коробка</label
            >
            <select
              id="transmission"
              v-model="form.transmission"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            >
              <option value="">Не указано</option>
              <option
                v-for="option in transmissionOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>
          </div>

          <div class="space-y-2">
            <label
              for="fuelType"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Топливо</label
            >
            <select
              id="fuelType"
              v-model="form.fuelType"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            >
              <option value="">Не указано</option>
              <option
                v-for="option in fuelTypeOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>
          </div>

          <div class="space-y-2">
            <label
              for="bodyType"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Тип кузова</label
            >
            <select
              id="bodyType"
              v-model="form.bodyType"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            >
              <option value="">Не указано</option>
              <option
                v-for="option in bodyTypeOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>
          </div>

          <div class="space-y-2">
            <label
              for="horsepower"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Мощность, л.с.</label
            >
            <input
              id="horsepower"
              v-model.number="form.horsepower"
              type="number"
              min="1"
              max="3000"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2">
            <label
              for="seats"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Мест</label
            >
            <input
              id="seats"
              v-model.number="form.seats"
              type="number"
              min="1"
              max="20"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2">
            <label
              for="doors"
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Дверей</label
            >
            <input
              id="doors"
              v-model.number="form.doors"
              type="number"
              min="1"
              max="6"
              class="w-full px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white"
            />
          </div>

          <div class="space-y-2 md:col-span-2">
            <label
              class="block text-sm font-semibold text-gray-700 dark:text-gray-300"
              >Теги</label
            >
            <div class="relative" ref="tagDropdownRef">
              <button
                type="button"
                class="w-full flex items-center justify-between gap-2 px-4 py-3 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-sm text-gray-500 dark:text-gray-400 hover:border-gray-300 dark:hover:border-gray-600 transition-colors"
                @click="tagDropdownOpen = !tagDropdownOpen"
              >
                <span>Добавить тег...</span>
                <svg
                  class="h-4 w-4 shrink-0 transition-transform"
                  :class="tagDropdownOpen ? 'rotate-180' : ''"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="2"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    d="M19 9l-7 7-7-7"
                  />
                </svg>
              </button>
              <div
                v-if="tagDropdownOpen"
                class="absolute z-10 mt-1 w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 shadow-lg overflow-hidden"
              >
                <button
                  v-for="option in availableTagOptions"
                  :key="option.value"
                  type="button"
                  class="w-full flex items-center gap-2 px-4 py-2.5 text-sm text-left text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                  @click="selectTag(option.value)"
                >
                  <span
                    v-if="suggestedTags.includes(option.value)"
                    class="h-1.5 w-1.5 rounded-full bg-emerald-500 shrink-0"
                    title="Рекомендовано системой"
                  />
                  <span v-else class="h-1.5 w-1.5 shrink-0" />
                  {{ option.label }}
                </button>
                <p
                  v-if="availableTagOptions.length === 0"
                  class="px-4 py-2.5 text-sm text-gray-400 dark:text-gray-500"
                >
                  Все теги добавлены
                </p>
              </div>
            </div>
            <div
              v-if="form.selectedTags.length > 0"
              class="flex flex-wrap gap-2"
            >
              <span
                v-for="tag in form.selectedTags"
                :key="tag"
                class="inline-flex items-center gap-1.5 rounded-full border border-primary-300 bg-primary-50 px-3 py-1 text-sm font-medium text-primary-700 dark:border-primary-500/40 dark:bg-primary-500/15 dark:text-primary-300"
              >
                <span
                  v-if="suggestedTags.includes(tag)"
                  class="h-1.5 w-1.5 rounded-full bg-emerald-500 shrink-0"
                  title="Рекомендовано системой"
                />
                {{ getSemanticTagLabel(tag) }}
                <button
                  type="button"
                  class="ml-0.5 rounded-full p-0.5 hover:bg-primary-200 dark:hover:bg-primary-500/30 transition-colors"
                  @click="toggleTag(tag)"
                >
                  <svg
                    class="h-3 w-3"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="2.5"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      d="M6 18L18 6M6 6l12 12"
                    />
                  </svg>
                </button>
              </span>
            </div>
            <p class="text-xs text-gray-500 dark:text-gray-400">
              Выберите теги из списка.
              <template v-if="suggestedTags.length > 0">
                Теги с
                <span
                  class="inline-block h-1.5 w-1.5 rounded-full bg-emerald-500 align-middle"
                />
                рекомендованы по характеристикам машины.
              </template>
              Менеджер сможет скорректировать.
            </p>
          </div>

          <PartnerCarPriceEstimate
            :can-estimate-price="canEstimatePrice"
            :estimating="estimating"
            :price-estimate="priceEstimate"
            @estimate="runPriceEstimate"
          />

          <PartnerCarOwnershipDocumentUpload
            :file="form.ownershipDocumentFile"
            @file-selected="applyPdfFile"
            @remove="form.ownershipDocumentFile = null"
          />

          <PartnerCarImagesUpload
            :images="form.carImages"
            :image-type-options="partnerCarImageTypeOptions"
            @add-files="applyImageFiles"
            @remove-image="removeImage"
            @update-image-type="updateCarImageType"
          />

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

      <PartnerCarsList
        :cars="cars"
        :loading="loadingCars"
        @refresh="loadCars"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref, watch } from "vue";
import {
  createPartnerCarTicket,
  type PartnerCarImageType,
} from "../api/tickets";
import { getCarPriceEstimate } from "../api/cars";
import {
  getCarBrands,
  getCarModelNames,
  getMyPartnerCars,
  type PartnerCarSummary,
} from "../api/partnerCars";
import PartnerCarImagesUpload from "../components/partner/PartnerCarImagesUpload.vue";
import PartnerCarOwnershipDocumentUpload from "../components/partner/PartnerCarOwnershipDocumentUpload.vue";
import PartnerCarPriceEstimate from "../components/partner/PartnerCarPriceEstimate.vue";
import PartnerCarsList from "../components/partner/PartnerCarsList.vue";
import { useToast } from "../composables/useToast";
import {
  bodyTypeOptions,
  fuelTypeOptions,
  getSemanticTagLabel,
  type SemanticTag,
  semanticTagOptions,
  suggestSemanticTags,
  transmissionOptions,
} from "../utils/partnerCarSemanticTags";

const { error, success } = useToast();

const cars = ref<PartnerCarSummary[]>([]);
const brandOptions = ref<string[]>([]);
const modelOptions = ref<string[]>([]);
const loadingCars = ref(false);
const loadingModels = ref(false);
const submitting = ref(false);
const submitted = ref(false);
const tagDropdownOpen = ref(false);
const tagDropdownRef = ref<HTMLElement | null>(null);
const customOptionValue = "__custom__";
const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

interface LocalPartnerCarImage {
  file: File;
  previewUrl: string;
  imageType: PartnerCarImageType;
}

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

const form = reactive({
  brandSelection: "",
  customCarBrand: "",
  modelSelection: "",
  customCarModel: "",
  carYear: null as number | null,
  licensePlate: "",
  transmission: "",
  fuelType: "",
  seats: null as number | null,
  doors: null as number | null,
  bodyType: "",
  horsepower: null as number | null,
  selectedTags: [] as SemanticTag[],
  ownershipDocumentFile: null as File | null,
  carImages: [] as LocalPartnerCarImage[],
});

const isCustomBrandSelected = computed(
  () => form.brandSelection === customOptionValue,
);
const isCustomModelSelected = computed(
  () => form.modelSelection === customOptionValue,
);

const resolvedCarBrand = computed(() => {
  return isCustomBrandSelected.value
    ? form.customCarBrand.trim()
    : form.brandSelection.trim();
});

const resolvedCarModel = computed(() => {
  return isCustomModelSelected.value
    ? form.customCarModel.trim()
    : form.modelSelection.trim();
});

const modelPlaceholder = computed(() => {
  if (loadingModels.value) {
    return "Загрузка моделей...";
  }

  if (modelOptions.value.length === 0) {
    return "Модели не найдены";
  }

  return "Выберите модель";
});

const modelHint = computed(() => {
  if (loadingModels.value) {
    return "Обновляем список моделей из каталога.";
  }

  if (modelOptions.value.length === 0) {
    return "Для выбранной марки модели не найдены. Можно указать свой вариант.";
  }

  return "Модели загружаются из каталога. При необходимости можно указать свою.";
});

const suggestedTags = computed(() =>
  suggestSemanticTags({
    fuelType: form.fuelType,
    bodyType: form.bodyType,
    seats: form.seats,
    horsepower: form.horsepower,
  }),
);

const availableTagOptions = computed(() =>
  semanticTagOptions.filter(
    (option) => !form.selectedTags.includes(option.value),
  ),
);

interface PriceEstimateResult {
  priceHour: number;
  priceDay: number;
  confidence: string;
  sampleCount: number;
}

const priceEstimate = ref<PriceEstimateResult | null>(null);
const estimating = ref(false);

const canEstimatePrice = computed(() => {
  const year = form.carYear;
  const brand = resolvedCarBrand.value;
  const model = resolvedCarModel.value;
  return (
    !!brand &&
    !!model &&
    typeof year === "number" &&
    year >= 1886 &&
    year <= maxAllowedCarYear
  );
});

async function runPriceEstimate() {
  if (!canEstimatePrice.value || !form.carYear) return;
  estimating.value = true;
  priceEstimate.value = null;
  try {
    const result = await getCarPriceEstimate(
      resolvedCarBrand.value,
      resolvedCarModel.value,
      form.carYear,
    );
    priceEstimate.value = result;
  } catch (e: any) {
    if (e?.response?.status === 404) {
      error(
        "Объявления для этой машины не найдены — рыночная стоимость недоступна.",
      );
    } else {
      error("Не удалось получить оценку стоимости.");
    }
  } finally {
    estimating.value = false;
  }
}

function toggleTag(value: SemanticTag) {
  const index = form.selectedTags.indexOf(value);
  if (index === -1) {
    form.selectedTags.push(value);
  } else {
    form.selectedTags.splice(index, 1);
  }
}

function selectTag(value: SemanticTag) {
  if (!form.selectedTags.includes(value)) {
    form.selectedTags.push(value);
  }
  tagDropdownOpen.value = false;
}

function closeTagDropdown() {
  tagDropdownOpen.value = false;
}

function onDocumentClick(event: MouseEvent) {
  if (
    tagDropdownRef.value &&
    !tagDropdownRef.value.contains(event.target as Node)
  ) {
    tagDropdownOpen.value = false;
  }
}

function isPdf(file: File): boolean {
  return (
    file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf")
  );
}

function validateOptionalInteger(
  value: number | null,
  min: number,
  max: number,
  label: string,
): boolean {
  if (value == null) {
    return true;
  }

  if (!Number.isInteger(value) || value < min || value > max) {
    error(`${label} должно быть в диапазоне ${min}-${max}.`);
    return false;
  }

  return true;
}

function applyPdfFile(file: File | null) {
  if (!file) return;
  if (!isPdf(file)) {
    error("Файл собственности должен быть PDF.");
    return;
  }
  form.ownershipDocumentFile = file;
}

function getDefaultPartnerCarImageType(index: number): PartnerCarImageType {
  if (index === 0) return "front";
  if (index === 1) return "back";
  if (index === 2 || index === 3) return "side";
  if (index === 4) return "interior";
  return "general";
}

function revokeCarImagePreview(image?: LocalPartnerCarImage) {
  if (image?.previewUrl) {
    URL.revokeObjectURL(image.previewUrl);
  }
}

function revokeAllCarImagePreviews() {
  form.carImages.forEach(revokeCarImagePreview);
}

function applyImageFiles(files: File[]) {
  if (files.length === 0) return;

  const invalidFile = files.find((file) => !file.type.startsWith("image/"));
  if (invalidFile) {
    error("Все файлы в фотографиях должны быть изображениями.");
    return;
  }

  const currentCount = form.carImages.length;
  if (currentCount + files.length > 12) {
    error("Можно загрузить не более 12 фотографий.");
    return;
  }

  const nextImages = files.map((file, index) => ({
    file,
    previewUrl: URL.createObjectURL(file),
    imageType: getDefaultPartnerCarImageType(currentCount + index),
  }));

  form.carImages.push(...nextImages);
}

function removeImage(index: number) {
  const [removedImage] = form.carImages.splice(index, 1);
  revokeCarImagePreview(removedImage);
}

function updateCarImageType(payload: {
  index: number;
  imageType: PartnerCarImageType;
}) {
  const image = form.carImages[payload.index];
  if (image) {
    image.imageType = payload.imageType;
  }
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

async function loadBrandOptions() {
  try {
    brandOptions.value = await getCarBrands();
  } catch (e: any) {
    error(e?.response?.data?.error || "Не удалось загрузить список марок.");
  }
}

async function loadModelOptions(brand?: string | null) {
  loadingModels.value = true;
  try {
    modelOptions.value = await getCarModelNames(brand);
  } catch (e: any) {
    modelOptions.value = [];
    error(e?.response?.data?.error || "Не удалось загрузить список моделей.");
  } finally {
    loadingModels.value = false;
  }
}

async function refreshModelOptionsForCurrentBrand() {
  const brandFilter = isCustomBrandSelected.value ? null : form.brandSelection;
  await loadModelOptions(brandFilter);
}

watch(
  () => form.brandSelection,
  async (nextBrand, previousBrand) => {
    if (nextBrand !== previousBrand) {
      form.modelSelection = "";
      form.customCarModel = "";
    }

    await refreshModelOptionsForCurrentBrand();
  },
);

watch(
  () => [
    form.brandSelection,
    form.customCarBrand,
    form.modelSelection,
    form.customCarModel,
    form.carYear,
  ],
  () => {
    priceEstimate.value = null;
  },
);

function resetForm() {
  form.brandSelection = "";
  form.customCarBrand = "";
  form.modelSelection = "";
  form.customCarModel = "";
  form.carYear = null;
  form.licensePlate = "";
  form.transmission = "";
  form.fuelType = "";
  form.seats = null;
  form.doors = null;
  form.bodyType = "";
  form.horsepower = null;
  form.selectedTags = [];
  form.ownershipDocumentFile = null;
  revokeAllCarImagePreviews();
  form.carImages = [];
}

async function submitTicket() {
  if (submitting.value) return;

  const carBrand = resolvedCarBrand.value;
  const carModel = resolvedCarModel.value;
  if (!carBrand || !carModel) {
    error("Укажите марку и модель машины.");
    return;
  }

  const carYear = Number(form.carYear);
  if (!Number.isInteger(carYear)) {
    error("Укажите год выпуска машины.");
    return;
  }

  if (carYear < 1886 || carYear > maxAllowedCarYear) {
    error(`Год выпуска должен быть в диапазоне 1886-${maxAllowedCarYear}.`);
    return;
  }

  if (!form.licensePlate.trim()) {
    error("Укажите гос номер.");
    return;
  }

  if (!validateOptionalInteger(form.seats, 1, 20, "Количество мест")) {
    return;
  }

  if (!validateOptionalInteger(form.doors, 1, 6, "Количество дверей")) {
    return;
  }

  if (!validateOptionalInteger(form.horsepower, 1, 3000, "Мощность")) {
    return;
  }

  if (!form.ownershipDocumentFile) {
    error("Загрузите файл подтверждения собственности.");
    return;
  }

  if (form.carImages.length === 0) {
    error("Добавьте хотя бы одну фотографию машины.");
    return;
  }

  submitting.value = true;
  try {
    await createPartnerCarTicket({
      carBrand,
      carModel,
      carYear,
      licensePlate: form.licensePlate.trim(),
      transmission: form.transmission || null,
      fuelType: form.fuelType || null,
      seats: form.seats,
      doors: form.doors,
      bodyType: form.bodyType || null,
      horsepower: form.horsepower,
      selectedTags: [
        ...new Set([...form.selectedTags, ...suggestedTags.value]),
      ],
      ownershipDocumentFile: form.ownershipDocumentFile,
      carImages: form.carImages.map((image) => ({
        file: image.file,
        imageType: image.imageType,
      })),
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
  document.addEventListener("mousedown", onDocumentClick);
  await Promise.all([
    loadCars(),
    loadBrandOptions(),
    refreshModelOptionsForCurrentBrand(),
  ]);
});

onUnmounted(() => {
  document.removeEventListener("mousedown", onDocumentClick);
  revokeAllCarImagePreviews();
});
</script>
