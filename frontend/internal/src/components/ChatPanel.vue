<template>
  <div
    class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl flex flex-col overflow-hidden"
    :style="{ height: height }"
  >
    <!-- Header -->
    <div class="px-5 py-3 border-b border-gray-200 dark:border-gray-800 flex items-center justify-between shrink-0 bg-white dark:bg-gray-900">
      <div class="flex items-center gap-2.5">
        <div class="w-7 h-7 rounded-lg bg-emerald-100 dark:bg-emerald-900/40 flex items-center justify-center">
          <svg class="w-4 h-4 text-emerald-600 dark:text-emerald-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
        </div>
        <div>
          <p class="text-sm font-bold text-gray-900 dark:text-white leading-tight">Чат по обращению</p>
        </div>
        <span
          v-if="conversation?.status === 'Closed'"
          class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wide bg-red-50 text-red-500 dark:bg-red-900/30 dark:text-red-400 border border-red-200 dark:border-red-800"
        >Закрыт</span>
      </div>
      <div v-if="canSendInternal && conversation" class="flex items-center gap-2">
        <label class="flex items-center gap-1.5 text-xs cursor-pointer select-none group">
          <input
            type="checkbox"
            v-model="internalMode"
            class="rounded border-gray-300 dark:border-gray-600 text-amber-500 focus:ring-amber-500 w-3.5 h-3.5"
          />
          <span class="text-gray-500 dark:text-gray-400 group-hover:text-amber-600 dark:group-hover:text-amber-400 transition-colors">
            Внутренняя заметка
          </span>
        </label>
      </div>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center space-y-3">
        <div class="w-10 h-10 mx-auto relative">
          <svg class="w-10 h-10 text-emerald-500 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
        </div>
        <p class="text-sm text-gray-400 dark:text-gray-500">Загрузка чата...</p>
      </div>
    </div>

    <!-- Error: 403 Forbidden -->
    <div v-else-if="errorType === 'forbidden'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3 max-w-xs">
        <div class="w-12 h-12 mx-auto rounded-full bg-red-50 dark:bg-red-900/20 flex items-center justify-center">
          <svg class="w-6 h-6 text-red-400 dark:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Нет доступа к переписке</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 leading-relaxed">
          Вы не являетесь участником этой переписки. Возьмите жалобу в работу или обратитесь к администратору.
        </p>
      </div>
    </div>

    <!-- Error: 500 Server Error -->
    <div v-else-if="errorType === 'server'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3 max-w-xs">
        <div class="w-12 h-12 mx-auto rounded-full bg-red-50 dark:bg-red-900/20 flex items-center justify-center">
          <svg class="w-6 h-6 text-red-400 dark:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Ошибка сервера</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 leading-relaxed">
          Не удалось загрузить чат. Попробуйте позже.
        </p>
        <button
          @click="retryWithRefresh"
          class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-700 dark:hover:bg-emerald-600 transition-colors shadow-sm"
        >
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Повторить
        </button>
      </div>
    </div>

    <!-- No conversation: complaint not taken -->
    <div v-else-if="!conversation && !loading && complaintState === 'not-taken'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3 max-w-xs">
        <div class="w-12 h-12 mx-auto rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
          <svg class="w-6 h-6 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Переписка недоступна</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 leading-relaxed">
          Возьмите жалобу в работу, чтобы подключиться к переписке
        </p>
      </div>
    </div>

    <!-- No conversation: complaint taken but conversation not created yet -->
    <div v-else-if="!conversation && !loading" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3 max-w-xs">
        <div class="w-12 h-12 mx-auto rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
          <svg class="w-6 h-6 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
        </div>
        <p class="text-sm font-medium text-gray-700 dark:text-gray-300">Переписка ещё не создана</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 leading-relaxed">
          Обновите страницу, чтобы проверить наличие переписки.
        </p>
        <button
          @click="retryWithRefresh"
          class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-700 dark:bg-emerald-700 dark:hover:bg-emerald-600 transition-colors shadow-sm"
        >
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Обновить
        </button>
      </div>
    </div>

    <!-- Conversation loaded: messages area -->
    <template v-else-if="conversation">
      <div
        ref="messagesContainer"
        class="flex-1 min-h-0 overflow-y-auto px-4 py-4"
        @scroll="onScroll"
      >
        <!-- Load more spinner -->
        <div v-if="loadingMore" class="flex justify-center py-3">
          <svg class="w-5 h-5 text-gray-300 dark:text-gray-600 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
        </div>

        <!-- Empty conversation -->
        <div v-if="messages.length === 0 && !loadingMore" class="flex flex-col items-center justify-center h-full text-center space-y-3 py-8">
          <div class="w-14 h-14 rounded-full bg-gray-50 dark:bg-gray-800 flex items-center justify-center">
            <svg class="w-7 h-7 text-gray-200 dark:text-gray-700" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </div>
          <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Переписка пока пуста</p>
          <p class="text-xs text-gray-400 dark:text-gray-500">Напишите сообщение или прикрепите файл.</p>
        </div>

        <!-- Message list -->
        <div v-else class="space-y-1">
          <template v-for="(msg, idx) in messages" :key="msg.id">
            <!-- Date separator -->
            <div
              v-if="shouldShowDateSeparator(idx)"
              class="flex items-center gap-3 py-3"
            >
              <div class="flex-1 h-px bg-gray-200 dark:bg-gray-700"></div>
              <span class="text-[10px] font-semibold uppercase tracking-wider text-gray-400 dark:text-gray-500 whitespace-nowrap">
                {{ formatDateSeparator(msg.createdAt) }}
              </span>
              <div class="flex-1 h-px bg-gray-200 dark:bg-gray-700"></div>
            </div>

            <!-- System message -->
            <div
              v-if="msg.messageType === 'System'"
              class="flex justify-center py-2"
            >
              <div class="flex items-center gap-2 px-4 py-1.5 rounded-full bg-gray-50 dark:bg-gray-800/60 border border-gray-100 dark:border-gray-700/50">
                <svg class="w-3 h-3 text-gray-400 dark:text-gray-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <p class="text-[11px] text-gray-400 dark:text-gray-500">{{ msg.body }}</p>
                <span class="text-[10px] text-gray-300 dark:text-gray-600">{{ formatTime(msg.createdAt) }}</span>
              </div>
            </div>

            <!-- Internal note -->
            <div
              v-else-if="msg.visibility === 'InternalOnly'"
              class="flex justify-end pl-12 py-0.5"
            >
              <div class="max-w-[75%]">
                <div class="flex items-center justify-end gap-2 mb-1">
                  <span class="text-[10px] font-bold uppercase tracking-wider text-amber-500 dark:text-amber-400">
                    ВНУТРЕННЯЯ
                  </span>
                  <span class="text-[10px] text-gray-400 dark:text-gray-500">{{ senderName(msg) }}</span>
                  <span class="text-[10px] text-gray-300 dark:text-gray-600">{{ formatTime(msg.createdAt) }}</span>
                </div>
                <div class="flex items-end justify-end gap-2">
                  <div class="rounded-2xl rounded-br-md px-4 py-2.5 bg-amber-50 dark:bg-amber-900/20 border border-amber-200 dark:border-amber-700/50 text-sm text-amber-900 dark:text-amber-200 shadow-sm">
                    <p v-if="msg.body" class="whitespace-pre-wrap break-words">{{ msg.body }}</p>
                    <div v-if="msg.attachments?.length" class="mt-2 space-y-2">
                      <template v-for="att in msg.attachments" :key="att.id">
                        <button
                          v-if="isImageMimeType(att.mimeType)"
                          type="button"
                          @click="openAttachment(att)"
                          class="block w-full overflow-hidden rounded-xl border border-amber-200/60 dark:border-amber-700/40 bg-amber-100/40 dark:bg-amber-800/20 hover:bg-amber-100 dark:hover:bg-amber-800/30 transition-colors text-left"
                        >
                          <img
                            v-if="attachmentPreviewUrls[att.id]"
                            :src="attachmentPreviewUrls[att.id]"
                            :alt="att.originalFileName"
                            class="h-40 w-full object-cover"
                            loading="lazy"
                          />
                          <div
                            v-else
                            class="h-32 flex items-center justify-center text-xs font-medium text-amber-600 dark:text-amber-300"
                          >
                            Загрузка изображения...
                          </div>
                          <div class="flex items-center gap-2 px-2.5 py-2 text-xs text-amber-900 dark:text-amber-200">
                            <svg class="w-3.5 h-3.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m2.25 15 5.159-5.159a2.25 2.25 0 0 1 3.182 0L15 14.25m-1.5-1.5 1.659-1.659a2.25 2.25 0 0 1 3.182 0L21.75 14.5M3.75 19.5h16.5A1.5 1.5 0 0 0 21.75 18V6A1.5 1.5 0 0 0 20.25 4.5H3.75A1.5 1.5 0 0 0 2.25 6v12A1.5 1.5 0 0 0 3.75 19.5Z" /></svg>
                            <span class="truncate max-w-[220px]">{{ att.originalFileName }}</span>
                          </div>
                        </button>
                        <button
                          v-else
                          type="button"
                          @click="openAttachment(att)"
                          class="flex items-center gap-1.5 text-xs px-2.5 py-1.5 rounded-lg bg-amber-100/60 dark:bg-amber-800/30 hover:bg-amber-100 dark:hover:bg-amber-800/50 border border-amber-200/60 dark:border-amber-700/40 transition-colors cursor-pointer"
                        >
                          <svg class="w-3.5 h-3.5 text-amber-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
                          <span class="truncate max-w-[180px]">{{ att.originalFileName }}</span>
                        </button>
                      </template>
                    </div>
                  </div>
                  <div
                    class="w-7 h-7 rounded-full bg-amber-100 dark:bg-amber-800/40 flex items-center justify-center text-[10px] font-bold text-amber-600 dark:text-amber-400 shrink-0"
                  >{{ avatarInitials(msg) }}</div>
                </div>
              </div>
            </div>

            <!-- Manager's own message (right-aligned, emerald) -->
            <div
              v-else-if="msg.senderUserId === currentUserId"
              class="flex justify-end pl-12 py-0.5"
            >
              <div class="max-w-[75%]">
                <div class="flex items-center justify-end gap-2 mb-1">
                  <span class="text-[10px] font-medium text-gray-400 dark:text-gray-500">{{ senderName(msg) }}</span>
                  <span class="text-[10px] font-semibold text-emerald-500 dark:text-emerald-400 uppercase tracking-wider">{{ actorLabel(msg.senderActorType) }}</span>
                  <span class="text-[10px] text-gray-300 dark:text-gray-600">{{ formatTime(msg.createdAt) }}</span>
                </div>
                <div class="flex items-end justify-end gap-2">
                  <div class="rounded-2xl rounded-br-md px-4 py-2.5 bg-emerald-50 dark:bg-emerald-900/20 border border-emerald-200 dark:border-emerald-700/50 text-sm text-gray-900 dark:text-gray-100 shadow-sm">
                    <p v-if="msg.body" class="whitespace-pre-wrap break-words">{{ msg.body }}</p>
                    <div v-if="msg.attachments?.length" class="mt-2 space-y-2">
                      <template v-for="att in msg.attachments" :key="att.id">
                        <button
                          v-if="isImageMimeType(att.mimeType)"
                          type="button"
                          @click="openAttachment(att)"
                          class="block w-full overflow-hidden rounded-xl border border-emerald-200/60 dark:border-emerald-700/40 bg-emerald-100/40 dark:bg-emerald-800/20 hover:bg-emerald-100 dark:hover:bg-emerald-800/30 transition-colors text-left"
                        >
                          <img
                            v-if="attachmentPreviewUrls[att.id]"
                            :src="attachmentPreviewUrls[att.id]"
                            :alt="att.originalFileName"
                            class="h-40 w-full object-cover"
                            loading="lazy"
                          />
                          <div
                            v-else
                            class="h-32 flex items-center justify-center text-xs font-medium text-emerald-600 dark:text-emerald-300"
                          >
                            Загрузка изображения...
                          </div>
                          <div class="flex items-center gap-2 px-2.5 py-2 text-xs text-gray-900 dark:text-gray-100">
                            <svg class="w-3.5 h-3.5 shrink-0 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m2.25 15 5.159-5.159a2.25 2.25 0 0 1 3.182 0L15 14.25m-1.5-1.5 1.659-1.659a2.25 2.25 0 0 1 3.182 0L21.75 14.5M3.75 19.5h16.5A1.5 1.5 0 0 0 21.75 18V6A1.5 1.5 0 0 0 20.25 4.5H3.75A1.5 1.5 0 0 0 2.25 6v12A1.5 1.5 0 0 0 3.75 19.5Z" /></svg>
                            <span class="truncate max-w-[220px]">{{ att.originalFileName }}</span>
                          </div>
                        </button>
                        <button
                          v-else
                          type="button"
                          @click="openAttachment(att)"
                          class="flex items-center gap-1.5 text-xs px-2.5 py-1.5 rounded-lg bg-emerald-100/60 dark:bg-emerald-800/30 hover:bg-emerald-100 dark:hover:bg-emerald-800/50 border border-emerald-200/60 dark:border-emerald-700/40 transition-colors cursor-pointer"
                        >
                          <svg class="w-3.5 h-3.5 text-emerald-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
                          <span class="truncate max-w-[180px]">{{ att.originalFileName }}</span>
                        </button>
                      </template>
                    </div>
                  </div>
                  <div
                    class="w-7 h-7 rounded-full bg-emerald-100 dark:bg-emerald-800/40 flex items-center justify-center text-[10px] font-bold text-emerald-600 dark:text-emerald-400 shrink-0"
                  >{{ avatarInitials(msg) }}</div>
                </div>
              </div>
            </div>

            <!-- Other user message (left-aligned, gray) -->
            <div
              v-else
              class="flex justify-start pr-12 py-0.5"
            >
              <div class="max-w-[75%]">
                <div class="flex items-center gap-2 mb-1">
                  <span class="text-[10px] font-semibold uppercase tracking-wider" :class="actorColor(msg.senderActorType)">{{ actorLabel(msg.senderActorType) }}</span>
                  <span class="text-[10px] font-medium text-gray-400 dark:text-gray-500">{{ senderName(msg) }}</span>
                  <span class="text-[10px] text-gray-300 dark:text-gray-600">{{ formatTime(msg.createdAt) }}</span>
                </div>
                <div class="flex items-end gap-2">
                  <div
                    class="w-7 h-7 rounded-full flex items-center justify-center text-[10px] font-bold shrink-0"
                    :class="avatarBgClass(msg.senderActorType)"
                  >{{ avatarInitials(msg) }}</div>
                  <div class="rounded-2xl rounded-bl-md px-4 py-2.5 bg-gray-100 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 text-sm text-gray-900 dark:text-gray-100 shadow-sm">
                    <p v-if="msg.body" class="whitespace-pre-wrap break-words">{{ msg.body }}</p>
                    <div v-if="msg.attachments?.length" class="mt-2 space-y-2">
                      <template v-for="att in msg.attachments" :key="att.id">
                        <button
                          v-if="isImageMimeType(att.mimeType)"
                          type="button"
                          @click="openAttachment(att)"
                          class="block w-full overflow-hidden rounded-xl border border-gray-200 dark:border-gray-600 bg-white/80 dark:bg-gray-700/80 hover:bg-white dark:hover:bg-gray-700 transition-colors text-left"
                        >
                          <img
                            v-if="attachmentPreviewUrls[att.id]"
                            :src="attachmentPreviewUrls[att.id]"
                            :alt="att.originalFileName"
                            class="h-40 w-full object-cover"
                            loading="lazy"
                          />
                          <div
                            v-else
                            class="h-32 flex items-center justify-center text-xs font-medium text-gray-400 dark:text-gray-500"
                          >
                            Загрузка изображения...
                          </div>
                          <div class="flex items-center gap-2 px-2.5 py-2 text-xs text-gray-700 dark:text-gray-200">
                            <svg class="w-3.5 h-3.5 shrink-0 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m2.25 15 5.159-5.159a2.25 2.25 0 0 1 3.182 0L15 14.25m-1.5-1.5 1.659-1.659a2.25 2.25 0 0 1 3.182 0L21.75 14.5M3.75 19.5h16.5A1.5 1.5 0 0 0 21.75 18V6A1.5 1.5 0 0 0 20.25 4.5H3.75A1.5 1.5 0 0 0 2.25 6v12A1.5 1.5 0 0 0 3.75 19.5Z" /></svg>
                            <span class="truncate max-w-[220px]">{{ att.originalFileName }}</span>
                          </div>
                        </button>
                        <button
                          v-else
                          type="button"
                          @click="openAttachment(att)"
                          class="flex items-center gap-1.5 text-xs px-2.5 py-1.5 rounded-lg bg-white/60 dark:bg-gray-700/60 hover:bg-white dark:hover:bg-gray-700 border border-gray-200 dark:border-gray-600 transition-colors cursor-pointer"
                        >
                          <svg class="w-3.5 h-3.5 text-gray-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
                          <span class="truncate max-w-[180px]">{{ att.originalFileName }}</span>
                        </button>
                      </template>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </template>

          <!-- Typing indicator -->
          <div v-if="typingDisplay" class="flex justify-start pl-9 py-1">
            <div class="flex items-center gap-2 px-3 py-2 rounded-2xl bg-gray-100 dark:bg-gray-800 border border-gray-200 dark:border-gray-700">
              <span class="text-xs text-gray-500 dark:text-gray-400">{{ typingDisplay }}</span>
              <span class="flex gap-0.5">
                <span class="w-1.5 h-1.5 rounded-full bg-gray-400 dark:bg-gray-500 animate-bounce" style="animation-delay: 0ms"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-gray-400 dark:bg-gray-500 animate-bounce" style="animation-delay: 150ms"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-gray-400 dark:bg-gray-500 animate-bounce" style="animation-delay: 300ms"></span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Input area: can write -->
      <div v-if="canWrite" class="border-t border-gray-200 dark:border-gray-800 shrink-0 bg-white dark:bg-gray-900">
        <!-- Internal mode indicator bar -->
        <div
          v-if="internalMode"
          class="px-4 py-1.5 bg-amber-50 dark:bg-amber-900/10 border-b border-amber-200 dark:border-amber-700/50 flex items-center gap-2"
        >
          <svg class="w-3.5 h-3.5 text-amber-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
          <span class="text-[11px] font-semibold text-amber-600 dark:text-amber-400">Режим внутренней заметки -- клиент не увидит это сообщение</span>
        </div>

        <!-- File preview -->
        <div v-if="selectedFiles.length > 0" class="px-4 pt-2.5 pb-0">
          <div class="flex flex-wrap gap-1.5">
            <div
              v-for="(file, idx) in selectedFiles"
              :key="idx"
              class="flex items-center gap-1.5 text-xs px-2.5 py-1.5 rounded-lg bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 group"
            >
              <svg class="w-3.5 h-3.5 text-gray-400 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
              <span class="truncate max-w-[120px] text-gray-600 dark:text-gray-300">{{ file.name }}</span>
              <button @click="removeFile(idx)" class="text-gray-300 hover:text-red-500 dark:text-gray-600 dark:hover:text-red-400 transition-colors ml-0.5">
                <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
              </button>
            </div>
          </div>
        </div>

        <!-- Composer -->
        <div
          class="px-4 py-3"
          :class="internalMode ? '' : ''"
        >
          <div
            class="flex items-end gap-2 rounded-xl border transition-all duration-200"
            :class="[
              internalMode
                ? 'border-amber-300 dark:border-amber-600 ring-2 ring-amber-200/50 dark:ring-amber-800/30 bg-amber-50/30 dark:bg-amber-900/10'
                : 'border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50'
            ]"
          >
            <button
              @click="openFilePicker"
              class="shrink-0 p-2.5 text-gray-400 hover:text-emerald-600 dark:hover:text-emerald-400 transition-colors rounded-lg"
              title="Прикрепить файл"
            >
              <svg class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" />
              </svg>
            </button>
            <input ref="fileInput" type="file" multiple class="hidden" @change="onFilesSelected" />
            <textarea
              v-model="newMessage"
              @keydown="onKeydown"
              @input="onTyping"
              :placeholder="internalMode ? 'Внутренняя заметка (не видна клиенту)...' : 'Написать сообщение...'"
              rows="1"
              class="flex-1 bg-transparent text-sm text-gray-900 dark:text-white py-2.5 focus:outline-none resize-none min-h-[36px] max-h-[120px] placeholder-gray-400 dark:placeholder-gray-500"
              :style="{ height: textareaHeight }"
            />
            <button
              @click="send"
              :disabled="(!newMessage.trim() && selectedFiles.length === 0) || sending"
              class="shrink-0 p-2.5 transition-colors rounded-lg disabled:opacity-40"
              :class="[
                internalMode
                  ? 'text-amber-500 hover:text-amber-600 dark:text-amber-400 dark:hover:text-amber-300'
                  : 'text-emerald-600 hover:text-emerald-700 dark:text-emerald-400 dark:hover:text-emerald-300'
              ]"
              :title="sending ? 'Отправка...' : 'Отправить (Enter)'"
            >
              <svg v-if="!sending" class="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
              </svg>
              <svg v-else class="w-5 h-5 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="3" />
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
              </svg>
            </button>
          </div>
          <p class="text-[10px] text-gray-300 dark:text-gray-600 mt-1.5 text-right">Enter -- отправить, Shift+Enter -- новая строка</p>
        </div>
      </div>

      <!-- Closed conversation bar -->
      <div
        v-else-if="conversation?.status === 'Closed'"
        class="px-4 py-3 border-t border-gray-200 dark:border-gray-800 shrink-0 bg-gray-50 dark:bg-gray-800/50"
      >
        <div class="flex items-center justify-center gap-2 py-1">
          <svg class="w-4 h-4 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
          </svg>
          <p class="text-xs text-gray-400 dark:text-gray-500 font-medium">
            Чат закрыт. Переписка доступна только для чтения.
          </p>
        </div>
      </div>

      <!-- Manager not participant bar -->
      <div
        v-else-if="conversation && !participant"
        class="px-4 py-3 border-t border-gray-200 dark:border-gray-800 shrink-0 bg-gray-50 dark:bg-gray-800/50"
      >
        <div class="flex items-center justify-center gap-2 py-1">
          <svg class="w-4 h-4 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
          </svg>
          <p class="text-xs text-gray-400 dark:text-gray-500 font-medium">
            Возьмите жалобу в работу, чтобы начать переписку
          </p>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick, computed } from "vue";
