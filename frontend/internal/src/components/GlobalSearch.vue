<template>
  <div class="relative" ref="containerRef">
    <!-- Trigger button -->
    <button
      @click="open = true"
      class="flex items-center gap-2 px-3 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 hover:border-gray-300 transition-colors w-full"
    >
      <svg class="w-4 h-4 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
      </svg>
      <span>Поиск...</span>
      <kbd class="ml-auto hidden sm:inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded bg-gray-100 dark:bg-gray-700 text-xs text-gray-400 font-mono">
        Ctrl+K
      </kbd>
    </button>

    <!-- Search modal -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div
          v-if="open"
          class="fixed inset-0 z-50 bg-black/40 backdrop-blur-sm flex items-start justify-center pt-[15vh]"
          @click.self="open = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 w-full max-w-lg mx-4 overflow-hidden">
            <!-- Search input -->
            <div class="flex items-center gap-3 px-4 py-3 border-b border-gray-100 dark:border-gray-800">
              <svg class="w-5 h-5 text-gray-400 flex-shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <input
                ref="inputRef"
                v-model="query"
                type="text"
                placeholder="Поиск по email, телефону, гос. номеру, ID..."
                class="flex-1 bg-transparent text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none"
                @keydown.escape="open = false"
                @keydown.enter="navigateToFirst"
              />
              <button
                v-if="query"
                @click="query = ''"
                class="text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
              >
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <!-- Results -->
            <div class="max-h-80 overflow-y-auto">
              <div v-if="searching" class="p-6 text-center text-sm text-gray-400">
                Поиск...
              </div>

              <div v-else-if="query.length < 2" class="p-6 text-center text-sm text-gray-400">
                Введите минимум 2 символа
              </div>

              <div v-else-if="results.length === 0" class="p-6 text-center text-sm text-gray-400">
                Ничего не найдено
              </div>

              <div v-else class="py-2">
                <button
                  v-for="r in results"
                  :key="r.type + r.id"
                  @click="navigateTo(r)"
                  class="w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
                >
                  <span :class="['w-8 h-8 rounded-lg flex items-center justify-center text-xs font-bold flex-shrink-0', r.badgeCss]">
                    {{ r.badge }}
                  </span>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-semibold text-gray-900 dark:text-white truncate">{{ r.title }}</p>
                    <p class="text-xs text-gray-400 truncate">{{ r.subtitle }}</p>
                  </div>
                  <span class="text-xs text-gray-400 flex-shrink-0">{{ r.typeLabel }}</span>
                </button>
              </div>
            </div>

            <!-- Footer -->
            <div class="px-4 py-2 border-t border-gray-100 dark:border-gray-800 flex items-center gap-4 text-xs text-gray-400">
              <span><kbd class="px-1 py-0.5 rounded bg-gray-100 dark:bg-gray-700 font-mono">Enter</kbd> перейти</span>
              <span><kbd class="px-1 py-0.5 rounded bg-gray-100 dark:bg-gray-700 font-mono">Esc</kbd> закрыть</span>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, onUnmounted, nextTick } from "vue";
import { useRouter } from "vue-router";
import { getClients } from "../api/clients";
import { getPartners } from "../api/partners";
import { getPartnerCars } from "../api/cars";
import { getAllBookings } from "../api/bookings";

interface SearchResult {
  type: string;
  id: string | number;
  title: string;
  subtitle: string;
  badge: string;
  badgeCss: string;
  typeLabel: string;
  route: string;
}

const router = useRouter();
const containerRef = ref<HTMLElement>();
const inputRef = ref<HTMLInputElement>();
const open = ref(false);
const query = ref("");
const searching = ref(false);
const results = ref<SearchResult[]>([]);

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

watch(open, async (val) => {
  if (val) {
    await nextTick();
    inputRef.value?.focus();
  } else {
    query.value = "";
    results.value = [];
  }
});

watch(query, (val) => {
  if (debounceTimer) clearTimeout(debounceTimer);
  if (val.length < 2) {
    results.value = [];
    return;
  }
  debounceTimer = setTimeout(() => search(val), 300);
});

