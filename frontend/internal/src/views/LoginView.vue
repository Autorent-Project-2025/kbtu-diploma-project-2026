<template>
  <div class="auth-page">
    <div class="auth-layout">
      <section class="auth-panel auth-panel--summary">
        <h1>Панель заявок AutoRent</h1>
        <p>Внутренний кабинет для проверки новых регистраций и связанных документов.</p>

        <dl class="auth-summary">
          <div class="auth-summary__item">
            <dt>Одна очередь</dt>
            <dd>Новые регистрации клиентов, партнёров и автомобилей собраны в одном рабочем списке.</dd>
          </div>
          <div class="auth-summary__item">
            <dt>Документы под рукой</dt>
            <dd>Паспорт, водительские права и файлы по автомобилю открываются прямо из карточки заявки.</dd>
          </div>
          <div class="auth-summary__item">
            <dt>Быстрое решение</dt>
            <dd>После проверки данные можно сразу подтвердить или вернуть с причиной отказа.</dd>
          </div>
        </dl>
      </section>

      <section class="auth-panel auth-panel--form">
        <div class="auth-panel__header">
          <h2>Вход в кабинет</h2>
          <p>Используйте рабочую учётную запись, чтобы открыть очередь заявок.</p>
        </div>

        <div v-if="errorMessage" class="error auth-alert">{{ errorMessage }}</div>

        <form class="auth-form" @submit.prevent="onSubmit">
          <div>
            <label class="label" for="email">Email</label>
            <input id="email" v-model="email" class="input" type="email" required autocomplete="username" />
          </div>

          <div>
            <label class="label" for="password">Пароль</label>
            <input
              id="password"
              v-model="password"
              class="input"
              type="password"
              required
              autocomplete="current-password"
            />
          </div>

          <button class="btn btn-primary auth-submit" :disabled="loading">
            {{ loading ? "Вход..." : "Войти" }}
          </button>
        </form>

        <p class="auth-footnote">Если вход не проходит, проверьте рабочие данные или обратитесь к администратору.</p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { auth } from "../store/auth";

const router = useRouter();
const email = ref("");
const password = ref("");
const loading = ref(false);
const errorMessage = ref("");

async function onSubmit() {
  if (loading.value) return;

  loading.value = true;
  errorMessage.value = "";

  try {
    await auth.login(email.value, password.value);

    if (!auth.hasPermission("Ticket.View")) {
      auth.logout();
      errorMessage.value = "Недостаточно прав для панели менеджера.";
      return;
    }

    router.push("/tickets");
  } catch {
    errorMessage.value = "Ошибка входа. Проверьте email и пароль.";
  } finally {
    loading.value = false;
  }
}
</script>
