<template>
  <div
    class="min-h-screen bg-white dark:bg-slate-950 text-gray-900 dark:text-white p-6"
  >
    <div class="max-w-6xl mx-auto space-y-8">
      <div class="space-y-2">
        <h1 class="text-3xl font-bold">Find the best car for your trip</h1>
        <p class="text-gray-600 dark:text-gray-400">
          Tell us what you need and we’ll recommend the best options.
        </p>
      </div>

      <div
        class="grid md:grid-cols-4 gap-4 p-6 rounded-3xl bg-gray-50 dark:bg-slate-900 shadow-xl"
      >
        <input
          v-model.number="filters.maxBudgetPerHour"
          type="number"
          placeholder="Max budget / hour"
          class="px-4 py-3 rounded-xl border bg-white dark:bg-slate-800"
        />

        <input
          v-model.number="filters.passengers"
          type="number"
          placeholder="Passengers"
          class="px-4 py-3 rounded-xl border bg-white dark:bg-slate-800"
        />

        <select
          v-model="filters.tripPurpose"
          class="px-4 py-3 rounded-xl border bg-white dark:bg-slate-800"
        >
          <option value="">Trip purpose</option>
          <option value="city">City</option>
          <option value="family">Family</option>
          <option value="business">Business</option>
          <option value="luxury">Luxury</option>
          <option value="travel">Travel</option>
        </select>

        <select
          v-model="filters.transmission"
          class="px-4 py-3 rounded-xl border bg-white dark:bg-slate-800"
        >
          <option value="">Transmission</option>
          <option value="automatic">Automatic</option>
          <option value="manual">Manual</option>
        </select>
      </div>

      <div class="flex gap-3">
        <button
          @click="fetchRecommendations"
          class="px-6 py-3 rounded-2xl bg-blue-600 text-white font-semibold hover:bg-blue-700"
        >
          Get recommendations
        </button>
      </div>

      <div v-if="loading" class="text-gray-500">Loading recommendations...</div>

      <div
        v-if="recommendations.length"
        class="grid md:grid-cols-2 xl:grid-cols-3 gap-6"
      >
        <div
          v-for="car in recommendations"
          :key="car.id"
          class="rounded-3xl overflow-hidden bg-white dark:bg-slate-900 shadow-xl border border-gray-200 dark:border-slate-800"
        >
          <img
            :src="car.imageUrl || 'https://placehold.co/600x400'"
            class="w-full h-52 object-cover"
            alt="Car image"
          />

          <div class="p-5 space-y-3">
            <div class="flex items-start justify-between gap-3">
              <div>
                <h3 class="text-xl font-bold">
                  {{ car.brand }} {{ car.model }}
                </h3>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  {{ car.year }}
                </p>
              </div>

              <span
                class="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300"
              >
                {{ car.reasonTag }}
              </span>
            </div>

            <div class="flex items-center justify-between">
              <span class="font-semibold"
                >{{ car.priceHour ?? "—" }} / hour</span
              >
              <span class="text-sm text-gray-500">Score: {{ car.score }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-else-if="!loading" class="text-gray-500">
        No recommendations yet.
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import axios from "axios";

type RecommendedCar = {
  id: number;
  brand: string;
  model: string;
  year: number;
  priceHour: number | null;
  score: number;
  reasonTag: string;
  imageUrl: string;
};

const loading = ref(false);

const filters = ref({
  maxBudgetPerHour: null as number | null,
  passengers: null as number | null,
  tripPurpose: "",
  transmission: "",
});

const recommendations = ref<RecommendedCar[]>([]);

async function fetchRecommendations() {
  try {
    loading.value = true;

    const { data } = await axios.get("/recommendations", {
      params: filters.value,
    });

    recommendations.value = data;
  } catch (error) {
    console.error("Failed to load recommendations:", error);
    recommendations.value = [];
  } finally {
    loading.value = false;
  }
}
</script>
