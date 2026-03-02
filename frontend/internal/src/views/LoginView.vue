<template>
  <div class="container">
    <div class="card" style="max-width: 420px; margin: 72px auto 0">
      <h1 style="margin-top: 0">Вход менеджера</h1>
      <p style="color: #6b7280; margin-top: -6px">Доступ только для ролей с правом Ticket.View</p>

      <div v-if="errorMessage" class="error" style="margin-bottom: 12px">{{ errorMessage }}</div>

      <form @submit.prevent="onSubmit">
        <div style="margin-bottom: 12px">
          <label class="label" for="email">Email</label>
          <input id="email" v-model="email" class="input" type="email" required />
        </div>

        <div style="margin-bottom: 16px">
          <label class="label" for="password">Пароль</label>
          <input id="password" v-model="password" class="input" type="password" required />
        </div>

        <button class="btn btn-primary" style="width: 100%" :disabled="loading">
          {{ loading ? "Вход..." : "Войти" }}
        </button>
      </form>
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