import type { Conversation, ChatAttachment, ChatMessage } from "../types/Chat";
import { getConversationByContext, getMessages, sendMessage, getAttachmentTemporaryLink } from "../api/chat";
import { createChatConnection } from "../utils/signalr";
import { auth } from "../store/auth";
import type { HubConnection } from "@microsoft/signalr";
import { isImageMimeType, resolveAttachmentPreviewUrl } from "../utils/attachmentPreview";

const props = defineProps<{
  contextType: string;
  contextId: string;
  height?: string;
  complaintState?: "not-taken" | "taken" | "closed";
  refreshContext?: () => Promise<void>;
}>();

const height = computed(() => props.height || "500px");

const conversation = ref<Conversation | null>(null);
const messages = ref<ChatMessage[]>([]);
const newMessage = ref("");
const internalMode = ref(false);
const loading = ref(false);
const errorType = ref<"forbidden" | "server" | null>(null);
const loadingMore = ref(false);
const sending = ref(false);
const hasMore = ref(true);
const selectedFiles = ref<File[]>([]);
const typingUsers = ref<{ userId: string; actorType: string }[]>([]);
const textareaHeight = ref("36px");
const attachmentPreviewUrls = ref<Record<string, string>>({});

const currentUserId = computed(() => auth.getUserId());
const messagesContainer = ref<HTMLElement | null>(null);
const fileInput = ref<HTMLInputElement | null>(null);

