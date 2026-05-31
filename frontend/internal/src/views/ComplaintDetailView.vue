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

      <!-- Two-column layout: info left, chat right -->
      <div class="flex flex-col lg:flex-row gap-6 items-start">

        <!-- LEFT COLUMN: complaint info -->
        <div class="w-full lg:w-1/2 space-y-6 min-w-0">

          <!-- Context cards -->
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
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
              <!-- Assigned manager gets auto-read access to booking review -->
              <template v-else-if="isAssignedManager">
                <router-link
                  :to="`/complaints/${complaint.id}/booking-review`"
                  class="inline-flex items-center gap-1.5 text-sm font-semibold text-emerald-600 dark:text-emerald-400 hover:text-emerald-700 dark:hover:text-emerald-300 transition-colors"
                >
                  Просмотр бронирования #{{ complaint.bookingId }}
                  <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
                  </svg>
                </router-link>
                <!-- Edit access request for higher-risk actions -->
                <div v-if="!accessRequest || accessRequest.status === 3 || accessRequest.status === 5" class="mt-1">
                  <button
                    @click="showAccessRequestModal = true"
                    class="text-xs font-medium text-amber-600 dark:text-amber-400 hover:text-amber-700 dark:hover:text-amber-300 transition-colors"
                  >
                    Запросить доступ на редактирование
                  </button>
                </div>
                <p v-else-if="accessRequest.status === 1" class="text-xs font-medium text-blue-600 dark:text-blue-400 mt-1">
                  Запрос на доступ к редактированию отправлен
                </p>
                <p v-else-if="accessRequest.status === 2 && !isGrantExpired" class="text-xs font-medium text-emerald-600 dark:text-emerald-400 mt-1">
                  Доступ на редактирование одобрен
                </p>
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
          </div>

          <!-- Counterparty (full width in left col) -->
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

          <!-- Charges / Payments -->
          <div
            v-if="hasPaymentView && bookingCharges.length > 0"
            class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-6"
          >
            <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Начисления</h2>
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="text-left text-xs font-bold uppercase tracking-wider text-gray-400 dark:text-gray-500">
                    <th class="pb-2 pr-4">ID</th>
                    <th class="pb-2 pr-4">Тип</th>
                    <th class="pb-2 pr-4">Сумма</th>
                    <th class="pb-2 pr-4">Статус</th>
                    <th class="pb-2">Дата</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-gray-100 dark:divide-gray-800">
                  <tr v-for="charge in bookingCharges" :key="charge.id">
                    <td class="py-2 pr-4 font-mono text-xs text-gray-600 dark:text-gray-400">#{{ charge.id }}</td>
                    <td class="py-2 pr-4 text-gray-900 dark:text-white">{{ chargeTypeLabels[charge.chargeType] ?? charge.chargeType }}</td>
                    <td class="py-2 pr-4 font-semibold text-gray-900 dark:text-white">{{ formatPrice(charge.amount) }}</td>
                    <td class="py-2 pr-4">
                      <span :class="chargeStatusClass(charge.status)">{{ chargeStatusLabel(charge.status) }}</span>
                    </td>
                    <td class="py-2 text-xs text-gray-500 dark:text-gray-400">{{ formatDateTime(charge.createdAt) }}</td>
                  </tr>
                </tbody>
              </table>
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
            <div v-if="creationImageAttachments.length > 0" class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-4">
              <button
                v-for="att in creationImageAttachments"
                :key="att.id"
                type="button"
                @click="openComplaintAttachmentPreview(att)"
                class="overflow-hidden rounded-2xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-left transition-all hover:border-emerald-300 hover:shadow-md dark:hover:border-emerald-500/40"
              >
                <img
                  v-if="complaintAttachmentPreviewUrls[att.id]"
                  :src="complaintAttachmentPreviewUrls[att.id]"
                  :alt="att.originalFileName"
                  class="h-48 w-full object-cover"
                  loading="lazy"
                />
                <div
                  v-else
                  class="h-48 w-full flex items-center justify-center bg-gray-100 dark:bg-gray-800 text-xs font-medium text-gray-400 dark:text-gray-500"
                >
                  Загрузка изображения...
                </div>
                <div class="flex items-center justify-between gap-3 px-4 py-3">
                  <span class="truncate text-sm font-medium text-gray-700 dark:text-gray-300">{{ att.originalFileName }}</span>
                  <span class="text-[11px] font-semibold uppercase tracking-wide text-emerald-600 dark:text-emerald-400">Открыть</span>
                </div>
              </button>
            </div>
            <ul class="space-y-2">
              <li v-for="att in creationFileAttachments" :key="att.id" class="flex items-center gap-3">
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

          <!-- Manager Actions -->
          <div
            v-if="complaint.assignedToManagerId && complaint.status !== 4 && complaint.status !== 5"
            class="rounded-2xl border border-indigo-200 dark:border-indigo-800/50 bg-white dark:bg-gray-900 shadow-xl p-8"
          >
            <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Действия менеджера</h2>
            <div class="space-y-3">

              <!-- Cancel Booking -->
              <div v-if="canCancelBooking" class="flex items-center justify-between gap-3">
                <div>
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">Отменить бронирование</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Бронирование #{{ complaint.bookingId }} ({{ complaint.snapshotData.status }})</p>
                </div>
                <button
                  @click="showCancelBookingModal = true"
                  class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors shrink-0"
                >
                  Отменить
                </button>
              </div>
              <div v-else-if="bookingNotCancelable" class="flex items-center gap-3">
                <div>
                  <p class="text-sm font-semibold text-gray-400 dark:text-gray-500">Отменить бронирование</p>
                  <p class="text-xs text-gray-400 dark:text-gray-500">Бронирование уже {{ bookingNotCancelableReason }}</p>
                </div>
              </div>

              <!-- Waive Charge -->
              <div v-if="complaint.chargeId" class="flex items-center justify-between gap-3">
                <div>
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">Аннулировать начисление</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Начисление #{{ complaint.chargeId }}</p>
                </div>
                <button
                  @click="showWaiveChargeModal = true"
                  class="px-4 py-2 text-sm font-semibold text-amber-600 dark:text-amber-400 border border-amber-300 dark:border-amber-700 hover:bg-amber-50 dark:hover:bg-amber-900/20 rounded-xl transition-colors shrink-0"
                >
                  Аннулировать
                </button>
              </div>

              <!-- Refund Charge -->
              <div v-if="complaint.chargeId" class="flex items-center justify-between gap-3">
                <div>
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">Возврат средств</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Возврат оплаченного начисления #{{ complaint.chargeId }}</p>
                </div>
                <button
                  @click="showRefundChargeModal = true"
                  class="px-4 py-2 text-sm font-semibold text-rose-600 dark:text-rose-400 border border-rose-300 dark:border-rose-700 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded-xl transition-colors shrink-0"
                >
                  Вернуть
                </button>
              </div>

              <!-- Escalate -->
              <div v-if="!complaint.isEscalated" class="flex items-center justify-between gap-3">
                <div>
                  <p class="text-sm font-semibold text-gray-900 dark:text-white">Эскалировать</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">Передать жалобу суперменеджеру</p>
                </div>
                <button
                  @click="showEscalateModal = true"
                  class="px-4 py-2 text-sm font-semibold text-purple-600 dark:text-purple-400 border border-purple-300 dark:border-purple-700 hover:bg-purple-50 dark:hover:bg-purple-900/20 rounded-xl transition-colors shrink-0"
                >
                  Эскалировать
                </button>
              </div>
              <div v-else class="flex items-center gap-3">
                <div>
                  <p class="text-sm font-semibold text-purple-600 dark:text-purple-400">Эскалирована</p>
                  <p class="text-xs text-gray-500 dark:text-gray-400">
                    {{ complaint.escalationReason }}
                    <span v-if="complaint.escalatedAt" class="ml-1 text-gray-400">
                      ({{ formatDateTime(complaint.escalatedAt) }})
                    </span>
                  </p>
                </div>
              </div>

            </div>
          </div>

          <!-- Resolution -->
          <div
            v-if="complaint.status === 4"
            class="rounded-2xl border border-emerald-200 dark:border-emerald-800/50 bg-emerald-50 dark:bg-emerald-900/20 shadow-xl p-8"
          >
            <h2 class="text-lg font-bold text-emerald-700 dark:text-emerald-400 mb-4">Решение</h2>
            <p v-if="complaint.resolutionType != null" class="text-sm font-semibold text-gray-900 dark:text-white mb-2">
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

          <!-- Reopen Requests -->
          <div
            v-if="reopenRequests.length > 0"
            class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl p-8"
          >
            <h2 class="text-lg font-bold text-gray-900 dark:text-white mb-4">Запросы на повторное открытие</h2>
            <div class="space-y-3">
              <div
                v-for="req in reopenRequests"
                :key="req.id"
                class="rounded-xl border p-4"
                :class="{
                  'border-amber-200 dark:border-amber-800/50 bg-amber-50 dark:bg-amber-900/10': req.status === 1,
                  'border-emerald-200 dark:border-emerald-800/50 bg-emerald-50 dark:bg-emerald-900/10': req.status === 2,
                  'border-red-200 dark:border-red-800/50 bg-red-50 dark:bg-red-900/10': req.status === 3,
                }"
              >
                <div class="flex items-start justify-between gap-3 mb-2">
                  <div>
                    <span
                      class="inline-flex items-center px-2 py-0.5 rounded-lg text-xs font-bold uppercase tracking-wide"
                      :class="{
                        'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400': req.status === 1,
                        'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400': req.status === 2,
                        'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400': req.status === 3,
                      }"
                    >
                      {{ reopenStatusLabels[req.status] ?? '—' }}
                    </span>
                    <span class="text-xs text-gray-400 dark:text-gray-500 ml-2">{{ formatDateTime(req.createdAt) }}</span>
                  </div>
                  <!-- Approve/Reject buttons for pending requests -->
                  <div v-if="req.status === 1" class="flex gap-2 shrink-0">
                    <button
                      @click="onApproveReopen(req.id)"
                      :disabled="actionLoading"
                      class="px-3 py-1.5 rounded-lg text-xs font-bold text-white bg-emerald-600 hover:bg-emerald-700 transition-colors disabled:opacity-60"
                    >
                      Одобрить
                    </button>
                    <button
                      @click="startRejectReopen(req.id)"
                      :disabled="actionLoading"
                      class="px-3 py-1.5 rounded-lg text-xs font-bold text-red-600 dark:text-red-400 border border-red-200 dark:border-red-800 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-60"
                    >
                      Отклонить
                    </button>
                  </div>
                </div>
                <p class="text-sm text-gray-700 dark:text-gray-300 whitespace-pre-wrap">{{ req.reason }}</p>
                <p v-if="req.decisionNote" class="text-xs text-gray-500 dark:text-gray-400 mt-2 italic">{{ req.decisionNote }}</p>
              </div>
            </div>
          </div>

        </div>

        <!-- RIGHT COLUMN: chat (sticky on desktop) -->
        <div class="w-full lg:w-1/2 lg:sticky lg:top-8 min-w-0">
          <ChatPanel
            :context-type="'complaint'"
            :context-id="complaint.id"
            height="calc(100vh - 120px)"
            :complaint-state="complaintState"
            :refresh-context="refreshComplaintForChat"
          />
        </div>

      </div>

    </template>

    <!-- Reject Reopen Modal -->
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
          v-if="showRejectReopenModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showRejectReopenModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Отклонить запрос на открытие</h3>
            <textarea
              v-model="rejectReopenNote"
              rows="3"
              placeholder="Причина отклонения (необязательно)..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showRejectReopenModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onRejectReopen"
                :disabled="actionLoading"
                class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors disabled:opacity-60"
              >
                {{ actionLoading ? 'Отправка...' : 'Отклонить' }}
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
                <label class="block text-sm font-semibold text-gray-700 dark:text-gray-300 mb-1">Комментарий</label>
                <textarea
                  v-model="resolveNote"
                  rows="3"
                  placeholder="Комментарий к закрытию..."
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
                :disabled="actionLoading || !resolveNote.trim()"
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

    <!-- Cancel Booking Modal -->
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
          v-if="showCancelBookingModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showCancelBookingModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Отменить бронирование</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Бронирование #{{ complaint?.bookingId }} будет отменено. Это действие необратимо.
            </p>
            <textarea
              v-model="cancelBookingReason"
              rows="3"
              placeholder="Причина отмены бронирования..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-red-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showCancelBookingModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onCancelBooking"
                :disabled="actionLoading || !cancelBookingReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-red-600 hover:bg-red-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Отменить бронирование" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Waive Charge Modal -->
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
          v-if="showWaiveChargeModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showWaiveChargeModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Аннулировать начисление</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Начисление #{{ complaint?.chargeId }} будет аннулировано. Аннулировать можно только pending-начисления.
            </p>
            <textarea
              v-model="waiveChargeReason"
              rows="3"
              placeholder="Причина аннулирования..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-amber-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showWaiveChargeModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onWaiveCharge"
                :disabled="actionLoading || !waiveChargeReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-amber-600 hover:bg-amber-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Аннулировать" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Escalate Modal -->
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
          v-if="showEscalateModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showEscalateModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Эскалировать жалобу</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Жалоба будет передана суперменеджеру. Приоритет будет повышен до "Срочный".
            </p>
            <textarea
              v-model="escalateReason"
              rows="3"
              placeholder="Причина эскалации..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-purple-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showEscalateModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onEscalate"
                :disabled="actionLoading || !escalateReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-purple-600 hover:bg-purple-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Эскалировать" }}
              </button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Refund Charge Modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div
          v-if="showRefundChargeModal"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm"
          @click.self="showRefundChargeModal = false"
        >
          <div class="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 w-full max-w-md mx-4">
            <h3 class="text-lg font-bold text-gray-900 dark:text-white mb-2">Возврат средств по начислению</h3>
            <p class="text-sm text-gray-500 dark:text-gray-400 mb-4">
              Средства будут возвращены клиенту. Доля партнёра будет списана из кошелька.
            </p>
            <textarea
              v-model="refundChargeReason"
              rows="3"
              placeholder="Причина возврата..."
              class="w-full rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white p-3 focus:outline-none focus:ring-2 focus:ring-rose-500 resize-none"
            />
            <div class="flex justify-end gap-3 mt-4">
              <button
                @click="showRefundChargeModal = false"
                class="px-4 py-2 text-sm font-semibold text-gray-600 dark:text-gray-400 rounded-xl border border-gray-200 dark:border-gray-700 hover:border-gray-300 transition-colors"
              >
                Отмена
              </button>
              <button
                @click="onRefundCharge"
                :disabled="actionLoading || !refundChargeReason.trim()"
                class="px-4 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-xl transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
              >
                {{ actionLoading ? "Обработка..." : "Вернуть средства" }}
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
  resolveComplaint,
  rejectComplaint,
  getComplaintAttachmentLink,
  getReopenRequests,
  approveReopenRequest,
  rejectReopenRequest,
  cancelComplaintBooking,
  waiveComplaintCharge,
  escalateComplaint,
  refundComplaintCharge,
} from "../api/complaints";
import type { Complaint, ComplaintAttachment, ReopenRequest } from "../types/Complaint";
import type { AccessRequest } from "../types/AccessRequest";
import { createAccessRequest, getMyAccessRequest } from "../api/accessRequests";
import { getBookingCharges, type BookingCharge } from "../api/payments";
import { formatDateTime, formatPrice } from "../utils/formatters";
import { useToast } from "../composables/useToast";
import { can } from "../accessControl";
import { auth } from "../store/auth";
import EntityLink from "../components/EntityLink.vue";
import ChatPanel from "../components/ChatPanel.vue";
import { isImageMimeType, resolveAttachmentPreviewUrl } from "../utils/attachmentPreview";

