<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <header
      class="relative overflow-hidden rounded-[28px] border border-gray-200 dark:border-gray-800 bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.18),_transparent_38%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.16),_transparent_40%),linear-gradient(135deg,_rgba(255,255,255,0.96),_rgba(243,244,246,0.92))] dark:bg-[radial-gradient(circle_at_top_left,_rgba(16,185,129,0.22),_transparent_35%),radial-gradient(circle_at_bottom_right,_rgba(139,92,246,0.22),_transparent_40%),linear-gradient(135deg,_rgba(17,24,39,0.98),_rgba(3,7,18,0.96))] shadow-2xl p-8"
    >
      <div class="flex items-center gap-4">
        <router-link
          to="/partners"
          class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-semibold hover:border-emerald-500 transition-colors"
        >
          ← Партнёры
        </router-link>
        <div class="space-y-1">
          <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
            Partner Management
          </p>
          <h1 class="text-3xl font-extrabold text-gray-900 dark:text-white">
            <template v-if="loading">Загрузка...</template>
            <template v-else-if="partner">
              {{ partner.ownerFirstName }} {{ partner.ownerLastName }}
            </template>
            <template v-else>Партнёр не найден</template>
          </h1>
          <p v-if="partner" class="text-gray-500 dark:text-gray-400 text-sm font-medium">
            {{ partner.phoneNumber || "—" }} · ID {{ partner.id }}
          </p>
        </div>
      </div>
    </header>

    <!-- Global loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка данных партнёра...
    </div>

    <!-- Not found -->
    <div
      v-else-if="!partner"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Партнёр не найден.
    </div>

    <template v-else>
      <!-- Summary cards -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <!-- Wallet balance -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Баланс кошелька</p>
          <p class="text-2xl font-extrabold text-emerald-600 dark:text-emerald-400">
            {{ wallet ? formatPrice(wallet.balance) : "—" }}
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">{{ wallet?.currency ?? "KZT" }}</p>
        </div>

        <!-- Total cars -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Машины</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ carsTotal }}</p>
          <p class="text-xs text-gray-400 dark:text-gray-500">Зарегистрированных</p>
        </div>

        <!-- Total bookings -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Бронирования</p>
          <p class="text-2xl font-extrabold text-gray-900 dark:text-white">{{ bookingsTotal }}</p>
          <p class="text-xs text-gray-400 dark:text-gray-500">Всего</p>
        </div>

        <!-- Registration date -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-1">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Регистрация</p>
          <p class="text-lg font-extrabold text-gray-900 dark:text-white">
            {{ formatDate(partner.registrationDate) }}
          </p>
          <p class="text-xs text-gray-400 dark:text-gray-500">
            До: {{ formatDate(partner.partnershipEndDate) }}
          </p>
        </div>
      </div>

      <!-- Tab bar -->
      <div class="flex gap-1 rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-1 overflow-x-auto">
        <button
          v-for="tab in tabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          :class="[
            'px-5 py-2 rounded-xl text-sm font-semibold whitespace-nowrap transition-colors',
            activeTab === tab.key
              ? 'bg-emerald-600 text-white shadow'
              : 'text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-800',
          ]"
        >
          {{ tab.label }}
        </button>
      </div>

      <!-- ─── Tab: Обзор ─────────────────────────────────────── -->
      <template v-if="activeTab === 'overview'">
        <!-- Profile card -->
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-5">
          <div class="flex items-center gap-4">
            <div class="h-14 w-14 rounded-full bg-emerald-100 dark:bg-emerald-500/20 flex items-center justify-center text-emerald-700 dark:text-emerald-300 font-extrabold text-xl shrink-0">
              {{ partnerInitials }}
            </div>
            <div>
              <h2 class="text-xl font-extrabold text-gray-900 dark:text-white">
                {{ partner.ownerFirstName }} {{ partner.ownerLastName }}
              </h2>
              <p class="text-sm text-gray-500 dark:text-gray-400">{{ partner.phoneNumber || "Телефон не указан" }}</p>
            </div>
          </div>

          <dl class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Имя</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ partner.ownerFirstName }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Фамилия</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ partner.ownerLastName }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Телефон</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ partner.phoneNumber || "—" }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">User ID</dt>
              <dd class="text-gray-500 dark:text-gray-400 font-mono text-sm truncate" :title="partner.relatedUserId">
                {{ partner.relatedUserId }}
              </dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Дата регистрации</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ formatDate(partner.registrationDate) }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Окончание партнёрства</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ formatDate(partner.partnershipEndDate) }}</dd>
            </div>
            <div>
              <dt class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Создан в системе</dt>
              <dd class="text-gray-900 dark:text-white font-medium">{{ formatDateTime(partner.createdOn) }}</dd>
            </div>
          </dl>
        </div>

        <!-- Recent bookings -->
        <div class="space-y-3">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Последние бронирования</h2>
          <div
            v-if="bookings.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-500 dark:text-gray-400 text-sm"
          >
            Бронирований нет.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Автомобиль</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Период</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Сумма</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="b in recentBookings"
                  :key="b.id"
                  @click="router.push(`/bookings/${b.id}`)"
                  class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ b.id }}</td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">{{ b.carBrand }} {{ b.carModel }}</td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs">
                    <p>{{ formatDateTime(b.startTime) }}</p>
                    <p class="text-gray-400 dark:text-gray-500">{{ formatDateTime(b.endTime) }}</p>
                  </td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-semibold whitespace-nowrap">
                    {{ b.totalPrice ? formatPrice(b.totalPrice) : "—" }}
                  </td>
                  <td class="px-5 py-3">
                    <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', bookingStatusBadge(b.status)]">
                      {{ bookingStatusLabel(b.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
            <div v-if="bookings.length > 5" class="px-5 py-3 border-t border-gray-100 dark:border-gray-800/60">
              <button
                @click="activeTab = 'bookings'"
                class="text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:underline"
              >
                Показать все {{ bookings.length }} бронирований →
              </button>
            </div>
          </div>
        </div>

        <!-- Recent cars -->
        <div class="space-y-3">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">Последние машины</h2>
          <div
            v-if="cars.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-500 dark:text-gray-400 text-sm"
          >
            Машин нет.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Марка / Модель</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Гос. номер</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена/час</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="car in recentCars"
                  :key="car.id"
                  @click="router.push(`/cars/${car.id}`)"
                  class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ car.id }}</td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">{{ car.modelBrand }} {{ car.modelName }}</td>
                  <td class="px-5 py-3 font-mono text-gray-700 dark:text-gray-300">{{ car.licensePlate }}</td>
                  <td class="px-5 py-3">
                    <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', carStatusBadge(car.status)]">
                      {{ carStatusLabel(car.status) }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 whitespace-nowrap">
                    {{ car.priceHour ? formatPrice(car.priceHour) + "/ч" : "—" }}
                  </td>
                </tr>
              </tbody>
            </table>
            <div v-if="cars.length > 5" class="px-5 py-3 border-t border-gray-100 dark:border-gray-800/60">
              <button
                @click="activeTab = 'cars'"
                class="text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:underline"
              >
                Показать все {{ cars.length }} машин →
              </button>
            </div>
          </div>
        </div>
      </template>

      <!-- ─── Tab: Машины ────────────────────────────────────── -->
      <template v-else-if="activeTab === 'cars'">
        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <h2 class="text-base font-bold text-gray-900 dark:text-white">
              Машины партнёра
              <span class="ml-2 px-2.5 py-0.5 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold">
                {{ carsTotal }}
              </span>
            </h2>
          </div>
          <div
            v-if="carsLoading"
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
          >
            Загрузка...
          </div>
          <div
            v-else-if="cars.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
          >
            У партнёра нет зарегистрированных машин.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Марка / Модель</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Гос. номер</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Цена/час</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Рейтинг</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="car in cars"
                  :key="car.id"
                  @click="router.push(`/cars/${car.id}`)"
                  class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ car.id }}</td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">
                    <EntityLink :to="`/cars/${car.id}`">{{ car.modelBrand }} {{ car.modelName }}</EntityLink>
                  </td>
                  <td class="px-5 py-3 font-mono text-gray-700 dark:text-gray-300">{{ car.licensePlate }}</td>
                  <td class="px-5 py-3">
                    <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', carStatusBadge(car.status)]">
                      {{ carStatusLabel(car.status) }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 whitespace-nowrap">
                    {{ car.priceHour ? formatPrice(car.priceHour) + "/ч" : "—" }}
                  </td>
                  <td class="px-5 py-3">
                    <span v-if="car.rating" class="text-amber-600 dark:text-amber-400 font-semibold">
                      {{ car.rating.toFixed(1) }}
                    </span>
                    <span v-else class="text-gray-400 dark:text-gray-600">—</span>
                    <span v-if="car.ratingsCount" class="text-xs text-gray-400 dark:text-gray-500 ml-1">
                      ({{ car.ratingsCount }})
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </template>

      <!-- ─── Tab: Бронирования ──────────────────────────────── -->
      <template v-else-if="activeTab === 'bookings'">
        <div class="space-y-3">
          <div class="flex items-center justify-between">
            <h2 class="text-base font-bold text-gray-900 dark:text-white">
              Бронирования
              <span class="ml-2 px-2.5 py-0.5 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold">
                {{ bookingsTotal }}
              </span>
            </h2>
          </div>
          <div
            v-if="bookingsLoading"
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
          >
            Загрузка...
          </div>
          <div
            v-else-if="bookings.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
          >
            Бронирований нет.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Автомобиль</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Период</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Сумма</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="b in bookings"
                  :key="b.id"
                  @click="router.push(`/bookings/${b.id}`)"
                  class="border-b border-gray-100 dark:border-gray-800/60 hover:bg-gray-50 dark:hover:bg-gray-800/40 transition-colors cursor-pointer"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ b.id }}</td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-medium">
                    <EntityLink :to="`/bookings/${b.id}`">{{ b.carBrand }} {{ b.carModel }}</EntityLink>
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs">
                    <p>{{ formatDateTime(b.startTime) }}</p>
                    <p class="text-gray-400 dark:text-gray-500">{{ formatDateTime(b.endTime) }}</p>
                  </td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-semibold whitespace-nowrap">
                    {{ b.totalPrice ? formatPrice(b.totalPrice) : "—" }}
                  </td>
                  <td class="px-5 py-3">
                    <span :class="['px-2.5 py-0.5 rounded-full text-xs font-semibold', bookingStatusBadge(b.status)]">
                      {{ bookingStatusLabel(b.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </template>

      <!-- ─── Tab: Финансы ───────────────────────────────────── -->
      <template v-else-if="activeTab === 'finance'">
        <!-- Wallet card -->
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Кошелёк</h2>
          <div
            v-if="walletLoading"
            class="text-gray-500 dark:text-gray-400 font-medium"
          >
            Загрузка...
          </div>
          <div v-else-if="wallet" class="flex items-center gap-8">
            <div>
              <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Баланс</p>
              <p class="text-3xl font-extrabold text-emerald-600 dark:text-emerald-400">
                {{ formatPrice(wallet.balance) }}
              </p>
            </div>
            <div>
              <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Валюта</p>
              <p class="text-xl font-bold text-gray-900 dark:text-white">{{ wallet.currency }}</p>
            </div>
            <div>
              <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">User ID</p>
              <p class="font-mono text-sm text-gray-500 dark:text-gray-400 truncate max-w-xs" :title="wallet.partnerUserId">
                {{ wallet.partnerUserId }}
              </p>
            </div>
          </div>
          <div v-else class="text-gray-500 dark:text-gray-400">
            Данные кошелька недоступны.
          </div>
        </div>

        <!-- Payouts -->
        <div class="space-y-3">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">
            Выплаты
            <span class="ml-2 px-2.5 py-0.5 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold">
              {{ payouts.length }}
            </span>
          </h2>
          <div
            v-if="financeLoading"
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
          >
            Загрузка...
          </div>
          <div
            v-else-if="payouts.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-500 dark:text-gray-400 text-sm"
          >
            Выплат нет.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Сумма</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Статус</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Причина</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Создано</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Обновлено</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="payout in payouts"
                  :key="payout.id"
                  class="border-b border-gray-100 dark:border-gray-800/60"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ payout.id }}</td>
                  <td class="px-5 py-3 text-gray-900 dark:text-white font-semibold whitespace-nowrap">
                    {{ formatPrice(payout.amount) }}
                  </td>
                  <td class="px-5 py-3">
                    <span
                      :class="[
                        'px-2.5 py-0.5 rounded-full text-xs font-semibold',
                        payoutStatusMap[payout.status]?.css ?? 'bg-gray-100 text-gray-500',
                      ]"
                    >
                      {{ payoutStatusMap[payout.status]?.label ?? payout.status }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs max-w-[180px] truncate" :title="payout.reason">
                    {{ payout.reason || "—" }}
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                    {{ formatDateTime(payout.createdAt) }}
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                    {{ payout.updatedAt ? formatDateTime(payout.updatedAt) : "—" }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Ledger -->
        <div class="space-y-3">
          <h2 class="text-base font-bold text-gray-900 dark:text-white">
            Транзакции (леджер)
            <span class="ml-2 px-2.5 py-0.5 rounded-full bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 text-xs font-semibold">
              {{ ledger.length }}
            </span>
          </h2>
          <div
            v-if="financeLoading"
            class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
          >
            Загрузка...
          </div>
          <div
            v-else-if="ledger.length === 0"
            class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-8 text-center text-gray-500 dark:text-gray-400 text-sm"
          >
            Транзакций нет.
          </div>
          <div v-else class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-gray-200 dark:border-gray-800">
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">ID</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Тип</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Сумма</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Описание</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Ссылка</th>
                  <th class="text-left px-5 py-3 text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400">Дата</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="entry in ledger"
                  :key="entry.id"
                  class="border-b border-gray-100 dark:border-gray-800/60"
                >
                  <td class="px-5 py-3 font-mono text-xs text-gray-500 dark:text-gray-400">{{ entry.id }}</td>
                  <td class="px-5 py-3">
                    <span
                      :class="[
                        'px-2.5 py-0.5 rounded-full text-xs font-semibold',
                        ledgerTypeBadge(entry.type),
                      ]"
                    >
                      {{ entry.type }}
                    </span>
                  </td>
                  <td
                    :class="[
                      'px-5 py-3 font-semibold whitespace-nowrap',
                      entry.amount >= 0
                        ? 'text-emerald-600 dark:text-emerald-400'
                        : 'text-red-600 dark:text-red-400',
                    ]"
                  >
                    {{ entry.amount >= 0 ? "+" : "" }}{{ formatPrice(entry.amount) }}
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs max-w-[200px] truncate" :title="entry.description">
                    {{ entry.description || "—" }}
                  </td>
                  <td class="px-5 py-3 font-mono text-xs text-gray-400 dark:text-gray-500 max-w-[120px] truncate" :title="entry.referenceId">
                    {{ entry.referenceId || "—" }}
                  </td>
                  <td class="px-5 py-3 text-gray-600 dark:text-gray-400 text-xs whitespace-nowrap">
                    {{ formatDateTime(entry.createdAt) }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </template>

      <!-- ─── Tab: Документы ─────────────────────────────────── -->
      <template v-else-if="activeTab === 'documents'">
        <div class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-6">
          <h2 class="text-lg font-bold text-gray-900 dark:text-white">Документы партнёра</h2>

          <!-- Contract -->
          <div class="flex items-start gap-4 p-5 rounded-2xl border border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/60">
            <div class="mt-0.5 flex-shrink-0 h-10 w-10 rounded-xl bg-emerald-100 dark:bg-emerald-500/20 flex items-center justify-center">
              <svg class="w-5 h-5 text-emerald-600 dark:text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Договор</p>
              <p v-if="partner.contractFileName" class="text-gray-900 dark:text-white font-medium truncate" :title="partner.contractFileName">
                {{ partner.contractFileName }}
              </p>
              <p v-else class="text-gray-400 dark:text-gray-600 italic text-sm">Файл не загружен</p>
            </div>
          </div>

          <!-- Identity document -->
          <div class="flex items-start gap-4 p-5 rounded-2xl border border-gray-100 dark:border-gray-800 bg-gray-50 dark:bg-gray-900/60">
            <div class="mt-0.5 flex-shrink-0 h-10 w-10 rounded-xl bg-violet-100 dark:bg-violet-500/20 flex items-center justify-center">
              <svg class="w-5 h-5 text-violet-600 dark:text-violet-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10 6H5a2 2 0 00-2 2v9a2 2 0 002 2h14a2 2 0 002-2V8a2 2 0 00-2-2h-5m-4 0V5a2 2 0 114 0v1m-4 0a2 2 0 104 0m-5 8a2 2 0 100-4 2 2 0 000 4zm0 0c1.306 0 2.417.835 2.83 2M9 14a3.001 3.001 0 00-2.83 2M15 11h3m-3 4h2" />
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold uppercase tracking-wider text-gray-500 dark:text-gray-400 mb-1">Удостоверение личности</p>
              <p v-if="partner.ownerIdentityFileName" class="text-gray-900 dark:text-white font-medium truncate" :title="partner.ownerIdentityFileName">
                {{ partner.ownerIdentityFileName }}
              </p>
              <p v-else class="text-gray-400 dark:text-gray-600 italic text-sm">Файл не загружен</p>
            </div>
          </div>

          <p class="text-xs text-gray-400 dark:text-gray-600 italic">
            Скачивание файлов доступно только самому партнёру через мобильное приложение.
          </p>
        </div>
      </template>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getPartner,
  getPartnerWallet,
  getPartnerLedger,
  getPartnerPayouts,
  type PartnerDto,
  type PartnerWalletDto,
  type LedgerEntryDto,
  type PayoutDto,
} from "../api/partners";
import { getPartnerCars, type PartnerCarDto } from "../api/cars";
import { getAllBookings, type BookingDto } from "../api/bookings";
import { formatDate, formatDateTime, formatPrice } from "../utils/formatters";
import {
  carStatusLabel,
  carStatusBadge,
  bookingStatusLabel,
  bookingStatusBadge,
  payoutStatusMap,
} from "../utils/statusMaps";
import { useToast } from "../composables/useToast";
import EntityLink from "../components/EntityLink.vue";

