import { computed } from "vue";

export function useAuth() {
  // Проверяем есть ли токен в localStorage
  const isAuthenticated = computed(() => {
    const token = localStorage.getItem("token");
    return !!token;
  });

  // Получить текущего пользователя из localStorage
  const currentUser = computed(() => {
    const userJson = localStorage.getItem("user");
    if (!userJson) return null;

    try {
      return JSON.parse(userJson);
    } catch (e) {
      console.error("Error parsing user from localStorage:", e);
      return null;
    }
  });

  // Получить userId
  const userId = computed(() => {
    return currentUser.value?.id || null;
  });

  // Получить userName
  const userName = computed(() => {
    return currentUser.value?.name || null;
  });

  return {
    isAuthenticated,
    currentUser,
    userId,
    userName,
  };
}