const route = useRoute();
const toast = useToast();

const loading = ref(false);
const notFound = ref(false);
const actionLoading = ref(false);
const complaint = ref<Complaint | null>(null);

// Reopen requests
const reopenRequests = ref<ReopenRequest[]>([]);
const showRejectReopenModal = ref(false);
const rejectReopenNote = ref("");
const rejectReopenTargetId = ref<string | null>(null);

// Complaint state for ChatPanel
const complaintState = computed<"not-taken" | "taken" | "closed">(() => {
  if (!complaint.value) return "not-taken";
  if (complaint.value.status === 4 || complaint.value.status === 5) return "closed";
  if (complaint.value.status === 1) return "not-taken";
  return "taken";
});

const reopenStatusLabels: Record<number, string> = {
  1: "Ожидает",
  2: "Одобрен",
  3: "Отклонён",
};

// Modal states
const showResolveModal = ref(false);
const showRejectModal = ref(false);

// Modal form data
const resolveNote = ref("");
const rejectReason = ref("");

// Manager action modal states
const showCancelBookingModal = ref(false);
const showWaiveChargeModal = ref(false);
const showEscalateModal = ref(false);
const showRefundChargeModal = ref(false);
const cancelBookingReason = ref("");
const waiveChargeReason = ref("");
const escalateReason = ref("");
const refundChargeReason = ref("");

