function required(name: string, fallback?: string): string {
  const value = process.env[name]?.trim() || fallback?.trim();
  if (!value) {
    throw new Error(`Environment variable ${name} is required.`);
  }

  return value;
}

function parsePort(rawValue: string | undefined, fallback: number): number {
  if (!rawValue) {
    return fallback;
  }

  const parsed = Number(rawValue);
  if (!Number.isInteger(parsed) || parsed <= 0 || parsed > 65535) {
    throw new Error(`Invalid port value: "${rawValue}"`);
  }

  return parsed;
}

function parsePositiveInteger(rawValue: string | undefined, fallback: number): number {
  if (!rawValue) {
    return fallback;
  }

  const parsed = Number(rawValue);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error(`Invalid positive integer value: "${rawValue}"`);
  }

  return parsed;
}

function parseBoolean(rawValue: string | undefined, fallback: boolean): boolean {
  if (!rawValue) {
    return fallback;
  }

  const normalized = rawValue.trim().toLowerCase();
  if (normalized === "true") {
    return true;
  }

  if (normalized === "false") {
    return false;
  }

  throw new Error(`Invalid boolean value: "${rawValue}"`);
}

export const config = {
  port: parsePort(process.env.PORT, 8080),
  databaseUrl: required("DATABASE_URL"),
  carServiceBaseUrl: required("CAR_SERVICE_BASE_URL"),
  partnerServiceBaseUrl: required("PARTNER_SERVICE_BASE_URL"),
  bookingServiceBaseUrl: required("BOOKING_SERVICE_BASE_URL"),
  apiGatewayPublicBaseUrl: required("API_GATEWAY_PUBLIC_BASE_URL", "http://localhost:9186"),
  rabbitMqUrl: process.env.RABBITMQ_URL?.trim() || null,
  rabbitMqExchange: required("RABBITMQ_EXCHANGE", "autorent.events"),
  observabilityLogPath: process.env.OBSERVABILITY_LOG_PATH?.trim() || null,
  autoIndexOnStartup: parseBoolean(process.env.AUTO_INDEX_ON_STARTUP, true),
  autoRefreshIntervalSeconds: parsePositiveInteger(
    process.env.AUTO_REFRESH_INTERVAL_SECONDS,
    900,
  ),
  embeddingDimensions: 128,
  localLlmBaseUrl: process.env.LOCAL_LLM_BASE_URL?.trim() || null,
  localLlmChatModel: required("LOCAL_LLM_CHAT_MODEL", "qwen2.5:1.5b"),
  localLlmEmbeddingModel: required(
    "LOCAL_LLM_EMBEDDING_MODEL",
    "nomic-embed-text",
  ),
  localLlmTimeoutSeconds: parsePositiveInteger(
    process.env.LOCAL_LLM_TIMEOUT_SECONDS,
    90,
  ),
  llmRecommendationSummaryEnabled: parseBoolean(
    process.env.LLM_RECOMMENDATION_SUMMARY_ENABLED,
    false,
  ),
  llmRecommendationSummaryTimeoutMs: parsePositiveInteger(
    process.env.LLM_RECOMMENDATION_SUMMARY_TIMEOUT_MS,
    5000,
  ),
  openAiApiKey: process.env.OPENAI_API_KEY?.trim() || null,
  openAiBaseUrl: required("OPENAI_BASE_URL", "https://api.openai.com/v1"),
  openAiChatModel: required("OPENAI_CHAT_MODEL", "gpt-4.1-mini"),
  openAiEmbeddingModel: required(
    "OPENAI_EMBEDDING_MODEL",
    "text-embedding-3-small",
  ),
};
