import { escapeHtml } from "./escapeHtml.ts";
import {
  renderCallout,
  renderEmailLayout,
  renderParagraph,
  renderQuote,
} from "./layout.ts";
import type { MailTemplate } from "./types.ts";

type ChatNewMessageTemplateParams = {
  recipientName: string;
  senderName: string;
  contextType: string;
  contextId: string;
  messagePreview: string;
};

export function chatNewMessageTemplate(params: ChatNewMessageTemplateParams): MailTemplate {
  const { recipientName, senderName, contextType, contextId, messagePreview } = params;

  const contextLabel = contextType === "complaint" ? "жалобе" : "диалогу";
  const subject = `Новое сообщение по ${contextLabel} #${contextId.slice(0, 8)}`;

  const text =
    `Здравствуйте, ${recipientName}!\n\n` +
    `У вас новое сообщение от ${senderName}.\n\n` +
    `"${messagePreview}"\n\n` +
    `Откройте платформу для просмотра и ответа.`;

  const html = renderEmailLayout({
    title: "Новое сообщение",
    preheader: `Новое сообщение от ${senderName} по ${contextLabel} #${contextId.slice(0, 8)}.`,
    eyebrow: "Сообщение в чате",
    tone: "info",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(recipientName)}</strong>!`) +
      renderParagraph(`У вас новое сообщение от <strong>${escapeHtml(senderName)}</strong>:`) +
      renderQuote(messagePreview) +
      renderCallout("Действие", "Откройте платформу для просмотра и ответа.", "info"),
  });

  return { subject, text, html };
}
