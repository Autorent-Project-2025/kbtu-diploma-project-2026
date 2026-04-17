<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">
    <!-- Header -->
    <div class="bg-white dark:bg-gray-900 border-b border-gray-200 dark:border-gray-800 rounded-2xl">
      <div class="max-w-7xl mx-auto px-6 py-6">
        <div class="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-6">
          <div class="space-y-1">
            <p class="text-xs font-bold uppercase tracking-[0.3em] text-emerald-600 dark:text-emerald-400">
              Operations CRM
            </p>
            <h1 class="text-2xl font-extrabold text-gray-900 dark:text-white">
              Рабочая очередь
            </h1>
            <p class="text-sm text-gray-500 dark:text-gray-400">
              Проверяйте новые регистрации, открывайте документы и принимайте решение по каждой заявке.
            </p>
          </div>

          <!-- Stats strip -->
          <div class="flex flex-wrap gap-3 items-center">
            <div
              class="flex rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow overflow-hidden"
            >
              <div
                v-for="(stat, i) in statsStrip"
                :key="stat.label"
                :class="[
                  'px-5 py-3 text-center',
                  i > 0 ? 'border-l border-gray-200 dark:border-gray-800' : '',
                ]"
              >
                <p class="text-2xl font-extrabold text-gray-900 dark:text-white">
                  {{ stat.value }}
                </p>
                <p class="text-xs text-gray-500 dark:text-gray-400 font-semibold uppercase tracking-wider mt-0.5">
                  {{ stat.label }}
                </p>
              </div>
            </div>
            <button
              @click="loadPending"
              :disabled="loading"
              class="px-5 py-3 rounded-2xl border border-gray-300 dark:border-gray-700 text-gray-800 dark:text-gray-100 font-semibold hover:border-emerald-500 transition-colors disabled:opacity-60"
            >
              Обновить
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Уведомления через toast-систему -->

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-600 dark:text-gray-400 font-medium"
    >
      Загрузка заявок...
    </div>

    <!-- Empty -->
    <div
      v-else-if="tickets.length === 0"
      class="rounded-3xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Сейчас нет заявок на рассмотрении.
    </div>

    <!-- Main review layout -->
    <div v-else class="grid xl:grid-cols-[340px,1fr] gap-6 items-start">
      <!-- Queue sidebar -->
      <div
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl overflow-hidden"
      >
        <div class="px-5 py-4 border-b border-gray-100 dark:border-gray-800">
          <div class="flex items-center justify-between">
            <h2 class="text-lg font-bold text-gray-900 dark:text-white">
              Очередь
            </h2>
            <span
              class="text-xs font-bold bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 px-2.5 py-1 rounded-full"
              >{{ tickets.length }}</span
            >
          </div>
          <p
            v-if="lastUpdatedAt"
            class="text-xs text-gray-400 dark:text-gray-500 mt-1"
          >
            Обновлено {{ formatDateTime(lastUpdatedAt) }}
          </p>
        </div>

        <ul
          class="divide-y divide-gray-100 dark:divide-gray-800 max-h-[70vh] overflow-y-auto"
        >
          <li v-for="ticket in tickets" :key="ticket.id">
            <button
              @click="selectTicket(ticket.id)"
              :class="[
                'w-full px-5 py-4 text-left transition-colors',
                selectedTicketId === ticket.id
                  ? 'bg-emerald-50 dark:bg-emerald-900/20 border-l-4 border-emerald-500'
                  : 'hover:bg-gray-50 dark:hover:bg-gray-800/60 border-l-4 border-transparent',
              ]"
            >
              <div class="flex items-start justify-between gap-3">
                <div class="flex items-center gap-3 min-w-0">
                  <div
                    class="w-9 h-9 flex-shrink-0 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 flex items-center justify-center text-xs font-bold"
                  >
                    {{ userInitials(ticket.fullName) }}
                  </div>
                  <div class="min-w-0">
                    <p
                      class="font-bold text-gray-900 dark:text-white text-sm truncate"
                    >
                      {{ ticket.fullName }}
                    </p>
                    <p
                      class="text-xs text-gray-500 dark:text-gray-400 truncate"
                    >
                      {{ ticket.email }}
                    </p>
                  </div>
                </div>
                <span
                  :class="getTicketTypeBadgeClass(ticket.ticketType)"
                  class="inline-flex px-2 py-0.5 rounded-full text-xs font-bold uppercase tracking-wide flex-shrink-0"
                >
                  {{ ticketTypeLabel(ticket.ticketType) }}
                </span>
              </div>
              <div
                class="flex justify-between mt-2 pl-12 text-xs text-gray-400 dark:text-gray-500"
              >
                <span>{{ ticket.phoneNumber }}</span>
                <span>{{ formatDateTime(ticket.createdAt) }}</span>
              </div>
            </button>
          </li>
        </ul>
      </div>

      <!-- Detail panel -->
      <div
        v-if="selectedTicket"
        class="rounded-3xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6 space-y-6"
      >
        <!-- Header -->
        <div
          class="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4 pb-6 border-b border-gray-100 dark:border-gray-800"
        >
          <div class="flex items-start gap-4">
            <div
              class="w-12 h-12 flex-shrink-0 rounded-2xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-700 dark:text-emerald-400 flex items-center justify-center text-base font-extrabold"
            >
              {{ userInitials(selectedTicket.fullName) }}
            </div>
            <div>
              <h2 class="text-2xl font-extrabold text-gray-900 dark:text-white">
                {{ selectedTicket.fullName }}
              </h2>
              <p class="text-gray-500 dark:text-gray-400 mt-1">
                {{ ticketTypeLabel(selectedTicket.ticketType) }} ·
                {{ selectedTicket.email }}
              </p>
            </div>
          </div>
          <div
            class="text-sm text-gray-500 dark:text-gray-400 space-y-1 text-right"
          >
            <p>
              Статус:
              <span class="font-semibold text-gray-700 dark:text-gray-300">{{
                statusLabel(selectedTicket.status)
              }}</span>
            </p>
            <p>Создана: {{ formatDateTime(selectedTicket.createdAt) }}</p>
          </div>
        </div>

        <div class="grid xl:grid-cols-[1fr,300px] gap-6 items-start">
          <!-- Left: data + docs -->
          <div class="space-y-6">
            <!-- Basic fields -->
            <section
              class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
            >
              <h3
                class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
              >
                Основные данные
              </h3>
              <dl
                class="grid sm:grid-cols-2 gap-x-6 gap-y-0 divide-y divide-gray-100 dark:divide-gray-800"
              >
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    ID заявки
                  </dt>
                  <dd
                    class="font-mono text-xs font-semibold text-gray-900 dark:text-white break-all"
                  >
                    {{ selectedTicket.id }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Тип
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ ticketTypeLabel(selectedTicket.ticketType) }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Email
                  </dt>
                  <dd
                    class="font-semibold text-gray-900 dark:text-white break-all"
                  >
                    {{ selectedTicket.email }}
                  </dd>
                </div>
                <div class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Телефон
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ selectedTicket.phoneNumber }}
                  </dd>
                </div>
                <div v-if="isClientTicket(selectedTicket)" class="py-3">
                  <dt class="text-xs text-gray-500 dark:text-gray-400 mb-1">
                    Дата рождения
                  </dt>
                  <dd class="font-semibold text-gray-900 dark:text-white">
                    {{ selectedTicket.birthDate || "Не указана" }}
                  </dd>
                </div>
              </dl>
            </section>

            <!-- Partner car form -->
            <section
              v-if="isPartnerCarTicket(selectedTicket)"
              class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
            >
              <div>
                <h3
                  class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Данные автомобиля
                </h3>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  При необходимости скорректируйте характеристики перед принятием решения.
                </p>
              </div>

              <div
                class="rounded-2xl border border-violet-100 dark:border-violet-900/40 bg-violet-50/70 dark:bg-violet-500/10 p-4"
              >
                <p class="text-xs font-bold uppercase tracking-[0.14em] text-violet-700 dark:text-violet-300">
                  {{ partnerCarRequestKindLabel(resolvePartnerCarRequestKind(selectedTicket)) }}
                </p>
                <p class="mt-2 text-sm text-gray-700 dark:text-gray-200">
                  <template v-if="selectedTicket.partnerCarId">
                    Изменения будут применены к машине #{{ selectedTicket.partnerCarId }} после одобрения.
                  </template>
                  <template v-else>
                    После одобрения будет создана новая машина партнера.
                  </template>
                </p>
              </div>

              <div class="grid sm:grid-cols-2 gap-4">
                <div
                  v-for="field in carFormFields"
                  :key="field.id"
                  class="space-y-1.5"
                >
                  <label
                    :for="field.id"
                    class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                    >{{ field.label }}</label
                  >
                  <input
                    :id="field.id"
                    v-model="
                      partnerCarForm[field.key as keyof typeof partnerCarForm]
                    "
                    :type="field.type || 'text'"
                    :min="field.min"
                    :max="field.max"
                    :step="field.step"
                    class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
                  />
                </div>
              </div>

              <div class="grid sm:grid-cols-2 gap-4">
                <div class="space-y-1.5">
                  <label
                    for="requestedStatus"
                    class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                  >
                    Статус машины
                  </label>
                  <select
                    id="requestedStatus"
                    v-model.number="partnerCarForm.requestedStatus"
                    class="w-full px-4 py-2.5 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors"
                  >
                    <option
                      v-for="option in partnerCarStatusOptions"
                      :key="option.value"
                      :value="option.value"
                    >
                      {{ option.label }}
                    </option>
                  </select>
                </div>

                <div class="rounded-2xl border border-gray-100 dark:border-gray-800 px-4 py-3 flex items-center justify-between gap-4">
                  <div>
                    <p class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
                      Активность
                    </p>
                    <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                      Если машина будет деактивирована, связанные бронирования отменятся.
                    </p>
                  </div>
                  <label class="inline-flex items-center gap-3 text-sm font-semibold text-gray-900 dark:text-white">
                    <input
                      v-model="partnerCarForm.isActive"
                      type="checkbox"
                      class="h-4 w-4 rounded border-gray-300 text-emerald-600 focus:ring-emerald-500"
                    />
                    <span>{{ partnerCarForm.isActive ? "Активна" : "Неактивна" }}</span>
                  </label>
                </div>
              </div>
            </section>

            <section
              v-if="isPartnerBookingCancellationTicket(selectedTicket)"
              class="rounded-2xl border border-rose-100 dark:border-rose-900/40 p-5 space-y-4"
            >
              <div>
                <h3
                  class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Запрос на отмену бронирования
                </h3>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  Одобрение этого тикета отправит команду на отмену брони в booking-service.
                </p>
              </div>

              <dl class="grid sm:grid-cols-2 gap-4">
                <div class="rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
                  <dt class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
                    Бронирование
                  </dt>
                  <dd class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
                    #{{ selectedTicket.bookingId }}
                  </dd>
                  <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                    {{ selectedTicket.carBrand }} {{ selectedTicket.carModel }}
                  </p>
                </div>

                <div class="rounded-2xl border border-gray-100 dark:border-gray-800 p-4">
                  <dt class="text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400">
                    Статус на момент запроса
                  </dt>
                  <dd class="mt-2 text-lg font-bold text-gray-900 dark:text-white">
                    {{ partnerBookingStatusLabel(partnerBookingCancellationData?.bookingStatus) }}
                  </dd>
                  <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
                    {{ formatDateTime(partnerBookingCancellationData?.bookingStartTime || "") }}
                    -
                    {{ formatDateTime(partnerBookingCancellationData?.bookingEndTime || "") }}
                  </p>
                </div>
              </dl>

              <div class="rounded-2xl border border-rose-200 dark:border-rose-500/30 bg-rose-50/70 dark:bg-rose-500/10 p-4">
                <p class="text-xs font-bold uppercase tracking-[0.14em] text-rose-700 dark:text-rose-300">
                  Причина партнёра
                </p>
                <p class="mt-3 text-sm leading-6 text-gray-700 dark:text-gray-200 whitespace-pre-line">
                  {{ partnerBookingCancellationData?.partnerReason }}
                </p>
              </div>
            </section>

            <!-- Documents -->
            <section
              class="rounded-2xl border border-gray-100 dark:border-gray-800 p-5 space-y-4"
            >
              <h3
                class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
              >
                Документы
              </h3>

              <ul
                v-if="hasSelectedDocuments"
                class="divide-y divide-gray-100 dark:divide-gray-800"
              >
                <li
                  v-if="selectedTicket.identityDocumentFileName"
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      {{
                        isPartnerTicket(selectedTicket)
                          ? "Документ владельца"
                          : "Документ личности"
                      }}
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.identityDocumentFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('identity')"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
                <li
                  v-if="
                    isClientTicket(selectedTicket) &&
                    selectedTicket.driverLicenseFileName
                  "
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      Водительские права
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.driverLicenseFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('license')"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
                <li
                  v-if="
                    isPartnerCarTicket(selectedTicket) &&
                    selectedTicket.ownershipDocumentFileName
                  "
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      Документ собственности
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ selectedTicket.ownershipDocumentFileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument('ownership')"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
                <li
                  v-for="photo in completionTicketPhotos"
                  :key="photo.slot"
                  class="flex items-center justify-between gap-4 py-3"
                >
                  <div>
                    <p
                      class="font-semibold text-sm text-gray-900 dark:text-white"
                    >
                      Фото {{ completionPhotoLabel(photo.slot) }}
                    </p>
                    <p class="text-xs text-gray-400 dark:text-gray-500">
                      {{ photo.fileName }}
                    </p>
                  </div>
                  <button
                    @click="openDocument(photo.slot)"
                    :disabled="actionLoading"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors disabled:opacity-60"
                  >
                    Открыть
                  </button>
                </li>
              </ul>
              <p v-else class="text-sm text-gray-400 dark:text-gray-500">
                К заявке не прикреплены документы.
              </p>

              <div
                v-if="
                  isPartnerCarTicket(selectedTicket) &&
                  partnerCarImages.length > 0
                "
                class="pt-4 border-t border-gray-100 dark:border-gray-800 space-y-3"
              >
                <h4
                  class="text-xs font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Фотографии авто
                </h4>
                <div class="flex flex-wrap gap-2">
                  <button
                    v-for="(image, index) in partnerCarImages"
                    :key="`${image.imageId}-${index}`"
                    @click="openImage(image.imageUrl)"
                    class="px-4 py-2 rounded-xl border border-gray-300 dark:border-gray-700 text-sm font-semibold text-gray-700 dark:text-gray-300 hover:border-emerald-500 transition-colors"
                  >
                    {{ partnerCarImageTypeLabel(image.imageType, index) }}
                  </button>
                </div>
              </div>
            </section>
          </div>

          <!-- Right: summary + decision -->
          <div class="space-y-4">
            <!-- Summary card -->
            <div
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-0 divide-y divide-gray-200 dark:divide-gray-800"
            >
              <h3
                class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400 pb-3"
              >
                Сводка
              </h3>
              <div
                v-for="row in summaryRows"
                :key="row.label"
                class="flex justify-between items-center py-3"
              >
                <dt class="text-sm text-gray-500 dark:text-gray-400">
                  {{ row.label }}
                </dt>
                <dd class="text-sm font-bold text-gray-900 dark:text-white">
                  {{ row.value }}
                </dd>
              </div>
            </div>

            <!-- Decision card -->
            <div
              class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-950 p-5 space-y-4"
            >
              <div>
                <h3
                  class="text-sm font-bold uppercase tracking-[0.2em] text-gray-500 dark:text-gray-400"
                >
                  Решение
                </h3>
                <p class="text-xs text-gray-400 dark:text-gray-500 mt-1">
                  Причину нужно указать только для отказа. Для завершения
                  поездки вынесите решение в отдельном блоке: либо одобрение,
                  либо штраф с комментарием.
                </p>
              </div>

              <div
                v-if="!isBookingCompletionTicket(selectedTicket)"
                class="space-y-1.5"
              >
                <label
                  for="rejectReason"
                  class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                  >Причина отказа</label
                >
                <textarea
                  id="rejectReason"
                  v-model="rejectReason"
                  placeholder="Укажите причину, если заявка отклоняется"
                  class="w-full px-4 py-3 rounded-xl border border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm min-h-[100px] resize-y focus:outline-none focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/20 transition-colors placeholder-gray-400"
                />
              </div>

              <div v-else class="space-y-4">
                <div class="rounded-2xl border border-emerald-200 dark:border-emerald-500/30 bg-emerald-50/70 dark:bg-emerald-500/10 p-4 space-y-3">
                  <div>
                    <p class="text-xs font-bold uppercase tracking-[0.14em] text-emerald-700 dark:text-emerald-300">
                      Одобрение без штрафа
                    </p>
                    <p class="text-xs text-emerald-700/80 dark:text-emerald-200/80 mt-1">
                      Кнопка активна только когда блок штрафа пустой.
                    </p>
                  </div>
                  <button
                    @click="approveSelected"
                    :disabled="actionLoading || !canApproveSelected"
                    class="w-full px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
                  >
                    {{ actionLoading ? "Обработка..." : "✓ Одобрить завершение" }}
                  </button>
                </div>

                <div class="rounded-2xl border border-red-200 dark:border-red-500/30 bg-red-50/70 dark:bg-red-500/10 p-4 space-y-4">
                  <div>
                    <p class="text-xs font-bold uppercase tracking-[0.14em] text-red-700 dark:text-red-300">
                      Выставление штрафа
                    </p>
                    <p class="text-xs text-red-700/80 dark:text-red-200/80 mt-1">
                      Укажите сумму и обязательно добавьте комментарий, чтобы клиент видел причину начисления.
                    </p>
                  </div>

                  <div class="space-y-1.5">
                    <label
                      for="fineAmount"
                      class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                      >Сумма штрафа</label
                    >
                    <input
                      id="fineAmount"
                      v-model="fineAmount"
                      type="number"
                      min="0.01"
                      step="0.01"
                      placeholder="Например 15000"
                      class="w-full px-4 py-3 rounded-xl border border-red-200 dark:border-red-500/30 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-500/20 transition-colors placeholder-gray-400"
                    />
                  </div>

                  <div class="space-y-1.5">
                    <label
                      for="fineComment"
                      class="block text-xs font-bold uppercase tracking-[0.1em] text-gray-500 dark:text-gray-400"
                      >Комментарий к штрафу</label
                    >
                    <textarea
                      id="fineComment"
                      v-model="fineComment"
                      placeholder="Опишите повреждение, недостающие элементы или иную причину начисления"
                      class="w-full px-4 py-3 rounded-xl border border-red-200 dark:border-red-500/30 bg-white dark:bg-gray-800 text-gray-900 dark:text-white text-sm min-h-[110px] resize-y focus:outline-none focus:border-red-500 focus:ring-2 focus:ring-red-500/20 transition-colors placeholder-gray-400"
                    />
                  </div>

                  <button
                    @click="issueFineSelected"
                    :disabled="actionLoading"
                    class="w-full px-5 py-3 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 disabled:cursor-not-allowed font-bold transition-colors"
                  >
                    {{ actionLoading ? "Обработка..." : "Выставить штраф" }}
                  </button>
                </div>
              </div>

              <div v-if="!isBookingCompletionTicket(selectedTicket)" class="flex flex-col gap-3">
                <button
                  @click="approveSelected"
                  :disabled="actionLoading"
                  class="w-full px-5 py-3 rounded-2xl bg-emerald-600 hover:bg-emerald-700 disabled:opacity-60 disabled:cursor-not-allowed text-white font-bold shadow-lg shadow-emerald-500/20 transition-colors"
                >
                  {{
                    actionLoading
                      ? "Обработка..."
                      : isPartnerBookingCancellationTicket(selectedTicket)
                        ? "✓ Одобрить отмену"
                        : "✓ Одобрить"
                  }}
                </button>
                <button
                  @click="rejectSelected"
                  :disabled="actionLoading"
                  class="w-full px-5 py-3 rounded-2xl border border-red-300 dark:border-red-700 text-red-700 dark:text-red-400 hover:bg-red-50 dark:hover:bg-red-900/20 disabled:opacity-60 disabled:cursor-not-allowed font-bold transition-colors"
                >
                  {{
                    actionLoading
                      ? "Обработка..."
                      : isPartnerBookingCancellationTicket(selectedTicket)
                        ? "✕ Отклонить запрос"
                        : "✕ Отклонить"
                  }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import {
  approveTicket,
  getPendingTickets,
  getTicketById,
  getTicketDocumentTemporaryLink,
  issueTicketFine,
  rejectTicket,
  type PartnerCarReviewPayload,
} from "../api/tickets";
import { useToast } from "../composables/useToast";
import type {
  BookingCompletionTicketData,
  BookingCompletionTicketPhotoData,
  PartnerBookingCancellationTicketData,
  PartnerCarTicketData,
  PartnerCarTicketImageData,
  Ticket,
} from "../types/Ticket";
import { formatDateTime, userInitials } from "../utils/formatters";

const tickets = ref<Ticket[]>([]);
const selectedTicket = ref<Ticket | null>(null);
const selectedTicketId = ref<string>("");
const rejectReason = ref("");
const fineAmount = ref("");
const fineComment = ref("");
const loading = ref(false);
const actionLoading = ref(false);
const toast = useToast();
const lastUpdatedAt = ref<string>("");
const maxAllowedCarYear = new Date().getUTCFullYear() + 1;

type PartnerCarFormField = {
  id: string;
  key: "carBrand" | "carModel" | "carYear" | "licensePlate" | "color";
  label: string;
  type?: string;
  min?: string;
  max?: string;
  step?: string;
};

const partnerCarForm = reactive({
  carBrand: "",
  carModel: "",
  carYear: null as number | null,
  licensePlate: "",
  color: "",
  requestedStatus: 0 as number | null,
  isActive: true,
});

const carFormFields: PartnerCarFormField[] = [
  { id: "carBrand", key: "carBrand", label: "Марка" },
  { id: "carModel", key: "carModel", label: "Модель" },
  {
    id: "carYear",
    key: "carYear",
    label: "Год",
    type: "number",
    min: "1886",
    max: String(maxAllowedCarYear),
  },
  { id: "licensePlate", key: "licensePlate", label: "Госномер" },
  { id: "color", key: "color", label: "Цвет" },
];

const partnerCarStatusOptions = [
  { value: 0, label: "Доступна" },
  { value: 1, label: "Забронирована" },
  { value: 2, label: "В поездке" },
  { value: 3, label: "На обслуживании" },
];

const partnerCarImages = computed<PartnerCarTicketImageData[]>(() => {
  if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value))
    return [];
  if (
    Array.isArray(selectedTicket.value.carImages) &&
    selectedTicket.value.carImages.length > 0
  )
    return selectedTicket.value.carImages;
  const data = selectedTicket.value.data;
  if (data && (data as PartnerCarTicketData).$type === "partner-car")
    return (data as PartnerCarTicketData).carImages ?? [];
  return [];
});

