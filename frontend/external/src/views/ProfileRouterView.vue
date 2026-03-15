<template>
  <!-- Пустой экран пока определяем роль -->
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 flex items-center justify-center"
  >
    <div class="flex items-center gap-3 text-gray-500 dark:text-gray-400">
      <svg class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
        <circle
          class="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          stroke-width="4"
        />
        <path
          class="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
        />
      </svg>
      <span class="text-sm font-medium">Загружаем профиль...</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useRouter } from "vue-router";
import { getMyPartner } from "../api/partners";

const router = useRouter();

onMounted(async () => {
  try {
    await getMyPartner();
    // Если запрос прошёл — это партнёр
    router.replace("/profile/partner");
  } catch (e: any) {
    // 404 или любая ошибка → обычный пользователь
    router.replace("/profile/user");
  }
});
</script>
