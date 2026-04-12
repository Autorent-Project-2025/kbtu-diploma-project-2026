import { createRouter, createWebHistory } from "vue-router";
import LoginView from "../views/LoginView.vue";
import ManagerDetailView from "../views/ManagerDetailView.vue";
import ManagerTicketsView from "../views/ManagerTicketsView.vue";
import SuperManagerView from "../views/SuperManagerView.vue";
import AdminControlView from "../views/AdminControlView.vue";
import ClientsTableView from "../views/ClientsTableView.vue";
import ClientDetailView from "../views/ClientDetailView.vue";
import PartnersTableView from "../views/PartnersTableView.vue";
import PartnerDetailView from "../views/PartnerDetailView.vue";
import CarsTableView from "../views/CarsTableView.vue";
import CarDetailView from "../views/CarDetailView.vue";
import BookingsTableView from "../views/BookingsTableView.vue";
import BookingDetailView from "../views/BookingDetailView.vue";
import ComplaintsQueueView from "../views/ComplaintsQueueView.vue";
import ComplaintDetailView from "../views/ComplaintDetailView.vue";
import BookingReviewView from "../views/BookingReviewView.vue";
import AccessRequestsView from "../views/AccessRequestsView.vue";
import FinanceView from "../views/FinanceView.vue";
import { auth } from "../store/auth";

const defaultRoutes: { path: string; permission: string }[] = [
  { path: "/tickets", permission: "Ticket.View" },
  { path: "/clients", permission: "Client.View" },
  { path: "/partners", permission: "Partner.View" },
  { path: "/cars", permission: "PartnerCar.View" },
  { path: "/bookings", permission: "Booking.View" },
  { path: "/complaints", permission: "Complaint.View" },
  { path: "/finance", permission: "Partner.View" },
  { path: "/super", permission: "Ticket.ViewAll" },
  { path: "/admin", permission: "User.View" },
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
      path: "/tickets",
      component: ManagerTicketsView,
      meta: { requiresAuth: true, requiredPermission: "Ticket.View" },
    },
    {
      path: "/clients",
      component: ClientsTableView,
      meta: { requiresAuth: true, requiredPermission: "Client.View" },
    },
    {
      path: "/clients/:id",
      component: ClientDetailView,
      meta: { requiresAuth: true, requiredPermission: "Client.View" },
    },
    {
      path: "/partners",
      component: PartnersTableView,
      meta: { requiresAuth: true, requiredPermission: "Partner.View" },
    },
    {
      path: "/partners/:id",
      component: PartnerDetailView,
      meta: { requiresAuth: true, requiredPermission: "Partner.View" },
    },
    {
      path: "/cars",
      component: CarsTableView,
      meta: { requiresAuth: true, requiredPermission: "PartnerCar.View" },
    },
    {
      path: "/cars/:id",
      component: CarDetailView,
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
      path: "/complaints",
      component: ComplaintsQueueView,
      meta: { requiresAuth: true, requiredPermission: "Complaint.View" },
    },
    {
      path: "/complaints/access-requests",
      component: AccessRequestsView,
      meta: { requiresAuth: true, requiredPermission: "AccessRequest.Review" },
    },
    {
      path: "/complaints/:id",
      component: ComplaintDetailView,
      meta: { requiresAuth: true, requiredPermission: "Complaint.View" },
    },
    {
      path: "/complaints/:complaintId/booking-review",
      component: BookingReviewView,
      meta: { requiresAuth: true, requiredPermission: "Complaint.Review" },
    },
    {
      path: "/finance",
      component: FinanceView,
      meta: { requiresAuth: true, requiredPermission: "Partner.View" },
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
      path: "/admin",
      component: AdminControlView,
      meta: { requiresAuth: true, requiredPermission: "User.View" },
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