const completionTicketPhotos = computed<BookingCompletionTicketPhotoData[]>(
  () => {
    if (
      !selectedTicket.value ||
      !isBookingCompletionTicket(selectedTicket.value)
    ) {
      return [];
    }

    if (
      Array.isArray(selectedTicket.value.completionPhotos) &&
      selectedTicket.value.completionPhotos.length > 0
    ) {
      return selectedTicket.value.completionPhotos;
    }

    const data = selectedTicket.value.data;
    if (
      data &&
      (data as BookingCompletionTicketData).$type === "booking-completion"
    ) {
      return (data as BookingCompletionTicketData).completionPhotos ?? [];
    }

    return [];
  },
);

const partnerBookingCancellationData =
  computed<PartnerBookingCancellationTicketData | null>(() => {
    if (
      !selectedTicket.value ||
      !isPartnerBookingCancellationTicket(selectedTicket.value)
    ) {
      return null;
    }

    const data = selectedTicket.value.data;
    if (
      data &&
      (data as PartnerBookingCancellationTicketData).$type ===
        "partner-booking-cancellation"
    ) {
      return data as PartnerBookingCancellationTicketData;
    }

    return null;
  });

const ticketStats = computed(() => {
  let client = 0,
    partner = 0,
    partnerCar = 0,
    bookingCompletion = 0;
  for (const t of tickets.value) {
    if (t.ticketType === 2) partner++;
    else if (t.ticketType === 3) partnerCar++;
    else if (t.ticketType === 4) bookingCompletion++;
    else client++;
  }
  return { client, partner, partnerCar, bookingCompletion };
});