async function search(q: string) {
  searching.value = true;
  const found: SearchResult[] = [];
  const lowerQ = q.toLowerCase();

  try {
    const [clients, partners, carsResult, bookingsResult] = await Promise.allSettled([
      getClients(),
      getPartners(),
      getPartnerCars({ page: 1, pageSize: 50 }),
      getAllBookings({ page: 1, pageSize: 50 }),
    ]);

    // Search clients
    if (clients.status === "fulfilled") {
      for (const c of clients.value) {
        if (
          c.firstName?.toLowerCase().includes(lowerQ) ||
          c.lastName?.toLowerCase().includes(lowerQ) ||
          c.phoneNumber?.toLowerCase().includes(lowerQ) ||
          String(c.id).includes(q) ||
          c.relatedUserId?.toLowerCase().includes(lowerQ)
        ) {
          found.push({
            type: "client",
            id: c.id,
            title: `${c.firstName} ${c.lastName}`,
            subtitle: c.phoneNumber || c.relatedUserId,
            badge: (c.firstName?.[0] ?? "") + (c.lastName?.[0] ?? ""),
            badgeCss: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
            typeLabel: "Клиент",
            route: `/clients/${c.id}`,
          });
        }
      }
    }

    // Search partners
    if (partners.status === "fulfilled") {
      for (const p of partners.value) {
        if (
          p.ownerFirstName?.toLowerCase().includes(lowerQ) ||
          p.ownerLastName?.toLowerCase().includes(lowerQ) ||
          p.phoneNumber?.toLowerCase().includes(lowerQ) ||
          String(p.id).includes(q)
        ) {
          found.push({
            type: "partner",
            id: p.id,
            title: `${p.ownerFirstName} ${p.ownerLastName}`,
            subtitle: p.phoneNumber || `ID: ${p.id}`,
            badge: (p.ownerFirstName?.[0] ?? "") + (p.ownerLastName?.[0] ?? ""),
            badgeCss: "bg-violet-100 text-violet-700 dark:bg-violet-900/30 dark:text-violet-400",
            typeLabel: "Партнёр",
            route: `/partners/${p.id}`,
          });
        }
      }
    }

    // Search cars
    if (carsResult.status === "fulfilled") {
      for (const car of carsResult.value.items) {
        if (
          car.licensePlate?.toLowerCase().includes(lowerQ) ||
          car.modelBrand?.toLowerCase().includes(lowerQ) ||
          car.modelName?.toLowerCase().includes(lowerQ) ||
          String(car.id).includes(q)
        ) {
          found.push({
            type: "car",
            id: car.id,
            title: `${car.modelBrand} ${car.modelName} (${car.modelYear})`,
            subtitle: car.licensePlate,
            badge: "A",
            badgeCss: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
            typeLabel: "Машина",
            route: `/cars/${car.id}`,
          });
        }
      }
    }

    // Search bookings
    if (bookingsResult.status === "fulfilled") {
      for (const b of bookingsResult.value.items) {
        if (
          String(b.id).includes(q) ||
          b.carBrand?.toLowerCase().includes(lowerQ) ||
          b.carModel?.toLowerCase().includes(lowerQ) ||
          b.userId?.toLowerCase().includes(lowerQ) ||
          b.partnerUserId?.toLowerCase().includes(lowerQ)
        ) {
          found.push({
            type: "booking",
            id: b.id,
            title: `Бронирование #${b.id}`,
            subtitle: `${b.carBrand} ${b.carModel}`,
            badge: "#",
            badgeCss: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
            typeLabel: "Бронирование",
            route: `/bookings/${b.id}`,
          });
        }
      }
    }
  } catch {
    // Silently fail partial searches
  }

  results.value = found.slice(0, 15);
  searching.value = false;
}

function navigateTo(r: SearchResult) {
  open.value = false;
  router.push(r.route);
}

function navigateToFirst() {
  if (results.value.length > 0) {
    navigateTo(results.value[0]);
  }
}

function handleKeydown(e: KeyboardEvent) {
  if ((e.ctrlKey || e.metaKey) && e.key === "k") {
    e.preventDefault();
    open.value = !open.value;
  }
}

onMounted(() => document.addEventListener("keydown", handleKeydown));
onUnmounted(() => document.removeEventListener("keydown", handleKeydown));
</script>