const route = useRoute();
const router = useRouter();
const toast = useToast();

// ── State ──────────────────────────────────────────────────────────────
const loading = ref(false);
const carsLoading = ref(false);
const bookingsLoading = ref(false);
const walletLoading = ref(false);
const financeLoading = ref(false);

const partner = ref<PartnerDto | null>(null);
const wallet = ref<PartnerWalletDto | null>(null);
const cars = ref<PartnerCarDto[]>([]);
const carsTotal = ref(0);
const bookings = ref<BookingDto[]>([]);
const bookingsTotal = ref(0);
const payouts = ref<PayoutDto[]>([]);
const ledger = ref<LedgerEntryDto[]>([]);

const activeTab = ref<"overview" | "cars" | "bookings" | "finance" | "documents">("overview");

const financeLoaded = ref(false);

// ── Tabs ───────────────────────────────────────────────────────────────
const tabs = [
  { key: "overview" as const, label: "Обзор" },
  { key: "cars" as const, label: "Машины" },
  { key: "bookings" as const, label: "Бронирования" },
  { key: "finance" as const, label: "Финансы" },
  { key: "documents" as const, label: "Документы" },
];

// ── Computed ───────────────────────────────────────────────────────────
const partnerInitials = computed(() => {
  if (!partner.value) return "";
  return (
    (partner.value.ownerFirstName?.[0] ?? "") +
    (partner.value.ownerLastName?.[0] ?? "")
  ).toUpperCase();
});