const statsStrip = computed(() => [
  { label: "В очереди", value: tickets.value.length },
  { label: "Клиенты", value: ticketStats.value.client },
  { label: "Партнёры", value: ticketStats.value.partner },
  { label: "Авто", value: ticketStats.value.partnerCar },
  { label: "Поездки", value: ticketStats.value.bookingCompletion },
]);

const hasSelectedDocuments = computed(() => {
  if (!selectedTicket.value) return false;
  return Boolean(
    selectedTicket.value.identityDocumentFileName ||
    (isClientTicket(selectedTicket.value) &&
      selectedTicket.value.driverLicenseFileName) ||
    (isPartnerCarTicket(selectedTicket.value) &&
      selectedTicket.value.ownershipDocumentFileName) ||
    completionTicketPhotos.value.length > 0,
  );
});

const selectedDocumentCount = computed(() => {
  if (!selectedTicket.value) return 0;
  let count = 0;
  if (selectedTicket.value.identityDocumentFileName) count++;
  if (
    isClientTicket(selectedTicket.value) &&
    selectedTicket.value.driverLicenseFileName
  )
    count++;
  if (
    isPartnerCarTicket(selectedTicket.value) &&
    selectedTicket.value.ownershipDocumentFileName
  )
    count++;
  count += completionTicketPhotos.value.length;
  return count;
});

