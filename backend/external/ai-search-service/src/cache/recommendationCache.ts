type CacheEntry<V> = {
  value: V;
  expiresAt: number;
};

const MAX_ENTRIES = 200;
const TTL_MS = 5 * 60 * 1000;

function normalizeKey(prompt: string, history: unknown[]): string {
  const normalizedPrompt = prompt.trim().toLowerCase().replace(/\s+/g, " ");
  const historyFingerprint = Array.isArray(history) && history.length > 0
    ? history.length.toString()
    : "0";
  return `${normalizedPrompt}|h=${historyFingerprint}`;
}

class RecommendationCache<V> {
  private readonly entries = new Map<string, CacheEntry<V>>();

  get(key: string): V | null {
    const entry = this.entries.get(key);
    if (!entry) {
      return null;
    }
    if (entry.expiresAt < Date.now()) {
      this.entries.delete(key);
      return null;
    }
    this.entries.delete(key);
    this.entries.set(key, entry);
    return entry.value;
  }

  set(key: string, value: V): void {
    if (this.entries.has(key)) {
      this.entries.delete(key);
    }
    this.entries.set(key, { value, expiresAt: Date.now() + TTL_MS });
    while (this.entries.size > MAX_ENTRIES) {
      const oldestKey = this.entries.keys().next().value;
      if (oldestKey === undefined) {
        break;
      }
      this.entries.delete(oldestKey);
    }
  }
}

const cache = new RecommendationCache<unknown>();

export function getCachedRecommendation<T>(
  prompt: string,
  history: unknown[],
): T | null {
  return cache.get(normalizeKey(prompt, history)) as T | null;
}

export function setCachedRecommendation<T>(
  prompt: string,
  history: unknown[],
  value: T,
): void {
  cache.set(normalizeKey(prompt, history), value as unknown);
}
