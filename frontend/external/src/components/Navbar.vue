<template>
  <nav
    class="fixed top-0 left-0 right-0 z-50 transition-all duration-300"
    :class="[
      scrolled
        ? 'bg-white/80 dark:bg-gray-900/80 backdrop-blur-xl shadow-lg'
        : 'bg-transparent',
    ]"
  >
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between h-20">
        <div class="flex items-center gap-12">
          <!-- Logo -->
          <div
            class="shrink-0 flex items-center cursor-pointer group"
            @click="$router.push('/')"
          >
            <span
              class="text-2xl font-bold bg-gradient-to-r from-primary-600 to-primary-400 bg-clip-text text-transparent group-hover:scale-105 transition-transform"
            >
              AutoRent
            </span>
          </div>

          <!-- Desktop Navigation -->
          <div class="hidden md:flex items-center gap-8">
            <router-link
              to="/"
              exact-active-class="text-primary-600 dark:text-primary-400"
              class="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 font-medium transition-colors relative group"
            >
              <span>Главная</span>
              <span
                class="absolute bottom-0 left-0 w-0 h-0.5 bg-primary-600 dark:bg-primary-400 group-hover:w-full transition-all duration-300"
              ></span>
            </router-link>

            <router-link
              to="/cars"
              active-class="text-primary-600 dark:text-primary-400"
              class="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 font-medium transition-colors relative group"
            >
              <span>Автомобили</span>
              <span
                class="absolute bottom-0 left-0 w-0 h-0.5 bg-primary-600 dark:bg-primary-400 group-hover:w-full transition-all duration-300"
              ></span>
            </router-link>

            <router-link
              v-if="!isAuthenticated"
              to="/apply"
              active-class="text-primary-600 dark:text-primary-400"
              class="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 font-medium transition-colors relative group"
            >
              <span>Подать заявку</span>
              <span
                class="absolute bottom-0 left-0 w-0 h-0.5 bg-primary-600 dark:bg-primary-400 group-hover:w-full transition-all duration-300"
              ></span>
            </router-link>

            <router-link
              v-if="isAuthenticated"
              to="/bookings"
              active-class="text-primary-600 dark:text-primary-400"
              class="text-gray-700 dark:text-gray-300 hover:text-primary-600 dark:hover:text-primary-400 font-medium transition-colors relative group"
            >
              <span>Мои бронирования</span>
              <span
                class="absolute bottom-0 left-0 w-0 h-0.5 bg-primary-600 dark:bg-primary-400 group-hover:w-full transition-all duration-300"
              ></span>
            </router-link>
          </div>
        </div>

        <!-- Right side -->
        <div class="flex items-center gap-4">
          <!-- Theme Toggle -->
          <ThemeToggle />

          <!-- Auth Buttons -->
          <div v-if="!isAuthenticated">
            <button
              @click="$router.push('/login')"
              class="px-6 py-2.5 bg-primary-600 hover:bg-primary-700 text-white font-semibold rounded-xl transition-all duration-200 hover:shadow-lg hover:shadow-primary-500/50 active:scale-95"
            >
              Войти
            </button>
          </div>

          <div v-else class="flex items-center gap-3">
            <!-- Profile avatar button -->
            <router-link
              to="/profile"
              class="w-9 h-9 rounded-xl bg-emerald-600 hover:bg-emerald-700 flex items-center justify-center transition-colors shadow-md shadow-emerald-500/20"
              title="Мой профиль"
            >
              <svg
                class="w-5 h-5 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                />
              </svg>
            </router-link>

            <button
              @click="logout"
              class="px-4 py-2 text-gray-700 dark:text-gray-300 hover:text-red-600 dark:hover:text-red-500 font-medium transition-colors"
            >
              Выйти
            </button>
          </div>

          <!-- Mobile Menu Button -->
          <button
            @click="mobileMenuOpen = !mobileMenuOpen"
            class="md:hidden p-2 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <svg
              v-if="!mobileMenuOpen"
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M4 6h16M4 12h16M4 18h16"
              />
            </svg>
            <svg
              v-else
              class="w-6 h-6"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>
      </div>
    </div>

    <!-- Mobile Menu -->
    <transition
      enter-active-class="transition-all duration-200"
      enter-from-class="opacity-0 -translate-y-4"
      enter-to-class="opacity-100 translate-y-0"
      leave-active-class="transition-all duration-150"
      leave-from-class="opacity-100 translate-y-0"
      leave-to-class="opacity-0 -translate-y-4"
    >
      <div
        v-if="mobileMenuOpen"
        class="md:hidden bg-white/95 dark:bg-gray-900/95 backdrop-blur-xl border-t border-gray-200 dark:border-gray-800"
      >
        <div class="px-4 py-4 space-y-2">
          <router-link
            to="/"
            exact-active-class="bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400"
            class="block px-4 py-3 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 font-medium transition-colors"
            @click="mobileMenuOpen = false"
          >
            Главная
          </router-link>

          <router-link
            to="/cars"
            active-class="bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400"
            class="block px-4 py-3 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 font-medium transition-colors"
            @click="mobileMenuOpen = false"
          >
            Автомобили
          </router-link>

          <router-link
            v-if="!isAuthenticated"
            to="/apply"
            active-class="bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400"
            class="block px-4 py-3 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 font-medium transition-colors"
            @click="mobileMenuOpen = false"
          >
            Подать заявку
          </router-link>

          <router-link
            v-if="isAuthenticated"
            to="/bookings"
            active-class="bg-primary-50 dark:bg-primary-900/20 text-primary-600 dark:text-primary-400"
            class="block px-4 py-3 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 font-medium transition-colors"
            @click="mobileMenuOpen = false"
          >
            Мои бронирования
          </router-link>

          <!-- Profile link in mobile menu -->
          <router-link
            v-if="isAuthenticated"
            to="/profile"
            active-class="bg-emerald-50 dark:bg-emerald-900/20 text-emerald-600 dark:text-emerald-400"
            class="block px-4 py-3 rounded-lg text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-800 font-medium transition-colors"
            @click="mobileMenuOpen = false"
          >
            Мой профиль
          </router-link>
        </div>
      </div>
    </transition>
  </nav>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from "vue";
import { useRouter } from "vue-router";
import { auth } from "../store/auth";
import ThemeToggle from "./ThemeToggle.vue";

const router = useRouter();
const isAuthenticated = computed(() => {
  if (auth.token) {
    return auth.checkTokenValidity();
  }
  return false;
});
const scrolled = ref(false);
const mobileMenuOpen = ref(false);

function logout() {
  auth.logout();
  router.push("/login");
}

const handleScroll = () => {
  scrolled.value = window.scrollY > 20;
};

onMounted(() => {
  window.addEventListener("scroll", handleScroll);
  auth.checkTokenValidity();
});

onUnmounted(() => {
  window.removeEventListener("scroll", handleScroll);
});
</script>
