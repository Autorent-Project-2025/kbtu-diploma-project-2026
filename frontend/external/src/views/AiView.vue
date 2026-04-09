<template>
  <div
    class="h-[100dvh] overflow-hidden bg-[radial-gradient(circle_at_top,_rgba(59,130,246,0.08),_transparent_24%),linear-gradient(180deg,_rgba(248,250,252,1),_rgba(241,245,249,1))] px-4 pb-4 pt-24 dark:bg-[radial-gradient(circle_at_top,_rgba(59,130,246,0.12),_transparent_24%),linear-gradient(180deg,_rgba(3,7,18,1),_rgba(2,6,23,1))] sm:px-6"
  >
    <div class="mx-auto flex h-full max-w-5xl flex-col">
      <div
        ref="messagesContainer"
        class="min-h-0 flex-1 overflow-y-auto pb-4"
      >
        <div
          v-if="messages.length === 0"
          class="flex h-full items-center justify-center"
        >
          <h1
            class="text-center text-3xl font-semibold tracking-tight text-gray-900 dark:text-white sm:text-4xl"
          >
            Чем могу помочь?
          </h1>
        </div>

        <div
          v-else
          class="mx-auto flex max-w-3xl flex-col gap-6 px-1 py-4"
        >
          <article
            v-for="message in messages"
            :key="message.id"
            class="flex"
            :class="message.role === 'user' ? 'justify-end' : 'justify-start'"
          >
            <div class="flex max-w-[85%] flex-col gap-3 sm:max-w-[80%]">
              <div
                class="rounded-[28px] px-5 py-4 text-[15px] leading-7 shadow-sm"
                :class="
                  message.role === 'user'
                    ? 'bg-gray-950 text-white dark:bg-white dark:text-gray-950'
                    : 'border border-gray-200/80 bg-white/90 text-gray-900 backdrop-blur-xl dark:border-gray-800 dark:bg-gray-900/90 dark:text-gray-100'
                "
              >
                {{ message.content }}
              </div>

              <div
                v-if="message.role === 'assistant' && message.cars.length > 0"
                class="grid gap-3"
              >
                <RouterLink
                  v-for="car in message.cars"
                  :key="`${message.id}-${car.partnerCarId}`"
                  :to="car.detailsUrl"
                  class="group overflow-hidden rounded-[28px] border border-gray-200/80 bg-white/92 transition-all hover:border-primary-300 hover:shadow-lg hover:shadow-primary-100/50 dark:border-gray-800 dark:bg-gray-900/92 dark:hover:border-primary-700 dark:hover:shadow-black/20"
                >
                  <div class="flex min-h-32">
                    <div class="h-auto w-32 shrink-0 overflow-hidden bg-gray-200 dark:bg-gray-800">
                      <img
                        v-if="car.imageUrl"
                        :src="car.imageUrl"
                        :alt="car.title"
                        class="h-full w-full object-cover transition-transform duration-300 group-hover:scale-[1.03]"
                      />
                      <div
                        v-else
                        class="flex h-full w-full items-center justify-center text-xs font-medium text-gray-500 dark:text-gray-400"
                      >
                        Нет фото
                      </div>
                    </div>

                    <div class="flex min-w-0 flex-1 flex-col gap-3 px-4 py-4">
                      <div class="flex items-start justify-between gap-4">
                        <div class="min-w-0">
                          <p class="truncate text-base font-semibold text-gray-950 dark:text-white">
                            {{ car.title }}
                          </p>
                          <p
                            v-if="car.carrierName"
                            class="mt-1 truncate text-sm text-gray-500 dark:text-gray-400"
                          >
                            {{ car.carrierName }}
                          </p>
                        </div>

                        <div class="shrink-0 text-right">
                          <p class="text-base font-semibold text-primary-700 dark:text-primary-300">
                            {{ formatPrice(car.priceHour) }}
                          </p>
                          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                            {{ formatRating(car.rating) }}
                          </p>
                        </div>
                      </div>

                      <div
                        v-if="car.reasons.length > 0"
                        class="flex flex-wrap gap-2"
                      >
                        <span
                          v-for="reason in car.reasons"
                          :key="`${car.partnerCarId}-${reason}`"
                          class="rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600 dark:bg-gray-800 dark:text-gray-300"
                        >
                          {{ reason }}
                        </span>
                      </div>
                    </div>
                  </div>
                </RouterLink>
              </div>
            </div>
          </article>

          <div
            v-if="isResponding"
            class="flex justify-start"
          >
            <div
              class="rounded-[28px] border border-gray-200/80 bg-white/90 px-5 py-4 shadow-sm backdrop-blur-xl dark:border-gray-800 dark:bg-gray-900/90"
            >
              <div class="flex items-center gap-2">
                <span class="h-2 w-2 animate-pulse rounded-full bg-primary-500"></span>
                <span class="h-2 w-2 animate-pulse rounded-full bg-primary-400 [animation-delay:120ms]"></span>
                <span class="h-2 w-2 animate-pulse rounded-full bg-emerald-400 [animation-delay:240ms]"></span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="mx-auto w-full max-w-3xl">
        <form
          class="rounded-[32px] border border-gray-200/60 bg-white/72 p-3 shadow-lg shadow-slate-200/20 backdrop-blur-xl dark:border-gray-800/80 dark:bg-gray-900/68 dark:shadow-black/10"
          @submit.prevent="sendDraft"
        >
          <textarea
            v-model="draft"
            rows="1"
            class="max-h-48 w-full resize-none border-0 bg-transparent px-3 py-3 text-[15px] leading-7 text-gray-900 outline-none placeholder:text-gray-400 dark:text-white"
            placeholder="Напишите сообщение..."
            @keydown="handleComposerKeydown"
          ></textarea>

          <div class="flex justify-end px-2 pb-1 pt-2">
            <button
              type="submit"
              class="inline-flex h-11 w-11 items-center justify-center rounded-full bg-gray-950 text-white transition-colors hover:bg-primary-600 disabled:cursor-not-allowed disabled:bg-gray-300 dark:bg-white dark:text-gray-950 dark:hover:bg-primary-400 dark:hover:text-white dark:disabled:bg-gray-700"
              :disabled="!canSend"
              aria-label="Отправить"
            >
              <svg
                class="h-5 w-5"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M5 12h14m-7-7 7 7-7 7"
                />
              </svg>
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from "vue";
import { RouterLink } from "vue-router";
import { getAiRecommendations, type AiRecommendationCard } from "../api/ai";
import { formatMoney } from "../utils/formatMoney";

