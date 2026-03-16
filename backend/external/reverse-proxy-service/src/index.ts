import { randomUUID } from "node:crypto";
import { readFileSync } from "node:fs";
import { createServer as createHttpServer } from "node:http";
import { createServer as createHttpsServer } from "node:https";

import cors from "cors";
import express from "express";
import { createProxyMiddleware } from "http-proxy-middleware";

type ServiceConfig = {
  route: string;
  envKey:
    | "IDENTITY_SERVICE_URL"
    | "CAR_SERVICE_URL"
    | "BOOKING_SERVICE_URL"
    | "CLIENT_SERVICE_URL"
    | "PARTNER_SERVICE_URL"
    | "INTERNAL_SERVICE_URL"
    | "TICKET_SERVICE_URL"
    | "FILE_SERVICE_URL";
};

type RateLimitBucket = {
  count: number;
  resetAt: number;
};

const services: ServiceConfig[] = [
  { route: "/identity", envKey: "IDENTITY_SERVICE_URL" },
  { route: "/cars", envKey: "CAR_SERVICE_URL" },
  { route: "/bookings", envKey: "BOOKING_SERVICE_URL" },
  { route: "/clients", envKey: "CLIENT_SERVICE_URL" },
  { route: "/partners", envKey: "PARTNER_SERVICE_URL" },
  { route: "/tickets", envKey: "TICKET_SERVICE_URL" },
  { route: "/files", envKey: "FILE_SERVICE_URL" },
  { route: "/internal", envKey: "INTERNAL_SERVICE_URL" },
];

const defaultAllowedOrigins = [
  "http://localhost:5173",
  "http://localhost:5174",
  "http://localhost:5175",
];

const parsePort = (rawValue: string | undefined, fallback: number): number => {
  if (!rawValue) {
    return fallback;
  }

  const parsed = Number(rawValue);
  if (!Number.isInteger(parsed) || parsed <= 0 || parsed > 65535) {
    throw new Error(`Invalid port value: "${rawValue}"`);
  }

  return parsed;
};

const parsePositiveInteger = (rawValue: string | undefined, fallback: number): number => {
  if (!rawValue) {
    return fallback;
  }

  const parsed = Number(rawValue);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error(`Invalid positive integer value: "${rawValue}"`);
  }

  return parsed;
};

const parseBoolean = (rawValue: string | undefined, fallback: boolean): boolean => {
  if (!rawValue) {
    return fallback;
  }

  const normalized = rawValue.trim().toLowerCase();
  if (normalized === "true") return true;
  if (normalized === "false") return false;

  throw new Error(`Invalid boolean value: "${rawValue}"`);
};

const parseAllowedOrigins = (rawValue: string | undefined): Set<string> => {
  const origins = rawValue
    ? rawValue
        .split(",")
        .map((origin) => origin.trim())
        .filter(Boolean)
    : defaultAllowedOrigins;

  return new Set(origins);
};

const parseTrustProxy = (rawValue: string | undefined): boolean | number | string => {
  if (!rawValue) {
    return "loopback, linklocal, uniquelocal";
  }

  const normalized = rawValue.trim().toLowerCase();
  if (normalized === "true") return true;
  if (normalized === "false") return false;

  const numericValue = Number(rawValue);
  if (Number.isInteger(numericValue) && numericValue >= 0) {
    return numericValue;
  }

  return rawValue.trim();
};

const isHttpsRequest = (req: any): boolean => {
  const forwardedProto = req.header("x-forwarded-proto")?.split(",")[0]?.trim().toLowerCase();
  const encryptedSocket = (req.socket as typeof req.socket & { encrypted?: boolean }).encrypted;

  return Boolean(req.secure || encryptedSocket || forwardedProto === "https");
};

const normalizeRequestId = (requestId: string | undefined): string => {
  const trimmed = requestId?.trim();
  return trimmed && trimmed.length <= 128 ? trimmed : randomUUID();
};

const app = express();
app.disable("x-powered-by");
app.set("trust proxy", parseTrustProxy(process.env.TRUST_PROXY));

const allowedOrigins = parseAllowedOrigins(process.env.ALLOWED_ORIGINS);
const rateLimitWindowMs = parsePositiveInteger(process.env.RATE_LIMIT_WINDOW_MS, 60_000);
const rateLimitMaxRequests = parsePositiveInteger(process.env.RATE_LIMIT_MAX_REQUESTS, 300);
const proxyTimeoutMs = parsePositiveInteger(process.env.PROXY_TIMEOUT_MS, 30_000);
const requestTimeoutMs = parsePositiveInteger(process.env.REQUEST_TIMEOUT_MS, 30_000);
const tlsEnabled = parseBoolean(process.env.TLS_ENABLED, true);
const redirectHttpToHttps = parseBoolean(process.env.HTTP_TO_HTTPS_REDIRECT, false);
const httpPort = parsePort(process.env.PORT, 8080);
const httpsPort = parsePort(process.env.HTTPS_PORT, 8443);
const rateLimitBuckets = new Map<string, RateLimitBucket>();