let connection: HubConnection | null = null;
let typingTimer: ReturnType<typeof setTimeout> | null = null;
const typingTimers = new Map<string, ReturnType<typeof setTimeout>>();
let retryTimer: ReturnType<typeof setTimeout> | null = null;

const participant = computed(() =>
  conversation.value?.participants.find(
    (p) => p.userId === currentUserId.value && !p.leftAt,
  ),
);

const canWrite = computed(
  () =>
    participant.value?.canWrite === true &&
    conversation.value?.status === "Open",
);

const canSendInternal = computed(
  () => participant.value?.canSendInternal === true,
);

const complaintState = computed(() => props.complaintState || "taken");

const typingDisplay = computed(() => {
  if (typingUsers.value.length === 0) return "";
  const labels = typingUsers.value.map((u) => actorLabel(u.actorType));
  if (labels.length === 1) return `${labels[0]} печатает`;
  return `${labels.join(", ")} печатают`;
});

function actorLabel(actorType: string): string {
  const labels: Record<string, string> = {
    Client: "Клиент",
    Partner: "Партнёр",
    Manager: "Менеджер",
    Supermanager: "Супер-менеджер",
    Admin: "Админ",
    System: "Система",
  };
  return labels[actorType] || actorType;
}

function actorColor(actorType: string): string {
  const colors: Record<string, string> = {
    Client: "text-blue-500 dark:text-blue-400",
    Partner: "text-purple-500 dark:text-purple-400",
    Manager: "text-emerald-500 dark:text-emerald-400",
    Supermanager: "text-emerald-600 dark:text-emerald-300",
    Admin: "text-red-500 dark:text-red-400",
    System: "text-gray-400 dark:text-gray-500",
  };
  return colors[actorType] || "text-gray-500 dark:text-gray-400";
}

