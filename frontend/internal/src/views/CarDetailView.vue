<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">

    <!-- ── Header ─────────────────────────────────────────────────── -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(249,115,22,0.14),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(249,115,22,0.18),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex items-start gap-4">
        <router-link
          to="/cars"
          class="mt-1 shrink-0 px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-emerald-500 dark:hover:border-emerald-500 transition-colors"
        >
          ← Назад
        </router-link>

        <div class="min-w-0 space-y-1 flex-1">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
            Data Management · Автомобиль
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white truncate">
            <template v-if="loading">Загрузка…</template>
            <template v-else-if="car">
              {{ car.modelBrand }} {{ car.modelName }}
              <span class="text-gray-400 dark:text-gray-500 font-medium">({{ car.modelYear }})</span>
            </template>
            <template v-else>Автомобиль</template>
          </h1>
          <div v-if="car" class="flex items-center gap-3 flex-wrap">
            <span class="font-mono text-sm text-gray-600 dark:text-gray-400">{{ car.licensePlate }}</span>
            <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', carStatusBadge(car.status)]">
              {{ carStatusLabel(car.status) }}
            </span>
          </div>
        </div>
      </div>
    </header>

    <!-- ── Loading state ──────────────────────────────────────────── -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-10 text-gray-500 dark:text-gray-400 font-medium text-center"
    >
      Загрузка данных автомобиля…
    </div>

    <!-- ── Not found ──────────────────────────────────────────────── -->
    <div
      v-else-if="notFound"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Автомобиль не найден.
    </div>

    <!-- ── Content ────────────────────────────────────────────────── -->
    <template v-else-if="car">

      <!-- ── Summary strip ────────────────────────────────────────── -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- Price / hour -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена / час</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ car.priceHour ? formatPrice(car.priceHour) : "—" }}
          </p>
        </div>

        <!-- Price / day -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена / сутки</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
            {{ car.priceDay ? formatPrice(car.priceDay) : "—" }}
          </p>
        </div>

        <!-- Rating -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Рейтинг</p>
          <div class="flex items-baseline gap-2">
            <p class="text-2xl font-extrabold text-amber-500 dark:text-amber-400">
              <template v-if="car.rating">
                <!-- star icon inline -->
                <span class="inline-flex items-center gap-1">
                  <svg class="w-5 h-5 fill-amber-400" viewBox="0 0 20 20"><path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.957a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.37 2.448a1 1 0 00-.364 1.118l1.287 3.957c.3.921-.755 1.688-1.54 1.118l-3.37-2.448a1 1 0 00-1.175 0l-3.37 2.448c-.784.57-1.838-.197-1.539-1.118l1.287-3.957a1 1 0 00-.364-1.118L2.063 9.384c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69L9.049 2.927z"/></svg>
                  {{ car.rating.toFixed(1) }}
                </span>
              </template>
              <template v-else>—</template>
            </p>
            <span class="text-sm text-gray-400 dark:text-gray-500 font-medium">({{ car.ratingsCount }})</span>
          </div>
        </div>

        <!-- Status -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 flex flex-col gap-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</p>
          <span :class="['mt-1 self-start px-3 py-1 rounded-full text-sm font-semibold', carStatusBadge(car.status)]">
            {{ carStatusLabel(car.status) }}
          </span>
        </div>
      </div>

      <!-- ── Photo gallery ─────────────────────────────────────────── -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
        <div class="px-8 py-5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Фотографии</h2>
          <span class="text-xs text-gray-400 dark:text-gray-500 font-medium">{{ images.length }} фото</span>
        </div>

        <!-- Loading images -->
        <div v-if="imagesLoading" class="p-8 text-gray-400 dark:text-gray-500 text-sm text-center font-medium">
          Загрузка фотографий…
        </div>

        <!-- Empty -->
        <div
          v-else-if="images.length === 0"
          class="p-12 text-center"
        >
          <div class="mx-auto mb-3 w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-400 dark:text-gray-500">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="m2.25 15.75 5.159-5.159a2.25 2.25 0 0 1 3.182 0l5.159 5.159m-1.5-1.5 1.409-1.409a2.25 2.25 0 0 1 3.182 0l2.909 2.909M3 20.25h18M3.75 4.5h16.5a.75.75 0 0 1 .75.75v13.5a.75.75 0 0 1-.75.75H3.75a.75.75 0 0 1-.75-.75V5.25a.75.75 0 0 1 .75-.75Z"/>
            </svg>
          </div>
          <p class="text-gray-500 dark:text-gray-400 font-medium text-sm">Фотографии не загружены</p>
        </div>

        <!-- Grid -->
        <div v-else class="p-6 grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3">
          <button
            v-for="img in images"
            :key="img.id"
            @click="openLightbox(img.imageUrl)"
            class="relative aspect-[4/3] rounded-2xl overflow-hidden border border-gray-200 dark:border-gray-700 hover:border-emerald-400 dark:hover:border-emerald-500 transition-colors group focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
          >
            <img
              :src="img.imageUrl"
              :alt="`Фото ${img.id}`"
              class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
              loading="lazy"
            />
            <span
              v-if="img.isPrimary"
              class="absolute top-1.5 left-1.5 px-2 py-0.5 rounded-full bg-emerald-500/90 text-white text-[10px] font-bold"
            >
              Главное
            </span>
          </button>
        </div>
      </div>

      <!-- ── Lightbox overlay ──────────────────────────────────────── -->
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
            v-if="lightboxUrl"
            class="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4"
            @click.self="lightboxUrl = null"
          >
            <div class="relative max-w-5xl w-full">
              <img
                :src="lightboxUrl"
                alt="Просмотр"
                class="w-full max-h-[85vh] object-contain rounded-2xl shadow-2xl"
              />
              <button
                @click="lightboxUrl = null"
                class="absolute -top-3 -right-3 w-9 h-9 rounded-full bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 shadow-lg flex items-center justify-center text-gray-500 hover:text-gray-900 dark:hover:text-white transition-colors"
                aria-label="Закрыть"
              >
                <svg class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12"/>
                </svg>
              </button>
            </div>
          </div>
        </Transition>
      </Teleport>

      <!-- ── Car info card (read-only) ─────────────────────────────── -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
        <h2 class="text-base font-bold text-gray-900 dark:text-white">Информация об автомобиле</h2>

        <dl class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Марка</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ car.modelBrand }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Модель</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ car.modelName }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Год выпуска</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ car.modelYear }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Гос. номер</dt>
            <dd class="font-mono text-gray-900 dark:text-white font-semibold">{{ car.licensePlate }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Цвет</dt>
            <dd class="text-gray-900 dark:text-white font-medium">{{ car.color || "—" }}</dd>
          </div>
          <div>
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Добавлена</dt>
            <dd class="text-gray-600 dark:text-gray-400 text-sm">{{ formatDate(car.createdAt) }}</dd>
          </div>
          <div class="sm:col-span-2 lg:col-span-3">
            <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Партнёр (ID)</dt>
            <dd>
              <EntityLink :to="`/partners/${car.partnerUserId}`">
                <span class="font-mono text-sm">{{ car.partnerUserId }}</span>
              </EntityLink>
            </dd>
          </div>
        </dl>

        <!-- Commercial badge tags -->
        <div v-if="car.commercialBadgeKeys?.length" class="pt-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-2">Теги</p>
          <div class="flex flex-wrap gap-2">
            <span
              v-for="tag in car.commercialBadgeKeys"
              :key="tag"
              :class="['px-3 py-1 rounded-full text-xs font-semibold', badgeTagClass(tag)]"
            >
              {{ getSemanticTagLabel(tag) }}
            </span>
          </div>
        </div>
      </div>

      <!-- ── Edit form (collapsible) ───────────────────────────────── -->
      <div
        v-if="canUpdate || canDelete"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <!-- Toggle header -->
        <button
          @click="editOpen = !editOpen"
          class="w-full flex items-center justify-between px-8 py-5 text-left hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors"
        >
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Редактирование</h2>
          <svg
            :class="['w-5 h-5 text-gray-400 transition-transform duration-200', editOpen ? 'rotate-180' : '']"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            stroke-width="2"
          >
            <path stroke-linecap="round" stroke-linejoin="round" d="m19 9-7 7-7-7"/>
          </svg>
        </button>

        <!-- Collapsible body -->
        <Transition
          enter-active-class="transition-all duration-200 ease-out"
          enter-from-class="opacity-0 -translate-y-2"
          enter-to-class="opacity-100 translate-y-0"
          leave-active-class="transition-all duration-150 ease-in"
          leave-from-class="opacity-100 translate-y-0"
          leave-to-class="opacity-0 -translate-y-2"
        >
          <div v-if="editOpen" class="px-8 pb-8 border-t border-gray-100 dark:border-gray-800">
            <form @submit.prevent="onSave" class="space-y-6 pt-6">
              <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
                <!-- License plate -->
                <div>
                  <label class="block text-sm font-bold uppercase tracking-[0.08em] text-gray-700 dark:text-gray-300 mb-2">
                    Гос. номер
                  </label>
                  <input
                    v-model="form.licensePlate"
                    type="text"
                    required
                    :disabled="!canUpdate"
                    placeholder="000 AA 00"
                    class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white font-mono placeholder-gray-400 focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                  />
                </div>

                <!-- Color -->
                <div>
                  <label class="block text-sm font-bold uppercase tracking-[0.08em] text-gray-700 dark:text-gray-300 mb-2">
                    Цвет
                  </label>
                  <input
                    v-model="form.color"
                    type="text"
                    :disabled="!canUpdate"
                    placeholder="Не указан"
                    class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white placeholder-gray-400 focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                  />
                </div>

                <!-- Status -->
                <div>
                  <label class="block text-sm font-bold uppercase tracking-[0.08em] text-gray-700 dark:text-gray-300 mb-2">
                    Статус
                  </label>
                  <select
                    v-model.number="form.status"
                    :disabled="!canUpdate"
                    class="w-full px-4 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                  >
                    <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">
                      {{ opt.label }}
                    </option>
                  </select>
                </div>
              </div>

              <!-- Actions -->
              <div class="flex items-center gap-3 pt-2">
                <button
                  v-if="canUpdate"
                  type="submit"
                  :disabled="saving"
                  class="px-6 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
                >
                  {{ saving ? "Сохранение…" : "Сохранить изменения" }}
                </button>

                <button
                  v-if="canDelete"
                  type="button"
                  @click="showDeleteModal = true"
                  :disabled="deleting"
                  class="px-6 py-3 rounded-2xl border border-red-300 dark:border-red-500/40 text-red-600 dark:text-red-400 font-bold hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
                >
                  {{ deleting ? "Удаление…" : "Удалить автомобиль" }}
                </button>
              </div>
            </form>
          </div>
        </Transition>
      </div>

      <!-- ── Reviews ────────────────────────────────────────────────── -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
        <div class="px-8 py-5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Отзывы</h2>
          <span class="text-xs text-gray-400 dark:text-gray-500 font-medium">{{ comments.length }} отзывов</span>
        </div>

        <div v-if="commentsLoading" class="p-8 text-gray-400 dark:text-gray-500 text-sm text-center font-medium">
          Загрузка отзывов…
        </div>

        <div
          v-else-if="comments.length === 0"
          class="p-12 text-center"
        >
          <div class="mx-auto mb-3 w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-400 dark:text-gray-500">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M7.5 8.25h9m-9 3H12m-9.75 1.51c0 1.6 1.123 2.994 2.707 3.227 1.129.166 2.27.293 3.423.379.35.026.67.21.865.501L12 21l2.755-4.133a1.14 1.14 0 0 1 .865-.501 48.172 48.172 0 0 0 3.423-.379c1.584-.233 2.707-1.626 2.707-3.228V6.741c0-1.602-1.123-2.995-2.707-3.228A48.394 48.394 0 0 0 12 3c-2.392 0-4.744.175-7.043.513C3.373 3.746 2.25 5.14 2.25 6.741v6.018Z"/>
            </svg>
          </div>
          <p class="text-gray-500 dark:text-gray-400 font-medium text-sm">Отзывов пока нет</p>
        </div>

        <ul v-else class="divide-y divide-gray-100 dark:divide-gray-800">
          <li
            v-for="comment in comments"
            :key="comment.id"
            class="px-8 py-5 space-y-2 hover:bg-gray-50 dark:hover:bg-gray-800/30 transition-colors"
          >
            <div class="flex items-center justify-between gap-4 flex-wrap">
              <div class="flex items-center gap-3">
                <div class="w-8 h-8 rounded-full bg-emerald-100 dark:bg-emerald-500/20 flex items-center justify-center text-emerald-700 dark:text-emerald-300 text-sm font-bold shrink-0">
                  {{ (comment.userName || "?").charAt(0).toUpperCase() }}
                </div>
                <span class="font-semibold text-gray-900 dark:text-white text-sm">
                  {{ comment.userName || "Аноним" }}
                </span>
                <!-- Star rating -->
                <div class="flex items-center gap-0.5">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    :class="['w-3.5 h-3.5', i <= comment.rating ? 'fill-amber-400 text-amber-400' : 'fill-gray-200 text-gray-200 dark:fill-gray-700 dark:text-gray-700']"
                    viewBox="0 0 20 20"
                  >
                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.286 3.957a1 1 0 00.95.69h4.162c.969 0 1.371 1.24.588 1.81l-3.37 2.448a1 1 0 00-.364 1.118l1.287 3.957c.3.921-.755 1.688-1.54 1.118l-3.37-2.448a1 1 0 00-1.175 0l-3.37 2.448c-.784.57-1.838-.197-1.539-1.118l1.287-3.957a1 1 0 00-.364-1.118L2.063 9.384c-.783-.57-.38-1.81.588-1.81h4.162a1 1 0 00.95-.69L9.049 2.927z"/>
                  </svg>
                  <span class="ml-1 text-xs text-gray-500 dark:text-gray-400 font-medium">{{ comment.rating }}/5</span>
                </div>
              </div>
              <time class="text-xs text-gray-400 dark:text-gray-500 shrink-0">{{ formatDateTime(comment.createdAt) }}</time>
            </div>
            <p class="text-sm text-gray-700 dark:text-gray-300 leading-relaxed pl-11">{{ comment.content }}</p>
          </li>
        </ul>
      </div>

      <!-- ── Related bookings ───────────────────────────────────────── -->
      <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
        <div class="px-8 py-5 border-b border-gray-100 dark:border-gray-800 flex items-center justify-between">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Бронирования</h2>
          <span class="text-xs text-gray-400 dark:text-gray-500 font-medium">{{ bookings.length }} записей</span>
        </div>

        <div v-if="bookingsLoading" class="p-8 text-gray-400 dark:text-gray-500 text-sm text-center font-medium">
          Загрузка бронирований…
        </div>

        <div
          v-else-if="bookings.length === 0"
          class="p-12 text-center"
        >
          <div class="mx-auto mb-3 w-12 h-12 rounded-2xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-gray-400 dark:text-gray-500">
            <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 0 1 2.25-2.25h13.5A2.25 2.25 0 0 1 21 7.5v11.25m-18 0A2.25 2.25 0 0 0 5.25 21h13.5A2.25 2.25 0 0 0 21 18.75m-18 0v-7.5A2.25 2.25 0 0 1 5.25 9h13.5A2.25 2.25 0 0 1 21 11.25v7.5"/>
            </svg>
          </div>
          <p class="text-gray-500 dark:text-gray-400 font-medium text-sm">Бронирований не найдено</p>
        </div>

        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr class="border-b border-gray-200 dark:border-gray-800">
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Период</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Итого</th>
                <th class="text-left px-6 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="booking in bookings"
                :key="booking.id"
                @click="router.push(`/bookings/${booking.id}`)"
                class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
              >
                <td class="px-6 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">
                  #{{ booking.id }}
                </td>
                <td class="px-6 py-3 text-gray-700 dark:text-gray-300 whitespace-nowrap text-xs">
                  {{ formatDateTime(booking.startTime) }}
                  <span class="text-gray-400 dark:text-gray-500 mx-1">→</span>
                  {{ formatDateTime(booking.endTime) }}
                </td>
                <td class="px-6 py-3 text-gray-900 dark:text-white font-semibold whitespace-nowrap">
                  {{ booking.totalPrice ? formatPrice(booking.totalPrice) : "—" }}
                </td>
                <td class="px-6 py-3">
                  <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', bookingStatusBadge(booking.status)]">
                    {{ bookingStatusLabel(booking.status) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

    </template><!-- /car -->

    <!-- ── Delete confirm modal ──────────────────────────────────────── -->
    <ConfirmModal
      :show="showDeleteModal"
      title="Удалить автомобиль"
      :message="`Вы уверены, что хотите удалить ${car?.modelBrand ?? ''} ${car?.modelName ?? ''} (${car?.licensePlate ?? ''})? Это действие необратимо.`"
      confirmText="Удалить"
      variant="danger"
      @confirm="onDelete"
      @cancel="showDeleteModal = false"
    />

  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";

import {
  getPartnerCar,
  getPartnerCarImages,
  getPartnerCarComments,
  updatePartnerCar,
  deletePartnerCar,
  type PartnerCarDto,
  type CarImageDto,
  type CarCommentDto,
} from "../api/cars";
import { getAllBookings, type BookingDto } from "../api/bookings";
import { formatDate, formatDateTime, formatPrice } from "../utils/formatters";
import {
  carStatusLabel,
  carStatusBadge,
  bookingStatusLabel,
  bookingStatusBadge,
} from "../utils/statusMaps";
import { getSemanticTagLabel } from "../utils/partnerCarSemanticTags";
import { useToast } from "../composables/useToast";
import { auth } from "../store/auth";
import EntityLink from "../components/EntityLink.vue";
import ConfirmModal from "../components/ConfirmModal.vue";

const route = useRoute();
const router = useRouter();
const toast = useToast();

// ── Reactive state ─────────────────────────────────────────────────────
const loading = ref(false);
const notFound = ref(false);
const car = ref<PartnerCarDto | null>(null);

const imagesLoading = ref(false);
const images = ref<CarImageDto[]>([]);

const commentsLoading = ref(false);
const comments = ref<CarCommentDto[]>([]);

const bookingsLoading = ref(false);
const bookings = ref<BookingDto[]>([]);

const lightboxUrl = ref<string | null>(null);

const editOpen = ref(false);
const saving = ref(false);
const deleting = ref(false);
const showDeleteModal = ref(false);

// ── Permissions ────────────────────────────────────────────────────────
const canUpdate = computed(() => auth.hasPermission("PartnerCar.Update"));
const canDelete = computed(() => auth.hasPermission("PartnerCar.Delete"));

// ── Edit form ──────────────────────────────────────────────────────────
const statusOptions = [
  { value: 0, label: "На модерации" },
  { value: 1, label: "Активна" },
  { value: 2, label: "Неактивна" },
  { value: 3, label: "Заблокирована" },
] as const;

const form = reactive({
  licensePlate: "",
  color: "",
  status: 0 as number,
});

function populateForm(data: PartnerCarDto) {
  form.licensePlate = data.licensePlate ?? "";
  form.color = data.color ?? "";
  form.status = data.status;
}

// ── Tag colours ────────────────────────────────────────────────────────
const tagColorMap: Record<string, string> = {
  econom:   "bg-sky-100    text-sky-700    dark:bg-sky-500/20    dark:text-sky-300",
  comfort:  "bg-blue-100   text-blue-700   dark:bg-blue-500/20   dark:text-blue-300",
  business: "bg-violet-100 text-violet-700 dark:bg-violet-500/20 dark:text-violet-300",
  sport:    "bg-rose-100   text-rose-700   dark:bg-rose-500/20   dark:text-rose-300",
  suv:      "bg-amber-100  text-amber-700  dark:bg-amber-500/20  dark:text-amber-300",
  electric: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
  family:   "bg-teal-100   text-teal-700   dark:bg-teal-500/20   dark:text-teal-300",
};

function badgeTagClass(tag: string): string {
  return tagColorMap[tag] ?? "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-300";
}

// ── Lightbox ───────────────────────────────────────────────────────────
function openLightbox(url: string) {
  lightboxUrl.value = url;
}

// ── Data loading ───────────────────────────────────────────────────────
async function loadCar() {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    const data = await getPartnerCar(id);
    car.value = data;
    populateForm(data);
  } catch {
    notFound.value = true;
    return;
  } finally {
    loading.value = false;
  }
}

async function loadImages(id: number) {
  imagesLoading.value = true;
  try {
    images.value = await getPartnerCarImages(id);
  } catch {
    // 404 or empty — treat gracefully
    images.value = [];
  } finally {
    imagesLoading.value = false;
  }
}

async function loadComments(id: number) {
  commentsLoading.value = true;
  try {
    comments.value = await getPartnerCarComments(id);
  } catch {
    comments.value = [];
  } finally {
    commentsLoading.value = false;
  }
}

async function loadBookings(id: number) {
  bookingsLoading.value = true;
  try {
    const result = await getAllBookings({ partnerCarId: id, page: 1, pageSize: 20 });
    bookings.value = result.items ?? [];
  } catch {
    bookings.value = [];
  } finally {
    bookingsLoading.value = false;
  }
}

// ── Save ───────────────────────────────────────────────────────────────
async function onSave() {
  if (saving.value || !car.value) return;
  saving.value = true;
  try {
    const updated = await updatePartnerCar(car.value.id, {
      licensePlate: form.licensePlate,
      color: form.color || undefined,
      status: form.status,
    });
    car.value = updated;
    populateForm(updated);
    toast.success("Автомобиль успешно обновлён");
  } catch {
    toast.error("Не удалось сохранить изменения");
  } finally {
    saving.value = false;
  }
}

// ── Delete ─────────────────────────────────────────────────────────────
async function onDelete() {
  if (deleting.value || !car.value) return;
  showDeleteModal.value = false;
  deleting.value = true;
  try {
    await deletePartnerCar(car.value.id);
    toast.success("Автомобиль удалён");
    router.push("/cars");
  } catch {
    toast.error("Не удалось удалить автомобиль");
  } finally {
    deleting.value = false;
  }
}

// ── Mount ──────────────────────────────────────────────────────────────
onMounted(async () => {
  const id = Number(route.params.id);
  if (!id) {
    notFound.value = true;
    return;
  }

  // Load car first; only proceed with side-panel data if car exists
  await loadCar();

  if (car.value) {
    // Fire secondary requests in parallel — failures are silent
    await Promise.allSettled([
      loadImages(id),
      loadComments(id),
      loadBookings(id),
    ]);
  }
});
</script>