const summaryRows = computed(() => {
  if (!selectedTicket.value) return [];
  const rows = [
    { label: "Статус", value: statusLabel(selectedTicket.value.status) },
    { label: "Тип", value: ticketTypeLabel(selectedTicket.value.ticketType) },
    { label: "Документы", value: String(selectedDocumentCount.value) },
  ];
  if (isPartnerCarTicket(selectedTicket.value))
    rows.push({
      label: "Фотографии",
      value: String(partnerCarImages.value.length),
    });
  if (isPartnerCarTicket(selectedTicket.value)) {
    rows.push({
      label: "Режим",
      value: partnerCarRequestKindLabel(
        selectedTicket.value.partnerCarRequestKind ??
          (selectedTicket.value.data as PartnerCarTicketData | undefined)
            ?.requestKind,
      ),
    });
    if (selectedTicket.value.partnerCarId) {
      rows.push({
        label: "Машина",
        value: `#${selectedTicket.value.partnerCarId}`,
      });
    }
  }
  if (isBookingCompletionTicket(selectedTicket.value)) {
    rows.push({
      label: "Фото после поездки",
      value: String(completionTicketPhotos.value.length),
    });
    rows.push({
      label: "Пеня за просрочку",
      value: selectedTicket.value.latePenaltyAmount
        ? `${selectedTicket.value.latePenaltyAmount.toFixed(2)} KZT`
        : "Нет",
    });
    rows.push({
      label: "Штраф за повреждение",
      value: selectedTicket.value.damageFineAmount
        ? `${selectedTicket.value.damageFineAmount.toFixed(2)} KZT`
        : "Не назначен",
    });
  }
  if (isPartnerBookingCancellationTicket(selectedTicket.value)) {
    rows.push({
      label: "Бронирование",
      value: `#${selectedTicket.value.bookingId ?? "?"}`,
    });
    rows.push({
      label: "Статус брони",
      value: partnerBookingStatusLabel(
        partnerBookingCancellationData.value?.bookingStatus,
      ),
    });
  }
  return rows;
});

