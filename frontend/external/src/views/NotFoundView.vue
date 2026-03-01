<template>
  <div
    class="relative min-h-screen overflow-hidden bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 flex flex-col items-center justify-center"
  >
    <!-- SVG Canyon с машинками и параллакс -->
    <svg
      viewBox="0 0 2000 720"
      class="absolute inset-0 w-full h-full"
      preserveAspectRatio="xMidYMid slice"
    >
      <!-- Background mountains -->
      <path
        :style="{ transform: `translate(${layers[2].x}px, ${layers[2].y}px)` }"
        class="fill-gray-800 stroke-gray-900"
        stroke-width="5"
        d="m1831 198l-8 565l-95 3v-576.3zm-441-42v633.1h-257v-622.1zm-340 36v597.3h-201.7v-596.3zm-246 20v531.7h-53v-534.7zm-136-20v575.1h-153.4v-576.3zm-348 3v574.7h-159v-566.8z"
      />
      <path
        :style="{ transform: `translate(${layers[2].x}px, ${layers[2].y}px)` }"
        class="fill-gray-900"
        d="m-203.5 227v-467.6h2433.1v553.6l-399.6-71l-102 29l-335-76l-258 80l-85-30l-202 32l-45-35l-50 19l-84-35l-154 61l-194-58l-160 58z"
      />

      <!-- Far cars (small, slow parallax) -->
      <g
        :style="{ transform: `translate(${layers[0].x}px, ${layers[0].y}px)` }"
        class="opacity-20"
      >
        <g
          v-for="(pos, i) in farCars"
          :key="`far-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>

      <!-- Mid cars -->
      <g
        :style="{ transform: `translate(${layers[1].x}px, ${layers[1].y}px)` }"
        class="opacity-40"
      >
        <g
          v-for="(pos, i) in midCars"
          :key="`mid-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>

      <!-- Near cars -->
      <g
        :style="{ transform: `translate(${layers[2].x}px, ${layers[2].y}px)` }"
        class="opacity-70"
      >
        <g
          v-for="(pos, i) in nearCars"
          :key="`near-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>

      <!-- Ground blur -->
      <path
        :style="{
          transform: `translate(${layers[2].x}px, ${layers[2].y}px)`,
          filter: 'blur(80px)',
        }"
        class="fill-gray-900"
        d="m-300,400 H2400 V700 H0 z"
      />

      <!-- Giant 404 text -->
      <text
        :style="{ transform: `translate(${layers[3].x}px, ${layers[3].y}px)` }"
        x="1000"
        y="550"
        text-anchor="middle"
        class="fill-gray-700 text-[660px] font-extrabold"
        style="filter: drop-shadow(0 0 50px rgba(0, 0, 0, 0.5))"
      >
        404
      </text>

      <!-- Foreground ground -->
      <path
        :style="{ transform: `translate(${layers[4].x}px, ${layers[4].y}px)` }"
        class="fill-gray-900"
        d="m2195 396v531.1h-2437.2v-538.1l359.2 60l96-22l63 44l169-40l83 39l348-47l147 28l125-32l75 47l75-21l221 28l263-75l109 31z"
      />

      <!-- Large foreground cars -->
      <g
        :style="{ transform: `translate(${layers[4].x}px, ${layers[4].y}px)` }"
      >
        <g
          v-for="(pos, i) in closeCars"
          :key="`close-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>

      <!-- Very close cars (blurred) -->
      <g
        :style="{
          transform: `translate(${layers[5].x}px, ${layers[5].y}px)`,
          filter: 'blur(5px) brightness(0.7)',
        }"
      >
        <g
          v-for="(pos, i) in veryCloseCars"
          :key="`vclose-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>

      <!-- Extreme close cars (very blurred) -->
      <g
        :style="{
          transform: `translate(${layers[6].x}px, ${layers[6].y}px)`,
          filter: 'blur(10px) brightness(0.5)',
        }"
      >
        <g
          v-for="(pos, i) in extremeCars"
          :key="`extreme-${i}`"
          :transform="`translate(${pos.x}, ${pos.y}) scale(${pos.scale})`"
        >
          <CarIcon />
        </g>
      </g>
    </svg>

    <!-- Content overlay -->
    <div class="relative z-10 text-center px-4 space-y-6">
      <h1
        class="text-7xl sm:text-9xl font-extrabold text-white"
        style="font-family: 'Montserrat', sans-serif"
      >
        Oops!
      </h1>
      <div class="space-y-3">
        <p class="text-xl text-gray-400 font-medium tracking-wide">
          Похоже, эта страница уехала...
        </p>
        <p class="text-gray-500 font-medium tracking-wide">
          Вы можете
          <router-link
            to="/"
            class="text-primary-500 hover:text-primary-400 hover:border-b-2 hover:border-primary-500 transition-all"
          >
            вернуться на главную
          </router-link>
          или
          <router-link
            to="/cars"
            class="text-primary-500 hover:text-primary-400 hover:border-b-2 hover:border-primary-500 transition-all"
          >
            посмотреть автомобили
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
// @ts-nocheck
import { ref, onMounted, onUnmounted } from "vue";

