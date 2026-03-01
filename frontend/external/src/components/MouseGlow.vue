<template>
  <div
    class="mouse-glow pointer-events-none fixed"
    :style="{
      left: `${x}px`,
      top: `${y}px`,
    }"
  />
</template>

<script setup lang="ts">
import { useMousePosition } from "../composables/useMousePosition";

const { x, y } = useMousePosition();
</script>

<style scoped>
.mouse-glow {
  width: 800px;
  height: 800px;
  background: radial-gradient(
    circle,
    rgba(59, 130, 246, 0.3) 0%,
    rgba(59, 130, 246, 0.15) 30%,
    transparent 70%
  );
  transform: translate(-50%, -50%);
  filter: blur(100px);
  z-index: 1;
}

.dark .mouse-glow {
  background: radial-gradient(
    circle,
    rgba(59, 130, 246, 0.4) 0%,
    rgba(59, 130, 246, 0.2) 30%,
    transparent 70%
  );
}

@media (max-width: 768px) {
  .mouse-glow {
    display: none;
  }
}
</style>
