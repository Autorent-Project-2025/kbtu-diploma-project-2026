import { escapeHtml } from "./escapeHtml.ts";
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

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Партнерская заявка отклонена</h2>
      <p>Здравствуйте, <b>${escapeHtml(params.fullName)}</b>!</p>
      <p>К сожалению, заявка на партнерство отклонена.</p>
      ${params.reason ? `<p><b>Причина:</b> ${escapeHtml(params.reason)}</p>` : ""}
      <p>Вы можете подать заявку повторно.</p>
    </div>
  `;

  return { subject, text, html };
}
