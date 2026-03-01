<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto">
      <!-- Header -->
      <div class="mb-12 space-y-4 animate-slide-up">
        <h1
          class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
        >
          Наш автопарк
        </h1>
        <p class="text-lg text-gray-600 dark:text-gray-400 max-w-2xl">
          Выберите автомобиль вашей мечты из нашей коллекции премиальных и
          бизнес-автомобилей
        </p>
      </div>

      <!-- Filters and Sorting -->
      <div
        class="mb-8 p-6 bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800"
      >
        <div class="flex flex-wrap items-center gap-4">
          <!-- Single Sort Selector -->
          <div class="flex items-center gap-2">
            <label
              class="text-sm font-semibold text-gray-700 dark:text-gray-300"
            >
              Сортировка:
            </label>
            <select
              v-model="sortType"
              class="px-4 py-3 bg-gray-50 dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 rounded-xl text-gray-900 dark:text-white font-medium focus:border-primary-500 focus:outline-none transition-colors min-w-[200px]"
            >
              <option value="popular">Популярные</option>
              <option value="newest">Новинки</option>
              <option value="price_asc">Сначала дешевые</option>
              <option value="price_desc">Сначала дорогие</option>
              <option value="rating">Высокий рейтинг</option>
            </select>
          </div>

          <!-- Results Count -->
          <div
            class="ml-auto text-sm text-gray-600 dark:text-gray-400 font-medium"
          >
            Всего автомобилей:
            <span class="font-bold text-gray-900 dark:text-white">{{
              totalCount
            }}</span>
          </div>
        </div>
      </div>

      <!-- Cars Grid (3x3) -->
      <div
        v-if="cars.length > 0"
        :key="gridKey"
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8"
      >
        <div
          v-for="(car, index) in carsWithStatus"
          :key="`${car.id}-${index}-${sortType}`"
          class="group relative bg-white dark:bg-gray-900 rounded-3xl overflow-hidden shadow-lg hover:shadow-2xl transition-all duration-500 card-hover border border-gray-200 dark:border-gray-800 flex flex-col"
          :class="{ 'opacity-75': !car.isAvailable }"
        >
          <!-- Image Container -->
          <div
            class="relative h-64 overflow-hidden bg-gradient-to-br from-gray-100 to-gray-200 dark:from-gray-800 dark:to-gray-900 flex-shrink-0"
          >
            <img
              :src="car.imageUrl || config.app.defaultCarImage"
              :alt="`${car.brand} ${car.model}`"
              class="car-card-image w-full h-full object-cover transform group-hover:scale-110 transition-transform duration-700"
            />

            <!-- Gradient Overlay -->
            <div
              class="absolute inset-0 bg-gradient-to-t from-black/60 via-transparent to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-500"
            ></div>

            <!-- Price Badge -->
            <div
              class="absolute top-4 right-4 glass px-4 py-2 rounded-full backdrop-blur-md"
            >
              <div class="flex items-baseline gap-1">
                <span class="text-2xl font-bold text-white"
                  >${{ car.priceHour }}</span
                >
                <span class="text-sm text-gray-300 font-medium">/час</span>
              </div>
            </div>

            <!-- Year Badge -->
            <div
              class="absolute top-4 left-4 glass px-3 py-1.5 rounded-full backdrop-blur-md"
            >
              <span class="text-sm font-semibold text-white">{{
                car.year
              }}</span>
            </div>

            <!-- ✅ ЗВЕЗДОЧКА С КОМПАКТНЫМ GLASS ФОНОМ -->
            <div
              v-if="car.rating !== null && car.rating !== undefined"
              class="absolute bottom-4 right-4 glass px-2 py-1 rounded-full backdrop-blur-md"
            >
              <div class="inline-flex items-center gap-1">
                <svg class="w-3.5 h-3.5 fill-yellow-400" viewBox="0 0 20 20">
                  <path
                    d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                  />
                </svg>
                <span class="text-xs font-bold text-white">{{
                  car.rating.toFixed(1)
                }}</span>
              </div>
            </div>

            <!-- Unavailable Overlay -->
            <div
              v-if="!car.isAvailable"
              class="absolute inset-0 bg-black/40 backdrop-blur-sm flex items-center justify-center"
            >
              <div
                class="bg-red-600 text-white px-6 py-3 rounded-full font-bold text-sm shadow-lg"
              >
                Забронирован
              </div>
            </div>
          </div>

          <!-- Content -->
          <div class="p-6 flex flex-col flex-1">
            <!-- Car Info -->
            <div class="mb-4">
              <h3
                class="text-2xl font-bold text-gray-900 dark:text-white group-hover:text-primary-600 dark:group-hover:text-primary-400 transition-colors mb-2"
              >
                {{ car.brand }} {{ car.model }}
              </h3>
              <p class="text-gray-600 dark:text-gray-400 text-sm font-medium">
                {{ car.year }} год выпуска
              </p>
            </div>

            <!-- Features -->
            <div class="flex flex-wrap gap-2 mb-6">
              <span
                class="px-3 py-1 bg-primary-50 dark:bg-primary-900/20 text-primary-700 dark:text-primary-300 text-xs font-semibold rounded-full"
              >
                Премиум
              </span>
              <span
                :class="
                  car.isAvailable
                    ? 'bg-green-50 dark:bg-green-900/20 text-green-700 dark:text-green-300'
                    : 'bg-red-50 dark:bg-red-900/20 text-red-700 dark:text-red-300'
                "
                class="px-3 py-1 text-xs font-semibold rounded-full"
              >
                {{ car.isAvailable ? "Доступен" : "Забронирован" }}
              </span>
            </div>

            <!-- Spacer -->
            <div class="flex-1"></div>

            <!-- Action Buttons -->
            <div class="flex gap-3">
              <!-- View Details Button -->
              <router-link
                :to="`/cars/${car.id}`"
                class="flex-1 px-6 py-4 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-gray-900 dark:text-white font-bold rounded-2xl transition-all hover:shadow-lg active:scale-95 flex items-center justify-center gap-2"
              >
                <span>Подробнее</span>
                <svg
                  class="w-5 h-5"
                  fill="none"
                  stroke="currentColor"
                  viewBox="0 0 24 24"
                >
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  />
                </svg>
              </router-link>

              <!-- Book Button -->
              <button
                @click="openBookingModal(car)"
                :disabled="!car.isAvailable"
                class="flex-1 relative overflow-hidden bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white font-bold py-4 px-6 rounded-2xl transition-all duration-300 hover:shadow-xl hover:shadow-primary-500/50 active:scale-95 disabled:cursor-not-allowed disabled:hover:shadow-none group/btn"
              >
                <span
                  class="relative z-10 flex items-center justify-center gap-2"
                >
                  <span v-if="car.isAvailable">Забронировать</span>
                  <span v-else>Недоступен</span>
                  <svg
                    v-if="car.isAvailable"
                    class="w-5 h-5 transform group-hover/btn:translate-x-1 transition-transform"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M13 7l5 5m0 0l-5 5m5-5H6"
                    />
                  </svg>
                </span>

                <!-- Shimmer effect -->
                <div
                  v-if="car.isAvailable"
                  class="absolute inset-0 bg-gradient-to-r from-transparent via-white/20 to-transparent translate-x-[-100%] group-hover/btn:translate-x-[100%] transition-transform duration-700"
                ></div>
              </button>
            </div>
          </div>

          <!-- Glow Effect on Hover -->
          <div
            v-if="car.isAvailable"
            class="absolute inset-0 rounded-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none"
            style="box-shadow: 0 0 40px rgba(59, 130, 246, 0.3)"
          ></div>
        </div>
      </div>

      <!-- Loading State -->
      <div v-else-if="loading" class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6">
          <div class="relative">
            <div
              class="w-16 h-16 rounded-full border-4 border-primary-200 dark:border-primary-900 border-t-primary-600 dark:border-t-primary-400 animate-spin"
            ></div>
            <div
              class="absolute inset-0 w-16 h-16 rounded-full glow-primary opacity-50"
            ></div>
          </div>
          <p class="text-gray-600 dark:text-gray-400 text-lg font-medium">
            Загрузка автомобилей...
          </p>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="text-center py-32">
        <div class="inline-flex flex-col items-center gap-6 max-w-md mx-auto">
          <div
            class="w-24 h-24 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
          >
            <svg
              class="w-12 h-12 text-gray-400"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
              />
            </svg>
          </div>
          <div class="space-y-2">
            <h3 class="text-2xl font-bold text-gray-900 dark:text-white">
              Автомобили не найдены
            </h3>
            <p class="text-gray-600 dark:text-gray-400">
              Попробуйте изменить параметры поиска
            </p>
          </div>
        </div>
      </div>

      <!-- Pagination (9 машин на страницу) -->
      <div v-if="totalPages > 1" class="mt-12">
        <Pagination
          :current-page="currentPage"
          :total-pages="totalPages"
          @page-change="handlePageChange"
        />
      </div>
    </div>

    <!-- Booking Modal -->
    <BookingModal
      v-if="selectedCar"
      :is-open="isModalOpen"
      :car="selectedCar"
      @close="closeBookingModal"
      @confirm="handleBookingConfirm"
    />

    <!-- Login Required Modal -->
    <LoginRequiredModal
      :is-open="showLoginModal"
      @close="showLoginModal = false"
      @login="goToLogin"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch, nextTick } from "vue";
