const rawApiUrl = import.meta.env.VITE_API_URL || "http://localhost:9186";
const normalizedApiUrl = rawApiUrl.replace(/\/+$/, "").replace(/\/api$/i, "");
const absoluteUrlPattern = /^[a-z][a-z\d+\-.]*:/i;
const imageMimeTypePattern = /^image\//i;

export function isImageMimeType(mimeType?: string | null): boolean {
  return !!mimeType?.trim() && imageMimeTypePattern.test(mimeType.trim());
}

export function resolveAttachmentPreviewUrl(url?: string | null): string | null {
  const trimmedUrl = url?.trim();

  if (!trimmedUrl) {
    return null;
  }

  if (absoluteUrlPattern.test(trimmedUrl)) {
    return trimmedUrl;
  }

  return new URL(trimmedUrl, `${normalizedApiUrl}/`).toString();
}