// Car SVG icon component (inline)
const CarIcon = {
  template: `
    <g class="car-icon">
      <!-- Car body -->
      <path
        class="fill-primary-700"
        d="M50,40 L150,40 L170,20 L190,20 L210,40 L250,40 L250,80 L50,80 Z"
      />
      <!-- Car windows -->
      <path
        class="fill-primary-900"
        d="M170,25 L185,25 L200,40 L160,40 Z"
      />
      <!-- Car details -->
      <path
        class="fill-primary-900 opacity-50"
        d="M60,50 L90,50 L90,70 L60,70 Z M210,50 L240,50 L240,70 L210,70 Z"
      />
      <!-- Wheels -->
      <circle class="fill-gray-900 stroke-gray-700" stroke-width="3" cx="90" cy="80" r="15" />
      <circle class="fill-gray-900 stroke-gray-700" stroke-width="3" cx="210" cy="80" r="15" />
      <circle class="fill-gray-600" cx="90" cy="80" r="8" />
      <circle class="fill-gray-600" cx="210" cy="80" r="8" />
    </g>
  `,
};

// Parallax layers
const layers = ref([
  { x: 0, y: 0 }, // Layer 0 (far)
  { x: 0, y: 0 }, // Layer 1
  { x: 0, y: 0 }, // Layer 2
  { x: 0, y: 0 }, // Layer 3 (404 text)
  { x: 0, y: 0 }, // Layer 4
  { x: 0, y: 0 }, // Layer 5
  { x: 0, y: 0 }, // Layer 6 (closest)
]);

// Car positions
const farCars = [
  { x: 0, y: -350, scale: 0.15 },
  { x: 300, y: -320, scale: 0.15 },
  { x: 520, y: -360, scale: 0.12 },
  { x: 800, y: -330, scale: 0.15 },
  { x: 1000, y: -380, scale: 0.12 },
  { x: 1150, y: -350, scale: 0.15 },
  { x: 1400, y: -360, scale: 0.15 },
];

const midCars = [
  { x: 80, y: -300, scale: 0.22 },
  { x: 380, y: -280, scale: 0.22 },
  { x: 600, y: -310, scale: 0.18 },
  { x: 700, y: -290, scale: 0.22 },
  { x: 1100, y: -320, scale: 0.18 },
  { x: 1250, y: -300, scale: 0.22 },
  { x: 1500, y: -310, scale: 0.22 },
];

const nearCars = [
  { x: -110, y: -200, scale: 0.3 },
  { x: 180, y: -180, scale: 0.35 },
  { x: 800, y: -190, scale: 0.3 },
  { x: 500, y: -230, scale: 0.25 },
  { x: 1300, y: -220, scale: 0.25 },
  { x: 1450, y: -200, scale: 0.3 },
];

const closeCars = [
  { x: 0, y: 80, scale: 0.55 },
  { x: 1000, y: 100, scale: 0.6 },
  { x: 1450, y: 80, scale: 0.55 },
];

const veryCloseCars = [
  { x: 100, y: 180, scale: 0.7 },
  { x: 700, y: 200, scale: 0.75 },
  { x: 1350, y: 250, scale: 0.7 },
];

const extremeCars = [
  { x: 0, y: 320, scale: 0.85 },
  { x: 400, y: 280, scale: 0.9 },
  { x: 1400, y: 350, scale: 0.85 },
];

// Parallax depths
const depths = [-240, -150, -80, -20, 80, 150, 300];

// Mouse move handler
const handleMouseMove = (e: MouseEvent) => {
  requestAnimationFrame(() => {
    const width = window.innerWidth;
    const height = window.innerHeight;
    const x = e.clientX / width - 0.5; // Normalized to -0.5 <-> 0.5
    const y = e.clientY / height - 0.5;

    layers.value = depths.map((depth) => ({
      x: depth * x,
      y: depth * y,
    }));
  });
};

onMounted(() => {
  document.addEventListener("mousemove", handleMouseMove);
});

onUnmounted(() => {
  document.removeEventListener("mousemove", handleMouseMove);
});
</script>

<style scoped>
/* Montserrat font for that premium look */
@import url("https://fonts.googleapis.com/css2?family=Montserrat:wght@400;500;600;700;800;900&display=swap");

.car-icon {
  transform-origin: center;
  transform-box: fill-box;
}
</style>
