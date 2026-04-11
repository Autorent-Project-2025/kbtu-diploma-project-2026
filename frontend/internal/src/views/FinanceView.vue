<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Финансы</h1>
        <p class="text-sm text-gray-500 dark:text-gray-400 mt-1">Обзор кошельков и выплат партнёров</p>
      </div>
    </div>

    <div class="max-w-7xl mx-auto px-6 py-6 space-y-6">
      <!-- Loading -->
      <div v-if="loading" class="space-y-4">
        <div v-for="i in 3" :key="i" class="h-24 bg-white dark:bg-gray-900 rounded-2xl animate-pulse" />
      </div>

      <template v-else>
        <!-- Aggregate stats -->
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 p-5">
            <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Всего партнёров</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">{{ partners.length }}</p>
          </div>
          <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 p-5">
            <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Загружено кошельков</p>
            <p class="text-2xl font-bold text-gray-900 dark:text-white mt-1">{{ wallets.length }}</p>
          </div>
          <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 p-5">
            <p class="text-xs font-semibold text-gray-400 uppercase tracking-wider">Общий баланс</p>
            <p class="text-2xl font-bold text-emerald-600 dark:text-emerald-400 mt-1">{{ formatPrice(totalBalance) }}</p>
          </div>
        </div>

        <!-- Partner search -->
        <div class="relative">
          <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
          <input
            v-model="search"
            type="text"
            placeholder="Поиск партнёра..."
            class="w-full pl-10 pr-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 transition-colors"
          />
        </div>

        <!-- Partner wallets -->
        <div class="bg-white dark:bg-gray-900 rounded-2xl border border-gray-200 dark:border-gray-800 overflow-hidden">
          <div class="px-6 py-4 border-b border-gray-100 dark:border-gray-800">
            <h2 class="text-sm font-bold text-gray-900 dark:text-white">Кошельки партнёров</h2>
          </div>

          <div v-if="filteredPartners.length === 0" class="p-8 text-center">
            <p class="text-sm text-gray-400">Нет данных</p>
          </div>

          <table v-else class="w-full">
            <thead>
              <tr class="border-b border-gray-100 dark:border-gray-800">
                <th class="text-left text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Партнёр</th>
                <th class="text-right text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Баланс</th>
                <th class="text-center text-xs font-semibold text-gray-500 dark:text-gray-400 uppercase tracking-wider px-6 py-3">Действия</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
              <tr v-for="pw in filteredPartners" :key="pw.partner.id" class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors">
                <td class="px-6 py-4">
                  <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-violet-100 dark:bg-violet-900/30 flex items-center justify-center text-violet-700 dark:text-violet-400 text-xs font-bold flex-shrink-0">
                      {{ (pw.partner.ownerFirstName?.[0] ?? "") + (pw.partner.ownerLastName?.[0] ?? "") }}
                    </div>
                    <div>
                      <p class="text-sm font-semibold text-gray-900 dark:text-white">{{ pw.partner.ownerFirstName }} {{ pw.partner.ownerLastName }}</p>
                      <p class="text-xs text-gray-400">{{ pw.partner.phoneNumber || "—" }}</p>
                    </div>
                  </div>
                </td>
                <td class="px-6 py-4 text-right">
                  <span :class="['text-sm font-bold', (pw.wallet?.balance ?? 0) >= 0 ? 'text-emerald-600 dark:text-emerald-400' : 'text-red-600 dark:text-red-400']">
                    {{ formatPrice(pw.wallet?.balance) }}
                  </span>
                </td>
                <td class="px-6 py-4 text-center">
                  <router-link
                    :to="`/partners/${pw.partner.id}`"
                    class="text-xs font-semibold text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
                  >
                    Подробнее
                  </router-link>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { getPartners, getPartnerWallet, type PartnerDto, type PartnerWalletDto } from "../api/partners";
import { formatPrice } from "../utils/formatters";
import { useToast } from "../composables/useToast";

const toast = useToast();

const partners = ref<PartnerDto[]>([]);
const wallets = ref<{ partnerId: number; wallet: PartnerWalletDto | null }[]>([]);
const loading = ref(true);
const search = ref("");

interface PartnerWithWallet {
  partner: PartnerDto;
  wallet: PartnerWalletDto | null;
}

const partnerWallets = computed<PartnerWithWallet[]>(() =>
  partners.value.map((p) => ({
    partner: p,
    wallet: wallets.value.find((w) => w.partnerId === p.id)?.wallet ?? null,
  })),
);

const filteredPartners = computed(() => {
  if (!search.value.trim()) return partnerWallets.value;
  const q = search.value.toLowerCase();
  return partnerWallets.value.filter(
    (pw) =>
      pw.partner.ownerFirstName?.toLowerCase().includes(q) ||
      pw.partner.ownerLastName?.toLowerCase().includes(q) ||
      pw.partner.phoneNumber?.toLowerCase().includes(q),
  );
});

const totalBalance = computed(() =>
  wallets.value.reduce((sum, w) => sum + (w.wallet?.balance ?? 0), 0),
);

async function loadData() {
  loading.value = true;
  try {
    partners.value = await getPartners();

    // Load wallets in parallel, tolerating individual failures
    const results = await Promise.allSettled(
      partners.value.map(async (p) => {
        try {
          const wallet = await getPartnerWallet(p.id);
          return { partnerId: p.id, wallet };
        } catch {
          return { partnerId: p.id, wallet: null };
        }
      }),
    );

    wallets.value = results
      .filter((r): r is PromiseFulfilledResult<{ partnerId: number; wallet: PartnerWalletDto | null }> => r.status === "fulfilled")
      .map((r) => r.value);
  } catch (e: any) {
    toast.error("Ошибка загрузки: " + (e?.response?.data?.error ?? e.message));
  } finally {
    loading.value = false;
  }
}

onMounted(loadData);
</script>
