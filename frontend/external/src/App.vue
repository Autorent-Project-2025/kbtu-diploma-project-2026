<template>
  <div
    class="relative min-h-screen bg-gray-50 dark:bg-gray-950 transition-colors duration-300"
  >
    <!-- Mouse Glow Effect - самый низкий слой -->
    <MouseGlow />

    <!-- Content - поверх -->
    <div class="relative z-10">
      <Navbar />
      <router-view />
      <ToastContainer />
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import Navbar from "./components/Navbar.vue";
import ToastContainer from "./components/ToastContainer.vue";
import MouseGlow from "./components/MouseGlow.vue";
import { useTheme } from "./composables/useTheme";

// Инициализация темы при загрузке приложения
const { initTheme } = useTheme();
onMounted(() => {
  initTheme();
});
</script>