// Computed: can booking be canceled?
// Pending/Confirmed: always allowed. Active/AwaitingReview: allowed (server checks edit access).
const canCancelBooking = computed(() => {
  if (!complaint.value) return false;
  const status = complaint.value.snapshotData.status?.toLowerCase();
  return status === "pending" || status === "confirmed" || status === "active" || status === "awaitingreview";
});
const bookingNotCancelable = computed(() => {
  if (!complaint.value) return false;
  return !canCancelBooking.value;
});
const bookingNotCancelableReason = computed(() => {
  if (!complaint.value) return "";
  const status = complaint.value.snapshotData.status?.toLowerCase();
  if (status === "completed") return "завершено";
  if (status === "canceled") return "отменено";
  return "";
});

// Access request state
const showAccessRequestModal = ref(false);
const accessRequestReason = ref("");
const accessRequest = ref<AccessRequest | null>(null);
const complaintAttachmentPreviewUrls = ref<Record<string, string>>({});
const hasBookingView = computed(() => can("Booking.View"));
const hasPaymentView = computed(() => can("Payment.View"));
const bookingCharges = ref<BookingCharge[]>([]);

const chargeTypeLabels: Record<string, string> = {
  LatePenalty: "Штраф за опоздание",
  DamageFine: "Штраф за повреждение",
};

