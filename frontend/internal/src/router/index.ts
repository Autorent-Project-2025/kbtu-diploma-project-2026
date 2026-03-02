import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import ManagerTicketsView from "../views/ManagerTicketsView.vue";
import { auth } from "../store/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: () => (localStorage.getItem("token") ? "/tickets" : "/login"),
    },
    {
      path: "/login",
      component: LoginView,
    },
    {
      path: "/tickets",
      component: ManagerTicketsView,
      meta: { requiresAuth: true, requiredPermission: "Ticket.View" },
    },
    {
      path: "/:pathMatch(.*)*",
      redirect: "/login",
    },
  ],
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");
  const requiredPermission = to.meta.requiredPermission as string | undefined;

  if (token) {
    const isValid = auth.checkTokenValidity();
    if (!isValid && to.meta.requiresAuth) {
      next("/login");
      return;
    }
  }

  if (to.meta.requiresAuth && !token) {
    next("/login");
    return;
  }

  if (requiredPermission && !auth.hasPermission(requiredPermission)) {
    next("/login");
    return;
  }

  next();
});

export { router };
