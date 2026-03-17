import { randomUUID } from "node:crypto";
import { createWriteStream, mkdirSync, readFileSync } from "node:fs";
import { createServer as createHttpServer } from "node:http";
import { createServer as createHttpsServer } from "node:https";
import { dirname } from "node:path";

import { trace } from "@opentelemetry/api";
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

type GatewayHttpMetric = {
  count: number;
  sumSeconds: number;
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

const observabilityLogPath = process.env.OBSERVABILITY_LOG_PATH?.trim();
const observabilityLogStream = observabilityLogPath
  ? (() => {
      mkdirSync(dirname(observabilityLogPath), { recursive: true });
      return createWriteStream(observabilityLogPath, { flags: "a" });
    })()
  : null;

const gatewayHttpMetrics = new Map<string, GatewayHttpMetric>();
let gatewayHttpRequestsInFlight = 0;

const buildHttpMetricKey = (method: string, route: string, statusCode: number): string => {
  return `${method.toUpperCase()}|${route}|${statusCode}`;
};

const recordGatewayHttpMetric = (method: string, route: string, statusCode: number, durationSeconds: number) => {
  const key = buildHttpMetricKey(method, route, statusCode);
  const current = gatewayHttpMetrics.get(key);

  if (current) {
    current.count += 1;
    current.sumSeconds += durationSeconds;
    return;
  }

  gatewayHttpMetrics.set(key, {
    count: 1,
    sumSeconds: durationSeconds,
  });
};

const escapeLabelValue = (value: string): string => {
  return value.replace(/\\/g, "\\\\").replace(/"/g, '\\"').replace(/\n/g, "\\n");
};

const appendLabels = (labels: Array<[string, string]>): string => {
  return `{${labels.map(([name, value]) => `${name}="${escapeLabelValue(value)}"`).join(",")}}`;
};

const extractGatewayRoute = (rawPath: string | undefined): string => {
  const trimmedPath = (rawPath || "/").trim();
  if (!trimmedPath || trimmedPath === "/") {
    return "/";
  }

  const [firstSegment] = trimmedPath.split("/", 3).filter(Boolean);
  if (!firstSegment) {
    return "/";
  }

  return `/${firstSegment.toLowerCase()}`;
};

const renderGatewayMetrics = (): string => {
  const lines: string[] = [
    "# HELP autorent_api_gateway_http_requests_in_flight Current number of HTTP requests being processed by the API gateway.",
    "# TYPE autorent_api_gateway_http_requests_in_flight gauge",
    `autorent_api_gateway_http_requests_in_flight ${gatewayHttpRequestsInFlight}`,
    "# HELP autorent_api_gateway_http_requests_total Total HTTP requests processed by the API gateway.",
    "# TYPE autorent_api_gateway_http_requests_total counter",
  ];

  const metrics = [...gatewayHttpMetrics.entries()]
    .map(([key, aggregate]) => {
      const [method, route, status] = key.split("|");
      return { method, route, status, aggregate };
    })
    .sort((left, right) => left.method.localeCompare(right.method) || left.route.localeCompare(right.route) || left.status.localeCompare(right.status));

  for (const metric of metrics) {
    const labels = appendLabels([
      ["method", metric.method],
      ["route", metric.route],
      ["status", metric.status],
    ]);

    lines.push(`autorent_api_gateway_http_requests_total${labels} ${metric.aggregate.count}`);
  }

  lines.push("# HELP autorent_api_gateway_http_request_duration_seconds Request duration observed at the API gateway.");
  lines.push("# TYPE autorent_api_gateway_http_request_duration_seconds summary");

  for (const metric of metrics) {
    const labels = appendLabels([
      ["method", metric.method],
      ["route", metric.route],
      ["status", metric.status],
    ]);

    lines.push(`autorent_api_gateway_http_request_duration_seconds_count${labels} ${metric.aggregate.count}`);
    lines.push(
      `autorent_api_gateway_http_request_duration_seconds_sum${labels} ${metric.aggregate.sumSeconds.toFixed(6)}`
    );
  }

  return `${lines.join("\n")}\n`;
};

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

const formatTraceParent = (traceId: string, spanId: string, traceFlags: number): string => {
  return `00-${traceId}-${spanId}-${traceFlags.toString(16).padStart(2, "0")}`;
};

const writeStructuredLog = (entry: Record<string, unknown>): void => {
  const line = JSON.stringify(entry);
  console.log(line);
  observabilityLogStream?.write(`${line}\n`);
};

process.once("exit", () => {
  observabilityLogStream?.end();
});

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
  const routeLabel = extractGatewayRoute(req.path);
  const startedAt = process.hrtime.bigint();
  const activeSpanContext = trace.getActiveSpan()?.spanContext();
  const generatedTraceParent = activeSpanContext
    ? formatTraceParent(activeSpanContext.traceId, activeSpanContext.spanId, activeSpanContext.traceFlags)
    : null;
  const traceParent = typeof req.headers["traceparent"] === "string" ? req.headers["traceparent"] : generatedTraceParent;

  req.headers["x-request-id"] = requestId;
  res.setHeader("x-request-id", requestId);
  if (generatedTraceParent) {
    res.setHeader("traceparent", generatedTraceParent);
  }
  res.setHeader("x-content-type-options", "nosniff");
  res.setHeader("x-frame-options", "DENY");
  res.setHeader("referrer-policy", "strict-origin-when-cross-origin");
  res.setHeader("x-permitted-cross-domain-policies", "none");
  res.setHeader("cross-origin-opener-policy", "same-origin");

  if (isHttpsRequest(req)) {
    res.setHeader("strict-transport-security", "max-age=31536000; includeSubDomains");
  }

  gatewayHttpRequestsInFlight += 1;
  let finalized = false;
  const finalize = () => {
    if (finalized) {
      return;
    }

    finalized = true;
    gatewayHttpRequestsInFlight = Math.max(0, gatewayHttpRequestsInFlight - 1);

    if (routeLabel === "/healthz" || routeLabel === "/metrics") {
      return;
    }

    const durationSeconds = Number(process.hrtime.bigint() - startedAt) / 1_000_000_000;
    recordGatewayHttpMetric(req.method, routeLabel, res.statusCode, durationSeconds);

    writeStructuredLog({
      timestamp: new Date().toISOString(),
      service: "api-gateway",
      event: "http_request_completed",
      requestId,
      traceId: activeSpanContext?.traceId ?? null,
      spanId: activeSpanContext?.spanId ?? null,
      traceParent,
      method: req.method,
      route: routeLabel,
      path: req.originalUrl,
      statusCode: res.statusCode,
      durationMs: Math.round(durationSeconds * 1000 * 100) / 100,
      ip: req.ip || null,
    });
  };

  res.once("finish", finalize);
  res.once("close", finalize);
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
    allowedHeaders: ["Authorization", "Content-Type", "X-Request-Id", "X-Requested-With", "traceparent", "tracestate"],
    exposedHeaders: ["X-Request-Id", "traceparent"],
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

app.get("/metrics", (_req, res) => {
  res.type("text/plain; version=0.0.4; charset=utf-8");
  res.status(200).send(renderGatewayMetrics());
});

app.use((req, res, next) => {
  if (req.path === "/healthz" || req.path === "/metrics") {
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
          const traceParentHeader = typeof req.headers["traceparent"] === "string" ? req.headers["traceparent"] : null;
          writeStructuredLog({
            timestamp: new Date().toISOString(),
            service: "api-gateway",
            event: "upstream_proxy_error",
            requestId: typeof req.headers["x-request-id"] === "string" ? req.headers["x-request-id"] : null,
            traceId: traceParentHeader ? traceParentHeader.split("-")[1] ?? null : null,
            traceParent: traceParentHeader,
            route: service.route,
            path: req.url,
            statusCode: 502,
          });
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