function avatarBgClass(actorType: string): string {
  const classes: Record<string, string> = {
    Client: "bg-blue-100 dark:bg-blue-900/40 text-blue-600 dark:text-blue-400",
    Partner: "bg-purple-100 dark:bg-purple-900/40 text-purple-600 dark:text-purple-400",
    Manager: "bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 dark:text-emerald-400",
    Supermanager: "bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 dark:text-emerald-400",
    Admin: "bg-red-100 dark:bg-red-900/40 text-red-600 dark:text-red-400",
    System: "bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400",
  };
  return classes[actorType] || "bg-gray-100 dark:bg-gray-800 text-gray-500 dark:text-gray-400";
}

function avatarInitials(msg: ChatMessage): string {
  const label = actorLabel(msg.senderActorType);
  if (label.length >= 2) return label.substring(0, 2).toUpperCase();
  return label.charAt(0).toUpperCase();
}

function senderName(msg: ChatMessage): string {
  const p = conversation.value?.participants.find((x) => x.userId === msg.senderUserId);
  if (p) return actorLabel(p.actorType);
  return actorLabel(msg.senderActorType);
}

function formatTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleString("ru-RU", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function formatDateSeparator(iso: string): string {
  const d = new Date(iso);
  const today = new Date();
  const yesterday = new Date();
  yesterday.setDate(yesterday.getDate() - 1);

  if (d.toDateString() === today.toDateString()) return "Сегодня";
  if (d.toDateString() === yesterday.toDateString()) return "Вчера";

  return d.toLocaleDateString("ru-RU", {
    day: "numeric",
    month: "long",
    year: d.getFullYear() !== today.getFullYear() ? "numeric" : undefined,
  });
}

function shouldShowDateSeparator(idx: number): boolean {
  if (idx === 0) return true;
  const prevMessage = messages.value[idx - 1];
  const currMessage = messages.value[idx];
  if (!prevMessage || !currMessage) return false;
  const prev = new Date(prevMessage.createdAt).toDateString();
  const curr = new Date(currMessage.createdAt).toDateString();
  return prev !== curr;
}

function openFilePicker() {
  fileInput.value?.click();
}

function onFilesSelected(e: Event) {
  const input = e.target as HTMLInputElement;
  if (input.files) {
    selectedFiles.value.push(...Array.from(input.files));
  }
  input.value = "";
}

function removeFile(idx: number) {
  selectedFiles.value.splice(idx, 1);
}

async function ensureAttachmentPreview(attachment: ChatAttachment): Promise<string | null> {
  if (!conversation.value || !isImageMimeType(attachment.mimeType)) {
    return null;
  }

  const existing = attachmentPreviewUrls.value[attachment.id];
  if (existing) {
    return existing;
  }

  try {
    const url = await getAttachmentTemporaryLink(conversation.value.id, attachment.id);
    const resolvedUrl = resolveAttachmentPreviewUrl(url);
    if (!resolvedUrl) {
      return null;
    }

    attachmentPreviewUrls.value = {
      ...attachmentPreviewUrls.value,
      [attachment.id]: resolvedUrl,
    };

    return resolvedUrl;
  } catch {
    return null;
  }
}

async function preloadAttachmentPreviews(messageList: ChatMessage[]): Promise<void> {
  await Promise.all(
    messageList.flatMap((message) => message.attachments ?? [])
      .filter((attachment) => isImageMimeType(attachment.mimeType))
      .map((attachment) => ensureAttachmentPreview(attachment)),
  );
}

async function openAttachment(attachment: ChatAttachment) {
  if (!conversation.value) {
    return;
  }

  const previewUrl = await ensureAttachmentPreview(attachment);
  if (previewUrl) {
    window.open(previewUrl, "_blank");
    return;
  }

  try {
    const url = await getAttachmentTemporaryLink(conversation.value.id, attachment.id);
    const resolvedUrl = resolveAttachmentPreviewUrl(url) ?? url;
    window.open(resolvedUrl, "_blank");
  } catch {
    // silently ignore
  }
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === "Enter" && !e.shiftKey) {
    e.preventDefault();
    send();
  }
  // Shift+Enter: default behavior (newline)
}

