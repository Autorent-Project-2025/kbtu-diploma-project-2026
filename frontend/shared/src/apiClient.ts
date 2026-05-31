import axios, {
  type AxiosError,
  type AxiosInstance,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from "axios";

export interface RefreshableAuthStore {
  tryRefresh(): Promise<boolean>;
  logout(): void;
}

export interface ApiClientOptions {
  baseURL: string;
  auth: RefreshableAuthStore;
  camelCaseResponse?: boolean;
  forbiddenRedirectPath?: string;
  loginPath?: string;
}

type RetriableRequestConfig = InternalAxiosRequestConfig & {
  _retry?: boolean;
};

export function normalizeApiBaseUrl(rawApiUrl: string): string {
  return rawApiUrl.replace(/\/+$/, "").replace(/\/api$/i, "");
}

export function toCamelCase(value: unknown): unknown {
  if (Array.isArray(value)) {
    return value.map((item) => toCamelCase(item));
  }

  if (value !== null && typeof value === "object") {
    return Object.keys(value).reduce<Record<string, unknown>>((acc, key) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      acc[camelKey] = toCamelCase((value as Record<string, unknown>)[key]);
      return acc;
    }, {});
  }

  return value;
}

function isLoginRequest(url: string | undefined, loginPath: string): boolean {
  return !!url && url.includes(loginPath);
}

export function createApiClient(options: ApiClientOptions): AxiosInstance {
  const api = axios.create({
    baseURL: options.baseURL,
  });

  api.interceptors.request.use((requestConfig) => {
    const token = localStorage.getItem("token");
    if (token) {
      requestConfig.headers.Authorization = `Bearer ${token}`;
    }

    return requestConfig;
  });

  let isRefreshing = false;
  let failedQueue: Array<{
    resolve: (value: unknown) => void;
    reject: (reason?: unknown) => void;
  }> = [];

  function processQueue(error: unknown, token: string | null = null) {
    failedQueue.forEach(({ resolve, reject }) => {
      if (error) reject(error);
      else resolve(token);
    });
    failedQueue = [];
  }

  api.interceptors.response.use(
    (response: AxiosResponse) => {
      if (options.camelCaseResponse && response.data) {
        response.data = toCamelCase(response.data);
      }

      return response;
    },
    async (error: AxiosError) => {
      const originalRequest = error.config as
        | RetriableRequestConfig
        | undefined;
      const status = error.response?.status;
      const loginPath = options.loginPath ?? "/identity/auth/login";

      if (status === 403 && options.forbiddenRedirectPath) {
        window.location.href = options.forbiddenRedirectPath;
        return Promise.reject(error);
      }

      if (status === 401 && isLoginRequest(originalRequest?.url, loginPath)) {
        return Promise.reject(error);
      }

      if (status === 401 && originalRequest && !originalRequest._retry) {
        if (isRefreshing) {
          return new Promise((resolve, reject) => {
            failedQueue.push({ resolve, reject });
          }).then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          });
        }

        originalRequest._retry = true;
        isRefreshing = true;

        try {
          const refreshed = await options.auth.tryRefresh();
          if (refreshed) {
            const newToken = localStorage.getItem("token");
            processQueue(null, newToken);
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return api(originalRequest);
          }

          processQueue(error, null);
          window.location.href = "/login";
          return Promise.reject(error);
        } catch (refreshError) {
          processQueue(refreshError, null);
          options.auth.logout();
          window.location.href = "/login";
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      return Promise.reject(error);
    },
  );

  return api;
}