import { useRouter } from "vue-router";
import { getCars, type GetCarsParams } from "../api/cars";
import { createBooking, getCarBookings } from "../api/booking";
import type { Car } from "../types/Car";
import type { Booking } from "../types/Booking";
import type { PaginatedResponse } from "../types/Pagination";
import { config } from "../config";
import { useToast } from "../composables/useToast";
import { useAuth } from "../composables/useAuth";
import { isCarAvailable } from "../utils/bookingUtils";
import BookingModal from "../components/BookingModal.vue";
import LoginRequiredModal from "../components/LoginRequiredModal.vue";
import Pagination from "../components/Pagination.vue";

interface CarWithStatus extends Car {
  isAvailable: boolean;
  bookings: Booking[];
}

const router = useRouter();
const cars = ref<Car[]>([]);
const carBookings = ref<Record<number, Booking[]>>({});
const selectedCar = ref<Car | null>(null);
const isModalOpen = ref(false);
const showLoginModal = ref(false);
const loading = ref(true);

// ✅ ДОБАВЛЕН: gridKey для force re-render
const gridKey = ref(0);

// Pagination (9 машин на странице)
const currentPage = ref(1);
const pageSize = ref(9);
const totalCount = ref(0);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));

// Sorting
const sortType = ref<
  "popular" | "newest" | "price_asc" | "price_desc" | "rating"