async function retryWithRefresh() {
  errorType.value = null;
  if (props.refreshContext) {
    try { await props.refreshContext(); } catch { /* ignore */ }
  }
  await loadConversation();
}

async function loadConversation(retryCount = 0) {
  loading.value = true;
  errorType.value = null;
  try {
    conversation.value = await getConversationByContext(
      props.contextType,
      props.contextId,
    );

    if (conversation.value) {
      const msgs = await getMessages(conversation.value.id);
      messages.value = msgs.reverse();
      void preloadAttachmentPreviews(messages.value);
      await nextTick();
      scrollToBottom();
      await connectSignalR();
    } else if (retryCount < 2 && complaintState.value === "taken") {
      retryTimer = setTimeout(() => loadConversation(retryCount + 1), 1500);
      return;
    }
  } catch (err: any) {
    const status = err?.response?.status;
    if (status === 403) {
      errorType.value = "forbidden";
    } else {
      errorType.value = "server";
    }
  }
  loading.value = false;
}

async function loadMore() {
  if (!conversation.value || !hasMore.value || loadingMore.value) return;
  const firstMsg = messages.value[0];
  if (!firstMsg) return;

  loadingMore.value = true;
  try {
    const older = await getMessages(conversation.value.id, firstMsg.id, 50);
    if (older.length < 50) hasMore.value = false;
    messages.value = [...older.reverse(), ...messages.value];
  } catch {
    // silently ignore load more errors
  }
  loadingMore.value = false;
}

