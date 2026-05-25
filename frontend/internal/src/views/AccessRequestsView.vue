<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">

    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(251,191,36,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div class="space-y-2">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-amber-600 dark:text-amber-400">
            Управление доступом
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            Запросы на доступ к бронированиям
          </h1>
          <p class="text-gray-600 dark:text-gray-400 text-sm">
            Менеджеры запрашивают временный доступ к бронированиям, связанным с жалобами.
          </p>
        </div>
        <button
          @click="loadRequests"
          :disabled="loading"
          class="px-5 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-amber-500 transition-colors disabled:opacity-60 shrink-0"
        >
          Обновить
        </button>
      </div>
    </header>

    <!-- Filters -->
    <div class="flex gap-1 rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 p-1.5 w-fit shadow">
      <button
        v-for="f in filters"
        :key="f.value ?? 'all'"
        @click="activeFilter = f.value"
        :class="[
          'px-5 py-2 rounded-xl text-sm font-semibold transition-colors',
          activeFilter === f.value
            ? 'bg-amber-600 text-white shadow'
            : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-100',
        ]"
      >
        {{ f.label }}
        <span v-if="f.count != null" class="ml-1 text-xs opacity-75">({{ f.count }})</span>
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-500 dark:text-gray-400 font-medium">
      Загрузка...
    </div>

    <!-- Empty -->
    <div
      v-else-if="filteredRequests.length === 0"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Нет запросов{{ activeFilter != null ? " с таким статусом" : "" }}.
    </div>

    <!-- Table -->
    <div
      v-else
      class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
    >
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr class="border-b border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/50">
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Менеджер</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Жалоба</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Бронирование</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Причина</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Запрошено</th>
              <th class="px-5 py-3 text-left text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Истекает</th>
              <th class="px-5 py-3 text-right text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Действия</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
            <tr
              v-for="req in filteredRequests"
              :key="req.id"
              class="hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
            >
              <td class="px-5 py-4">
                <p class="font-mono text-xs text-gray-700 dark:text-gray-300 truncate max-w-[120px]" :title="req.requestedByManagerId">
                  {{ req.requestedByManagerId.slice(0, 8) }}...
                </p>
              </td>
              <td class="px-5 py-4">
                <router-link
                  :to="`/complaints/${req.complaintId}`"
                  class="text-emerald-600 dark:text-emerald-400 hover:underline font-medium"
                >
                  {{ req.complaintId.slice(0, 8) }}...
                </router-link>
              </td>
              <td class="px-5 py-4 font-semibold text-gray-900 dark:text-white">
                #{{ req.bookingId }}
              </td>
              <td class="px-5 py-4">
                <p class="text-gray-700 dark:text-gray-300 truncate max-w-[200px]" :title="req.reason">
                  {{ req.reason }}
                </p>
              </td>
              <td class="px-5 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-xs font-bold', statusBadge(req.status)]">
                  {{ statusLabel(req.status) }}
                </span>
              </td>
              <td class="px-5 py-4 text-gray-500 dark:text-gray-400 text-xs">
                {{ formatDateTime(req.requestedAt) }}
              </td>
              <td class="px-5 py-4 text-gray-500 dark:text-gray-400 text-xs">
                {{ req.expiresAt ? formatDateTime(req.expiresAt) : "—" }}
              </td>
              <td class="px-5 py-4 text-right">
                <div class="flex items-center justify-end gap-2">
                  <!-- Pending actions -->
                  <template v-if="req.status === 1">
                    <button
                      @click="selectedRequest = req; showApproveModal = true"
                      class="px-3 py-1.5 text-xs font-semibold rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors"
                    >
                      Approve
                    </button>
                    <button
                      @click="selectedRequest = req; showRejectModal = true"
                      class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
                    >
                      Reject
                    </button>
                  </template>
                  <!-- Approved — revoke -->
                  <button
                    v-if="req.status === 2 && !isExpired(req)"
                    @click="onRevoke(req.id)"
                    :disabled="actionLoading"
                    class="px-3 py-1.5 text-xs font-semibold rounded-lg border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                  >
                    Revoke
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Approve Modal -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div
          v-if="showApproveModal && selectedRequest"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showApproveModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Одобрить запрос</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Менеджер получит временный read-only доступ к бронированию #{{ selectedRequest.bookingId }}.
            </p>
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1">Срок действия (часы)</label>
                <select
                  v-model="approveHours"
                  class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                >
                  <option :value="2">2 часа</option>
                  <option :value="8">8 часов</option>
                  <option :value="24">24 часа</option>
                  <option :value="48">48 часов</option>
                  <option :value="168">7 дней</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1">Примечание (необязательно)</label>
                <textarea
                  v-model="approveNote"
                  rows="2"
                  placeholder="Комментарий..."
                  class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
                />
              </div>
            </div>
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showApproveModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onApprove"
                :disabled="actionLoading"
                class="px-4 py-2 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Одобрить" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Reject Modal -->
    <Teleport to="body">
      <Transition
        enter-active-class="transition duration-200"
        enter-from-class="opacity-0"
        enter-to-class="opacity-100"
        leave-active-class="transition duration-150"
        leave-from-class="opacity-100"
        leave-to-class="opacity-0"
      >
        <div
          v-if="showRejectModal && selectedRequest"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showRejectModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Отклонить запрос</h3>
            <textarea
              v-model="rejectNote"
              rows="3"
              placeholder="Причина отклонения (необязательно)..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showRejectModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onReject"
                :disabled="actionLoading"
                class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Отклонить" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  getAllAccessRequests,
  approveAccessRequest,
  rejectAccessRequest,
  revokeAccessRequest,
} from "../api/accessRequests";
import type { AccessRequest } from "../types/AccessRequest";
import { formatDateTime } from "../utils/formatters";
import { useToast } from "../composables/useToast";

