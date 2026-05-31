import { Teleport, defineComponent, h } from "vue";
import ToastItem from "./ToastItem";
import { useToast } from "./useToast";

export default defineComponent({
  name: "SharedToastContainer",
  setup() {
    const { toasts, removeToast } = useToast();

    return () =>
      h(Teleport, { to: "body" }, [
        h(
          "div",
          {
            class:
              "fixed top-4 right-4 z-[9999] flex flex-col items-end pointer-events-none",
            style: {
              position: "fixed",
              top: "1rem",
              right: "1rem",
              zIndex: 9999,
              display: "flex",
              flexDirection: "column",
              alignItems: "flex-end",
              pointerEvents: "none",
            },
          },
          [
            h(
              "div",
              {
                class: "pointer-events-auto",
                style: {
                  pointerEvents: "auto",
                },
              },
              toasts.map((toast) =>
                h(ToastItem, {
                  key: toast.id,
                  id: toast.id,
                  message: toast.message,
                  type: toast.type,
                  duration: toast.duration,
                  onClose: removeToast,
                }),
              ),
            ),
          ],
        ),
      ]);
  },
});
