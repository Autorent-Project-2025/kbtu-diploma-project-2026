<template>
  <div class="pt-4 border-t border-gray-100 dark:border-gray-800 space-y-3">
    <div class="flex items-center justify-between gap-3">
      <h4
        class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
      >
        AI-оценка повреждений
      </h4>
      <span
        class="rounded-full px-3 py-1 text-xs font-bold uppercase tracking-[0.12em]"
        :class="{
          'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300':
            aiStatusBadge(assessment.status).tone === 'ok',
          'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300':
            aiStatusBadge(assessment.status).tone === 'warn',
          'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-300':
            aiStatusBadge(assessment.status).tone === 'error',
          'bg-gray-200 text-gray-600 dark:bg-gray-800 dark:text-gray-300':
            aiStatusBadge(assessment.status).tone === 'muted',
        }"
      >
        {{ aiStatusBadge(assessment.status).label }}
      </span>
    </div>

    <p class="text-sm text-gray-700 dark:text-gray-300">
      <span class="font-semibold">Вердикт:</span>
      {{ aiVerdictLabel(assessment.verdict) }}
      <span class="text-gray-400 dark:text-gray-500">
        ·
        {{ assessment.validPhotosCount }}/5 фото принято
      </span>
    </p>

    <p
      v-if="assessment.status === 'unavailable' || assessment.status === 'error'"
      class="rounded-2xl bg-amber-50 dark:bg-amber-900/20 p-3 text-xs text-amber-700 dark:text-amber-300"
    >
      AI-анализ не был выполнен, решение принимается вручную. Это не блокирует
      подтверждение завершения — используйте фотографии ниже и свой опыт.
      <span
        v-if="assessment.errorMessage"
        class="block mt-1 text-amber-600/80 dark:text-amber-400/70"
      >
        Детали: {{ assessment.errorMessage }}
      </span>
    </p>

    <div
      v-if="assessment.damages && assessment.damages.length > 0"
      class="space-y-2"
    >
      <p class="text-xs font-bold uppercase tracking-[0.16em] text-gray-500 dark:text-gray-400">
        Найденные повреждения
      </p>
      <ul class="space-y-1">
        <li
          v-for="(damage, index) in assessment.damages"
          :key="index"
          class="rounded-xl bg-red-50 dark:bg-red-900/20 px-3 py-2 text-xs"
        >
          <div class="flex items-center justify-between gap-2">
            <span class="font-semibold text-red-700 dark:text-red-300">
              {{ damage.type }}
            </span>
            <span class="text-red-600/70 dark:text-red-400/70">
              {{ formatConfidencePercent(damage.confidence) }}
            </span>
          </div>
          <p
            v-if="damage.slot"
            class="text-red-600/80 dark:text-red-400/80 mt-0.5"
          >
            Фото: {{ completionPhotoLabel(damage.slot) }}
          </p>
        </li>
      </ul>
    </div>

    <div
      v-if="assessment.rejectedPhotos && assessment.rejectedPhotos.length > 0"
      class="space-y-2"
    >
      <p class="text-xs font-bold uppercase tracking-[0.16em] text-gray-500 dark:text-gray-400">
        Отклонённые фото
      </p>
      <ul class="space-y-1">
        <li
          v-for="(rejected, index) in assessment.rejectedPhotos"
          :key="index"
          class="rounded-xl bg-amber-50 dark:bg-amber-900/20 px-3 py-2 text-xs text-amber-700 dark:text-amber-300"
        >
          <span class="font-semibold">
            {{
              rejected.slot
                ? completionPhotoLabel(rejected.slot)
                : rejected.fileName
            }}:
          </span>
          {{ rejected.reason }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BookingCompletionTicketData } from "../../types/Ticket";
import {
  aiStatusBadge,
  aiVerdictLabel,
  completionPhotoLabel,
  formatConfidencePercent,
} from "../../utils/ticketLabels";

type AiAssessment = NonNullable<BookingCompletionTicketData["aiAssessment"]>;

defineProps<{
  assessment: AiAssessment;
}>();
</script>