const canApproveSelected = computed(() => {
  if (!isBookingCompletionTicket(selectedTicket.value)) {
    return true;
  }

  return !fineAmount.value.trim() && !fineComment.value.trim();
});

function statusLabel(status: number) {
  if (status === 1) return "На рассмотрении";
  if (status === 2) return "Одобрена";
  if (status === 3) return "Отклонена";
  if (status === 4) return "Выставлен штраф";
  return "Неизвестно";
}

function ticketTypeLabel(ticketType: number) {
  if (ticketType === 2) return "Партнёр";
  if (ticketType === 3) return "Авто партнёра";
  if (ticketType === 4) return "Завершение поездки";
  if (ticketType === 5) return "Отмена бронирования";
  return "Клиент";
}

function getTicketTypeBadgeClass(ticketType: number) {
  if (ticketType === 2)
    return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
  if (ticketType === 3)
    return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
  if (ticketType === 4)
    return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
  if (ticketType === 5)
    return "bg-rose-100 text-rose-800 dark:bg-rose-900/30 dark:text-rose-300";
  return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
}

function isClientTicket(ticket: Ticket) {
  return ticket.ticketType === 1;
}
function isPartnerTicket(ticket: Ticket) {
  return ticket.ticketType === 2;
}
function isPartnerCarTicket(ticket: Ticket) {
  return ticket.ticketType === 3;
}
function isBookingCompletionTicket(ticket: Ticket | null | undefined) {
  return ticket?.ticketType === 4;
}
function isPartnerBookingCancellationTicket(ticket: Ticket | null | undefined) {
  return ticket?.ticketType === 5;
}

