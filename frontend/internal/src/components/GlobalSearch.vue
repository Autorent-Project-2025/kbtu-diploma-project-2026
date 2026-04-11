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
import { getAllTickets } from "../api/tickets";
import { auth } from "../store/auth";

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
let searchCounter = 0;

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
  const thisSearch = ++searchCounter;
  searching.value = true;
  const found: SearchResult[] = [];

  try {
    const promises: Promise<unknown>[] = [
      getClients(q),
      getPartners(q),
      getPartnerCars({ page: 1, pageSize: 5, search: q }),
      getAllBookings({ page: 1, pageSize: 5, search: q }),
    ];

    if (auth.hasPermission("Ticket.ViewAll")) {
      promises.push(getAllTickets(q));
    }

    const settled = await Promise.allSettled(promises);
    if (thisSearch !== searchCounter) return;

    // Clients
    const clients = settled[0];
    if (clients.status === "fulfilled") {
      for (const c of (clients.value as ReturnType<typeof getClients> extends Promise<infer R> ? R : never).slice(0, 5)) {
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

    // Partners
    const partners = settled[1];
    if (partners.status === "fulfilled") {
      for (const p of (partners.value as ReturnType<typeof getPartners> extends Promise<infer R> ? R : never).slice(0, 5)) {
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

    // Cars
    const carsResult = settled[2];
    if (carsResult.status === "fulfilled") {
      const carsData = carsResult.value as { items: Array<{ id: number; modelBrand: string; modelName: string; modelYear: number; licensePlate: string }> };
      for (const car of carsData.items) {
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

    // Bookings
    const bookingsResult = settled[3];
    if (bookingsResult.status === "fulfilled") {
      const bookingsData = bookingsResult.value as { items: Array<{ id: number; carBrand: string; carModel: string }> };
      for (const b of bookingsData.items) {
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

    // Tickets
    if (settled.length > 4) {
      const ticketsResult = settled[4];
      if (ticketsResult.status === "fulfilled") {
        const tickets = ticketsResult.value as Array<{ id: string; fullName?: string; email?: string; phoneNumber?: string; ticketType?: string; status?: string }>;
        for (const t of tickets.slice(0, 5)) {
          found.push({
            type: "ticket",
            id: t.id,
            title: t.fullName || `Заявка ${t.id.slice(0, 8)}...`,
            subtitle: t.email || t.phoneNumber || t.ticketType || "",
            badge: "T",
            badgeCss: "bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400",
            typeLabel: "Заявка",
            route: `/tickets`,
          });
        }
      }
    }
  } catch {
    // Silently fail partial searches
  }

  if (thisSearch === searchCounter) {
    results.value = found.slice(0, 15);
    searching.value = false;
  }
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
