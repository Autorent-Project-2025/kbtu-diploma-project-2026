<template>
  <div class="rounded-2xl border border-gray-200 dark:border-gray-800 bg-white dark:bg-gray-900 shadow-xl flex flex-col overflow-hidden" :style="{ height: height }">

    <!-- Header -->
    <div class="px-5 py-3 border-b border-gray-200 dark:border-gray-800 flex items-center gap-2.5 shrink-0 bg-white dark:bg-gray-900">
      <div class="w-8 h-8 rounded-full bg-blue-100 dark:bg-blue-900/40 flex items-center justify-center shrink-0">
        <svg class="w-4 h-4 text-blue-600 dark:text-blue-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
        </svg>
      </div>
      <div class="flex-1 min-w-0">
        <p class="text-sm font-bold text-gray-900 dark:text-white leading-tight">Чат по обращению</p>
        <p v-if="conversation?.status === 'Open'" class="text-[11px] text-green-600 dark:text-green-400 leading-tight">Активен</p>
      </div>
      <span
        v-if="conversation?.status === 'Closed'"
        class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-wide bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400 border border-gray-200 dark:border-gray-700"
      >Закрыт</span>
    </div>

    <!-- Loading state -->
    <div v-if="loading" class="flex-1 flex items-center justify-center">
      <div class="text-center space-y-3">
        <svg class="w-8 h-8 text-blue-300 dark:text-blue-700 animate-spin mx-auto" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
        <p class="text-xs text-gray-400 dark:text-gray-500">Загрузка чата...</p>
      </div>
    </div>

    <!-- Error state: 403 No Access -->
    <div v-else-if="errorType === 'forbidden'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3">
        <div class="w-14 h-14 rounded-full bg-red-50 dark:bg-red-900/20 flex items-center justify-center mx-auto">
          <svg class="w-7 h-7 text-red-400 dark:text-red-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M18.364 18.364A9 9 0 005.636 5.636m12.728 12.728A9 9 0 015.636 5.636m12.728 12.728L5.636 5.636" />
          </svg>
        </div>
        <p class="text-sm font-semibold text-gray-800 dark:text-gray-200">Нет доступа к переписке</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 max-w-[240px] mx-auto">У вас нет прав для просмотра данной переписки. Обратитесь в поддержку, если считаете, что это ошибка.</p>
      </div>
    </div>

    <!-- Error state: 500 Server Error -->
    <div v-else-if="errorType === 'server'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3">
        <div class="w-14 h-14 rounded-full bg-orange-50 dark:bg-orange-900/20 flex items-center justify-center mx-auto">
          <svg class="w-7 h-7 text-orange-400 dark:text-orange-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
          </svg>
        </div>
        <p class="text-sm font-semibold text-gray-800 dark:text-gray-200">Не удалось загрузить чат</p>
        <p class="text-xs text-gray-400 dark:text-gray-500">Произошла ошибка на сервере. Попробуйте ещё раз.</p>
        <button
          @click="retryWithRefresh"
          class="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg text-xs font-semibold text-white bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 transition-colors shadow-sm"
        >
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Повторить
        </button>
      </div>
    </div>

    <!-- No conversation state (404) -->
    <div v-else-if="errorType === 'not_found'" class="flex-1 flex items-center justify-center px-8">
      <div class="text-center space-y-3">
        <div class="w-16 h-16 rounded-full bg-blue-50 dark:bg-blue-900/20 flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-blue-300 dark:text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <p class="text-sm font-semibold text-gray-800 dark:text-gray-200">Ожидайте ответа</p>
        <p class="text-xs text-gray-400 dark:text-gray-500 max-w-[260px] mx-auto leading-relaxed">Менеджер свяжется с вами после рассмотрения обращения. Чат появится здесь автоматически.</p>
        <button
          @click="retryWithRefresh"
          class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold text-blue-600 dark:text-blue-400 border border-blue-200 dark:border-blue-800 hover:bg-blue-50 dark:hover:bg-blue-900/20 transition-colors"
        >
          <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Обновить
        </button>
      </div>
    </div>

    <!-- Messages area -->
    <template v-else>
      <div ref="messagesContainer" class="flex-1 min-h-0 overflow-y-auto px-4 py-4" @scroll="onScroll">

        <!-- Loading older messages -->
        <div v-if="loadingMore" class="flex items-center justify-center py-3">
          <svg class="w-4 h-4 text-gray-300 dark:text-gray-600 animate-spin" fill="none" viewBox="0 0 24 24">
            <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
            <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
        </div>

        <!-- Empty conversation -->
        <div v-if="messages.length === 0 && !loadingMore" class="flex flex-col items-center justify-center h-full text-center space-y-3 py-8">
          <div class="w-14 h-14 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
            <svg class="w-7 h-7 text-gray-300 dark:text-gray-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
            </svg>
          </div>
          <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Переписка пока пуста</p>
          <p class="text-xs text-gray-400 dark:text-gray-500">Менеджер свяжется с вами здесь.</p>
        </div>

        <!-- Messages list -->
        <template v-for="(msg, idx) in messages" :key="msg.id">

          <!-- Date separator -->
          <div
            v-if="shouldShowDateSeparator(idx)"
            class="flex items-center gap-3 my-4"
          >
            <div class="flex-1 h-px bg-gray-200 dark:bg-gray-700"></div>
            <span class="text-[10px] font-medium text-gray-400 dark:text-gray-500 uppercase tracking-wider shrink-0">{{ formatDateSeparator(msg.createdAt) }}</span>
            <div class="flex-1 h-px bg-gray-200 dark:bg-gray-700"></div>
          </div>

          <!-- System message -->
          <div
            v-if="msg.messageType === 'System'"
            class="flex items-center gap-2 my-3"
          >
            <div class="flex-1 h-px bg-gray-100 dark:bg-gray-800"></div>
            <p class="text-[11px] text-gray-400 dark:text-gray-500 shrink-0 px-2">{{ msg.body }}</p>
            <div class="flex-1 h-px bg-gray-100 dark:bg-gray-800"></div>
          </div>

          <!-- User / Manager message -->
          <div
            v-else
            :class="[
              'flex gap-2.5 mb-1',
              msg.senderUserId === currentUserId ? 'flex-row-reverse' : 'flex-row',
              !isSameSenderAsPrevious(idx) ? 'mt-4' : 'mt-0.5'
            ]"
          >
            <!-- Avatar -->
            <div
              v-if="!isSameSenderAsPrevious(idx)"
              :class="[
                'w-8 h-8 rounded-full flex items-center justify-center shrink-0 text-xs font-bold',
                msg.senderUserId === currentUserId
                  ? 'bg-blue-100 dark:bg-blue-900/40 text-blue-600 dark:text-blue-400'
                  : 'bg-gray-200 dark:bg-gray-700 text-gray-500 dark:text-gray-400'
              ]"
            >
              {{ msg.senderUserId === currentUserId ? 'Вы' : avatarInitials(msg.senderActorType) }}
            </div>
            <!-- Spacer when grouped -->
            <div v-else class="w-8 shrink-0"></div>

            <!-- Bubble -->
            <div class="max-w-[75%] min-w-0">
              <!-- Sender label (only for first in group) -->
              <div
                v-if="!isSameSenderAsPrevious(idx)"
                :class="[
                  'text-[11px] font-semibold mb-0.5 px-1',
                  msg.senderUserId === currentUserId ? 'text-right text-blue-600 dark:text-blue-400' : 'text-left text-gray-500 dark:text-gray-400'
                ]"
              >
                {{ msg.senderUserId === currentUserId ? 'Вы' : actorLabel(msg.senderActorType) }}
              </div>

              <div
                :class="[
                  'rounded-2xl px-3.5 py-2.5 text-sm leading-relaxed',
                  msg.senderUserId === currentUserId
                    ? 'bg-blue-600 dark:bg-blue-600 text-white rounded-tr-md'
                    : 'bg-gray-100 dark:bg-gray-800 text-gray-900 dark:text-gray-100 rounded-tl-md',
                  isSameSenderAsPrevious(idx) && msg.senderUserId === currentUserId ? 'rounded-tr-2xl' : '',
                  isSameSenderAsPrevious(idx) && msg.senderUserId !== currentUserId ? 'rounded-tl-2xl' : ''
                ]"
              >
                <p v-if="msg.body" class="whitespace-pre-wrap break-words">{{ msg.body }}</p>

                <!-- Attachments -->
                <div v-if="msg.attachments?.length" :class="['space-y-1', msg.body ? 'mt-2' : '']">
                  <template v-for="att in msg.attachments" :key="att.id">
                    <button
                      v-if="isImageMimeType(att.mimeType)"
                      type="button"
                      @click="openAttachment(att)"
                      :class="[
                        'block w-full overflow-hidden rounded-xl border transition-colors text-left',
                        msg.senderUserId === currentUserId
                          ? 'border-blue-400/30 bg-blue-500/20 hover:bg-blue-500/30'
                          : 'border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600'
                      ]"
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
                        class="h-32 flex items-center justify-center text-xs font-medium"
                        :class="msg.senderUserId === currentUserId ? 'text-blue-100' : 'text-gray-400 dark:text-gray-500'"
                      >
                        Загрузка изображения...
                      </div>
                      <div
                        :class="[
                          'flex items-center gap-2 px-2.5 py-2 text-xs',
                          msg.senderUserId === currentUserId ? 'text-white' : 'text-gray-700 dark:text-gray-200'
                        ]"
                      >
                        <svg class="w-3.5 h-3.5 shrink-0 opacity-70" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m2.25 15 5.159-5.159a2.25 2.25 0 0 1 3.182 0L15 14.25m-1.5-1.5 1.659-1.659a2.25 2.25 0 0 1 3.182 0L21.75 14.5M3.75 19.5h16.5A1.5 1.5 0 0 0 21.75 18V6A1.5 1.5 0 0 0 20.25 4.5H3.75A1.5 1.5 0 0 0 2.25 6v12A1.5 1.5 0 0 0 3.75 19.5Z" /></svg>
                        <span class="truncate">{{ att.originalFileName }}</span>
                      </div>
                    </button>
                    <button
                      v-else
                      type="button"
                      @click="openAttachment(att)"
                      :class="[
                        'flex items-center gap-2 text-xs px-2.5 py-1.5 rounded-lg transition-colors cursor-pointer w-full text-left',
                        msg.senderUserId === currentUserId
                          ? 'bg-blue-500/30 hover:bg-blue-500/40 text-white'
                          : 'bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 border border-gray-200 dark:border-gray-600 text-gray-700 dark:text-gray-200'
                      ]"
                    >
                      <svg class="w-3.5 h-3.5 shrink-0 opacity-70" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
                      <span class="truncate">{{ att.originalFileName }}</span>
                    </button>
                  </template>
                </div>
              </div>

              <!-- Timestamp -->
              <div
                :class="[
                  'text-[10px] text-gray-400 dark:text-gray-500 mt-0.5 px-1',
                  msg.senderUserId === currentUserId ? 'text-right' : 'text-left'
                ]"
              >
                {{ formatTime(msg.createdAt) }}
              </div>
            </div>
          </div>
        </template>

        <!-- Typing indicator -->
        <div v-if="typingUsers.length > 0" class="flex items-center gap-2.5 mt-4 mb-1">
          <div class="w-8 h-8 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center shrink-0">
            <svg class="w-4 h-4 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 12h.01M12 12h.01M16 12h.01" />
            </svg>
          </div>
          <div class="bg-gray-100 dark:bg-gray-800 rounded-2xl rounded-tl-md px-4 py-2.5">
            <div class="flex items-center gap-1">
              <span class="text-xs text-gray-500 dark:text-gray-400">Менеджер печатает</span>
              <span class="flex gap-0.5 items-end h-4">
                <span class="w-1 h-1 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce" style="animation-delay: 0ms; animation-duration: 1.2s;"></span>
                <span class="w-1 h-1 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce" style="animation-delay: 200ms; animation-duration: 1.2s;"></span>
                <span class="w-1 h-1 bg-gray-400 dark:bg-gray-500 rounded-full animate-bounce" style="animation-delay: 400ms; animation-duration: 1.2s;"></span>
              </span>
            </div>
          </div>
        </div>
      </div>

      <!-- Closed banner -->
      <div
        v-if="conversation?.status === 'Closed'"
        class="px-4 py-2.5 border-t border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 shrink-0"
      >
        <div class="flex items-center justify-center gap-2">
          <svg class="w-4 h-4 text-gray-400 dark:text-gray-500 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M16.5 10.5V6.75a4.5 4.5 0 10-9 0v3.75m-.75 11.25h10.5a2.25 2.25 0 002.25-2.25v-6.75a2.25 2.25 0 00-2.25-2.25H6.75a2.25 2.25 0 00-2.25 2.25v6.75a2.25 2.25 0 002.25 2.25z" />
          </svg>
          <p class="text-xs text-gray-500 dark:text-gray-400">Обращение закрыто. Переписка доступна только для чтения.</p>
        </div>
      </div>

      <!-- Waiting for manager to join -->
      <div
        v-else-if="conversation && !participant"
        class="px-4 py-2.5 border-t border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/50 shrink-0"
      >
        <p class="text-xs text-gray-400 dark:text-gray-500 text-center">Ожидайте -- менеджер подключится к чату после рассмотрения обращения</p>
      </div>

      <!-- Composer -->
      <div v-else-if="canWrite" class="px-3 py-3 border-t border-gray-200 dark:border-gray-800 shrink-0 bg-white dark:bg-gray-900">
        <!-- File chips -->
        <div v-if="selectedFiles.length > 0" class="flex flex-wrap gap-1.5 mb-2 px-1">
          <div
            v-for="(file, idx) in selectedFiles"
            :key="idx"
            class="flex items-center gap-1.5 text-xs px-2.5 py-1 rounded-full bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 text-blue-700 dark:text-blue-300"
          >
            <svg class="w-3 h-3 shrink-0 opacity-60" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
            <span class="truncate max-w-[120px]">{{ file.name }}</span>
            <span class="text-[10px] opacity-60">{{ formatFileSize(file.size) }}</span>
            <button @click="removeFile(idx)" class="text-blue-400 hover:text-red-500 dark:hover:text-red-400 transition-colors ml-0.5">
              <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
            </button>
          </div>
        </div>

        <!-- Input row -->
        <div class="flex items-end gap-2">
          <button
            @click="openFilePicker"
            class="shrink-0 p-2 rounded-full text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            title="Прикрепить файл"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.172 7l-6.586 6.586a2 2 0 102.828 2.828l6.414-6.586a4 4 0 00-5.656-5.656l-6.415 6.585a6 6 0 108.486 8.486L20.5 13" /></svg>
          </button>
          <input ref="fileInput" type="file" multiple class="hidden" @change="onFilesSelected" />
          <textarea
            v-model="newMessage"
            @keydown.enter.exact.prevent="send"
            @input="onTyping"
            rows="1"
            :placeholder="composerPlaceholder"
            class="flex-1 rounded-2xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 text-sm text-gray-900 dark:text-white px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-blue-500 dark:focus:ring-blue-400 focus:border-transparent resize-none max-h-32 leading-relaxed"
          />
          <button
            @click="send"
            :disabled="(!newMessage.trim() && selectedFiles.length === 0) || sending"
            :class="[
              'shrink-0 w-9 h-9 rounded-full flex items-center justify-center transition-all',
              (!newMessage.trim() && selectedFiles.length === 0) || sending
                ? 'bg-gray-100 dark:bg-gray-800 text-gray-300 dark:text-gray-600 cursor-not-allowed'
                : 'bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-white shadow-sm'
            ]"
          >
            <svg v-if="!sending" class="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 12L3.269 3.126A59.768 59.768 0 0121.485 12 59.77 59.77 0 013.27 20.876L5.999 12zm0 0h7.5" />
            </svg>
            <svg v-else class="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
          </button>
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
  refreshContext?: () => Promise<void>;
}>();

