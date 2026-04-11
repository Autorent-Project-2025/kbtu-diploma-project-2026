import { escapeHtml } from "./escapeHtml.ts";
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

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Новое сообщение</h2>
      <p>Здравствуйте, <b>${escapeHtml(recipientName)}</b>!</p>
      <p>У вас новое сообщение от <b>${escapeHtml(senderName)}</b>:</p>
      <blockquote style="border-left:3px solid #ddd;padding-left:12px;color:#555;margin:12px 0;">
        ${escapeHtml(messagePreview)}
      </blockquote>
      <p>Откройте платформу для просмотра и ответа.</p>
      <p style="color:#666;font-size:12px">Если это письмо пришло по ошибке — проигнорируйте его.</p>
    </div>
  `;

  return { subject, text, html };
}
