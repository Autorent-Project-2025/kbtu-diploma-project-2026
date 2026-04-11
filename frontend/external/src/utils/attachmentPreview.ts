import { resolveAssetUrl } from "./resolveAssetUrl";

const imageMimeTypePattern = /^image\//i;

export function isImageMimeType(mimeType?: string | null): boolean {
  return !!mimeType?.trim() && imageMimeTypePattern.test(mimeType.trim());
}

export function resolveAttachmentPreviewUrl(url?: string | null): string | null {
  return resolveAssetUrl(url);
}
