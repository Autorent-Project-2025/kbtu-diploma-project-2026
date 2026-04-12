import { createApp } from "vue";
import App from "./App.vue";
import { router } from "./router";
import "./assets/main.css";

const app = createApp(App);

app.use(router);

function updateFaviconByTheme() {
  const isDark = document.documentElement.classList.contains("dark");
  let favicon = document.querySelector(
    "link[rel='icon']",
  ) as HTMLLinkElement | null;

  if (!favicon) {
    favicon = document.createElement("link");
    favicon.rel = "icon";
    favicon.type = "image/svg+xml";
    document.head.appendChild(favicon);
  }

  favicon.href = isDark ? "/favicon-dark.svg" : "/favicon.svg";
}

updateFaviconByTheme();

const faviconObserver = new MutationObserver(() => {
  updateFaviconByTheme();
});

faviconObserver.observe(document.documentElement, {
  attributes: true,
  attributeFilter: ["class"],
});

app.mount("#app");
