import { config } from "../config/env";

function ensureBaseUrl(): string {
  if (!config.localLlmBaseUrl) {
    throw new Error("LOCAL_LLM_BASE_URL is not configured.");
  }

  return config.localLlmBaseUrl.replace(/\/$/, "");
}

export async function postToOllama<TResponse>(
  path: string,
  payload: Record<string, unknown>,
): Promise<TResponse> {
  const baseUrl = ensureBaseUrl();
  const controller = new AbortController();
  const timeoutHandle = setTimeout(
    () => controller.abort(),
    config.localLlmTimeoutSeconds * 1000,
  );

  try {
    const response = await fetch(`${baseUrl}${path}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
      signal: controller.signal,
    });

    if (!response.ok) {
      throw new Error(
        `Ollama request to ${path} failed with status ${response.status}.`,
      );
    }

    return (await response.json()) as TResponse;
  } finally {
    clearTimeout(timeoutHandle);
  }
}
