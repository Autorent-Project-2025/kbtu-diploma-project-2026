import { reactive } from "vue";
import { login as apiLogin } from "../api/auth";

interface TokenData {
  token: string;
  timestamp: number;
}

export const auth = reactive({
  token: localStorage.getItem("token") || "",
  user: null as any,

  async login(email: string, password: string) {
    const token = await apiLogin(email, password);
    this.token = token;

    // Сохраняем токен с временной меткой
    const tokenData: TokenData = {
      token,
      timestamp: Date.now(),
    };

    localStorage.setItem("token", token);
    localStorage.setItem("tokenTimestamp", tokenData.timestamp.toString());
  },

  logout() {
    this.token = "";
    localStorage.removeItem("token");
    localStorage.removeItem("tokenTimestamp");
    localStorage.removeItem("user");
  },

  //  Проверяет, истек ли токен на основе времени хранения из .env
  isTokenExpired(): boolean {
    const token = localStorage.getItem("token");
    const timestampStr = localStorage.getItem("tokenTimestamp");

    if (!token || !timestampStr) {
      return true;
    }

    const timestamp = parseInt(timestampStr, 10);
    if (isNaN(timestamp)) {
      return true;
    }

    // Получаем срок действия токена из .env (по умолчанию 3 часа)
    const expiryHours = parseInt(
      import.meta.env.VITE_TOKEN_EXPIRY_HOURS || "3",
      10
    );
    const expiryMs = expiryHours * 60 * 60 * 1000;

    const now = Date.now();
    const elapsed = now - timestamp;

    return elapsed >= expiryMs;
  },

  // Проверяет токен и автоматически разлогинивает, если истек
  checkTokenValidity(): boolean {
    if (this.isTokenExpired()) {
      this.logout();
      return false;
    }
    return true;
  },
});
