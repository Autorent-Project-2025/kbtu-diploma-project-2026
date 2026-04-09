import { config } from "../config/env";

export async function createEmbeddingWithOpenAi(text: string): Promise<number[]> {
  if (!config.openAiApiKey) {
    throw new Error("OPENAI_API_KEY is not configured.");
  }

  const response = await fetch(`${config.openAiBaseUrl.replace(/\/$/, "")}/embeddings`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${config.openAiApiKey}`,
    },
    body: JSON.stringify({
      model: config.openAiEmbeddingModel,
      input: text,
    }),
  });

  if (!response.ok) {
    throw new Error(`OpenAI embeddings request failed with status ${response.status}.`);
  }

  const payload = (await response.json()) as {
    data?: Array<{ embedding?: number[] }>;
  };

  const embedding = payload.data?.[0]?.embedding;
  if (!embedding || !Array.isArray(embedding)) {
    throw new Error("OpenAI embeddings response did not contain an embedding.");
  }

  return embedding.map((value) => Number(value));
}
