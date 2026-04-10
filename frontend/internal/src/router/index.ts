import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import ManagerDetailView from "../views/ManagerDetailView.vue";
import ManagerTicketsView from "../views/ManagerTicketsView.vue";
import SuperManagerView from "../views/SuperManagerView.vue";
import AdminControlView from "../views/AdminControlView.vue";
import ClientsTableView from "../views/ClientsTableView.vue";
import ClientEditView from "../views/ClientEditView.vue";
import CarsTableView from "../views/CarsTableView.vue";
import CarEditView from "../views/CarEditView.vue";
import BookingsTableView from "../views/BookingsTableView.vue";
import BookingDetailView from "../views/BookingDetailView.vue";
import { auth } from "../store/auth";

const defaultRoutes: { path: string; permission: string }[] = [
  { path: "/admin", permission: "User.View" },
  { path: "/super", permission: "Ticket.ViewAll" },
  { path: "/tickets", permission: "Ticket.View" },
  { path: "/clients", permission: "Client.View" },
  { path: "/cars", permission: "PartnerCar.View" },
  { path: "/bookings", permission: "Booking.View" },
];

function resolveHome(): string {
  for (const r of defaultRoutes) {
    if (auth.hasPermission(r.permission)) return r.path;
  }
  return "/login";
}

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      redirect: () => {
        if (!localStorage.getItem("token")) return "/login";
        return resolveHome();
      },
    },
    {
      path: "/login",
      component: LoginView,
    },
    {
      path: "/admin",
      component: AdminControlView,
      meta: { requiresAuth: true, requiredPermission: "User.View" },
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
      path: "/clients",
      component: ClientsTableView,
      meta: { requiresAuth: true, requiredPermission: "Client.View" },
    },
    {
      path: "/clients/:id",
      component: ClientEditView,
      meta: { requiresAuth: true, requiredPermission: "Client.View" },
    },
    {
      path: "/cars",
      component: CarsTableView,
      meta: { requiresAuth: true, requiredPermission: "PartnerCar.View" },
    },
    {
      path: "/cars/:id",
      component: CarEditView,
      meta: { requiresAuth: true, requiredPermission: "PartnerCar.View" },
    },
    {
      path: "/bookings",
      component: BookingsTableView,
      meta: { requiresAuth: true, requiredPermission: "Booking.View" },
    },
    {
      path: "/bookings/:id",
      component: BookingDetailView,
      meta: { requiresAuth: true, requiredPermission: "Booking.View" },
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
    next(token ? resolveHome() : "/login");
    return;
  }

  next();
});

export { router };
