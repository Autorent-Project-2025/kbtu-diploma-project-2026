<template>
  <div
    class="min-h-screen bg-gray-50 dark:bg-gray-950 py-24 px-4 sm:px-6 lg:px-8 transition-colors duration-300"
  >
    <div class="max-w-7xl mx-auto space-y-8">
      <!-- ── Hero Header ─────────────────────────────────────────────────── -->
      <header
        class="relative overflow-hidden rounded-[32px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(59,130,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8 sm:p-10"
      >
        <div class="flex flex-col sm:flex-row sm:items-center gap-6">
          <!-- Avatar -->
          <div class="relative shrink-0">
            <div
              class="w-24 h-24 rounded-3xl overflow-hidden border-2 border-emerald-400/40 shadow-xl"
            >
              <img
                v-if="profile?.avatarUrl"
                :src="profile.avatarUrl"
                :alt="`${profile.firstName} ${profile.lastName}`"
                class="w-full h-full object-cover"
              />
              <div
                v-else
                class="w-full h-full bg-gradient-to-br from-emerald-500 to-emerald-700 flex items-center justify-center"
              >
                <span class="text-white text-3xl font-extrabold">
                  {{ initials }}
                </span>
              </div>
            </div>
            <!-- Upload button -->
            <label
              class="absolute -bottom-2 -right-2 w-8 h-8 rounded-xl bg-emerald-600 hover:bg-emerald-700 flex items-center justify-center cursor-pointer shadow-lg transition-colors"
              title="Загрузить фото"
            >
              <svg
                class="w-4 h-4 text-white"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 4v16m8-8H4"
                />
              </svg>
              <input
                type="file"
                accept="image/*"
                class="hidden"
                @change="handleAvatarUpload"
              />
            </label>
          </div>

          <!-- Name & meta -->
          <div class="space-y-1 flex-1">
            <p
              class="text-sm font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400"
            >
              Личный кабинет
            </p>
            <h1
              class="text-4xl sm:text-5xl font-extrabold text-gray-900 dark:text-white"
            >
              <span v-if="profile"
                >{{ profile.firstName }} {{ profile.lastName }}</span
              >
              <span v-else class="opacity-40">Загрузка...</span>
            </h1>
            <p
              v-if="profile"
              class="text-base text-gray-500 dark:text-gray-400"
            >
              {{ profile.phoneNumber }} · На сайте с
              {{ formatDate(profile.createdOn) }}
            </p>
          </div>

          <!-- Edit toggle -->
          <button
            @click="editMode = !editMode"
            class="shrink-0 px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-emerald-500 dark:hover:border-emerald-500 transition-colors"
          >
            {{ editMode ? "Отмена" : "Редактировать" }}
          </button>
        </div>
      </header>

      <!-- ── Loading / Error ─────────────────────────────────────────────── -->
      <div
        v-if="loading"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-10 text-center text-gray-500 dark:text-gray-400"
      >
        Загружаем профиль...
      </div>

      <div
        v-else-if="loadError"
        class="rounded-3xl border border-red-300/70 dark:border-red-500/30 bg-red-50 dark:bg-red-900/20 shadow-xl p-8 text-red-700 dark:text-red-300"
      >
        {{ loadError }}
      </div>

      <template v-else-if="profile">
        <!-- ── Stats strip ──────────────────────────────────────────────── -->
        <section class="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
          <article
            class="rounded-3xl border border-emerald-200/70 dark:border-emerald-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-emerald-600 dark:text-emerald-400 font-bold"
            >
              Всего броней
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ stats?.totalCount ?? "—" }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">За всё время</p>
          </article>

          <article
            class="rounded-3xl border border-blue-200/70 dark:border-blue-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-blue-600 dark:text-blue-400 font-bold"
            >
              Активных
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ stats?.activeCount ?? "—" }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Сейчас в процессе
            </p>
          </article>

          <article
            class="rounded-3xl border border-amber-200/70 dark:border-amber-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-amber-600 dark:text-amber-400 font-bold"
            >
              Завершённых
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ stats?.completedCount ?? "—" }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Успешно закрыто
            </p>
          </article>

          <article
            class="rounded-3xl border border-violet-200/70 dark:border-violet-700/40 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-3"
          >
            <p
              class="text-sm uppercase tracking-[0.18em] text-violet-600 dark:text-violet-400 font-bold"
            >
              Потрачено
            </p>
            <p class="text-4xl font-extrabold text-gray-900 dark:text-white">
              {{ formatMoney(stats?.totalSpent ?? 0) }}
            </p>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              По завершённым
            </p>
          </article>
        </section>

        <!-- ── Edit Form ────────────────────────────────────────────────── -->
        <section
          v-if="editMode"
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-6"
        >
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">
            Редактировать профиль
          </h2>

          <div class="grid sm:grid-cols-2 gap-4">
            <div class="space-y-1.5">
              <label
                class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >Имя</label
              >
              <input
                v-model="form.firstName"
                type="text"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-transparent text-gray-900 dark:text-white focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all"
              />
              <p v-if="formErrors.firstName" class="text-xs text-red-500">
                {{ formErrors.firstName }}
              </p>
            </div>

            <div class="space-y-1.5">
              <label
                class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >Фамилия</label
              >
              <input
                v-model="form.lastName"
                type="text"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-transparent text-gray-900 dark:text-white focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all"
              />
              <p v-if="formErrors.lastName" class="text-xs text-red-500">
                {{ formErrors.lastName }}
              </p>
            </div>

            <div class="space-y-1.5">
              <label
                class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >Телефон</label
              >
              <input
                v-model="form.phoneNumber"
                type="tel"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-transparent text-gray-900 dark:text-white focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all"
              />
              <p v-if="formErrors.phoneNumber" class="text-xs text-red-500">
                {{ formErrors.phoneNumber }}
              </p>
            </div>

            <div class="space-y-1.5">
              <label
                class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >Дата рождения</label
              >
              <input
                v-model="form.birthDate"
                type="date"
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-transparent text-gray-900 dark:text-white focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all"
              />
              <p v-if="formErrors.birthDate" class="text-xs text-red-500">
                {{ formErrors.birthDate }}
              </p>
            </div>

            <div class="space-y-1.5 sm:col-span-2">
              <label
                class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >URL аватара</label
              >
              <input
                v-model="form.avatarUrl"
                type="url"
                placeholder="https://..."
                class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-transparent text-gray-900 dark:text-white focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 outline-none transition-all"
              />
            </div>
          </div>

          <div class="flex items-center gap-3">
            <button
              @click="saveProfile"
              :disabled="saving"
              class="px-6 py-3 bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 text-white font-bold rounded-2xl shadow-lg shadow-emerald-500/20 transition-colors"
            >
              {{ saving ? "Сохраняем..." : "Сохранить" }}
            </button>
            <button
              @click="editMode = false"
              class="px-6 py-3 border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 font-semibold rounded-2xl hover:border-gray-400 transition-colors"
            >
              Отмена
            </button>
          </div>
        </section>

        <!-- ── Documents ────────────────────────────────────────────────── -->
        <section
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-4"
        >
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">
            Документы
          </h2>

          <div class="space-y-3">
            <div
              class="flex items-center justify-between py-3 border-b border-gray-100 dark:border-gray-800"
            >
              <div class="flex items-center gap-3">
                <div
                  class="w-9 h-9 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
                >
                  <svg
                    class="w-5 h-5 text-gray-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2"
                    />
                  </svg>
                </div>
                <span class="font-medium text-gray-900 dark:text-white"
                  >Удостоверение личности</span
                >
              </div>
              <span
                :class="
                  profile.identityDocumentFileName
                    ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400'
                    : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400'
                "
                class="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide"
              >
                {{
                  profile.identityDocumentFileName ? "Загружен" : "Не загружен"
                }}
              </span>
            </div>

            <div class="flex items-center justify-between py-3">
              <div class="flex items-center gap-3">
                <div
                  class="w-9 h-9 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center"
                >
                  <svg
                    class="w-5 h-5 text-gray-500"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"
                    />
                  </svg>
                </div>
                <span class="font-medium text-gray-900 dark:text-white"
                  >Водительские права</span
                >
              </div>
              <span
                :class="
                  profile.driverLicenseFileName
                    ? 'bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400'
                    : 'bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400'
                "
                class="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wide"
              >
                {{ profile.driverLicenseFileName ? "Загружен" : "Не загружен" }}
              </span>
            </div>
          </div>
        </section>

        <!-- ── Bookings history ─────────────────────────────────────────── -->
        <section
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
        >
          <div
            class="p-6 sm:p-8 border-b border-gray-100 dark:border-gray-800 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4"
          >
            <h2 class="text-xl font-bold text-gray-900 dark:text-white">
              История броней
            </h2>
            <router-link
              to="/bookings"
              class="text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:underline"
            >
              Все бронирования →
            </router-link>
          </div>

          <div
            v-if="bookingsLoading"
            class="p-8 text-center text-gray-500 dark:text-gray-400"
          >
            Загружаем бронирования...
          </div>

          <div
            v-else-if="recentBookings.length === 0"
            class="p-8 text-center text-gray-400 dark:text-gray-500 border-2 border-dashed border-gray-200 dark:border-gray-800 m-6 rounded-2xl"
          >
            Броней пока нет
          </div>

          <table v-else class="w-full text-sm">
            <thead class="bg-gray-50 dark:bg-gray-800/50">
              <tr>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Автомобиль
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden sm:table-cell"
                >
                  Период
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400 hidden md:table-cell"
                >
                  Сумма
                </th>
                <th
                  class="px-6 py-3 text-left text-xs font-bold uppercase tracking-[0.15em] text-gray-500 dark:text-gray-400"
                >
                  Статус
                </th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="b in recentBookings"
                :key="b.id"
                class="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
              >
                <td class="px-6 py-4 font-medium text-gray-900 dark:text-white">
                  {{ b.carBrand }} {{ b.carModel }}
                </td>
                <td
                  class="px-6 py-4 text-gray-600 dark:text-gray-400 hidden sm:table-cell"
                >
                  {{ formatDateShort(b.startDate) }} —
                  {{ formatDateShort(b.endDate) }}
                </td>
                <td
                  class="px-6 py-4 font-semibold text-gray-900 dark:text-white hidden md:table-cell"
                >
                  {{ b.price != null ? formatMoney(b.price) : "—" }}
                </td>
                <td class="px-6 py-4">
                  <span
                    :class="statusClass(b.status)"
                    class="px-2.5 py-1 rounded-full text-xs font-bold uppercase tracking-wide"
                  >
                    {{ statusLabel(b.status) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </section>

        <!-- ── My Reviews ───────────────────────────────────────────────── -->
        <section
          class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 sm:p-8 space-y-5"
        >
          <h2 class="text-xl font-bold text-gray-900 dark:text-white">
            Мои отзывы
          </h2>

          <div
            v-if="commentsLoading"
            class="text-center text-gray-500 dark:text-gray-400 py-4"
          >
            Загружаем отзывы...
          </div>

          <div
            v-else-if="comments.length === 0"
            class="py-10 text-center text-gray-400 dark:text-gray-500 border-2 border-dashed border-gray-200 dark:border-gray-800 rounded-2xl"
          >
            Вы ещё не оставляли отзывов
          </div>

          <div v-else class="space-y-4">
            <article
              v-for="c in comments"
              :key="c.id"
              class="p-4 rounded-2xl border border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/40 space-y-2"
            >
              <div class="flex items-center justify-between gap-2">
                <!-- Stars -->
                <div class="flex items-center gap-0.5">
                  <svg
                    v-for="n in 5"
                    :key="n"
                    class="w-4 h-4"
                    :class="
                      n <= c.rating
                        ? 'text-amber-400'
                        : 'text-gray-300 dark:text-gray-600'
                    "
                    fill="currentColor"
                    viewBox="0 0 20 20"
                  >
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"
                    />
                  </svg>
                  <span
                    class="ml-1 text-sm font-bold text-gray-700 dark:text-gray-300"
                    >{{ c.rating }}/5</span
                  >
                </div>
                <span class="text-xs text-gray-400 dark:text-gray-500">{{
                  formatDate(c.createdOn)
                }}</span>
              </div>
              <p
                class="text-gray-700 dark:text-gray-300 text-sm leading-relaxed"
              >
                {{ c.content }}
              </p>
            </article>
          </div>

          <!-- Pagination -->
          <div
            v-if="commentsTotalPages > 1"
            class="flex items-center justify-center gap-2 pt-2"
          >
            <button
              :disabled="commentsPage === 1"
              @click="loadComments(commentsPage - 1)"
              class="px-3 py-1.5 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold disabled:opacity-40 hover:border-emerald-500 transition-colors"
            >
              ←
            </button>
            <button
              v-for="p in commentsTotalPages"
              :key="p"
              @click="loadComments(p)"
              :class="[
                'px-3 py-1.5 rounded-xl text-sm font-bold transition-colors',
                p === commentsPage
                  ? 'bg-gray-900 text-white dark:bg-white dark:text-gray-900'
                  : 'border border-gray-300 dark:border-gray-700 hover:border-emerald-500',
              ]"
            >
              {{ p }}
            </button>
            <button
              :disabled="commentsPage === commentsTotalPages"
              @click="loadComments(commentsPage + 1)"
              class="px-3 py-1.5 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold disabled:opacity-40 hover:border-emerald-500 transition-colors"
            >
              →
            </button>
          </div>
        </section>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import {
  getMyProfile,
  updateMyProfile,
  getMyBookingStats,
  getMyComments,
  type ClientProfile,
  type BookingStats,
  type MyComment,
} from "../api/profile";
import { getMyBookings } from "../api/booking";
import { useToast } from "../composables/useToast";
import type { Booking } from "../types/Booking";

const { showToast } = useToast();

// ─── State ────────────────────────────────────────────────────────────────────
const loading = ref(true);
const loadError = ref<string | null>(null);
const saving = ref(false);
const editMode = ref(false);
const bookingsLoading = ref(true);
const commentsLoading = ref(true);

const profile = ref<ClientProfile | null>(null);
const stats = ref<BookingStats | null>(null);
const recentBookings = ref<Booking[]>([]);
const comments = ref<MyComment[]>([]);
const commentsPage = ref(1);
const commentsTotalPages = ref(1);

const form = ref({
  firstName: "",
  lastName: "",
  birthDate: "",
  phoneNumber: "",
  avatarUrl: "",
});

const formErrors = ref<Record<string, string>>({});

// ─── Computed ─────────────────────────────────────────────────────────────────
const initials = computed(() => {
  if (!profile.value) return "?";
  return (
    (profile.value.firstName[0] ?? "") + (profile.value.lastName[0] ?? "")
  ).toUpperCase();
});

// ─── Lifecycle ────────────────────────────────────────────────────────────────
onMounted(async () => {
  await Promise.all([
    loadProfile(),
    loadStats(),
    loadRecentBookings(),
    loadComments(1),
  ]);
});

// ─── Data loaders ─────────────────────────────────────────────────────────────
async function loadProfile() {
  try {
    loading.value = true;
    profile.value = await getMyProfile();
    form.value = {
      firstName: profile.value.firstName,
      lastName: profile.value.lastName,
      birthDate: profile.value.birthDate,
      phoneNumber: profile.value.phoneNumber,
      avatarUrl: profile.value.avatarUrl ?? "",
    };
  } catch (e) {
    loadError.value = "Не удалось загрузить профиль";
  } finally {
    loading.value = false;
  }
}

async function loadStats() {
  try {
    stats.value = await getMyBookingStats();
  } catch {
    // stats are non-critical, silently fail
  }
}

async function loadRecentBookings() {
  try {
    bookingsLoading.value = true;
    const result = await getMyBookings({ page: 1, pageSize: 5 });
    recentBookings.value = Array.isArray(result) ? result : result.items;
  } catch {
    // non-critical
  } finally {
    bookingsLoading.value = false;
  }
}

async function loadComments(page: number) {
  try {
    commentsLoading.value = true;
    commentsPage.value = page;
    const result = await getMyComments(page, 5);
    comments.value = result.items;
    commentsTotalPages.value = result.totalPages;
  } catch {
    // non-critical
  } finally {
    commentsLoading.value = false;
  }
}

// ─── Actions ──────────────────────────────────────────────────────────────────
function validateForm(): boolean {
  const errors: Record<string, string> = {};
  if (!form.value.firstName.trim()) errors.firstName = "Обязательное поле";
  if (!form.value.lastName.trim()) errors.lastName = "Обязательное поле";
  if (!form.value.phoneNumber.trim()) errors.phoneNumber = "Обязательное поле";
  if (!form.value.birthDate) errors.birthDate = "Обязательное поле";
  formErrors.value = errors;
  return Object.keys(errors).length === 0;
}

async function saveProfile() {
  if (!validateForm()) return;
  try {
    saving.value = true;
    profile.value = await updateMyProfile({
      firstName: form.value.firstName.trim(),
      lastName: form.value.lastName.trim(),
      birthDate: form.value.birthDate,
      phoneNumber: form.value.phoneNumber.trim(),
      avatarUrl: form.value.avatarUrl.trim() || null,
    });
    editMode.value = false;
    showToast("Профиль обновлён", "success");
  } catch {
    showToast("Не удалось сохранить", "error");
  } finally {
    saving.value = false;
  }
}

function handleAvatarUpload(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;

  const reader = new FileReader();
  reader.onload = (e) => {
    const result = e.target?.result as string;
    if (form.value !== null) form.value.avatarUrl = result;
    if (profile.value) profile.value.avatarUrl = result;
  };
  reader.readAsDataURL(file);
}

// ─── Formatting ───────────────────────────────────────────────────────────────
function formatDate(iso: string): string {
  try {
    return new Date(iso).toLocaleDateString("ru-RU", {
      day: "numeric",
      month: "long",
      year: "numeric",
    });
  } catch {
    return iso;
  }
}

function formatDateShort(iso: string): string {
  try {
    return new Date(iso).toLocaleDateString("ru-RU", {
      day: "2-digit",
      month: "2-digit",
      year: "2-digit",
    });
  } catch {
    return iso;
  }
}

function formatMoney(amount: number): string {
  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency: "KZT",
    maximumFractionDigits: 0,
  }).format(amount);
}

function statusLabel(status: string): string {
  const map: Record<string, string> = {
    pending: "Ожидает",
    confirmed: "Подтверждён",
    active: "Активен",
    completed: "Завершён",
    canceled: "Отменён",
  };
  return map[status] ?? status;
}

function statusClass(status: string): string {
  const map: Record<string, string> = {
    pending:
      "bg-amber-100 dark:bg-amber-900/30 text-amber-700 dark:text-amber-400",
    confirmed:
      "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-400",
    active:
      "bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400",
    completed: "bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400",
    canceled: "bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-600";
}
</script>
