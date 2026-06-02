import { ref, type Ref } from "vue";
import { getComplaintAttachmentLink } from "../api/complaints";
import type { Complaint, ComplaintAttachment } from "../types/Complaint";
import {
  isImageMimeType,
  resolveAttachmentPreviewUrl,
} from "../utils/attachmentPreview";
import { useToast } from "./useToast";

/**
 * Handles temporary-link resolution, image preview caching, download and
 * open-in-new-tab behaviour for complaint attachments. Depends on the
 * `complaint` ref owned by the parent composable.
 */
export function useComplaintAttachments(complaint: Ref<Complaint | null>) {
  const toast = useToast();
  const previewUrls = ref<Record<string, string>>({});

  async function downloadAttachment(attachmentId: string, fileName: string) {
    if (!complaint.value) return;
    try {
      const link = await getComplaintAttachmentLink(
        complaint.value.id,
        attachmentId,
      );
      window.open(link.url, "_blank");
    } catch {
      toast.error(`Ошибка при загрузке файла: ${fileName}`);
    }
  }

  async function ensurePreview(
    attachment: ComplaintAttachment,
  ): Promise<string | null> {
    if (!complaint.value || !isImageMimeType(attachment.fileType)) {
      return null;
    }

    const existing = previewUrls.value[attachment.id];
    if (existing) {
      return existing;
    }

    try {
      const link = await getComplaintAttachmentLink(
        complaint.value.id,
        attachment.id,
      );
      const resolvedUrl = resolveAttachmentPreviewUrl(link.url);
      if (!resolvedUrl) {
        return null;
      }

      previewUrls.value = {
        ...previewUrls.value,
        [attachment.id]: resolvedUrl,
      };

      return resolvedUrl;
    } catch {
      return null;
    }
  }

  async function preloadPreviews(
    targetComplaint: Complaint | null,
  ): Promise<void> {
    if (!targetComplaint) {
      return;
    }

    await Promise.all(
      targetComplaint.attachments
        .filter((attachment) => isImageMimeType(attachment.fileType))
        .map((attachment) => ensurePreview(attachment)),
    );
  }

  async function openPreview(attachment: ComplaintAttachment): Promise<void> {
    const previewUrl = await ensurePreview(attachment);

    if (previewUrl) {
      window.open(previewUrl, "_blank");
      return;
    }

    await downloadAttachment(attachment.id, attachment.originalFileName);
  }

  return {
    previewUrls,
    downloadAttachment,
    ensurePreview,
    preloadPreviews,
    openPreview,
  };
}
