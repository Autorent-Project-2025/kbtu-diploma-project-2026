import { ref, onMounted, onUnmounted } from "vue";

export function useMousePosition() {
  const x = ref(0);
  const y = ref(0);

  const updatePosition = (event: MouseEvent) => {
    x.value = event.clientX;
    y.value = event.clientY;
  };

  onMounted(() => {
    window.addEventListener("mousemove", updatePosition);
  });

  onUnmounted(() => {
    window.removeEventListener("mousemove", updatePosition);
  });

  return { x, y };
}