function chargeStatusLabel(status: string): string {
  const map: Record<string, string> = { Pending: "Ожидает", Paid: "Оплачен", Canceled: "Отменён", Refunded: "Возвращён" };
  return map[status] ?? status;
}

function chargeStatusClass(status: string): string {
  const base = "px-2 py-0.5 rounded-full text-xs font-bold";
  const map: Record<string, string> = {
    Pending: `${base} bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400`,
    Paid: `${base} bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400`,
    Canceled: `${base} bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400`,
    Refunded: `${base} bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400`,
  };
  return map[status] ?? `${base} bg-gray-100 text-gray-500`;
}
const isAssignedManager = computed(() => {
  if (!complaint.value) return false;
  const userId = auth.getUserId();
  return !!userId && complaint.value.assignedToManagerId === userId;
});
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
const creationImageAttachments = computed(() =>
  creationAttachments.value.filter((attachment) => isImageMimeType(attachment.fileType)),
);
const creationFileAttachments = computed(() =>
  creationAttachments.value.filter((attachment) => !isImageMimeType(attachment.fileType)),
);

const responseAttachments = computed(() =>
  complaint.value?.attachments.filter((a) => a.attachmentPhase === 2) ?? [],
);

// Re-fetch complaint to trigger backend EnsureConversationExists
async function refreshComplaintForChat(): Promise<void> {
  const id = route.params.id as string;
  if (!id) return;
  try {
    complaint.value = await getComplaintById(id);
    void preloadComplaintAttachmentPreviews(complaint.value);
  } catch { /* ignore */ }
}

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
    void preloadComplaintAttachmentPreviews(complaint.value);

    // Load reopen requests and access request in parallel
    const promises: Promise<void>[] = [];

    promises.push(
      getReopenRequests(id).then((r) => { reopenRequests.value = r; }).catch(() => {}),
    );

    if (!hasBookingView.value) {
      promises.push(
        getMyAccessRequest(id).then((r) => { accessRequest.value = r; }).catch(() => {}),
      );
    }

    if (hasPaymentView.value && complaint.value) {
      promises.push(
        getBookingCharges(complaint.value.bookingId).then((c) => { bookingCharges.value = c; }).catch(() => {}),
      );
    }

    await Promise.all(promises);
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

