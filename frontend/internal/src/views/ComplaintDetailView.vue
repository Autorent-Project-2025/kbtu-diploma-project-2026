<template>
  <div class="min-h-screen bg-gray-50 dark:bg-gray-950 py-8 px-6 space-y-6">

    <!-- Header -->
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
              {{ loading ? "Загрузка..." : complaint?.subject ?? "Не найдена" }}
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
            </div>
          </div>
        </div>
      </div>
    </header>

    <!-- Loading -->
    <div
      v-if="loading"
      class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 text-gray-500 dark:text-gray-400 font-medium"
    >
      Загрузка...
    </div>

    <!-- Not found -->
    <div
      v-else-if="notFound"
      class="rounded-2xl border border-dashed border-gray-300 dark:border-gray-700 p-12 text-center text-gray-500 dark:text-gray-400 font-medium"
    >
      Жалоба не найдена.
    </div>

    <template v-else-if="complaint">

      <!-- Context cards -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <!-- Booking snapshot -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Бронирование</p>
          <div class="flex gap-3">
            <div
              v-if="complaint.snapshotData.coverImageUrl"
              class="shrink-0 w-20 h-14 rounded-lg overflow-hidden border border-gray-200 dark:border-gray-700 bg-gray-100 dark:bg-gray-800"
            >
              <img :src="complaint.snapshotData.coverImageUrl" class="w-full h-full object-cover" />
            </div>
            <div class="min-w-0">
              <p class="text-sm font-bold text-gray-900 dark:text-white">
                {{ complaint.snapshotData.carBrand }} {{ complaint.snapshotData.carModel }}
              </p>
              <p class="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
                {{ formatDateTime(complaint.snapshotData.startTime) }} → {{ formatDateTime(complaint.snapshotData.endTime) }}
              </p>
              <p v-if="complaint.snapshotData.totalPrice != null" class="text-xs font-semibold text-gray-700 dark:text-gray-300 mt-0.5">
                {{ formatPrice(complaint.snapshotData.totalPrice) }}
              </p>
            </div>
          </div>
          <!-- Booking access link -->
          <template v-if="hasBookingView">
            <EntityLink :to="`/bookings/${complaint.bookingId}`">
              Бронирование #{{ complaint.bookingId }}
            </EntityLink>
          </template>
          <template v-else>
            <div class="space-y-2">
              <p class="text-xs text-gray-500 dark:text-gray-400">
                Бронирование #{{ complaint.bookingId }}
              </p>

              <!-- No request yet -->
              <button
                v-if="!accessRequest"
                @click="showAccessRequestModal = true"
                class="text-sm font-semibold text-amber-600 dark:text-amber-400 hover:text-amber-700 dark:hover:text-amber-300 transition-colors"
              >
                Запросить доступ к бронированию
              </button>

              <!-- Pending -->
              <p
                v-else-if="accessRequest.status === 1"
                class="text-sm font-semibold text-blue-600 dark:text-blue-400"
              >
                Запрос на доступ отправлен
              </p>

              <!-- Approved -->
              <router-link
                v-else-if="accessRequest.status === 2 && !isGrantExpired"
                :to="`/complaints/${complaint.id}/booking-review`"
                class="inline-flex items-center gap-1.5 text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
              >
                Открыть review бронирования
                <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
                </svg>
              </router-link>

              <!-- Expired -->
              <p
                v-else-if="accessRequest.status === 2 && isGrantExpired"
                class="text-sm font-semibold text-gray-500 dark:text-gray-400"
              >
                Срок доступа истёк
              </p>

              <!-- Rejected -->
              <p
                v-else-if="accessRequest.status === 3"
                class="text-sm font-semibold text-red-600 dark:text-red-400"
              >
                Доступ отклонён
                <span v-if="accessRequest.decisionNote" class="font-normal text-xs block mt-0.5 text-gray-500 dark:text-gray-400">
                  {{ accessRequest.decisionNote }}
                </span>
              </p>

              <!-- Revoked -->
              <p
                v-else-if="accessRequest.status === 5"
                class="text-sm font-semibold text-gray-500 dark:text-gray-400"
              >
                Доступ отозван
              </p>
            </div>
          </template>
        </div>

        <!-- Reporter -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Заявитель</p>
          <p class="text-sm font-bold text-gray-900 dark:text-white">{{ complaint.snapshotData.reporterFullName }}</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">
            Тип: {{ reporterLabels[complaint.reporterActorType] ?? "—" }}
          </p>
          <div>
            <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">User ID</p>
            <p class="font-mono text-xs text-gray-700 dark:text-gray-300 break-all leading-relaxed">
              {{ complaint.createdByUserId }}
            </p>
          </div>
        </div>

        <!-- Counterparty -->
        <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow p-5 space-y-3">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">Контрагент</p>
          <p class="text-sm font-bold text-gray-900 dark:text-white">{{ complaint.snapshotData.counterpartyName }}</p>
          <p class="text-xs text-gray-500 dark:text-gray-400">
            Тип: {{ targetLabels[complaint.targetType] ?? "—" }}
          </p>
          <div>
            <p class="text-xs text-gray-400 dark:text-gray-500 mb-0.5">User ID</p>
            <p class="font-mono text-xs text-gray-700 dark:text-gray-300 break-all leading-relaxed">
              {{ complaint.snapshotData.counterpartyUserId }}
            </p>
          </div>
        </div>
      </div>

      <!-- Description -->
      <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8">
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Описание</h2>
        <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap leading-relaxed">{{ complaint.description }}</p>
      </div>

      <!-- Attachments (creation phase) -->
      <div
        v-if="creationAttachments.length > 0"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8"
      >
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Вложения</h2>
        <ul class="space-y-2">
          <li v-for="att in creationAttachments" :key="att.id" class="flex items-center gap-3">
            <svg class="w-4 h-4 text-gray-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
            </svg>
            <button
              @click="downloadAttachment(att.id, att.originalFileName)"
              class="text-sm text-emerald-600 dark:text-emerald-400 hover:underline font-medium"
            >
              {{ att.originalFileName }}
            </button>
            <span class="text-xs text-gray-400">{{ att.fileType }}</span>
          </li>
        </ul>
      </div>

      <!-- Info Request / Response -->
      <div
        v-if="complaint.infoRequestText"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8 space-y-4"
      >
        <h2 class="text-lg font-bold text-gray-900 dark:text-white">Запрос информации</h2>
        <div class="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800/50 rounded-xl p-4">
          <p class="text-xs font-bold uppercase tracking-wider text-blue-600 dark:text-blue-400 mb-1">Запрос от менеджера</p>
          <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.infoRequestText }}</p>
          <p v-if="complaint.infoRequestAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.infoRequestAt) }}</p>
        </div>
        <div v-if="complaint.infoResponseText" class="bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-800/50 rounded-xl p-4">
          <p class="text-xs font-bold uppercase tracking-wider text-emerald-600 dark:text-emerald-400 mb-1">Ответ заявителя</p>
          <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.infoResponseText }}</p>
          <p v-if="complaint.infoResponseAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.infoResponseAt) }}</p>
        </div>
        <!-- Response attachments -->
        <div v-if="responseAttachments.length > 0" class="pt-2">
          <p class="text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500 mb-2">Вложения к ответу</p>
          <ul class="space-y-2">
            <li v-for="att in responseAttachments" :key="att.id" class="flex items-center gap-3">
              <svg class="w-4 h-4 text-gray-400 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
              </svg>
              <button
                @click="downloadAttachment(att.id, att.originalFileName)"
                class="text-sm text-emerald-600 dark:text-emerald-400 hover:underline font-medium"
              >
                {{ att.originalFileName }}
              </button>
              <span class="text-xs text-gray-400">{{ att.fileType }}</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Manager Note -->
      <div
        v-if="complaint.managerNote"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8"
      >
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Заметка менеджера</h2>
        <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.managerNote }}</p>
        <p v-if="complaint.managerNoteAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.managerNoteAt) }}</p>
      </div>

      <!-- Resolution -->
      <div
        v-if="complaint.status === 4 && complaint.resolutionType != null"
        class="rounded-2xl border border-emerald-200 dark:border-emerald-800/50 bg-emerald-50 dark:bg-emerald-900/20 shadow-xl p-8"
      >
        <h2 class="text-lg font-bold text-emerald-700 dark:text-emerald-400 mb-4">Решение</h2>
        <p class="text-sm font-semibold text-gray-900 dark:text-white mb-2">
          {{ resolutionLabels[complaint.resolutionType] ?? "—" }}
        </p>
        <p v-if="complaint.resolutionNote" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.resolutionNote }}</p>
        <p v-if="complaint.resolvedAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.resolvedAt) }}</p>
      </div>

      <!-- Rejection -->
      <div
        v-if="complaint.status === 5"
        class="rounded-2xl border border-red-200 dark:border-red-800/50 bg-red-50 dark:bg-red-900/20 shadow-xl p-8"
      >
        <h2 class="text-lg font-bold text-red-700 dark:text-red-400 mb-4">Отклонена</h2>
        <p v-if="complaint.rejectionReason" class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ complaint.rejectionReason }}</p>
        <p v-if="complaint.rejectedAt" class="text-xs text-gray-400 mt-2">{{ formatDateTime(complaint.rejectedAt) }}</p>
      </div>

      <!-- Actions panel -->
      <div
        v-if="complaint.status === 1 || complaint.status === 2 || complaint.status === 3"
        class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8"
      >
        <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Действия</h2>

        <!-- Status=New -->
        <div v-if="complaint.status === 1">
          <button
            @click="onTake"
            :disabled="actionLoading"
            class="px-5 py-2.5 rounded-2xl bg-emerald-600 text-white font-semibold hover:bg-emerald-700 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          >
            {{ actionLoading ? "Обработка..." : "Взять в работу" }}
          </button>
        </div>

        <!-- Status=InReview -->
        <div v-if="complaint.status === 2" class="flex flex-wrap gap-3">
          <button
            @click="showRequestInfoModal = true"
            class="px-5 py-2.5 rounded-2xl border border-blue-300 dark:border-blue-500/30 text-blue-600 dark:text-blue-400 font-semibold bg-white/60 dark:bg-transparent hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors"
          >
            Запросить информацию
          </button>
          <button
            @click="showNoteModal = true"
            class="px-5 py-2.5 rounded-2xl border border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 font-semibold bg-white/60 dark:bg-transparent hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors"
          >
            Добавить заметку
          </button>
          <button
            @click="showResolveModal = true"
            class="px-5 py-2.5 rounded-2xl bg-emerald-600 text-white font-semibold hover:bg-emerald-700 transition-colors"
          >
            Решить
          </button>
          <button
            @click="showRejectModal = true"
            class="px-5 py-2.5 rounded-2xl border border-red-300 dark:border-red-500/30 text-red-600 dark:text-red-400 font-semibold bg-white/60 dark:bg-transparent hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors"
          >
            Отклонить
          </button>
        </div>

        <!-- Status=AwaitingResponse -->
        <div v-if="complaint.status === 3">
          <p class="text-sm text-orange-600 dark:text-orange-400 font-semibold">
            Ожидание ответа от заявителя
          </p>
        </div>
      </div>

    </template>

    <!-- Request Info Modal -->
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
          v-if="showRequestInfoModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showRequestInfoModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Запросить информацию</h3>
            <textarea
              v-model="requestInfoText"
              rows="4"
              placeholder="Опишите, какая информация необходима..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showRequestInfoModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onRequestInfo"
                :disabled="actionLoading || !requestInfoText.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Отправка..." : "Отправить" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Note Modal -->
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
          v-if="showNoteModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showNoteModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Добавить заметку</h3>
            <textarea
              v-model="noteText"
              rows="4"
              placeholder="Заметка менеджера..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showNoteModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onAddNote"
                :disabled="actionLoading || !noteText.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Сохранение..." : "Сохранить" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Resolve Modal -->
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
          v-if="showResolveModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showResolveModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Решить жалобу</h3>
            <div class="space-y-4">
              <div>
                <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1">Тип решения</label>
                <select
                  v-model="resolveType"
                  class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                >
                  <option value="">Выберите...</option>
                  <option value="1">В пользу заявителя</option>
                  <option value="2">В пользу контрагента</option>
                  <option value="3">Компромисс</option>
                  <option value="4">Действий не требуется</option>
                </select>
              </div>
              <div>
                <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1">Примечание</label>
                <textarea
                  v-model="resolveNote"
                  rows="3"
                  placeholder="Описание решения..."
                  class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
                />
              </div>
            </div>
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showResolveModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onResolve"
                :disabled="actionLoading || !resolveType || !resolveNote.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Сохранение..." : "Решить" }}
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
          v-if="showRejectModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showRejectModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Отклонить жалобу</h3>
            <textarea
              v-model="rejectReason"
              rows="4"
              placeholder="Причина отклонения..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none"
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
                :disabled="actionLoading || !rejectReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Отправка..." : "Отклонить" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Access Request Modal -->
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
          v-if="showAccessRequestModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showAccessRequestModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Запросить доступ к бронированию</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Укажите причину, по которой вам необходим доступ к данным бронирования #{{ complaint?.bookingId }}.
              Запрос будет рассмотрен супер-менеджером.
            </p>
            <textarea
              v-model="accessRequestReason"
              rows="4"
              placeholder="Причина запроса доступа..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-amber-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showAccessRequestModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onRequestAccess"
                :disabled="actionLoading || !accessRequestReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-amber-600 hover:bg-amber-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Отправка..." : "Отправить запрос" }}
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
import { useRoute } from "vue-router";
import {
  getComplaintById,
  takeComplaint,
  requestInfo,
  addManagerNote,
  resolveComplaint,
  rejectComplaint,
  getComplaintAttachmentLink,
} from "../api/complaints";
import type { Complaint } from "../types/Complaint";
import type { AccessRequest } from "../types/AccessRequest";
import { createAccessRequest, getMyAccessRequest } from "../api/accessRequests";
import { formatDateTime, formatPrice } from "../utils/formatters";
import { useToast } from "../composables/useToast";
import { auth } from "../store/auth";
import EntityLink from "../components/EntityLink.vue";

