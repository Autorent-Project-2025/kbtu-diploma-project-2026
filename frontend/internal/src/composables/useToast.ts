import { reactive } from "vue";
import type { Toast, ToastType } from "../types/Toast";

interface ToastState {
  toasts: Toast[];
}

const state = reactive<ToastState>({
  toasts: [],
});

let toastId = 0;

export function useToast() {
  const addToast = (
    message: string,
    type: ToastType = "info",
    duration = 3000
  ) => {
    const id = `toast-${toastId++}`;
    const toast: Toast = {
      id,
      message,
      type,
      duration,
    };

    state.toasts.push(toast);

    // Автоматически удаляем через duration
    if (duration > 0) {
      setTimeout(() => {
        removeToast(id);
      }, duration);
    }

    return id;
  };

  const removeToast = (id: string) => {
    const index = state.toasts.findIndex((t) => t.id === id);
    if (index > -1) {
      state.toasts.splice(index, 1);
    }
  };

  const success = (message: string, duration = 3000) => {
    return addToast(message, "success", duration);
  };

  const error = (message: string, duration = 4000) => {
    return addToast(message, "error", duration);
  };

  const warning = (message: string, duration = 3500) => {
    return addToast(message, "warning", duration);
  };

  const info = (message: string, duration = 3000) => {
    return addToast(message, "info", duration);
  };

  return {
    toasts: state.toasts,
    addToast,
    removeToast,
    success,
    error,
    warning,
    info,
  };
}