>("popular");

const { success, error } = useToast();
const { isAuthenticated } = useAuth();

// Вычисляем машины со статусом доступности
const carsWithStatus = computed<CarWithStatus[]>(() => {
  return cars.value.map((car) => {
    const bookings = carBookings.value[car.id] || [];
    const now = new Date();

    const available = isCarAvailable(
      bookings,
      now,
      new Date(now.getTime() + 1000)
    );

    return {
      ...car,
      isAvailable: available,
      bookings,
    };
  });
});

onMounted(async () => {
  await loadCars();
});

// ✅ ИСПРАВЛЕНО: watch с force re-render
watch(sortType, async () => {
  currentPage.value = 1;
  await loadCars();
  // ✅ Force re-render grid
  gridKey.value++;
  await nextTick();
});

async function loadCars() {
  loading.value = true;
  try {
    let sortBy: "rating" | "priceHour" | "year" = "rating";
    let sortOrder: "asc" | "desc" = "desc";

    switch (sortType.value) {
      case "popular":
        sortBy = "rating";
        sortOrder = "desc";
        break;
      case "newest":
        sortBy = "year";
        sortOrder = "desc";
        break;
      case "price_asc":
        sortBy = "priceHour";
        sortOrder = "asc";
        break;
      case "price_desc":
        sortBy = "priceHour";
        sortOrder = "desc";
        break;
      case "rating":
        sortBy = "rating";
        sortOrder = "desc";
        break;
    }

    const params: GetCarsParams = {
      page: currentPage.value,
      pageSize: pageSize.value,
      sortBy,
      sortOrder,
    };

    console.log("🔍 loadCars called with sortType:", sortType.value);
    console.log("📊 Converted to params:", params);

    const response = await getCars(params);

    if (Array.isArray(response)) {
      cars.value = response;
      totalCount.value = response.length;
    } else {
      cars.value = response.items;
      totalCount.value = response.totalCount;
    }

    // ✅ ПОДРОБНЫЙ ЛОГ - теперь видно ВСЕ машины с ценами
    console.log(
      "🚗 ALL cars after load:",
      cars.value.map((car, idx) => ({
        index: idx,
        brand: car.brand,
        model: car.model,
        price: car.priceHour,
        rating: car.rating,
        year: car.year,
      }))
    );

    await loadAllCarBookings();
  } catch (e) {
    console.error("❌ Ошибка при загрузке авто:", e);
    error("Не удалось загрузить список автомобилей");
  } finally {
    loading.value = false;
  }
}