function completionPhotoLabel(slot: string) {
  if (slot === "front") return "спереди";
  if (slot === "back") return "сзади";
  if (slot === "side_left") return "сбоку слева";
  if (slot === "side_right") return "сбоку справа";
  if (slot === "interior") return "из салона";
  return slot;
}

function partnerCarImageTypeLabel(imageType?: string | null, index?: number) {
  if (imageType === "front") return "Фото спереди";
  if (imageType === "back") return "Фото сзади";
  if (imageType === "side") return "Фото сбоку";
  if (imageType === "interior") return "Фото салона";
  if (imageType === "general") return "Общий вид";
  return `Фото ${(index ?? 0) + 1}`;
}

function partnerCarRequestKindLabel(value?: string | null) {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "update") return "Изменение машины";
  return "Новая машина";
}

function resolvePartnerCarRequestKind(ticket: Ticket | null | undefined) {
  if (!ticket || !isPartnerCarTicket(ticket)) {
    return "create";
  }

  const data = ticket.data as PartnerCarTicketData | undefined;
  return ticket.partnerCarRequestKind ?? data?.requestKind ?? "create";
}

function partnerCarStatusLabel(status?: number | null) {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Не указано";
}

function partnerBookingStatusLabel(status?: string | null) {
  const normalized = (status ?? "").trim().toLowerCase();
  if (normalized === "pending") return "Ожидает оплаты";
  if (normalized === "confirmed") return "Подтверждено";
  if (normalized === "active") return "Активно";
  if (normalized === "awaitingreview") return "Ожидает проверки";
  if (normalized === "completed") return "Завершено";
  if (normalized === "canceled") return "Отменено";
  return status || "Неизвестно";
}

