<template>
  <div
    class="min-h-screen bg-white dark:bg-slate-950 text-gray-900 dark:text-white"
  >
    <section class="relative overflow-hidden">
      <div
        class="absolute inset-0 bg-gradient-to-br from-blue-900 via-slate-900 to-blue-700 opacity-95"
      ></div>
      <div class="relative max-w-7xl mx-auto px-6 py-16 lg:px-10">
        <div class="max-w-3xl">
          <p
            class="text-sm uppercase tracking-[0.3em] text-blue-200 font-semibold"
          >
            Mobility Subscription
          </p>
          <h1
            class="mt-4 text-4xl md:text-5xl font-extrabold leading-tight text-white"
          >
            Choose a plan instead of renting every time
          </h1>
          <p class="mt-4 text-base md:text-lg text-blue-100 max-w-2xl">
            Get recurring access to cars with included bookings and simpler trip
            planning.
          </p>
        </div>
      </div>
    </section>

    <section class="max-w-7xl mx-auto px-6 py-10 lg:px-10 space-y-8">
      <div
        v-if="mySubscription"
        class="rounded-3xl border border-emerald-200 dark:border-emerald-800 bg-emerald-50 dark:bg-emerald-950/30 p-6 shadow-xl"
      >
        <div
          class="flex flex-col md:flex-row md:items-center md:justify-between gap-6"
        >
          <div>
            <p
              class="text-xs uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400 font-semibold"
            >
              Active subscription
            </p>
            <h2 class="mt-2 text-2xl font-extrabold">
              {{ mySubscription.planName }}
            </h2>
            <p class="mt-2 text-sm text-gray-600 dark:text-gray-300">
              Status:
              <span class="font-semibold">{{ mySubscription.status }}</span>
            </p>
            <p class="text-sm text-gray-600 dark:text-gray-300">
              Period: {{ formatDate(mySubscription.startDate) }} —
              {{ formatDate(mySubscription.endDate) }}
            </p>
          </div>

          <div class="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div class="rounded-2xl bg-white dark:bg-slate-900 p-4 shadow">
              <p class="text-xs uppercase tracking-[0.2em] text-gray-400">
                Included
              </p>
              <p class="mt-2 text-2xl font-extrabold">
                {{ mySubscription.includedBookings }}
              </p>
            </div>
            <div class="rounded-2xl bg-white dark:bg-slate-900 p-4 shadow">
              <p class="text-xs uppercase tracking-[0.2em] text-gray-400">
                Used
              </p>
              <p class="mt-2 text-2xl font-extrabold">
                {{ mySubscription.usedBookings }}
              </p>
            </div>
            <div
              class="rounded-2xl bg-white dark:bg-slate-900 p-4 shadow col-span-2 md:col-span-1"
            >
              <p class="text-xs uppercase tracking-[0.2em] text-gray-400">
                Remaining
              </p>
              <p
                class="mt-2 text-2xl font-extrabold text-emerald-600 dark:text-emerald-400"
              >
                {{ mySubscription.remainingBookings }}
              </p>
            </div>
          </div>
        </div>

        <div class="mt-6">
          <button
            @click="cancelSubscription"
            :disabled="actionLoading"
            class="px-5 py-3 rounded-2xl bg-red-600 text-white font-semibold hover:bg-red-700 disabled:opacity-60"
          >
            {{ actionLoading ? "Cancelling..." : "Cancel subscription" }}
          </button>
        </div>
      </div>

      <div class="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <p
            class="text-sm uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400 font-semibold"
          >
            Available plans
          </p>
          <h2 class="mt-2 text-3xl font-extrabold">Subscription plans</h2>
        </div>

        <button
          @click="loadData"
          :disabled="loading"
          class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-slate-700 bg-white dark:bg-slate-900 font-semibold hover:bg-gray-50 dark:hover:bg-slate-800"
        >
          {{ loading ? "Refreshing..." : "Refresh" }}
        </button>
      </div>

      <div
        v-if="errorMessage"
        class="rounded-2xl border border-red-200 bg-red-50 text-red-700 px-4 py-3 dark:border-red-900 dark:bg-red-950/30 dark:text-red-300"
      >
        {{ errorMessage }}
      </div>

      <div
        v-if="loading && plans.length === 0"
        class="text-gray-500 dark:text-gray-400"
      >
        Loading subscription plans...
      </div>

      <div v-else class="grid md:grid-cols-2 xl:grid-cols-3 gap-6">
        <div
          v-for="plan in plans"
          :key="plan.id"
          class="rounded-3xl border border-gray-200 dark:border-slate-800 bg-white dark:bg-slate-900 shadow-xl p-6 flex flex-col"
        >
          <div class="flex items-start justify-between gap-3">
            <div>
              <p
                class="text-xs uppercase tracking-[0.3em] text-blue-600 dark:text-blue-400 font-semibold"
              >
                {{ plan.planType }}
              </p>
              <h3 class="mt-2 text-2xl font-extrabold">{{ plan.name }}</h3>
            </div>

            <span
              class="px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300"
            >
              {{ getPlanBadge(plan.planType) }}
            </span>
          </div>

          <div class="mt-6">
            <p class="text-4xl font-extrabold">
              {{ formatPrice(plan.price) }}
            </p>
            <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
              Includes {{ plan.includedBookings }} bookings
            </p>
          </div>

          <div class="mt-6 space-y-3 text-sm text-gray-600 dark:text-gray-300">
            <div class="flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
              <span>{{ plan.includedBookings }} bookings included</span>
            </div>
            <div class="flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
              <span>Fast booking flow</span>
            </div>
            <div class="flex items-center gap-2">
              <span class="w-2 h-2 rounded-full bg-emerald-500"></span>
              <span>Good for repeat users</span>
            </div>
          </div>

          <div class="mt-8 pt-6 border-t border-gray-200 dark:border-slate-800">
            <label class="flex items-center gap-3 mb-4">
              <input
                v-model="autoRenewByPlan[plan.id]"
                type="checkbox"
                class="rounded"
              />
              <span class="text-sm text-gray-600 dark:text-gray-300"
                >Enable auto-renew</span
              >
            </label>

            <button
              @click="subscribe(plan.id)"
              :disabled="actionLoading || !!mySubscription"
              class="w-full px-5 py-3 rounded-2xl bg-blue-600 text-white font-semibold hover:bg-blue-700 disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {{
                actionLoading
                  ? "Processing..."
                  : mySubscription
                    ? "You already have a plan"
                    : "Subscribe"
              }}
            </button>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import api from "../api/axios";

