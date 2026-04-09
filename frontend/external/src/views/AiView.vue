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
            <div
              class="max-w-[85%] rounded-[28px] px-5 py-4 text-[15px] leading-7 shadow-sm sm:max-w-[80%]"
              :class="
                message.role === 'user'
                  ? 'bg-gray-950 text-white dark:bg-white dark:text-gray-950'
                  : 'border border-gray-200/80 bg-white/90 text-gray-900 backdrop-blur-xl dark:border-gray-800 dark:bg-gray-900/90 dark:text-gray-100'
              "
            >
              {{ message.content }}
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

type Message = {
  id: number;
  role: "assistant" | "user";
  content: string;
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
  sendMessage(content);
}

function sendMessage(content: string) {
  messages.value.push({
    id: counter.value++,
    role: "user",
    content,
  });

  isResponding.value = true;

  window.setTimeout(() => {
    messages.value.push(buildReply(content));
    isResponding.value = false;
  }, 450);
}

function buildReply(content: string): Message {
  const normalized = content.toLowerCase();

  if (
    normalized.includes("сем") ||
    normalized.includes("багаж") ||
    normalized.includes("дет")
  ) {
    return {
      id: counter.value++,
      role: "assistant",
      content:
        "Для семьи я бы смотрел на более просторные и практичные варианты: нормальный запас места, комфортную посадку и адекватную цену на весь интервал аренды.",
    };
  }

  if (
    normalized.includes("бизнес") ||
    normalized.includes("встреч") ||
    normalized.includes("аэропорт")
  ) {
    return {
      id: counter.value++,
      role: "assistant",
      content:
        "Под деловой сценарий лучше подходят спокойные комфортные седаны: свежий внешний вид, аккуратный салон и без лишней переплаты за слишком яркий вариант.",
    };
  }

  if (
    normalized.includes("цен") ||
    normalized.includes("бюдж") ||
    normalized.includes("сколь")
  ) {
    return {
      id: counter.value++,
      role: "assistant",
      content:
        "Смотрите не только на цену в час. Важнее общий бюджет на поездку, длительность аренды и то, насколько машина подходит под ваш сценарий.",
    };
  }

  if (
    normalized.includes("город") ||
    normalized.includes("ежеднев") ||
    normalized.includes("пара")
  ) {
    return {
      id: counter.value++,
      role: "assistant",
      content:
        "Для города обычно лучше работают более компактные и спокойные варианты: проще парковка, понятнее бюджет и меньше переплата за лишний объём.",
    };
  }

  return {
    id: counter.value++,
    role: "assistant",
    content:
      "Опишите задачу в одном предложении: куда едете, сколько человек и какой бюджет. Этого уже достаточно, чтобы сузить выбор.",
  };
}
</script>
