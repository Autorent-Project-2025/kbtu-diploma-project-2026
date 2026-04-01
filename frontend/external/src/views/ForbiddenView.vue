<template>
  <div
    class="relative min-h-screen overflow-hidden bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 flex flex-col items-center justify-center px-4"
  >
    <!-- Background glow -->
    <div class="absolute inset-0 pointer-events-none">
      <div class="absolute top-1/3 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[600px] bg-red-600/10 rounded-full blur-3xl" />
    </div>

    <div class="relative z-10 text-center space-y-8 max-w-lg">
      <!-- Big 403 -->
      <div class="relative">
        <p class="text-[140px] font-extrabold leading-none bg-gradient-to-b from-red-400 to-red-700 bg-clip-text text-transparent select-none">
          403
        </p>
        <!-- Lock icon overlay -->
        <div class="absolute inset-0 flex items-center justify-center">
          <div class="w-20 h-20 rounded-3xl bg-gray-900/80 border border-red-500/30 flex items-center justify-center shadow-2xl">
            <svg class="w-10 h-10 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5"
                d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>
        </div>
      </div>

      <!-- Text -->
      <div class="space-y-3">
        <h1 class="text-3xl font-extrabold text-white">
          Доступ запрещён
        </h1>
        <p class="text-gray-400 text-lg leading-relaxed">
          У вас нет прав для просмотра этой страницы.
          Если считаете это ошибкой — обратитесь в поддержку.
        </p>
      </div>

      <!-- Actions -->
      <div class="flex flex-col sm:flex-row items-center justify-center gap-3">
        <button
          @click="$router.back()"
          class="px-6 py-3 rounded-2xl border border-gray-700 text-gray-300 hover:text-white hover:border-gray-500 font-semibold transition-colors"
        >
          ← Назад
        </button>
        <router-link
          to="/"
          class="px-6 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
        >
          На главную
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// No logic needed
</script>