type SubscriptionPlan = {
  id: number;
  name: string;
  planType: string;
  price: number;
  includedBookings: number;
};

type MySubscription = {
  id: number;
  subscriptionPlanId: number;
  planName: string;
  status: string;
  startDate: string;
  endDate: string;
  autoRenew: boolean;
  includedBookings: number;
  usedBookings: number;
  remainingBookings: number;
};

const plans = ref<SubscriptionPlan[]>([]);
const mySubscription = ref<MySubscription | null>(null);
const loading = ref(false);
const actionLoading = ref(false);
const errorMessage = ref("");
const autoRenewByPlan = ref<Record<number, boolean>>({});

function formatPrice(value: number) {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "KZT",
    maximumFractionDigits: 0,
  }).format(value);
}

function formatDate(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;

  return new Intl.DateTimeFormat("ru-RU", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(date);
}

function getPlanBadge(planType: string) {
  const normalized = planType.toLowerCase();
  if (normalized === "weekly") return "Flexible";
  if (normalized === "monthly") return "Popular";
  return "Plan";
}

async function loadPlans() {
  const { data } = await api.get("/subscriptions/plans");
  plans.value = data;
  for (const plan of plans.value) {
    if (!(plan.id in autoRenewByPlan.value)) {
      autoRenewByPlan.value[plan.id] = false;
    }
  }
}

async function loadMySubscription() {
  try {
    const { data } = await api.get("/subscriptions/my");
    mySubscription.value = data;
  } catch (error: any) {
    if (error?.response?.status === 401) {
      mySubscription.value = null;
      return;
    }

    mySubscription.value = null;
  }
}

async function loadData() {
  try {
    loading.value = true;
    errorMessage.value = "";

    await Promise.all([loadPlans(), loadMySubscription()]);
  } catch (error: any) {
    errorMessage.value =
      error?.response?.data?.detail ||
      error?.response?.data?.message ||
      "Failed to load subscription data.";
  } finally {
    loading.value = false;
  }
}

async function subscribe(subscriptionPlanId: number) {
  try {
    actionLoading.value = true;
    errorMessage.value = "";

    await api.post("/subscriptions", {
      subscriptionPlanId,
      autoRenew: !!autoRenewByPlan.value[subscriptionPlanId],
    });

    await loadData();
  } catch (error: any) {
    errorMessage.value =
      error?.response?.data?.detail ||
      error?.response?.data?.message ||
      "Failed to create subscription.";
  } finally {
    actionLoading.value = false;
  }
}

async function cancelSubscription() {
  if (!mySubscription.value) return;

  try {
    actionLoading.value = true;
    errorMessage.value = "";

    await api.post(`/subscriptions/${mySubscription.value.id}/cancel`);
    await loadData();
  } catch (error: any) {
    errorMessage.value =
      error?.response?.data?.detail ||
      error?.response?.data?.message ||
      "Failed to cancel subscription.";
  } finally {
    actionLoading.value = false;
  }
}

onMounted(loadData);
</script>
