<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(249,115,22,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(249,115,22,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex items-center gap-4">
        <router-link
          to="/cars"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-orange-500 transition-colors"
        >
          Назад
        </router-link>
        <div class="space-y-1">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-orange-600 dark:text-orange-400">
            Data Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            {{ loading ? "Загрузка..." : `${originalCar?.modelBrand ?? ""} ${originalCar?.modelName ?? ""}` }}
          </h1>
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

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Машина не найдена.
    </div>

    <!-- Form -->
    <template v-else>
      <!-- Car info (read-only) -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Информация об автомобиле</h2>
        <dl class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Марка / Модель</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ originalCar?.modelBrand }} {{ originalCar?.modelName }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Год</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ originalCar?.modelYear }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Владелец (Partner ID)</dt>
            <dd class="text-gray-600 dark:text-gray-400 font-mono text-sm">{{ originalCar?.partnerUserId }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Цена/час</dt>
            <dd class="text-gray-900 dark:text-white font-medium">
              {{ originalCar?.priceHour ? formatPrice(originalCar.priceHour) : "—" }}
            </dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Рейтинг</dt>
            <dd class="text-gray-900 dark:text-white font-medium">
              <template v-if="originalCar?.rating">
                <span class="text-amber-600 dark:text-amber-400">{{ originalCar.rating.toFixed(1) }}</span>
                <span class="text-xs text-gray-400 ml-1">({{ originalCar.ratingsCount }})</span>
              </template>
              <span v-else class="text-gray-400">—</span>
            </dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Добавлена</dt>
            <dd class="text-gray-600 dark:text-gray-400 text-sm">{{ formatDateTime(originalCar?.createdAt ?? "") }}</dd>
          </div>
        </dl>

        <div v-if="originalCar?.commercialBadgeKeys?.length" class="flex flex-wrap gap-1.5 pt-2">
          <span
            v-for="tag in originalCar.commercialBadgeKeys"
            :key="tag"
            class="px-2.5 py-0.5 rounded-full bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300 text-xs font-semibold"
          >
            {{ tag }}
          </span>
        </div>
      </div>

      <!-- Editable fields -->
      <form @submit.prevent="onSave" class="space-y-6">
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Редактирование</h2>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Гос. номер</label>
              <input
                v-model="form.licensePlate"
                type="text"
                required
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-orange-500 focus:ring-2 focus:ring-orange-500/20 transition-colors font-mono disabled:opacity-60 disabled:cursor-not-allowed"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Цвет</label>
              <input
                v-model="form.color"
                type="text"
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-orange-500 focus:ring-2 focus:ring-orange-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                placeholder="Не указан"
              />
            </div>

            <div>
              <label class="block text-sm font-bold text-gray-700 dark:text-gray-300 mb-2 uppercase tracking-[0.1em]">Статус</label>
              <select
                v-model.number="form.status"
                :disabled="!canUpdate"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-orange-500 focus:ring-2 focus:ring-orange-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                <option v-for="s in statusOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-3">
          <button
            v-if="canUpdate"
            type="submit"
            :disabled="saving"
            class="px-6 py-3 rounded-2xl bg-orange-600 hover:bg-orange-700 disabled:bg-gray-300 disabled:text-gray-500 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-orange-500/20 transition-colors"
          >
            {{ saving ? "Сохранение..." : "Сохранить" }}
          </button>

          <button
            v-if="canDelete"
            type="button"
            @click="onDelete"
            :disabled="deleting"
            class="px-6 py-3 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-bold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ deleting ? "Удаление..." : "Удалить" }}
          </button>
        </div>
      </form>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getPartnerCar, updatePartnerCar, deletePartnerCar, type PartnerCarDto } from "../api/cars";
import { auth } from "../store/auth";
import { useToast } from "../composables/useToast";

const route = useRoute();
const router = useRouter();
const toast = useToast();

const loading = ref(false);
const saving = ref(false);
const deleting = ref(false);
const notFound = ref(false);
const originalCar = ref<PartnerCarDto | null>(null);

const canUpdate = computed(() => auth.hasPermission("PartnerCar.Update"));
const canDelete = computed(() => auth.hasPermission("PartnerCar.Delete"));

const statusOptions = [
  { value: 0, label: "На модерации" },
  { value: 1, label: "Активна" },
  { value: 2, label: "Неактивна" },
  { value: 3, label: "Заблокирована" },
];

const form = reactive({
  licensePlate: "",
  color: "",
  status: 0,
});

function populateForm(car: PartnerCarDto) {
  form.licensePlate = car.licensePlate ?? "";
  form.color = car.color ?? "";
  form.status = car.status;
}

function formatPrice(value: number): string {
  return new Intl.NumberFormat("ru-RU", { style: "currency", currency: "KZT", maximumFractionDigits: 0 }).format(value);
}

function formatDateTime(dateStr: string): string {
  if (!dateStr) return "";
  return new Date(dateStr).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

async function loadCar() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    const car = await getPartnerCar(id);
    originalCar.value = car;
    populateForm(car);
  } catch {
    notFound.value = true;
  } finally {
    loading.value = false;
  }
}

async function onSave() {
  if (saving.value || !originalCar.value) return;
  saving.value = true;

  try {
    const updated = await updatePartnerCar(originalCar.value.id, {
      licensePlate: form.licensePlate,
      color: form.color || undefined,
      status: form.status,
    });
    originalCar.value = updated;
    populateForm(updated);
    toast.success("Машина успешно обновлена");
  } catch {
    toast.error("Ошибка при сохранении машины");
  } finally {
    saving.value = false;
  }
}

async function onDelete() {
  if (deleting.value || !originalCar.value) return;
  if (!confirm("Вы уверены, что хотите удалить эту машину?")) return;

  deleting.value = true;
  try {
    await deletePartnerCar(originalCar.value.id);
    toast.success("Машина удалена");
    router.push("/cars");
  } catch {
    toast.error("Ошибка при удалении машины");
  } finally {
    deleting.value = false;
  }
}

onMounted(loadCar);
</script>