setInterval(() => {
  const now = Date.now();

  for (const [key, bucket] of rateLimitBuckets.entries()) {
    if (bucket.resetAt <= now) {
      rateLimitBuckets.delete(key);
    }
  }
}, rateLimitWindowMs).unref();

app.use((req, res, next) => {
  const requestId = normalizeRequestId(req.header("x-request-id"));

  req.headers["x-request-id"] = requestId;
  res.setHeader("x-request-id", requestId);
  res.setHeader("x-content-type-options", "nosniff");
  res.setHeader("x-frame-options", "DENY");
  res.setHeader("referrer-policy", "strict-origin-when-cross-origin");
  res.setHeader("x-permitted-cross-domain-policies", "none");
  res.setHeader("cross-origin-opener-policy", "same-origin");

  if (isHttpsRequest(req)) {
    res.setHeader("strict-transport-security", "max-age=31536000; includeSubDomains");
  }

  next();
});

app.use(
  cors({
    origin(origin, callback) {
      if (!origin || allowedOrigins.has(origin)) {
        callback(null, true);
        return;
      }

      callback(new Error("Origin is not allowed by gateway CORS policy"));
    },
    credentials: true,
    methods: ["GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS"],
    allowedHeaders: ["Authorization", "Content-Type", "X-Request-Id", "X-Requested-With"],
    exposedHeaders: ["X-Request-Id"],
  })
);

app.use((error: Error, _req: any, res: any, next: any) => {
  if (error.message === "Origin is not allowed by gateway CORS policy") {
    res.status(403).json({ message: error.message });
    return;
  }

  next(error);
});

app.get("/healthz", (_req, res) => {
  res.status(200).json({ status: "ok" });
});

app.use((req, res, next) => {
  if (req.path === "/healthz") {
    next();
    return;
  }

  if (tlsEnabled && redirectHttpToHttps && !isHttpsRequest(req)) {
    const redirectHost = req.hostname.includes(":") ? `[${req.hostname}]` : req.hostname;
    const redirectUrl = `https://${redirectHost}:${httpsPort}${req.originalUrl}`;

    res.redirect(308, redirectUrl);
    return;
  }

  const now = Date.now();
  const rateLimitKey = req.ip || "unknown";
  const currentBucket = rateLimitBuckets.get(rateLimitKey);
  const bucket =
    !currentBucket || currentBucket.resetAt <= now
      ? { count: 0, resetAt: now + rateLimitWindowMs }
      : currentBucket;

  bucket.count += 1;
  rateLimitBuckets.set(rateLimitKey, bucket);

  const retryAfterSeconds = Math.max(1, Math.ceil((bucket.resetAt - now) / 1000));
  res.setHeader("x-ratelimit-limit", String(rateLimitMaxRequests));
  res.setHeader("x-ratelimit-remaining", String(Math.max(rateLimitMaxRequests - bucket.count, 0)));
  res.setHeader("x-ratelimit-reset", String(Math.ceil(bucket.resetAt / 1000)));

  if (bucket.count > rateLimitMaxRequests) {
    res.setHeader("retry-after", String(retryAfterSeconds));
    res.status(429).json({ message: "Rate limit exceeded. Please retry later." });
    return;
  }

  next();
});

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
      xfwd: true,
      proxyTimeout: proxyTimeoutMs,
      timeout: requestTimeoutMs,
      pathRewrite: {
        [`^${service.route}`]: "",
      },
      on: {
        proxyReq(proxyReq, req) {
          const requestId = req.headers["x-request-id"];
          if (typeof requestId === "string" && requestId.trim()) {
            proxyReq.setHeader("x-request-id", requestId);
          }
        },
        error(_error, req, res) {
          const response = res as any;
          response.status(502).json({
            message: `Upstream service for route "${req.url}" is unavailable.`,
          });
        },
      },
    })
  );
}

app.use((_req, res) => {
  res.status(404).json({ message: "Route not found" });
});

const httpServer = createHttpServer(app);
httpServer.listen(httpPort, () => {
  console.log(`Gateway HTTP listener is running on port ${httpPort}`);
});

if (tlsEnabled) {
  const tlsKeyPath = process.env.TLS_KEY_PATH ?? "/tmp/autorent-edge.key";
  const tlsCertPath = process.env.TLS_CERT_PATH ?? "/tmp/autorent-edge.crt";

  const httpsServer = createHttpsServer(
    {
      key: readFileSync(tlsKeyPath),
      cert: readFileSync(tlsCertPath),
    },
    app
  );

  httpsServer.listen(httpsPort, () => {
    console.log(`Gateway HTTPS listener is running on port ${httpsPort}`);
  });
}
