import cors from "cors";
import express from "express";
import { config } from "./config/env";
import { observabilityLogger } from "./observability/logger";
import { ensureSchemaReachable, reindexEverything, reindexPartnerCar } from "./indexing/searchIndexer";
import { parseRecommendationQuery } from "./ai/queryParser";
import { searchCars } from "./search/searchService";
import {
  composeClarificationResponse,
  composeRecommendationResponse,
  shouldAskClarifyingQuestion,
} from "./ai/answerComposer";
import { startIndexingConsumer } from "./messaging/indexingConsumer";
import { closeSql, sql } from "./db/sql";

type RequestLogMetric = {
  count: number;
  sumSeconds: number;
};

const requestMetrics = new Map<string, RequestLogMetric>();

function buildMetricKey(method: string, route: string, statusCode: number) {
  return `${method}|${route}|${statusCode}`;
}

function recordMetric(method: string, route: string, statusCode: number, durationSeconds: number) {
  const key = buildMetricKey(method, route, statusCode);
  const current = requestMetrics.get(key);

  if (current) {
    current.count += 1;
    current.sumSeconds += durationSeconds;
    return;
  }

  requestMetrics.set(key, {
    count: 1,
    sumSeconds: durationSeconds,
  });
}

function renderMetrics() {
  const lines: string[] = [
    "# HELP autorent_ai_search_http_requests_total Total HTTP requests processed by ai-search-service.",
    "# TYPE autorent_ai_search_http_requests_total counter",
  ];

  const entries = [...requestMetrics.entries()]
    .map(([key, aggregate]) => {
      const [method, route, status] = key.split("|");
      return { method, route, status, aggregate };
    })
    .sort((left, right) => left.method.localeCompare(right.method) || left.route.localeCompare(right.route));

  for (const entry of entries) {
    const labels = `{method="${entry.method}",route="${entry.route}",status="${entry.status}"}`;
    lines.push(`autorent_ai_search_http_requests_total${labels} ${entry.aggregate.count}`);
  }

  lines.push("# HELP autorent_ai_search_http_request_duration_seconds Request duration observed by ai-search-service.");
  lines.push("# TYPE autorent_ai_search_http_request_duration_seconds summary");

  for (const entry of entries) {
    const labels = `{method="${entry.method}",route="${entry.route}",status="${entry.status}"}`;
    lines.push(
      `autorent_ai_search_http_request_duration_seconds_count${labels} ${entry.aggregate.count}`,
    );
    lines.push(
      `autorent_ai_search_http_request_duration_seconds_sum${labels} ${entry.aggregate.sumSeconds.toFixed(6)}`,
    );
  }

  return `${lines.join("\n")}\n`;
}

async function main() {
  await ensureSchemaReachable();

  if (config.autoIndexOnStartup) {
    await reindexEverything();
  }

  const rabbitConnection = await startIndexingConsumer();
  const app = express();
  app.disable("x-powered-by");
  app.use(cors());
  app.use(express.json({ limit: "1mb" }));

  app.use((req, res, next) => {
    const startedAt = process.hrtime.bigint();

    res.on("finish", () => {
      const durationSeconds = Number(process.hrtime.bigint() - startedAt) / 1_000_000_000;
      recordMetric(req.method, req.path, res.statusCode, durationSeconds);

      observabilityLogger.info("http_request_completed", {
        method: req.method,
        route: req.path,
        statusCode: res.statusCode,
        durationMs: Math.round(durationSeconds * 1000 * 100) / 100,
      });
    });

    next();
  });

  app.get("/healthz", async (_req, res) => {
    await sql`select 1`;
    res.status(200).json({ status: "ok" });
  });

  app.get("/metrics", (_req, res) => {
    res.type("text/plain; version=0.0.4; charset=utf-8");
    res.status(200).send(renderMetrics());
  });

  app.post("/recommendations", async (req, res) => {
    const prompt = typeof req.body?.prompt === "string" ? req.body.prompt.trim() : "";
    if (!prompt) {
      res.status(400).json({ message: "prompt is required." });
      return;
    }

    const parsedQuery = await parseRecommendationQuery(prompt);
    if (shouldAskClarifyingQuestion(parsedQuery)) {
      res.status(200).json(await composeClarificationResponse(parsedQuery));
      return;
    }

    const cars = await searchCars(prompt, parsedQuery);
    res.status(200).json(await composeRecommendationResponse(parsedQuery, cars));
  });

  app.post("/internal/reindex", async (_req, res) => {
    const indexedCount = await reindexEverything();
    res.status(200).json({ indexedCount });
  });

  app.post("/internal/reindex/partner-cars/:partnerCarId", async (req, res) => {
    const partnerCarId = Number(req.params.partnerCarId);
    if (!Number.isInteger(partnerCarId) || partnerCarId <= 0) {
      res.status(400).json({ message: "partnerCarId must be a positive integer." });
      return;
    }

    const indexed = await reindexPartnerCar(partnerCarId);
    res.status(200).json({ indexed });
  });

  const refreshInterval = setInterval(() => {
    reindexEverything().catch((error) => {
      observabilityLogger.error("scheduled_reindex_failed", error);
    });
  }, config.autoRefreshIntervalSeconds * 1000);
  refreshInterval.unref();

  const server = app.listen(config.port, () => {
    observabilityLogger.info("server_started", { port: config.port });
  });

  const shutdown = async (signal: string) => {
    observabilityLogger.info("service_shutdown_started", { signal });
    clearInterval(refreshInterval);
    server.close();
    await rabbitConnection?.close().catch(() => undefined);
    await closeSql().catch(() => undefined);
    process.exit(0);
  };

  process.once("SIGINT", () => void shutdown("SIGINT"));
  process.once("SIGTERM", () => void shutdown("SIGTERM"));
}

main().catch((error) => {
  observabilityLogger.error("service_startup_failed", error);
  process.exit(1);
});
