<template>
  <section
    class="rounded-[32px] border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-2xl overflow-hidden"
  >
    <div
      class="relative px-6 py-8 sm:px-8 sm:py-10 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.14),_transparent_30%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.14),_transparent_35%),linear-gradient(135deg,_rgba(255,255,255,0.98),_rgba(243,244,246,0.96))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_30%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.18),_transparent_35%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))]"
    >
      <div
        class="flex flex-col lg:flex-row lg:items-start lg:justify-between gap-8"
      >
        <div class="max-w-2xl space-y-3">
          <p
            class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
          >
            Smart Match
          </p>
          <h2
            class="text-3xl sm:text-4xl font-extrabold text-gray-900 dark:text-white"
          >
            Let the platform pick the best car for you
          </h2>
          <p class="text-gray-600 dark:text-gray-400 text-base leading-relaxed">
            Tell us your budget and trip purpose — we’ll recommend the strongest
            matches in seconds.
          </p>
        </div>

        <div class="shrink-0">
          <button
            @click="fetchRecommendations"
            :disabled="loading"
            class="px-6 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
          >
            {{ loading ? "Finding..." : "Find my car" }}
          </button>
        </div>
      </div>

      <div class="mt-8 grid md:grid-cols-2 xl:grid-cols-4 gap-4">
        <div class="space-y-1.5">
          <label
            class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
          >
            Budget / hour
          </label>
          <input
            v-model.number="filters.maxBudgetPerHour"
            type="number"
            min="0"
            placeholder="e.g. 6000"
            class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white/90 dark:bg-gray-950/70 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all"
          />
        </div>

        <div class="space-y-1.5">
          <label
            class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
          >
            Passengers
          </label>
          <input
            v-model.number="filters.passengers"
            type="number"
            min="1"
            placeholder="e.g. 4"
            class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white/90 dark:bg-gray-950/70 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all"
          />
        </div>

        <div class="space-y-1.5">
          <label
            class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
          >
            Trip purpose
          </label>
          <select
            v-model="filters.tripPurpose"
            class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white/90 dark:bg-gray-950/70 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all"
          >
            <option value="">Any</option>
            <option value="city">City</option>
            <option value="family">Family</option>
            <option value="business">Business</option>
            <option value="luxury">Luxury</option>
            <option value="travel">Travel</option>
          </select>
        </div>

        <div class="space-y-1.5">
          <label
            class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
          >
            Transmission
          </label>
          <select
            v-model="filters.transmission"
            class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white/90 dark:bg-gray-950/70 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-all"
          >
            <option value="">Any</option>
            <option value="automatic">Automatic</option>
            <option value="manual">Manual</option>
          </select>
        </div>
      </div>
    </div>

    <div class="p-6 sm:p-8">
      <div
        v-if="errorMessage"
        class="mb-6 rounded-2xl border border-red-200 dark:border-red-900 bg-red-50 dark:bg-red-950/20 px-4 py-3 text-sm text-red-700 dark:text-red-300"
      >
        {{ errorMessage }}
      </div>

      <div v-if="loading" class="grid md:grid-cols-2 xl:grid-cols-3 gap-5">
        <div
          v-for="n in 3"
          :key="n"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 p-4 animate-pulse bg-gray-50 dark:bg-gray-800/40"
        >
          <div class="h-44 rounded-2xl bg-gray-200 dark:bg-gray-700"></div>
          <div
            class="mt-4 h-5 w-2/3 rounded bg-gray-200 dark:bg-gray-700"
          ></div>
          <div
            class="mt-3 h-4 w-1/2 rounded bg-gray-200 dark:bg-gray-700"
          ></div>
          <div class="mt-6 h-10 rounded-2xl bg-gray-200 dark:bg-gray-700"></div>
        </div>
      </div>

      <div v-else-if="recommendations.length > 0" class="space-y-5">
        <div class="flex items-center justify-between gap-4 flex-wrap">
          <div>
            <p
              class="text-sm font-bold uppercase tracking-[0.2em] text-emerald-600 dark:text-emerald-400"
            >
              Personalized results
            </p>
            <h3 class="text-2xl font-extrabold text-gray-900 dark:text-white">
              Best matches for your trip
            </h3>
          </div>

          <button
            @click="resetFilters"
            class="px-4 py-2 rounded-2xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors"
          >
            Reset filters
          </button>
        </div>

        <div class="grid md:grid-cols-2 xl:grid-cols-3 gap-6">
          <article
            v-for="car in recommendations"
            :key="car.partnerCarId"
            class="group rounded-3xl overflow-hidden border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl hover:shadow-2xl transition-all"
          >
            <div class="relative h-52 overflow-hidden">
              <img
                v-if="car.imageUrl"
                :src="car.imageUrl"
                :alt="`${car.brand} ${car.model}`"
                class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500"
              />
              <div
                v-else
                class="w-full h-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 text-gray-400"
              >
                No image
              </div>

              <div class="absolute top-4 left-4 flex flex-wrap gap-2">
                <span
                  class="px-3 py-1 rounded-full bg-emerald-600 text-white text-xs font-bold shadow"
                >
                  {{ car.reasonTag }}
                </span>
                <span
                  class="px-3 py-1 rounded-full bg-black/70 text-white text-xs font-bold shadow"
                >
                  Score {{ car.score }}
                </span>
              </div>
            </div>

            <div class="p-5 space-y-4">
              <div>
                <h4
                  class="text-xl font-extrabold text-gray-900 dark:text-white"
                >
                  {{ car.brand }} {{ car.model }}
                </h4>
                <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">
                  {{ car.year }} ·
                  {{ car.transmission || "Transmission n/a" }} ·
                  {{ car.seats ?? "—" }} seats
                </p>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div class="rounded-2xl bg-gray-50 dark:bg-gray-800/60 p-3">
                  <p class="text-xs uppercase tracking-[0.12em] text-gray-400">
                    Price / hour
                  </p>
                  <p
                    class="mt-1 text-lg font-bold text-gray-900 dark:text-white"
                  >
                    {{ formatMoney(car.priceHour) }}
                  </p>
                </div>

                <div class="rounded-2xl bg-gray-50 dark:bg-gray-800/60 p-3">
                  <p class="text-xs uppercase tracking-[0.12em] text-gray-400">
                    Rating
                  </p>
                  <p
                    class="mt-1 text-lg font-bold text-gray-900 dark:text-white"
                  >
                    {{ car.rating ?? "—" }}
                  </p>
                </div>
              </div>

              <router-link
                :to="`/cars/${car.carModelId}`"
                class="inline-flex w-full items-center justify-center gap-2 px-5 py-3 rounded-2xl bg-gray-900 hover:bg-black dark:bg-white dark:text-gray-900 dark:hover:bg-gray-100 text-white font-bold transition-colors"
              >
                View details
                <svg
                  class="w-4 h-4"
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
              </router-link>
            </div>
          </article>
        </div>
      </div>

      <div
        v-else
        class="rounded-3xl border-2 border-dashed border-gray-200 dark:border-gray-800 p-10 text-center"
      >
        <p class="text-4xl mb-4">🤖</p>
        <h3 class="text-2xl font-extrabold text-gray-900 dark:text-white">
          Smart picks will appear here
        </h3>
        <p class="mt-2 text-gray-600 dark:text-gray-400">
          Fill in your preferences and let the system find the strongest match.
        </p>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from "vue";