function onScroll() {
  const el = messagesContainer.value;
  if (el && el.scrollTop < 50) {
    loadMore();
  }
}

function scrollToBottom() {
  const el = messagesContainer.value;
  if (el) el.scrollTop = el.scrollHeight;
}

function onTyping() {
  // Auto-resize textarea
  const textarea = document.querySelector("textarea");
  if (textarea) {
    textarea.style.height = "36px";
    const scrollH = textarea.scrollHeight;
    textareaHeight.value = Math.min(scrollH, 120) + "px";
  }

  if (!connection || !conversation.value) return;
  connection.invoke("StartTyping", conversation.value.id).catch(() => {});
  if (typingTimer) clearTimeout(typingTimer);
  typingTimer = setTimeout(() => {
    if (connection && conversation.value) {
      connection.invoke("StopTyping", conversation.value.id).catch(() => {});
    }
  }, 3000);
}

async function send() {
  if (!conversation.value || (!newMessage.value.trim() && selectedFiles.value.length === 0) || sending.value) return;
  sending.value = true;
  try {
    const files = selectedFiles.value.length > 0 ? [...selectedFiles.value] : undefined;
    const msg = await sendMessage(
      conversation.value.id,
      newMessage.value.trim(),
      internalMode.value,
      files,
    );
    if (!messages.value.find((m) => m.id === msg.id)) {
      messages.value.push(msg);
    }
    void preloadAttachmentPreviews([msg]);
    newMessage.value = "";
    selectedFiles.value = [];
    textareaHeight.value = "36px";
    await nextTick();
    scrollToBottom();
  } finally {
    sending.value = false;
  }
}

