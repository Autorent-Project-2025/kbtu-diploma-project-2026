import { config } from "../config";

const absoluteUrlPattern = /^[a-z][a-z\d+\-.]*:/i;

export function resolveAssetUrl(url?: string | null): string | null {
  const trimmedUrl = url?.trim();

  if (!trimmedUrl) {
    return null;
  }

  if (absoluteUrlPattern.test(trimmedUrl)) {
    return trimmedUrl;
  }

  return new URL(trimmedUrl, `${config.api.baseURL}/`).toString();
}