const recentBookings = computed(() => bookings.value.slice(0, 5));
const recentCars = computed(() => cars.value.slice(0, 5));

// ── Helpers ────────────────────────────────────────────────────────────
function ledgerTypeBadge(type: string): string {
  const map: Record<string, string> = {
    Credit: "bg-emerald-100 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-300",
    Debit: "bg-red-100 text-red-700 dark:bg-red-500/20 dark:text-red-300",
    Refund: "bg-blue-100 text-blue-700 dark:bg-blue-500/20 dark:text-blue-300",
    Fee: "bg-orange-100 text-orange-700 dark:bg-orange-500/20 dark:text-orange-300",
  };
  return map[type] ?? "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400";
}

// ── Data loaders ───────────────────────────────────────────────────────
async function loadCars(partnerUserId: string) {
  carsLoading.value = true;
  try {
    const result = await getPartnerCars({ partnerUserId, page: 1, pageSize: 100 });
    cars.value = result.items;
    carsTotal.value = result.totalCount;
  } catch {
    toast.error("Ошибка загрузки машин партнёра");
  } finally {
    carsLoading.value = false;
  }
}

async function loadBookings(partnerUserId: string) {
  bookingsLoading.value = true;
  try {
    const result = await getAllBookings({ partnerUserId, page: 1, pageSize: 100 });
    bookings.value = result.items;
    bookingsTotal.value = result.totalCount;
  } catch {
    toast.error("Ошибка загрузки бронирований партнёра");
  } finally {
    bookingsLoading.value = false;
  }
}