const height = computed(() => props.height || "400px");

const conversation = ref<Conversation | null>(null);
const messages = ref<ChatMessage[]>([]);
const newMessage = ref("");
const loading = ref(false);
const errorType = ref<"not_found" | "forbidden" | "server" | null>(null);
const loadingMore = ref(false);
const sending = ref(false);
const hasMore = ref(true);
const selectedFiles = ref<File[]>([]);
const typingUsers = ref<string[]>([]);
const attachmentPreviewUrls = ref<Record<string, string>>({});

const currentUserId = computed(() => auth.getUserId() || "");
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

const composerPlaceholder = computed(() => {
  if (conversation.value?.status === "Closed") return "Чат закрыт";
  return "Напишите сообщение...";
});

function actorLabel(actorType: string): string {
  const labels: Record<string, string> = {
    Manager: "Менеджер",
    Supermanager: "Супер-менеджер",
    Admin: "Админ",
    System: "Система",
  };
  return labels[actorType] || "Менеджер";
}

function avatarInitials(actorType: string): string {
  const initials: Record<string, string> = {
    Manager: "М",
    Supermanager: "СМ",
    Admin: "А",
    System: "С",
  };
  return initials[actorType] || "М";
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
  const prev = messages.value[idx - 1];
  const curr = messages.value[idx];
  if (!prev || !curr) return false;
  const prevDate = new Date(prev.createdAt).toDateString();
  const currDate = new Date(curr.createdAt).toDateString();
  return prevDate !== currDate;
}

