import { config } from "../config/env";
import { postToOllama } from "../llm/ollamaClient";

type OllamaEmbedResponse = {
  embeddings?: number[][] | number[];
  embedding?: number[];
};

function normalizeEmbedding(payload: OllamaEmbedResponse): number[] {
  const embeddedArray = payload.embeddings;

  if (
    Array.isArray(embeddedArray) &&
    embeddedArray.length > 0 &&
    Array.isArray(embeddedArray[0])
  ) {
    return (embeddedArray[0] as number[]).map((value) => Number(value));
  }

  if (
    Array.isArray(embeddedArray) &&
    embeddedArray.length > 0 &&
    typeof embeddedArray[0] === "number"
  ) {
    return (embeddedArray as number[]).map((value) => Number(value));
  }

  if (Array.isArray(payload.embedding) && payload.embedding.length > 0) {
    return payload.embedding.map((value) => Number(value));
  }

  throw new Error("Local LLM embeddings response did not contain an embedding.");
}

export async function createEmbeddingWithLocalLlm(
  text: string,
): Promise<number[]> {
  if (!config.localLlmBaseUrl) {
    throw new Error("LOCAL_LLM_BASE_URL is not configured.");
  }

  try {
    const payload = await postToOllama<OllamaEmbedResponse>("/api/embed", {
      model: config.localLlmEmbeddingModel,
      input: text,
    });

    return normalizeEmbedding(payload);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    if (!message.includes("/api/embed")) {
      throw error;
    }

    const payload = await postToOllama<OllamaEmbedResponse>("/api/embeddings", {
      model: config.localLlmEmbeddingModel,
      prompt: text,
    });

    return normalizeEmbedding(payload);
  }
}
