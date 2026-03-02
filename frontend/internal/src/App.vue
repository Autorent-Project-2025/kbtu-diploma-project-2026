<template>
  <div>
    <header class="card" style="border-radius: 0; border-left: 0; border-right: 0">
      <div
        class="container"
        style="padding-top: 4px; padding-bottom: 4px; display: flex; align-items: center; justify-content: space-between"
      >
        <strong>AutoRent Internal</strong>
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
