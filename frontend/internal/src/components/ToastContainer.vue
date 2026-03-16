<template>
  <teleport to="body">
    <div
      class="fixed top-4 right-4 z-[9999] flex flex-col items-end pointer-events-none"
    >
      <div class="pointer-events-auto">
        <ToastItem
          v-for="toast in toasts"
          :key="toast.id"
          :id="toast.id"
          :message="toast.message"
          :type="toast.type"
          @close="removeToast"
        />
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
import { useToast } from "../composables/useToast";
import ToastItem from "./ToastItem.vue";

const { toasts, removeToast } = useToast();
</script>