type Message = {
  id: number;
  role: "assistant" | "user";
  content: string;
  cars: AiRecommendationCard[];
};

const messages = ref<Message[]>([]);
const draft = ref("");
const isResponding = ref(false);
const counter = ref(1);
const messagesContainer = ref<HTMLElement | null>(null);

const canSend = computed(
  () => draft.value.trim().length > 0 && !isResponding.value,
);

watch(
  () => messages.value.length,
  async () => {
    await nextTick();
    const container = messagesContainer.value;
    if (!container) {
      return;
    }

    container.scrollTo({
      top: container.scrollHeight,
      behavior: "smooth",
    });
  },
);

function handleComposerKeydown(event: KeyboardEvent) {
  if (event.key === "Enter" && !event.shiftKey) {
    event.preventDefault();
    sendDraft();
  }
}

function sendDraft() {
  const content = draft.value.trim();
  if (!content || isResponding.value) {
    return;
  }

  draft.value = "";
  void sendMessage(content);
}

async function sendMessage(content: string) {
  messages.value.push({
    id: counter.value++,
    role: "user",
    content,
    cars: [],
  });

  isResponding.value = true;

  try {
    const response = await getAiRecommendations(content);
    messages.value.push({
      id: counter.value++,
      role: "assistant",
      content: response.assistantText,
      cars: response.cars ?? [],
    });
  } catch (error) {
    console.error("AI recommendation request failed:", error);
    messages.value.push({
      id: counter.value++,
      role: "assistant",
      content:
        "Не удалось получить подборку машин. Попробуйте повторить запрос или немного упростить формулировку.",
      cars: [],
    });
  } finally {
    isResponding.value = false;
  }
}

function formatPrice(priceHour: number | null): string {
  if (priceHour == null) {
    return "По запросу";
  }

  return `${formatMoney(priceHour)}/час`;
}

function formatRating(rating: number | null): string {
  if (rating == null) {
    return "Без рейтинга";
  }

  return `${rating.toFixed(1)} / 5`;
}
</script>