async function connectSignalR() {
  if (!conversation.value) return;
  try {
    const chatConnection = createChatConnection();
    connection = chatConnection;

    chatConnection.on("NewMessage", (msg: ChatMessage) => {
      if (
        msg.conversationId === conversation.value?.id &&
        !messages.value.find((m) => m.id === msg.id)
      ) {
        messages.value.push(msg);
        void preloadAttachmentPreviews([msg]);
        nextTick(() => scrollToBottom());
      }
    });

    chatConnection.on("ConversationClosed", () => {
      if (conversation.value) {
        conversation.value = { ...conversation.value, status: "Closed" };
      }
    });

    chatConnection.on("ConversationReopened", () => {
      if (conversation.value) {
        conversation.value = { ...conversation.value, status: "Open" };
        loadConversation();
      }
    });

    chatConnection.on("UserTyping", (data: { userId: string; actorType?: string; isTyping: boolean }) => {
      if (data.userId === currentUserId.value) return;
      const existing = typingTimers.get(data.userId);
      if (existing) clearTimeout(existing);

      if (data.isTyping) {
        if (!typingUsers.value.find((u) => u.userId === data.userId)) {
          typingUsers.value.push({
            userId: data.userId,
            actorType: data.actorType || resolveActorType(data.userId),
          });
        }
        typingTimers.set(data.userId, setTimeout(() => {
          typingUsers.value = typingUsers.value.filter((u) => u.userId !== data.userId);
          typingTimers.delete(data.userId);
        }, 5000));
      } else {
        typingUsers.value = typingUsers.value.filter((u) => u.userId !== data.userId);
        typingTimers.delete(data.userId);
      }
    });

    await chatConnection.start();
    await chatConnection.invoke("JoinConversation", conversation.value.id);
  } catch {
    startPolling();
  }
}

