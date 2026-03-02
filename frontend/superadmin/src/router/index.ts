import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import SuperadminUsersView from "../views/SuperadminUsersView.vue";
import { auth } from "../store/auth";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: () => (localStorage.getItem("token") ? "/users" : "/login"),
    },
    {
      path: "/login",
      component: LoginView,
    },
    {
      path: "/users",
      component: SuperadminUsersView,
      meta: { requiresAuth: true, requiredPermission: "User.View" },
    },
    {
      path: "/:pathMatch(.*)*",
      redirect: "/users",
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
