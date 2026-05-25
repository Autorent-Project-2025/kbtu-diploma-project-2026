import { createRouter, createWebHistory } from "vue-router";
import { auth } from "../store/auth";

const HomeView = () => import("../views/HomeView.vue");
const LoginView = () => import("../views/LoginView.vue");
const RegisterView = () => import("../views/RegisterView.vue");
const PartnerApplyView = () => import("../views/PartnerApplyView.vue");
const ActivateAccountView = () => import("../views/ActivateAccountView.vue");
const CarsView = () => import("../views/CarsView.vue");
const MyBookingsView = () => import("../views/MyBookingsView.vue");
const BookingDetailView = () => import("../views/BookingDetailView.vue");
const BookingPaymentView = () => import("../views/BookingPaymentView.vue");
const BookingCompletionView = () => import("../views/BookingCompletionView.vue");
const NotFoundView = () => import("../views/NotFoundView.vue");
const CarDetailView = () => import("@/views/CarDetailView.vue");
const PublicPartnerCarDetailView = () =>
  import("../views/PublicPartnerCarDetailView.vue");
const PartnerProfileView = () => import("../views/PartnerProfileView.vue");
const PartnerCarsView = () => import("../views/PartnerCarsView.vue");
const PartnerCarDetailView = () => import("../views/PartnerCarDetailView.vue");
const PartnerBookingsView = () => import("../views/PartnerBookingsView.vue");
const ProfileView = () => import("../views/ProfileView.vue");
const ProfileRouterView = () => import("../views/ProfileRouterView.vue");
const ForbiddenView = () => import("../views/ForbiddenView.vue");
const AiView = () => import("../views/AiView.vue");
const MyComplaintsView = () => import("../views/MyComplaintsView.vue");
const ComplaintDetailView = () => import("../views/ComplaintDetailView.vue");

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
    meta: { requiresAuth: false },
  },
  {
    path: "/ai",
    component: AiView,
    meta: { requiresAuth: false },
  },
  {
    path: "/bookings",
    component: MyBookingsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/bookings/:id",
    component: BookingDetailView,
    meta: { requiresAuth: true },
  },
  {
    path: "/bookings/:id/payment",
    component: BookingPaymentView,
    meta: { requiresAuth: true },
  },
  {
    path: "/bookings/:id/complete",
    component: BookingCompletionView,
    meta: { requiresAuth: true },
  },
  {
    path: "/complaints",
    name: "MyComplaints",
    component: MyComplaintsView,
    meta: { requiresAuth: true },
  },
  {
    path: "/complaints/:id",
    name: "ComplaintDetail",
    component: ComplaintDetailView,
    meta: { requiresAuth: true },
  },

  // /profile — определяет профиль по actor_type в JWT
  {
    path: "/profile",
    component: ProfileRouterView,
    meta: { requiresAuth: true },
  },

  // Конкретные профили
  {
    path: "/profile/user",
    component: ProfileView,
    meta: { requiresAuth: true },
  },
  {
    path: "/profile/partner",
    component: PartnerProfileView,
    meta: { requiresAuth: true, actorType: "partner" },
  },

  // Старый маршрут — редирект для совместимости
  {
    path: "/partner/me",
    redirect: "/profile",
  },

  {
    path: "/partner/cars",
    component: PartnerCarsView,
    meta: { requiresAuth: true, actorType: "partner" },
  },
  {
    path: "/partner/bookings",
    component: PartnerBookingsView,
    meta: { requiresAuth: true, actorType: "partner" },
  },
  {
    path: "/partner/cars/:id",
    component: PartnerCarDetailView,
    meta: { requiresAuth: true, actorType: "partner" },
  },
  {
    path: "/car-recommendations",
    name: "car-recommendations",
    redirect: "/ai",
  },
  {
    path: "/cars/:id",
    name: "CarDetail",
    component: CarDetailView,
    meta: { requiresAuth: false },
  },
  {
    path: "/cars/partner-cars/:id",
    name: "PublicPartnerCarDetail",
    component: PublicPartnerCarDetailView,
    meta: { requiresAuth: false },
  },
  {
    path: "/403",
    component: ForbiddenView,
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
  const token = auth.token || localStorage.getItem("token");
  const isAuthenticated = token ? auth.checkTokenValidity() : false;

  if (to.meta.requiresAuth && !isAuthenticated) {
    next("/login");
    return;
  }

  const requiredActorType =
    typeof to.meta.actorType === "string" ? to.meta.actorType : null;

  if (requiredActorType && !auth.isActorType(requiredActorType)) {
    next("/profile/user");
    return;
  }

  if (to.path === "/profile/user" && auth.isActorType("partner")) {
    next("/profile/partner");
    return;
  }

  next();
});
