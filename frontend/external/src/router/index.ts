import { createRouter, createWebHistory } from "vue-router";
import HomeView from "../views/HomeView.vue";
import LoginView from "../views/LoginView.vue";
import RegisterView from "../views/RegisterView.vue";
import PartnerApplyView from "../views/PartnerApplyView.vue";
import ActivateAccountView from "../views/ActivateAccountView.vue";
import CarsView from "../views/CarsView.vue";
import MyBookingsView from "../views/MyBookingsView.vue";
import BookingPaymentView from "../views/BookingPaymentView.vue";
import NotFoundView from "../views/NotFoundView.vue";
import CarDetailView from "@/views/CarDetailView.vue";
import PartnerProfileView from "../views/PartnerProfileView.vue";
import PartnerCarsView from "../views/PartnerCarsView.vue";
import PartnerCarDetailView from "../views/PartnerCarDetailView.vue";
import PartnerBookingsView from "../views/PartnerBookingsView.vue";
import { auth } from "../store/auth";

const routes = [
  {
    path: "/",
    component: HomeView,
    meta: { requiresAuth: false },
  },
  {
    path: "/login",
    component: LoginView,
  },
  {
    path: "/apply",
    component: RegisterView,
  },
  {
    path: "/partner/apply",
    component: PartnerApplyView,
  },
  {
    path: "/register",
    redirect: "/apply",
  },
  {
    path: "/activate",
    component: ActivateAccountView,
  },
  {
    path: "/cars",
    component: CarsView,
    meta: { requiresAuth: false }, // Доступно для всех
  },
  {
    path: "/bookings",
    component: MyBookingsView,
    meta: { requiresAuth: true }, // Только для авторизованных
  },
  {
    path: "/bookings/:id/payment",
    component: BookingPaymentView,
    meta: { requiresAuth: true },
  },
  {
    path: "/partner/me",
    component: PartnerProfileView,
    meta: { requiresAuth: true },
  },
  {
    path: "/partner/cars",
    component: PartnerCarsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/partner/bookings",
    component: PartnerBookingsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/partner/cars/:id",
    component: PartnerCarDetailView,
    meta: { requiresAuth: true },
  },
  {
    path: "/cars/:id",
    name: "CarDetail",
    component: CarDetailView,
    meta: { requiresAuth: false }, // Доступно для всех
  },
  {
    path: "/:pathMatch(.*)*",
    component: NotFoundView,
  },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0, behavior: "smooth" };
    }
  },
});

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem("token");

  // Проверяем валидность токена перед каждым переходом
  if (token) {
    const isValid = auth.checkTokenValidity();

    if (!isValid && to.meta.requiresAuth) {
      // Токен истек и требуется авторизация - редирект на логин
      next("/login");
      return;
    }
  }

  // Стандартная проверка авторизации
  if (to.meta.requiresAuth && !token) {
    next("/login");
  } else {
    next();
  }
});