async function loadAllCarBookings() {
  try {
    const bookingsPromises = cars.value.map(async (car) => {
      try {
        const bookings = await getCarBookings(car.id);
        return { carId: car.id, bookings };
      } catch (e) {
        return { carId: car.id, bookings: [] };
      }
    });

    const results = await Promise.all(bookingsPromises);

    const bookingsMap: Record<number, Booking[]> = {};
    results.forEach((result) => {
      bookingsMap[result.carId] = result.bookings;
    });

    carBookings.value = bookingsMap;
  } catch (e) {
    console.error("Ошибка при загрузке бронирований:", e);
  }
}

function handlePageChange(page: number) {
  currentPage.value = page;
  loadCars();
  window.scrollTo({ top: 0, behavior: "smooth" });
}

function openBookingModal(car: CarWithStatus) {
  if (!isAuthenticated.value) {
    showLoginModal.value = true;
    return;
  }

  if (!car.isAvailable) {
    error("Этот автомобиль сейчас забронирован");
    return;
  }

  selectedCar.value = car;
  isModalOpen.value = true;
}

function closeBookingModal() {
  isModalOpen.value = false;
  setTimeout(() => {
    selectedCar.value = null;
  }, 300);
}

function goToLogin() {
  showLoginModal.value = false;
  router.push("/login");
}

async function handleBookingConfirm(startDate: string, endDate: string) {
  if (!selectedCar.value) return;

  const carId = selectedCar.value.id;
  const bookings = carBookings.value[carId] || [];

  const start = new Date(startDate);
  const end = new Date(endDate);

  if (!isCarAvailable(bookings, start, end)) {
    error("Этот автомобиль уже забронирован на выбранный период");
    return;
  }

  try {
    await createBooking(carId, startDate, endDate);
    success(
      `${selectedCar.value.brand} ${selectedCar.value.model} успешно забронирован!`
    );
    closeBookingModal();
    await loadAllCarBookings();
  } catch (e) {
    console.error("Ошибка бронирования:", e);
    error("Ошибка бронирования. Попробуйте снова.");
  }
}
</script>
