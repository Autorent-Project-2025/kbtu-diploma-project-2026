import { Redis } from "ioredis";
import { config } from "../config/env";
import { observabilityLogger } from "../observability/logger";

type InMemoryEntry<V> = {
  value: V;
  expiresAt: number;
};

const KEY_PREFIX = "ai-search:recommendation:";
const MEMORY_MAX_ENTRIES = 200;
const MEMORY_TTL_MS = 5 * 60 * 1000;

let redisClient: Redis | null = null;
let redisAvailable = false;

if (config.redisUrl) {
  redisClient = new Redis(config.redisUrl, {
    lazyConnect: false,
    maxRetriesPerRequest: 2,
    enableOfflineQueue: false,
  });

  redisClient.on("ready", () => {
    redisAvailable = true;
    observabilityLogger.info("recommendation_cache_redis_connected", {
      url: config.redisUrl,
    });
  });

  redisClient.on("error", (error: Error) => {
    if (redisAvailable) {
      observabilityLogger.warn("recommendation_cache_redis_error", {
        errorMessage: error.message,
      });
    }
    redisAvailable = false;
  });

  redisClient.on("end", () => {
    redisAvailable = false;
  });
}

const memoryCache = new Map<string, InMemoryEntry<unknown>>();

function normalizeKey(prompt: string, history: unknown[]): string {
  const normalizedPrompt = prompt.trim().toLowerCase().replace(/\s+/g, " ");
  const historyFingerprint = Array.isArray(history) && history.length > 0
    ? history.length.toString()
    : "0";
  return `${KEY_PREFIX}${normalizedPrompt}|h=${historyFingerprint}`;
}

function memoryGet<V>(key: string): V | null {
  const entry = memoryCache.get(key);
  if (!entry) return null;
  if (entry.expiresAt < Date.now()) {
    memoryCache.delete(key);
    return null;
  }
  memoryCache.delete(key);
  memoryCache.set(key, entry);
  return entry.value as V;
}

function memorySet<V>(key: string, value: V): void {
  memoryCache.delete(key);
  memoryCache.set(key, { value, expiresAt: Date.now() + MEMORY_TTL_MS });
  while (memoryCache.size > MEMORY_MAX_ENTRIES) {
    const oldestKey = memoryCache.keys().next().value;
    if (oldestKey === undefined) break;
    memoryCache.delete(oldestKey);
  }
}

export async function getCachedRecommendation<T>(
  prompt: string,
  history: unknown[],
): Promise<T | null> {
  const key = normalizeKey(prompt, history);

  if (redisClient && redisAvailable) {
    try {
      const raw = await redisClient.get(key);
      if (raw) {
        return JSON.parse(raw) as T;
      }
      return null;
    } catch (error) {
      observabilityLogger.warn("recommendation_cache_redis_get_failed", {
        errorMessage: error instanceof Error ? error.message : String(error),
      });
      return memoryGet<T>(key);
    }
  }

  return memoryGet<T>(key);
}

export async function setCachedRecommendation<T>(
  prompt: string,
  history: unknown[],
  value: T,
): Promise<void> {
  const key = normalizeKey(prompt, history);
  const serialized = JSON.stringify(value);

  if (redisClient && redisAvailable) {
    try {
      await redisClient.set(key, serialized, "EX", config.redisCacheTtlSeconds);
      return;
    } catch (error) {
      observabilityLogger.warn("recommendation_cache_redis_set_failed", {
        errorMessage: error instanceof Error ? error.message : String(error),
      });
    }
  }

  memorySet(key, value);
}

export async function closeCache(): Promise<void> {
  if (redisClient) {
    await redisClient.quit().catch(() => undefined);
  }
}
