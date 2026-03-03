<template>
  <div class="container auth-page">
    <section class="auth-card">
      <div class="auth-card__header">
        <span class="auth-eyebrow">Manager Access</span>
        <h1>Вход менеджера</h1>
        <p>Панель управления заявками на регистрацию в AutoRent.</p>
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
    </section>
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
      errorMessage.value = "Недостаточно прав для manager panel.";
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