async function onResolve() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await resolveComplaint(
      complaint.value.id,
      resolveNote.value.trim(),
    );
    showResolveModal.value = false;
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

async function onApproveReopen(requestId: string) {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    await approveReopenRequest(requestId);
    // Reload complaint (status changes to InReview) and reopen requests
    complaint.value = await getComplaintById(complaint.value.id);
    reopenRequests.value = await getReopenRequests(complaint.value.id);
    toast.success("Запрос одобрен, жалоба открыта повторно");
  } catch {
    toast.error("Ошибка при одобрении запроса");
  } finally {
    actionLoading.value = false;
  }
}

function startRejectReopen(requestId: string) {
  rejectReopenTargetId.value = requestId;
  rejectReopenNote.value = "";
  showRejectReopenModal.value = true;
}

async function onRejectReopen() {
  if (actionLoading.value || !rejectReopenTargetId.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    await rejectReopenRequest(rejectReopenTargetId.value, rejectReopenNote.value.trim() || undefined);
    reopenRequests.value = await getReopenRequests(complaint.value.id);
    showRejectReopenModal.value = false;
    rejectReopenTargetId.value = null;
    toast.success("Запрос отклонён");
  } catch {
    toast.error("Ошибка при отклонении запроса");
  } finally {
    actionLoading.value = false;
  }
}