function syncPartnerCarForm(ticket: Ticket | null) {
  if (!ticket || !isPartnerCarTicket(ticket)) {
    Object.assign(partnerCarForm, {
      carBrand: "",
      carModel: "",
      carYear: null,
      licensePlate: "",
      color: "",
      requestedStatus: 0,
      isActive: true,
    });
    return;
  }
  const data = ticket.data as PartnerCarTicketData | undefined;
  partnerCarForm.carBrand = (ticket.carBrand ?? data?.carBrand ?? "").trim();
  partnerCarForm.carModel = (ticket.carModel ?? data?.carModel ?? "").trim();
  const rawYear = ticket.carYear ?? data?.carYear ?? null;
  partnerCarForm.carYear = Number.isInteger(rawYear) ? Number(rawYear) : null;
  partnerCarForm.licensePlate = (
    ticket.licensePlate ??
    data?.licensePlate ??
    ""
  ).trim();
  partnerCarForm.color = (ticket.color ?? data?.color ?? "").trim();
  partnerCarForm.requestedStatus =
    ticket.requestedPartnerCarStatus ??
    data?.requestedStatus ??
    0;
  partnerCarForm.isActive =
    ticket.isActive ??
    data?.isActive ??
    true;
}

function buildPartnerCarPayload(): PartnerCarReviewPayload | null | undefined {
  if (!selectedTicket.value || !isPartnerCarTicket(selectedTicket.value))
    return undefined;
  const carBrand = partnerCarForm.carBrand.trim();
  const carModel = partnerCarForm.carModel.trim();
  const carYear = Number(partnerCarForm.carYear);
  const licensePlate = partnerCarForm.licensePlate.trim();

  if (!carBrand || !carModel || !licensePlate || !Number.isInteger(carYear)) {
    toast.error("Заполните марку, модель, год и госномер.");
    return null;
  }
  if (carYear < 1886 || carYear > maxAllowedCarYear) {
    toast.error(`Год машины должен быть в диапазоне 1886-${maxAllowedCarYear}.`);
    return null;
  }
  return {
    carBrand,
    carModel,
    carYear,
    licensePlate,
    color: partnerCarForm.color.trim() || null,
    requestedStatus: partnerCarForm.requestedStatus,
    isActive: Boolean(partnerCarForm.isActive),
  };
}

