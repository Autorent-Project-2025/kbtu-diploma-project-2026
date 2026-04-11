<template>
  <div
    v-if="showWorkspace"
    class="min-h-screen bg-gray-50 dark:bg-gray-950 flex"
  >
    <!-- Sidebar -->
    <aside
      class="w-64 flex-shrink-0 bg-gray-900 dark:bg-gray-950 border-r border-gray-800 flex flex-col"
    >
      <div class="p-6 border-b border-gray-800">
        <div class="flex items-center gap-3">
          <div
            class="w-9 h-9 rounded-xl bg-gradient-to-br from-emerald-500 to-emerald-700 flex items-center justify-center text-white text-xs font-extrabold"
          >
            AR
          </div>
          <div>
            <p class="text-white font-bold text-sm">AutoRent</p>
            <p class="text-gray-400 text-xs">Operations CRM</p>
          </div>
        </div>
      </div>

      <div class="px-4 pt-4 pb-2">
        <GlobalSearch />
      </div>

      <nav class="flex-1 min-h-0 overflow-y-auto p-4 space-y-1">
        <router-link
          v-for="link in visibleNav"
          :key="link.to"
          :to="link.to"
          :class="[
            'flex items-center gap-3 px-4 py-2.5 rounded-xl border transition-colors font-semibold text-sm',
            isActive(link.to)
              ? 'bg-gray-800 text-white border-emerald-500'
              : 'border-transparent text-gray-400 hover:text-white hover:bg-gray-800',
          ]"
        >
          <span class="w-5 text-center" v-html="link.icon" />
          {{ link.label }}
        </router-link>
      </nav>

      <div class="p-4 border-t border-gray-800">
        <button
          @click="logout"
          class="w-full px-4 py-2.5 rounded-xl border border-gray-700 text-gray-300 hover:text-white hover:border-gray-600 font-semibold text-sm transition-colors"
        >
          Выйти
        </button>
      </div>
    </aside>

    <main class="flex-1 min-w-0">
      <router-view />
    </main>
  </div>

  <div v-else class="min-h-screen bg-gray-50 dark:bg-gray-950">
    <router-view />
  </div>

  <ToastContainer />
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { auth } from "./store/auth";
import ToastContainer from "./components/ToastContainer.vue";
import GlobalSearch from "./components/GlobalSearch.vue";

interface NavLink {
  to: string;
  label: string;
  permission: string;
  icon: string;
}

const navLinks: NavLink[] = [
  {
    to: "/tickets",
    label: "Заявки",
    permission: "Ticket.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/></svg>',
  },
  {
    to: "/clients",
    label: "Клиенты",
    permission: "Client.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/></svg>',
  },
  {
    to: "/partners",
    label: "Партнёры",
    permission: "Partner.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"/></svg>',
  },
  {
    to: "/cars",
    label: "Машины",
    permission: "PartnerCar.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M8 17h.01M16 17h.01M3 11l1.5-5A2 2 0 016.4 4h11.2a2 2 0 011.9 1.4L21 11M3 11v6a1 1 0 001 1h1m16-7v6a1 1 0 01-1 1h-1M3 11h18"/></svg>',
  },
  {
    to: "/bookings",
    label: "Бронирования",
    permission: "Booking.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>',
  },
  {
    to: "/complaints",
    label: "Жалобы",
    permission: "Complaint.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"/></svg>',
  },
  {
    to: "/complaints/access-requests",
    label: "Запросы доступа",
    permission: "AccessRequest.Review",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z"/></svg>',
  },
  {
    to: "/finance",
    label: "Финансы",
    permission: "Partner.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>',
  },
  {
    to: "/super",
    label: "Обзор системы",
    permission: "Ticket.ViewAll",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>',
  },
  {
    to: "/admin",
    label: "Администрирование",
    permission: "User.View",
    icon: '<svg xmlns="http://www.w3.org/2000/svg" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z"/><path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"/></svg>',
  },
];

const route = useRoute();
const router = useRouter();

const isAuthenticated = computed(() => Boolean(auth.token));
const showWorkspace = computed(
  () => isAuthenticated.value && route.path !== "/login",
);
const visibleNav = computed(() =>
  navLinks.filter((link) => auth.hasPermission(link.permission)),
);

function isActive(to: string): boolean {
  return route.path === to || route.path.startsWith(to + "/");
}

function logout() {
  auth.logout();
  router.push("/login");
}
</script>