// Manager action handlers
async function onCancelBooking() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await cancelComplaintBooking(
      complaint.value.id,
      cancelBookingReason.value.trim(),
    );
    showCancelBookingModal.value = false;
    cancelBookingReason.value = "";
    toast.success("Бронирование отменено");
  } catch (e: any) {
    const msg = e?.response?.data?.error || e?.response?.data?.message || "Ошибка при отмене бронирования";
    toast.error(msg);
  } finally {
    actionLoading.value = false;
  }
}

async function onWaiveCharge() {
  if (actionLoading.value || !complaint.value || !complaint.value.chargeId) return;
  actionLoading.value = true;
  try {
    complaint.value = await waiveComplaintCharge(
      complaint.value.id,
      complaint.value.chargeId,
      waiveChargeReason.value.trim(),
    );
    showWaiveChargeModal.value = false;
    waiveChargeReason.value = "";
    toast.success("Начисление аннулировано");
  } catch (e: any) {
    const msg = e?.response?.data?.error || e?.response?.data?.message || "Ошибка при аннулировании начисления";
    toast.error(msg);
  } finally {
    actionLoading.value = false;
  }
}

async function onEscalate() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await escalateComplaint(
      complaint.value.id,
      escalateReason.value.trim(),
    );
    showEscalateModal.value = false;
    escalateReason.value = "";
    toast.success("Жалоба эскалирована суперменеджеру");
  } catch (e: any) {
    const msg = e?.response?.data?.error || e?.response?.data?.message || "Ошибка при эскалации жалобы";
    toast.error(msg);
  } finally {
    actionLoading.value = false;
  }
}

async function onRefundCharge() {
  if (actionLoading.value || !complaint.value) return;
  actionLoading.value = true;
  try {
    complaint.value = await refundComplaintCharge(
      complaint.value.id,
      complaint.value.chargeId!,
      refundChargeReason.value.trim(),
    );
    showRefundChargeModal.value = false;
    refundChargeReason.value = "";
    toast.success("Средства возвращены по начислению");
  } catch (e: any) {
    const msg = e?.response?.data?.error || e?.response?.data?.message || "Ошибка при возврате средств";
    toast.error(msg);
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

async function ensureComplaintAttachmentPreview(attachment: ComplaintAttachment): Promise<string | null> {
  if (!complaint.value || !isImageMimeType(attachment.fileType)) {
    return null;
  }

  const existing = complaintAttachmentPreviewUrls.value[attachment.id];
  if (existing) {
    return existing;
  }

  try {
    const link = await getComplaintAttachmentLink(complaint.value.id, attachment.id);
    const resolvedUrl = resolveAttachmentPreviewUrl(link.url);
    if (!resolvedUrl) {
      return null;
    }

    complaintAttachmentPreviewUrls.value = {
      ...complaintAttachmentPreviewUrls.value,
      [attachment.id]: resolvedUrl,
    };

    return resolvedUrl;
  } catch {
    return null;
  }
}

async function preloadComplaintAttachmentPreviews(targetComplaint: Complaint | null): Promise<void> {
  if (!targetComplaint) {
    return;
  }

  await Promise.all(
    targetComplaint.attachments
      .filter((attachment) => isImageMimeType(attachment.fileType))
      .map((attachment) => ensureComplaintAttachmentPreview(attachment)),
  );
}

async function openComplaintAttachmentPreview(attachment: ComplaintAttachment): Promise<void> {
  const previewUrl = await ensureComplaintAttachmentPreview(attachment);

  if (previewUrl) {
    window.open(previewUrl, "_blank");
    return;
  }

  await downloadAttachment(attachment.id, attachment.originalFileName);
}

onMounted(loadComplaint);
</script>