function isSameSenderAsPrevious(idx: number): boolean {
  if (idx === 0) return false;
  const prev = messages.value[idx - 1];
  const curr = messages.value[idx];
  if (!prev || !curr) return false;
  if (prev.messageType === "System" || curr.messageType === "System") return false;
  if (prev.senderUserId !== curr.senderUserId) return false;
  // Also break grouping if date separator is shown
  const prevDate = new Date(prev.createdAt).toDateString();
  const currDate = new Date(curr.createdAt).toDateString();
  if (prevDate !== currDate) return false;
  // Break grouping if more than 5 minutes apart
  const diff = new Date(curr.createdAt).getTime() - new Date(prev.createdAt).getTime();
  return diff < 5 * 60 * 1000;
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return bytes + " Б";
  if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(0) + " КБ";
  return (bytes / (1024 * 1024)).toFixed(1) + " МБ";
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
    } else if (retryCount < 3) {
      retryTimer = setTimeout(() => loadConversation(retryCount + 1), 1500);
      return;
    } else {
      errorType.value = "not_found";
    }
  } catch (err: any) {
    const status = err?.response?.status;
    if (status === 404) {
      errorType.value = "not_found";
    } else if (status === 403) {
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
    // ignore load more errors
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
      files,
    );
    if (!messages.value.find((m) => m.id === msg.id)) {
      messages.value.push(msg);
    }
    void preloadAttachmentPreviews([msg]);
    newMessage.value = "";
    selectedFiles.value = [];
    await nextTick();
    scrollToBottom();
  } finally {
    sending.value = false;
  }
}

