import { computed, defineComponent, h, onMounted, onUnmounted, ref } from "vue";
import type { ToastType } from "./types";

function getToneStyle(type: ToastType) {
  switch (type) {
    case "success":
      return {
        backgroundColor: "#f0fdf4",
        borderColor: "#22c55e",
        color: "#166534",
      };
    case "error":
      return {
        backgroundColor: "#fef2f2",
        borderColor: "#ef4444",
        color: "#991b1b",
      };
    case "warning":
      return {
        backgroundColor: "#fefce8",
        borderColor: "#eab308",
        color: "#854d0e",
      };
    case "info":
    default:
      return {
        backgroundColor: "#eff6ff",
        borderColor: "#3b82f6",
        color: "#1e40af",
      };
  }
}

function renderIcon(type: ToastType) {
  switch (type) {
    case "success":
      return h(
        "svg",
        {
          class: "w-5 h-5 text-green-500 dark:text-green-400",
          fill: "currentColor",
          viewBox: "0 0 20 20",
        },
        [
          h("path", {
            "fill-rule": "evenodd",
            d: "M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z",
            "clip-rule": "evenodd",
          }),
        ],
      );

    case "error":
      return h(
        "svg",
        {
          class: "w-5 h-5 text-red-500 dark:text-red-400",
          fill: "currentColor",
          viewBox: "0 0 20 20",
        },
        [
          h("path", {
            "fill-rule": "evenodd",
            d: "M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z",
            "clip-rule": "evenodd",
          }),
        ],
      );

    case "warning":
      return h(
        "svg",
        {
          class: "w-5 h-5 text-yellow-500 dark:text-yellow-400",
          fill: "currentColor",
          viewBox: "0 0 20 20",
        },
        [
          h("path", {
            "fill-rule": "evenodd",
            d: "M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z",
            "clip-rule": "evenodd",
          }),
        ],
      );

    case "info":
    default:
      return h(
        "svg",
        {
          class: "w-5 h-5 text-blue-500 dark:text-blue-400",
          fill: "currentColor",
          viewBox: "0 0 20 20",
        },
        [
          h("path", {
            "fill-rule": "evenodd",
            d: "M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z",
            "clip-rule": "evenodd",
          }),
        ],
      );
  }
}

export default defineComponent({
  name: "SharedToastItem",
  props: {
    id: {
      type: String,
      required: true,
    },
    message: {
      type: String,
      required: true,
    },
    type: {
      type: String as () => ToastType,
      required: true,
    },
    duration: {
      type: Number,
      default: 3000,
    },
  },
  emits: {
    close: (_id: string) => true,
  },
  setup(props, { emit }) {
    const visible = ref(false);
    let closeTimer: ReturnType<typeof setTimeout> | null = null;
    let removeTimer: ReturnType<typeof setTimeout> | null = null;

    function close() {
      if (!visible.value) return;
      if (closeTimer) {
        clearTimeout(closeTimer);
        closeTimer = null;
      }

      visible.value = false;
      removeTimer = setTimeout(() => {
        emit("close", props.id);
      }, 200);
    }

    onMounted(() => {
      setTimeout(() => {
        visible.value = true;
      }, 10);

      if (props.duration > 0) {
        closeTimer = setTimeout(close, props.duration);
      }
    });

    onUnmounted(() => {
      if (closeTimer) clearTimeout(closeTimer);
      if (removeTimer) clearTimeout(removeTimer);
    });

    const toastClasses = computed(() => {
      const baseClasses = "border-l-4";

      switch (props.type) {
        case "success":
          return `${baseClasses} bg-green-50 dark:bg-green-900/30 border-green-500 text-green-800 dark:text-green-200`;
        case "error":
          return `${baseClasses} bg-red-50 dark:bg-red-900/30 border-red-500 text-red-800 dark:text-red-200`;
        case "warning":
          return `${baseClasses} bg-yellow-50 dark:bg-yellow-900/30 border-yellow-500 text-yellow-800 dark:text-yellow-200`;
        case "info":
        default:
          return `${baseClasses} bg-blue-50 dark:bg-blue-900/30 border-blue-500 text-blue-800 dark:text-blue-200`;
      }
    });

    const toastStyle = computed(() => {
      const toneStyle = getToneStyle(props.type);

      return {
        alignItems: "center",
        backdropFilter: "blur(4px)",
        backgroundColor: toneStyle.backgroundColor,
        borderLeft: `4px solid ${toneStyle.borderColor}`,
        borderRadius: "0.5rem",
        boxShadow:
          "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)",
        color: toneStyle.color,
        display: "flex",
        gap: "0.75rem",
        marginBottom: "0.75rem",
        maxWidth: "24rem",
        opacity: visible.value ? "1" : "0",
        padding: "1rem",
        transform: visible.value ? "translateX(0)" : "translateX(100%)",
        transition: visible.value
          ? "transform 300ms ease-out, opacity 300ms ease-out"
          : "transform 200ms ease-in, opacity 200ms ease-in",
        width: "100%",
      };
    });

    return () =>
      h(
        "div",
        {
          class: "transform transition duration-200 ease-in",
          style: {
            transform: "translateZ(0)",
          },
        },
        [
          h(
            "div",
            {
              class: [
                toastClasses.value,
                "flex items-center gap-3 p-4 mb-3 rounded-lg shadow-lg max-w-sm w-full backdrop-blur-sm transform",
              ],
              role: "alert",
              style: toastStyle.value,
            },
            [
              h("div", { class: "flex-shrink-0" }, [renderIcon(props.type)]),
              h(
                "div",
                {
                  class: "flex-1 text-sm font-medium",
                  style: {
                    flex: "1 1 auto",
                    fontSize: "0.875rem",
                    fontWeight: 500,
                    lineHeight: "1.25rem",
                  },
                },
                [props.message],
              ),
              h(
                "button",
                {
                  class:
                    "flex-shrink-0 inline-flex items-center justify-center w-8 h-8 rounded-lg hover:bg-black/10 dark:hover:bg-white/10 transition-colors",
                  "aria-label": "Закрыть",
                  onClick: close,
                  style: {
                    alignItems: "center",
                    background: "transparent",
                    border: "0",
                    borderRadius: "0.5rem",
                    color: "inherit",
                    cursor: "pointer",
                    display: "inline-flex",
                    flexShrink: 0,
                    height: "2rem",
                    justifyContent: "center",
                    padding: 0,
                    width: "2rem",
                  },
                },
                [
                  h(
                    "svg",
                    {
                      class: "w-4 h-4",
                      fill: "currentColor",
                      viewBox: "0 0 20 20",
                      style: {
                        height: "1rem",
                        width: "1rem",
                      },
                    },
                    [
                      h("path", {
                        "fill-rule": "evenodd",
                        d: "M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z",
                        "clip-rule": "evenodd",
                      }),
                    ],
                  ),
                ],
              ),
            ],
          ),
        ],
      );
  },
});
