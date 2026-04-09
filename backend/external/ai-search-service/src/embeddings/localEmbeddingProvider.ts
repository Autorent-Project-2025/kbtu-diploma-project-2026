import { config } from "../config/env";

const tokenSynonyms: Record<string, string[]> = {
  sport: ["sport", "sports", "спортив", "спорт", "спорткар", "rx7", "supra", "coupe", "купе"],
  business: ["business", "бизнес", "делов", "meeting", "airport", "aeroport"],
  family: ["family", "сем", "children", "дет", "багаж", "trunk"],
  city: ["city", "город", "urban", "ежеднев", "парковк"],
  luxury: ["luxury", "premium", "люкс", "премиум"],
  automatic: ["automatic", "автомат", "акпп"],
  manual: ["manual", "механик", "мкпп"],
  budget: ["budget", "бюджет", "cheap", "деш", "эконом"],
};

function normalizeToken(token: string): string {
  return token.trim().toLowerCase().replace(/[^\p{L}\p{N}]+/gu, "");
}

function tokenize(text: string): string[] {
  return text
    .split(/\s+/g)
    .map(normalizeToken)
    .filter(Boolean);
}

function tokenToDimension(token: string): number {
  let hash = 0;
  for (let index = 0; index < token.length; index += 1) {
    hash = (hash * 31 + token.charCodeAt(index)) >>> 0;
  }

  return hash % config.embeddingDimensions;
}

function expandSemanticTokens(tokens: string[]): string[] {
  const expanded = [...tokens];

  for (const token of tokens) {
    for (const [semanticToken, variants] of Object.entries(tokenSynonyms)) {
      if (variants.some((variant) => token.includes(variant))) {
        expanded.push(semanticToken);
      }
    }
  }

  return expanded;
}

export function buildLocalEmbedding(text: string): number[] {
  const vector = new Array<number>(config.embeddingDimensions).fill(0);
  const tokens = expandSemanticTokens(tokenize(text));

  for (const token of tokens) {
    const dimension = tokenToDimension(token);
    vector[dimension] += 1;
  }

  const magnitude = Math.sqrt(vector.reduce((sum, value) => sum + value * value, 0));
  if (magnitude === 0) {
    return vector;
  }

  return vector.map((value) => Number((value / magnitude).toFixed(6)));
}
