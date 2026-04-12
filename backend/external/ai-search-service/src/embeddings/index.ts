import { config } from "../config/env";
import { buildLocalEmbedding } from "./localEmbeddingProvider";
import { createEmbeddingWithLocalLlm } from "./localLlmEmbeddingProvider";
import { createEmbeddingWithOpenAi } from "./openAiEmbeddingProvider";
import { observabilityLogger } from "../observability/logger";

function normalizeDimensions(values: number[]): number[] {
  if (values.length === config.embeddingDimensions) {
    return values;
  }

  if (values.length > config.embeddingDimensions) {
    return values.slice(0, config.embeddingDimensions);
  }

  return [
    ...values,
    ...new Array<number>(config.embeddingDimensions - values.length).fill(0),
  ];
}

export async function createEmbedding(text: string): Promise<number[]> {
  if (config.localLlmBaseUrl) {
    try {
      const embedding = await createEmbeddingWithLocalLlm(text);
      observabilityLogger.info("local_llm_embedding_succeeded", {
        model: config.localLlmEmbeddingModel,
      });
      return normalizeDimensions(embedding);
    } catch (error) {
      observabilityLogger.warn("local_llm_embedding_failed_fallback_to_local", {
        errorMessage: error instanceof Error ? error.message : String(error),
        model: config.localLlmEmbeddingModel,
      });
      return normalizeDimensions(buildLocalEmbedding(text));
    }
  }

  if (!config.openAiApiKey) {
    return normalizeDimensions(buildLocalEmbedding(text));
  }

  try {
    return normalizeDimensions(await createEmbeddingWithOpenAi(text));
  } catch (error) {
    observabilityLogger.warn("openai_embedding_failed_fallback_to_local", {
      errorMessage: error instanceof Error ? error.message : String(error),
    });
    return normalizeDimensions(buildLocalEmbedding(text));
  }
}