function resolveActorType(userId: string): string {
  const p = conversation.value?.participants.find((x) => x.userId === userId);
  return p?.actorType || "System";
}

let pollTimer: ReturnType<typeof setInterval> | null = null;

function startPolling() {
  if (pollTimer) return;
  pollTimer = setInterval(async () => {
    if (!conversation.value) return;
    try {
      const msgs = await getMessages(conversation.value.id);
      const reversed = msgs.reverse();
      if (reversed.length > 0) {
        const lastKnown = messages.value[messages.value.length - 1];
        const newMsgs = lastKnown
          ? reversed.filter(
              (m) => new Date(m.createdAt) > new Date(lastKnown.createdAt),
            )
          : reversed;
        if (newMsgs.length > 0) {
          messages.value.push(...newMsgs);
          await nextTick();
          scrollToBottom();
        }
      }
    } catch {
      // silently ignore polling errors
    }
  }, 5000);
}

onMounted(loadConversation);

onBeforeUnmount(() => {
  if (connection) {
    connection.stop();
  }
  if (pollTimer) {
    clearInterval(pollTimer);
  }
  if (typingTimer) {
    clearTimeout(typingTimer);
  }
  if (retryTimer) {
    clearTimeout(retryTimer);
  }
  for (const timer of typingTimers.values()) {
    clearTimeout(timer);
  }
});
</script>
