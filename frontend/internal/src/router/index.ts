import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import ManagerDetailView from "../views/ManagerDetailView.vue";
import ManagerTicketsView from "../views/ManagerTicketsView.vue";
import SuperManagerView from "../views/SuperManagerView.vue";
import { auth } from "../store/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: () => {
        if (!localStorage.getItem("token")) return "/login";
        if (auth.hasPermission("Ticket.ViewAll")) return "/super";
        return "/tickets";
      },
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
      path: "/super",
      component: SuperManagerView,
      meta: { requiresAuth: true, requiredPermission: "Ticket.ViewAll" },
    },
    {
      path: "/super/managers/:id",
      component: ManagerDetailView,
      meta: { requiresAuth: true, requiredPermission: "Ticket.ViewAll" },
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
    next(token ? (auth.hasPermission("Ticket.ViewAll") ? "/super" : "/tickets") : "/login");
    return;
  }

  next();
});

export { router };