async function loadWallet(partnerId: number) {
  walletLoading.value = true;
  try {
    wallet.value = await getPartnerWallet(partnerId);
  } catch {
    // wallet may return 404 if not created yet — don't toast
  } finally {
    walletLoading.value = false;
  }
}

async function loadFinance(partnerId: number) {
  if (financeLoaded.value) return;
  financeLoading.value = true;
  try {
    const [payoutsData, ledgerData] = await Promise.all([
      getPartnerPayouts(partnerId),
      getPartnerLedger(partnerId),
    ]);
    payouts.value = payoutsData;
    ledger.value = ledgerData;
    financeLoaded.value = true;
  } catch {
    toast.error("Ошибка загрузки финансовых данных");
  } finally {
    financeLoading.value = false;
  }
}

async function loadAll() {
  const rawId = route.params.id;
  const partnerId = Number(Array.isArray(rawId) ? rawId[0] : rawId);
  if (!partnerId) return;

  loading.value = true;
  try {
    partner.value = await getPartner(partnerId);
  } catch {
    toast.error("Ошибка загрузки партнёра");
    loading.value = false;
    return;
  }
  loading.value = false;

  const p = partner.value!;
  await Promise.all([
    loadWallet(partnerId),
    loadCars(p.relatedUserId),
    loadBookings(p.relatedUserId),
  ]);
}

// ── Lazy finance load on tab switch ───────────────────────────────────
watch(activeTab, (tab) => {
  if (tab === "finance" && partner.value) {
    loadFinance(partner.value.id);
  }
});

onMounted(loadAll);
</script>