function resetDecisionForm() {
  rejectReason.value = "";
  fineAmount.value = "";
  fineComment.value = "";
}

async function loadPending() {
  loading.value = true;
  try {
    const data = await getPendingTickets();
    tickets.value = data;
    lastUpdatedAt.value = new Date().toISOString();
    if (data.length === 0) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      resetDecisionForm();
      syncPartnerCarForm(null);
      return;
    }
    const fallback = data[0];
    if (!fallback) {
      selectedTicket.value = null;
      selectedTicketId.value = "";
      resetDecisionForm();
      syncPartnerCarForm(null);
      return;
    }
    const nextId = data.some((t) => t.id === selectedTicketId.value)
      ? selectedTicketId.value
      : fallback.id;
    await selectTicket(nextId);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось получить список заявок.");
  } finally {
    loading.value = false;
  }
}

async function selectTicket(ticketId: string) {
  selectedTicketId.value = ticketId;
  resetDecisionForm();
  try {
    selectedTicket.value = await getTicketById(ticketId);
    syncPartnerCarForm(selectedTicket.value);
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось загрузить заявку.");
  }
}

async function approveSelected() {
  if (!selectedTicket.value || actionLoading.value) return;
  if (!canApproveSelected.value) {
    toast.error("Очистите блок штрафа, если хотите одобрить завершение поездки без начислений.");
    return;
  }
  actionLoading.value = true;
  try {
    const payload = buildPartnerCarPayload();
    if (payload === null) return;
    await approveTicket(selectedTicket.value.id, payload);
    toast.success("✓ Заявка одобрена");
    await loadPending();
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось одобрить заявку.");
  } finally {
    actionLoading.value = false;
  }
}

async function rejectSelected() {
  if (!selectedTicket.value || actionLoading.value) return;
  if (!rejectReason.value.trim()) {
    toast.error("Укажите причину отказа.");
    return;
  }
  actionLoading.value = true;
  try {
    const payload = buildPartnerCarPayload();
    if (payload === null) return;
    await rejectTicket(
      selectedTicket.value.id,
      rejectReason.value.trim(),
      payload,
    );
    toast.success("✕ Заявка отклонена", 4000);
    await loadPending();
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось отклонить заявку.");
  } finally {
    actionLoading.value = false;
  }
}

async function issueFineSelected() {
  if (!selectedTicket.value || actionLoading.value) return;
  const amount = Number(fineAmount.value);
  if (!Number.isFinite(amount) || amount <= 0) {
    toast.error("Укажите корректную сумму штрафа.");
    return;
  }
  if (!fineComment.value.trim()) {
    toast.error("Добавьте комментарий к штрафу.");
    return;
  }

  actionLoading.value = true;
  try {
    await issueTicketFine(selectedTicket.value.id, amount, fineComment.value.trim());
    toast.success("Штраф выставлен");
    fineAmount.value = "";
    fineComment.value = "";
    await loadPending();
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось выставить штраф.");
  } finally {
    actionLoading.value = false;
  }
}

function openImage(url: string) {
  if (!url) return;
  window.open(url, "_blank", "noopener,noreferrer");
}

async function openDocument(
  documentType:
    | "identity"
    | "license"
    | "ownership"
    | "front"
    | "back"
    | "side_left"
    | "side_right"
    | "interior",
) {
  if (!selectedTicket.value || actionLoading.value) return;
  actionLoading.value = true;
  try {
    const link = await getTicketDocumentTemporaryLink(
      selectedTicket.value.id,
      documentType,
    );
    window.open(link.url, "_blank", "noopener,noreferrer");
  } catch (e: any) {
    toast.error(e?.response?.data?.error || "Не удалось получить ссылку на документ.");
  } finally {
    actionLoading.value = false;
  }
}

onMounted(async () => {
  await loadPending();
});
</script>