const toast = useToast();

const loading = ref(false);
const actionLoading = ref(false);
const requests = ref<AccessRequest[]>([]);
const activeFilter = ref<number | null>(null);

const selectedRequest = ref<AccessRequest | null>(null);
const showApproveModal = ref(false);
const showRejectModal = ref(false);
const approveHours = ref(24);
const approveNote = ref("");
const rejectNote = ref("");

const filters = computed(() => [
  { label: "Все", value: null as number | null, count: requests.value.length },
  { label: "Ожидающие", value: 1, count: requests.value.filter((r) => r.status === 1).length },
  { label: "Одобренные", value: 2, count: requests.value.filter((r) => r.status === 2).length },
  { label: "Отклонённые", value: 3, count: requests.value.filter((r) => r.status === 3).length },
]);

const filteredRequests = computed(() => {
  if (activeFilter.value == null) return requests.value;
  return requests.value.filter((r) => r.status === activeFilter.value);
});

function statusLabel(status: number): string {
  const map: Record<number, string> = {
    1: "Ожидает",
    2: "Одобрен",
    3: "Отклонён",
    4: "Истёк",
    5: "Отозван",
  };
  return map[status] ?? "—";
}

function statusBadge(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    2: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    3: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
    4: "bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400",
    5: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-500";
}

function isExpired(req: AccessRequest): boolean {
  if (!req.expiresAt) return false;
  return new Date(req.expiresAt) <= new Date();
}

async function loadRequests() {
  loading.value = true;
  try {
    requests.value = await getAllAccessRequests();
  } catch {
    toast.error("Ошибка при загрузке запросов");
  } finally {
    loading.value = false;
  }
}

async function onApprove() {
  if (actionLoading.value || !selectedRequest.value) return;
  actionLoading.value = true;
  try {
    await approveAccessRequest(
      selectedRequest.value.id,
      approveNote.value.trim() || undefined,
      approveHours.value,
    );
    showApproveModal.value = false;
    approveNote.value = "";
    approveHours.value = 24;
    toast.success("Запрос одобрен");
    await loadRequests();
  } catch {
    toast.error("Ошибка при одобрении запроса");
  } finally {
    actionLoading.value = false;
  }
}

async function onReject() {
  if (actionLoading.value || !selectedRequest.value) return;
  actionLoading.value = true;
  try {
    await rejectAccessRequest(
      selectedRequest.value.id,
      rejectNote.value.trim() || undefined,
    );
    showRejectModal.value = false;
    rejectNote.value = "";
    toast.success("Запрос отклонён");
    await loadRequests();
  } catch {
    toast.error("Ошибка при отклонении запроса");
  } finally {
    actionLoading.value = false;
  }
}

async function onRevoke(id: string) {
  if (actionLoading.value) return;
  actionLoading.value = true;
  try {
    await revokeAccessRequest(id);
    toast.success("Доступ отозван");
    await loadRequests();
  } catch {
    toast.error("Ошибка при отзыве доступа");
  } finally {
    actionLoading.value = false;
  }
}

onMounted(loadRequests);
</script>
