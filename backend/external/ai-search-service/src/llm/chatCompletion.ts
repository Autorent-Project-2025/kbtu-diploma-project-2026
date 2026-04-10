import { config } from "../config/env";
import { postToOllama } from "./ollamaClient";

type ResponseType = "text" | "json";

type CompletionOptions = {
  systemPrompt: string;
  userPrompt: string;
  responseType: ResponseType;
  temperature?: number;
  maxOutputTokens?: number;
};

type CompletionProvider = "local" | "openai";

export type LlmCompletionResult = {
  content: string;
  model: string;
  provider: CompletionProvider;
};

type OllamaChatResponse = {
  message?: {
    content?: string | null;
  };
  response?: string | null;
};

function unwrapJson(content: string): string {
  const trimmed = content.trim();
  if (!trimmed.startsWith("```")) {
    return trimmed;
  }

  return trimmed
    .replace(/^```(?:json)?/i, "")
    .replace(/```$/i, "")
    .trim();
}

function normalizeContent(content: string, responseType: ResponseType): string {
  const trimmed = content.trim();
  return responseType === "json" ? unwrapJson(trimmed) : trimmed.replace(/\s+/g, " ").trim();
}

function getConfiguredProvider(): CompletionProvider | null {
  if (config.localLlmBaseUrl) {
    return "local";
  }

  if (config.openAiApiKey) {
    return "openai";
  }

  return null;
}

export function describeConfiguredLlm(): { provider: CompletionProvider; model: string } | null {
  const provider = getConfiguredProvider();
  if (provider === "local") {
    return {
      provider,
      model: config.localLlmChatModel,
    };
  }

  if (provider === "openai") {
    return {
      provider,
      model: config.openAiChatModel,
    };
  }

  return null;
}

async function completeWithLocalLlm(
  options: CompletionOptions,
): Promise<LlmCompletionResult> {
  const payload = await postToOllama<OllamaChatResponse>("/api/chat", {
    model: config.localLlmChatModel,
    stream: false,
    ...(options.responseType === "json" ? { format: "json" } : {}),
    options: {
      temperature: options.temperature ?? 0,
      num_predict: options.maxOutputTokens ?? 160,
    },
    messages: [
      {
        role: "system",
        content: options.systemPrompt.trim(),
      },
      {
        role: "user",
        content: options.userPrompt.trim(),
      },
    ],
  });

  const content = payload.message?.content?.trim() || payload.response?.trim() || "";
  if (!content) {
    throw new Error("Local LLM completion returned empty content.");
  }

  return {
    content: normalizeContent(content, options.responseType),
    model: config.localLlmChatModel,
    provider: "local",
  };
}

async function completeWithOpenAi(
  options: CompletionOptions,
): Promise<LlmCompletionResult> {
  const response = await fetch(`${config.openAiBaseUrl.replace(/\/$/, "")}/chat/completions`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${config.openAiApiKey}`,
    },
    body: JSON.stringify({
      model: config.openAiChatModel,
      temperature: options.temperature ?? 0,
      max_tokens: options.maxOutputTokens ?? 160,
      ...(options.responseType === "json"
        ? { response_format: { type: "json_object" } }
        : {}),
      messages: [
        {
          role: "system",
          content: options.systemPrompt.trim(),
        },
        {
          role: "user",
          content: options.userPrompt.trim(),
        },
      ],
    }),
  });

  if (!response.ok) {
    throw new Error(`OpenAI chat request failed with status ${response.status}.`);
  }

  const payload = (await response.json()) as {
    choices?: Array<{ message?: { content?: string | null } }>;
  };

  const content = payload.choices?.[0]?.message?.content?.trim() || "";
  if (!content) {
    throw new Error("OpenAI completion returned empty content.");
  }

  return {
    content: normalizeContent(content, options.responseType),
    model: config.openAiChatModel,
    provider: "openai",
  };
}

export async function completeWithPreferredLlm(
  options: CompletionOptions,
): Promise<LlmCompletionResult | null> {
  const provider = getConfiguredProvider();
  if (provider === "local") {
    return completeWithLocalLlm(options);
  }

  if (provider === "openai") {
    return completeWithOpenAi(options);
  }

  return null;
}
