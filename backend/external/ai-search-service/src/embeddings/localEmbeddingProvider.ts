import { config } from "../config/env";
import { LOCAL_EMBEDDING_TOKEN_SYNONYMS } from "../queryTaxonomy";

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
    for (const [semanticToken, variants] of Object.entries(LOCAL_EMBEDDING_TOKEN_SYNONYMS)) {
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
