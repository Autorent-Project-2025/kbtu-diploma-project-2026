import { escapeHtml } from "./escapeHtml.ts";
import {
  renderCallout,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
import type { MailTemplate } from "./types.ts";

type PartnerRejectedTemplateParams = {
  fullName: string;
  reason?: string;
};

export function partnerRejectedTemplate(params: PartnerRejectedTemplateParams): MailTemplate {
  const subject = "Ваша партнерская заявка отклонена";
  const text =
    `Здравствуйте, ${params.fullName}!\n\n` +
    `К сожалению, заявка на партнерство отклонена.` +
    (params.reason ? `\nПричина: ${params.reason}\n` : "\n") +
    `\nВы можете подать заявку повторно.`;

  const html = renderEmailLayout({
    title: "Партнерская заявка отклонена",
    preheader: "К сожалению, заявка на партнерство отклонена. Вы можете подать заявку повторно.",
    eyebrow: "Статус заявки",
    tone: "danger",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(params.fullName)}</strong>!`) +
      renderParagraph("К сожалению, заявка на партнерство отклонена.") +
      (params.reason ? renderCallout("Причина", params.reason, "danger") : "") +
      renderParagraph("Вы можете исправить данные и подать заявку повторно."),
  });

  return { subject, text, html };
}