const route = useRoute();
const toast = useToast();

const loading = ref(false);
const notFound = ref(false);
const actionLoading = ref(false);
const complaint = ref<Complaint | null>(null);

// Modal states
const showRequestInfoModal = ref(false);
const showNoteModal = ref(false);
const showResolveModal = ref(false);
const showRejectModal = ref(false);

// Modal form data
const requestInfoText = ref("");
const noteText = ref("");
const resolveType = ref("");
const resolveNote = ref("");
const rejectReason = ref("");

// Access request state
const showAccessRequestModal = ref(false);
const accessRequestReason = ref("");
const accessRequest = ref<AccessRequest | null>(null);
const hasBookingView = computed(() => auth.hasPermission("Booking.View"));
const isGrantExpired = computed(() => {
  if (!accessRequest.value?.expiresAt) return true;
  return new Date(accessRequest.value.expiresAt) <= new Date();
});

// Label maps
const categoryLabels: Record<number, string> = {
  1: "Состояние авто",
  2: "Задержка передачи",
  3: "Качество сервиса",
  4: "Безопасность",
  5: "Поведение клиента",
  99: "Другое",
};

const statusLabels: Record<number, string> = {
  1: "Новая",
  2: "На рассмотрении",
  3: "Ожидает ответа",
  4: "Решена",
  5: "Отклонена",
};

