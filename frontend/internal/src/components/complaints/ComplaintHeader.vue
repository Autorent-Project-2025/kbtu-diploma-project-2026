<template>
  <header
    class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
  >
    <div class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
      <div class="flex items-start gap-4">
        <router-link
          to="/complaints"
          class="mt-1 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-emerald-500 transition-colors shrink-0"
        >
          ← Назад
        </router-link>
        <div class="space-y-2">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
            Жалоба
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            {{ loading ? "Загрузка..." : (complaint?.subject ?? "Не найдена") }}
          </h1>
          <div v-if="complaint" class="flex flex-wrap items-center gap-2 pt-1">
            <span :class="['px-3 py-1 rounded-full text-sm font-bold', complaintStatusBadge(complaint.status)]">
              {{ statusLabels[complaint.status] ?? "—" }}
            </span>
            <span :class="['px-3 py-1 rounded-full text-sm font-bold', priorityBadge(complaint.priority)]">
              {{ priorityLabels[complaint.priority] ?? "—" }}
            </span>
            <span class="px-3 py-1 rounded-full bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 text-sm font-bold">
              {{ categoryLabels[complaint.category] ?? "Другое" }}
            </span>
            <span
              v-if="complaint.isEscalated"
              class="px-3 py-1 rounded-full bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400 text-sm font-bold"
            >
              Эскалирована
            </span>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import type { Complaint } from "../../types/Complaint";
import {
  categoryLabels,
  complaintStatusBadge,
  priorityBadge,
  priorityLabels,
  statusLabels,
} from "../../utils/complaintLabels";

defineProps<{
  complaint: Complaint | null;
  loading: boolean;
}>();
</script>
