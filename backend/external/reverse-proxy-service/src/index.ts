import cors from "cors";
import express from "express";
import { createProxyMiddleware } from "http-proxy-middleware";

const app = express();
app.use(cors());

type ServiceConfig = {
  route: string;
  envKey: "IDENTITY_SERVICE_URL" | "CAR_SERVICE_URL" | "BOOKING_SERVICE_URL" | "INTERNAL_SERVICE_URL" | "TICKET_SERVICE_URL";
};

const services: ServiceConfig[] = [
  { route: "/identity", envKey: "IDENTITY_SERVICE_URL" },
  { route: "/cars", envKey: "CAR_SERVICE_URL" },
  { route: "/bookings", envKey: "BOOKING_SERVICE_URL" },
  { route: "/tickets", envKey: "TICKET_SERVICE_URL" },
  { route: "/internal", envKey: "INTERNAL_SERVICE_URL"},
];

for (const service of services) {
  const target = process.env[service.envKey];

  if (!target) {
    throw new Error(`Missing required environment variable: ${service.envKey}`);
  }

  app.use(
    service.route,
    createProxyMiddleware({
      target,
      changeOrigin: true,
      pathRewrite: {
        [`^${service.route}`]: ""
      }
    })
  );
}

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

const port = Number(process.env.PORT ?? 8080);

app.listen(port, () => {
  console.log(`Gateway running on port ${port}`);
});
