import { escapeHtml } from "./escapeHtml.ts";
import {
  renderCallout,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
import type { MailTemplate } from "./types.ts";

type RejectedTemplateParams = {
  fullName: string;
  reason?: string;
};

export function rejectedTemplate(params: RejectedTemplateParams): MailTemplate {
  const subject = "Ваша заявка отклонена";
  const text =
    `Здравствуйте, ${params.fullName}!\n\n` +
    `К сожалению, ваша заявка отклонена.` +
    (params.reason ? `\nПричина: ${params.reason}\n` : "\n") +
    `\nВы можете подать заявку повторно.`;

  const html = renderEmailLayout({
    title: "Заявка отклонена",
    preheader: "К сожалению, ваша заявка отклонена. Вы можете подать заявку повторно.",
    eyebrow: "Статус заявки",
    tone: "danger",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(params.fullName)}</strong>!`) +
      renderParagraph("К сожалению, ваша заявка отклонена.") +
      (params.reason ? renderCallout("Причина", params.reason, "danger") : "") +
      renderParagraph("Вы можете исправить данные и подать заявку повторно."),
  });

  return { subject, text, html };
}
