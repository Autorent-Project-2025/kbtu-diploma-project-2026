<template>
  <div
    class="relative min-h-screen overflow-hidden bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-950"
  >
    <div
      v-if="showRoleChoiceModal"
      class="fixed inset-0 z-[60] flex items-center justify-center bg-black/55 px-4"
    >
      <div class="w-full max-w-xl rounded-3xl border border-gray-200 bg-white p-8 shadow-2xl dark:border-gray-800 dark:bg-gray-900">
        <h2 class="text-2xl sm:text-3xl font-extrabold text-gray-900 dark:text-white">
          Добро пожаловать в AutoRent
        </h2>
        <p class="mt-3 text-gray-600 dark:text-gray-300">
          Выберите вариант продолжения:
        </p>

        <div class="mt-8 grid gap-4 sm:grid-cols-2">
          <button
            class="rounded-2xl border border-gray-300 px-5 py-4 text-left text-gray-800 transition hover:border-primary-500 hover:bg-primary-50 dark:border-gray-700 dark:text-gray-200 dark:hover:bg-gray-800"
            @click="continueAsClient"
          >
            <div class="text-lg font-bold">Я клиент</div>
            <div class="mt-1 text-sm text-gray-500 dark:text-gray-400">Открыть сайт как обычно</div>
          </button>

          <button
            class="rounded-2xl bg-primary-600 px-5 py-4 text-left text-white transition hover:bg-primary-700"
            @click="continueAsPartner"
          >
            <div class="text-lg font-bold">Я партнер</div>
            <div class="mt-1 text-sm text-primary-100">Перейти к форме партнерской заявки</div>
          </button>
        </div>
      </div>
    </div>

    <!-- Hero Section -->
    <section
      class="relative min-h-screen flex items-center justify-center px-4 sm:px-6 lg:px-8"
    >
      <div class="max-w-7xl mx-auto w-full py-20">
        <div class="grid lg:grid-cols-2 gap-12 items-center">
          <!-- Left: Text Content -->
          <div class="space-y-8 animate-slide-up">
            <div class="space-y-4">
              <h1
                class="text-5xl sm:text-6xl lg:text-7xl font-extrabold leading-tight"
              >
                <span class="text-gray-900 dark:text-white">Самый простой</span>
                <br />
                <span class="text-gradient-primary">способ арендовать</span>
                <br />
                <span class="text-gray-900 dark:text-white">автомобиль</span>
              </h1>

              <p
                class="text-lg sm:text-xl text-gray-700 dark:text-gray-300 max-w-2xl"
              >
                Бесшовная, полностью цифровая аренда с проверенным автопарком
                внедорожников, премиальных и бизнес-автомобилей. Арендуйте от 1
                до 365 дней — без хлопот, максимум комфорта.
              </p>
            </div>

            <!-- Stats -->
            <div class="flex flex-wrap gap-8 text-gray-900 dark:text-white">
              <div class="space-y-1">
                <div class="flex items-center gap-2">
                  <svg
                    class="w-6 h-6 text-primary-500 dark:text-primary-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <p class="text-3xl font-bold">200+</p>
                </div>
                <p class="text-gray-600 dark:text-gray-400 text-sm">
                  автомобилей
                </p>
              </div>

              <div class="space-y-1">
                <div class="flex items-center gap-2">
                  <svg
                    class="w-6 h-6 text-primary-500 dark:text-primary-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                  <p class="text-3xl font-bold">15</p>
                </div>
                <p class="text-gray-600 dark:text-gray-400 text-sm">брендов</p>
              </div>
            </div>

            <!-- Features -->
            <div class="space-y-3">
              <div
                class="flex items-center gap-3 text-gray-800 dark:text-gray-200"
              >
                <div
                  class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
                >
                  <svg
                    class="w-5 h-5 text-primary-600 dark:text-primary-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                </div>
                <p class="font-medium">Гибкие условия аренды</p>
              </div>

              <div
                class="flex items-center gap-3 text-gray-800 dark:text-gray-200"
              >
                <div
                  class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
                >
                  <svg
                    class="w-5 h-5 text-primary-600 dark:text-primary-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                    />
                  </svg>
                </div>
                <p class="font-medium">Полная страховка</p>
              </div>

              <div
                class="flex items-center gap-3 text-gray-800 dark:text-gray-200"
              >
                <div
                  class="w-8 h-8 rounded-full bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
                >
                  <svg
                    class="w-5 h-5 text-primary-600 dark:text-primary-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                    />
                  </svg>
                </div>
                <p class="font-medium">Без залога</p>
              </div>
            </div>

            <!-- CTA Button -->
            <div class="pt-4">
              <router-link
                to="/cars"
                class="btn-premium inline-flex items-center gap-3 text-lg group"
              >
                <span>Выбрать автомобиль</span>
                <svg
                  class="w-6 h-6 transform group-hover:translate-x-1 transition-transform"
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
          </div>

          <!-- Right: Car Images / Visual -->
          <div class="relative lg:block hidden">
            <div class="relative h-[600px] flex items-center justify-center">
              <!-- Brand Logos -->
              <div class="absolute inset-0">
                <div
                  class="absolute top-10 right-20 w-16 h-16 rounded-2xl bg-white/60 dark:bg-white/10 backdrop-blur-sm flex items-center justify-center animate-float shadow-lg"
                >
                  <span class="text-2xl font-bold text-gray-900 dark:text-white"
                    >M</span
                  >
                </div>
                <div
                  class="absolute top-40 right-10 w-16 h-16 rounded-2xl bg-white/60 dark:bg-white/10 backdrop-blur-sm flex items-center justify-center animate-float shadow-lg"
                  style="animation-delay: 0.5s"
                >
                  <span class="text-2xl font-bold text-gray-900 dark:text-white"
                    >N</span
                  >
                </div>
                <div
                  class="absolute bottom-40 right-32 w-16 h-16 rounded-2xl bg-white/60 dark:bg-white/10 backdrop-blur-sm flex items-center justify-center animate-float shadow-lg"
                  style="animation-delay: 1s"
                >
                  <span class="text-2xl font-bold text-gray-900 dark:text-white"
                    >P</span
                  >
                </div>
                <div
                  class="absolute bottom-20 right-10 w-16 h-16 rounded-2xl bg-white/60 dark:bg-white/10 backdrop-blur-sm flex items-center justify-center animate-float shadow-lg"
                  style="animation-delay: 1.5s"
                >
                  <span class="text-2xl font-bold text-gray-900 dark:text-white"
                    >T</span
                  >
                </div>
              </div>

              <!-- Center glow -->
              <div class="w-96 h-96 rounded-full glow-primary"></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Features Section -->
    <section
      class="relative py-20 px-4 sm:px-6 lg:px-8 bg-white/50 dark:bg-white/5 backdrop-blur-sm"
    >
      <div class="max-w-7xl mx-auto">
        <h2
          class="text-3xl sm:text-4xl font-bold text-center text-gray-900 dark:text-white mb-16"
        >
          Почему выбирают нас
        </h2>

        <div class="grid md:grid-cols-3 gap-8">
          <div class="glass p-8 rounded-2xl space-y-4 card-hover">
            <div
              class="w-12 h-12 rounded-xl bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
            >
              <svg
                class="w-6 h-6 text-primary-600 dark:text-primary-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 18h.01M8 21h8a2 2 0 002-2V5a2 2 0 00-2-2H8a2 2 0 00-2 2v14a2 2 0 002 2z"
                />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white">
              Онлайн бронирование
            </h3>
            <p class="text-gray-700 dark:text-gray-300">
              Забронируйте автомобиль за пару кликов через наше приложение или
              веб-сайт
            </p>
          </div>

          <div class="glass p-8 rounded-2xl space-y-4 card-hover">
            <div
              class="w-12 h-12 rounded-xl bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
            >
              <svg
                class="w-6 h-6 text-primary-600 dark:text-primary-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z"
                />
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 11a3 3 0 11-6 0 3 3 0 016 0z"
                />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white">
              Доставка в любую точку
            </h3>
            <p class="text-gray-700 dark:text-gray-300">
              Мы доставим автомобиль туда, где вам удобно, в любое время
            </p>
          </div>

          <div class="glass p-8 rounded-2xl space-y-4 card-hover">
            <div
              class="w-12 h-12 rounded-xl bg-primary-100 dark:bg-primary-500/20 flex items-center justify-center"
            >
              <svg
                class="w-6 h-6 text-primary-600 dark:text-primary-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"
                />
              </svg>
            </div>
            <h3 class="text-xl font-bold text-gray-900 dark:text-white">
              Полная страховка
            </h3>
            <p class="text-gray-700 dark:text-gray-300">
              Все автомобили застрахованы, арендуйте с уверенностью
            </p>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { auth } from "../store/auth";

const router = useRouter();
const showRoleChoiceModal = ref(false);
const roleModalSessionKey = "external-role-choice-shown";

const isAuthenticated = computed(() => {
  if (!auth.token) return false;
  return auth.checkTokenValidity();
});

function continueAsClient() {
  sessionStorage.setItem(roleModalSessionKey, "1");
  showRoleChoiceModal.value = false;
}

function continueAsPartner() {
  sessionStorage.setItem(roleModalSessionKey, "1");
  showRoleChoiceModal.value = false;
  router.push("/partner/apply");
}

onMounted(() => {
  const modalAlreadyShown = sessionStorage.getItem(roleModalSessionKey) === "1";
  showRoleChoiceModal.value = !isAuthenticated.value && !modalAlreadyShown;
});
</script>