async function connectSignalR() {
  if (!conversation.value) return;
  try {
    connection = createChatConnection();

    connection.on("NewMessage", (msg: ChatMessage) => {
      if (
        msg.conversationId === conversation.value?.id &&
        !messages.value.find((m) => m.id === msg.id)
      ) {
        messages.value.push(msg);
        void preloadAttachmentPreviews([msg]);
        nextTick(() => scrollToBottom());
      }
    });

    connection.on("ConversationClosed", () => {
      if (conversation.value) {
        conversation.value = { ...conversation.value, status: "Closed" };
      }
    });

    connection.on("ConversationReopened", () => {
      if (conversation.value) {
        conversation.value = { ...conversation.value, status: "Open" };
        loadConversation();
      }
    });

    connection.on("UserTyping", (data: { userId: string; isTyping: boolean }) => {
      if (data.userId === currentUserId.value) return;
      const existing = typingTimers.get(data.userId);
      if (existing) clearTimeout(existing);

      if (data.isTyping) {
        if (!typingUsers.value.includes(data.userId)) {
          typingUsers.value.push(data.userId);
        }
        typingTimers.set(data.userId, setTimeout(() => {
          typingUsers.value = typingUsers.value.filter(u => u !== data.userId);
          typingTimers.delete(data.userId);
        }, 5000));
      } else {
        typingUsers.value = typingUsers.value.filter(u => u !== data.userId);
        typingTimers.delete(data.userId);
      }
    });

    await connection.start();
    await connection.invoke("JoinConversation", conversation.value.id);
  } catch {
    startPolling();
  }
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
