const rawApiUrl = import.meta.env.VITE_API_URL || "http://localhost:9186";
const normalizedApiUrl = rawApiUrl.replace(/\/+$/, "").replace(/\/api$/i, "");

export const config = {
  api: {
    baseURL: normalizedApiUrl,
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