const priorityLabels: Record<number, string> = {
  1: "Обычный",
  2: "Высокий",
  3: "Срочный",
};

const reporterLabels: Record<number, string> = {
  1: "Клиент",
  2: "Партнёр",
};

const targetLabels: Record<number, string> = {
  1: "Партнёр",
  2: "Клиент",
};

const resolutionLabels: Record<number, string> = {
  1: "В пользу заявителя",
  2: "В пользу контрагента",
  3: "Компромисс",
  4: "Действий не требуется",
};

function complaintStatusBadge(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    4: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    5: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-500";
}

function priorityBadge(priority: number): string {
  const map: Record<number, string> = {
    1: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[priority] ?? "bg-gray-100 text-gray-500";
}

// Computed attachment lists
const creationAttachments = computed(() =>
  complaint.value?.attachments.filter((a) => a.attachmentPhase === 1) ?? [],
);

const responseAttachments = computed(() =>
  complaint.value?.attachments.filter((a) => a.attachmentPhase === 2) ?? [],
);

// Data loading
async function loadComplaint() {
  const id = route.params.id as string;
  if (!id) {
    notFound.value = true;
    return;
  }

  loading.value = true;
  try {
    complaint.value = await getComplaintById(id);

    if (!hasBookingView.value) {
      try {
        accessRequest.value = await getMyAccessRequest(id);
      } catch {
        // No access request yet — that's fine
      }
    }
  } catch {
    notFound.value = true;
  } finally {
    loading.value = false;
  }
}

// Actions
async function onTake() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await takeComplaint(complaint.value.id);
    toast.success("Жалоба взята в работу");
  } catch {
    toast.error("Ошибка при взятии жалобы в работу");
  } finally {
    actionLoading.value = false;
  }
}