import axios from "axios";

type RecommendationItem = {
  partnerCarId: number;
  carModelId: number;
  brand: string;
  model: string;
  year: number;
  priceHour: number | null;
  priceDay: number | null;
  seats: number | null;
  transmission: string | null;
  rating: number | null;
  score: number;
  reasonTag: string;
  imageUrl: string | null;
};

const loading = ref(false);
const errorMessage = ref("");
const recommendations = ref<RecommendationItem[]>([]);

const filters = ref({
  maxBudgetPerHour: null as number | null,
  passengers: null as number | null,
  tripPurpose: "",
  transmission: "",
});

function formatMoney(value: number | null) {
  if (value == null) return "—";
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "KZT",
    maximumFractionDigits: 0,
  }).format(value);
}

function resetFilters() {
  filters.value = {
    maxBudgetPerHour: null,
    passengers: null,
    tripPurpose: "",
    transmission: "",
  };
  recommendations.value = [];
  errorMessage.value = "";
}

async function fetchRecommendations() {
  try {
    loading.value = true;
    errorMessage.value = "";

    const { data } = await axios.get("/recommendations", {
      params: filters.value,
    });

    recommendations.value = Array.isArray(data) ? data : [];
  } catch (error: any) {
    recommendations.value = [];
    errorMessage.value =
      error?.response?.data?.detail ||
      error?.response?.data?.message ||
      "Failed to load recommendations.";
  } finally {
    loading.value = false;
  }
}
</script>
