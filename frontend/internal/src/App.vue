<template>
  <div v-if="showWorkspace" class="workspace-shell">
    <aside class="workspace-sidebar">
      <div class="workspace-sidebar__header">
        <strong>AutoRent</strong>
        <p>Панель заявок</p>
      </div>

      <nav class="workspace-nav" aria-label="Основная навигация">
        <router-link class="workspace-nav__link" exact-active-class="is-active" to="/tickets">Заявки</router-link>
      </nav>

      <div class="workspace-sidebar__footer">
        <button class="btn btn-secondary workspace-sidebar__logout" @click="logout">Выйти</button>
      </div>
    </aside>

    <main class="workspace-main">
      <router-view />
    </main>
  </div>

  <div v-else class="auth-shell">
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { auth } from "./store/auth";

const route = useRoute();
const router = useRouter();
const isAuthenticated = computed(() => Boolean(auth.token));
const showWorkspace = computed(() => isAuthenticated.value && route.path !== "/login");

function logout() {
  auth.logout();
  router.push("/login");
}
</script>
