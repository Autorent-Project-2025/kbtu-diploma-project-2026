export const config = {
  api: {
    baseURL: import.meta.env.VITE_API_URL || "http://localhost:2651/api",
  },
  app: {
    name: import.meta.env.VITE_APP_NAME || "AutoRent",
    defaultCarImage:
      import.meta.env.VITE_DEFAULT_CAR_IMAGE ||
      "https://placehold.co/600x400?text=No+Image",
  },
  booking: {
    defaultDurationHours:
      Number(import.meta.env.VITE_DEFAULT_BOOKING_HOURS) || 3,
  },
} as const;

export default config;
