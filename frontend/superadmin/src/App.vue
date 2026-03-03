<template>
  <div class="app-shell">
    <header class="topbar">
      <div class="container topbar-inner">
        <div class="brand">
          <span class="brand-mark">AR</span>
          <div>
            <div class="brand-title">AutoRent Superadmin</div>
            <div class="brand-subtitle">Управление пользователями и access control</div>
          </div>
        </div>
        <button v-if="isAuthenticated" class="btn btn-outline" @click="logout">Выйти</button>
      </div>
    </header>
    <router-view />
  </div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";
import { auth } from "./store/auth";

const router = useRouter();
const isAuthenticated = computed(() => Boolean(auth.token));

function logout() {
  auth.logout();
  router.push("/login");
}
</script>