async function onRequestInfo() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await requestInfo(complaint.value.id, requestInfoText.value.trim());
    showRequestInfoModal.value = false;
    requestInfoText.value = "";
    toast.success("Запрос информации отправлен");
  } catch {
    toast.error("Ошибка при отправке запроса");
  } finally {
    actionLoading.value = false;
  }
}

async function onAddNote() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await addManagerNote(complaint.value.id, noteText.value.trim());
    showNoteModal.value = false;
    noteText.value = "";
    toast.success("Заметка добавлена");
  } catch {
    toast.error("Ошибка при добавлении заметки");
  } finally {
    actionLoading.value = false;
  }
}

async function onResolve() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await resolveComplaint(
      complaint.value.id,
      resolveType.value,
      resolveNote.value.trim(),
    );
    showResolveModal.value = false;
    resolveType.value = "";
    resolveNote.value = "";
    toast.success("Жалоба решена");
  } catch {
    toast.error("Ошибка при решении жалобы");
  } finally {
    actionLoading.value = false;
  }
}

async function onReject() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await rejectComplaint(complaint.value.id, rejectReason.value.trim());
    showRejectModal.value = false;
    rejectReason.value = "";
    toast.success("Жалоба отклонена");
  } catch {
    toast.error("Ошибка при отклонении жалобы");
  } finally {
    actionLoading.value = false;
  }
}

async function onRequestAccess() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    accessRequest.value = await createAccessRequest(
      complaint.value.id,
      accessRequestReason.value.trim(),
    );
    showAccessRequestModal.value = false;
    accessRequestReason.value = "";
    toast.success("Запрос на доступ отправлен");
  } catch {
    toast.error("Ошибка при отправке запроса на доступ");
  } finally {
    actionLoading.value = false;
  }
}

async function downloadAttachment(attachmentId: string, fileName: string) {
  if (!complaint.value) return;
  try {
    const link = await getComplaintAttachmentLink(complaint.value.id, attachmentId);
    window.open(link.url, "_blank");
  } catch {
    toast.error(`Ошибка при загрузке файла: ${fileName}`);
  }
}

onMounted(loadComplaint);
</script>
