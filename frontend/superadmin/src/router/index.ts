import { createRouter, createWebHistory } from "vue-router";
import { createRouteAccessGuard } from "@shared/routeGuard";
import { access } from "../accessControl";
import { auth } from "../store/auth";

const LoginView = () => import("../views/LoginView.vue");
const SuperadminUsersView = () => import("../views/SuperadminUsersView.vue");

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

router.beforeEach(
  createRouteAccessGuard({
    access,
    auth,
    loginPath: "/login",
    getForbiddenRedirect() {
      return "/login";
    },
  }),
);

export { router };
