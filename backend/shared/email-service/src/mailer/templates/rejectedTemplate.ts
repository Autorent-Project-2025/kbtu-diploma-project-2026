import { escapeHtml } from "./escapeHtml.ts";
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

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Заявка отклонена</h2>
      <p>Здравствуйте, <b>${escapeHtml(params.fullName)}</b>!</p>
      <p>К сожалению, ваша заявка отклонена.</p>
      ${params.reason ? `<p><b>Причина:</b> ${escapeHtml(params.reason)}</p>` : ""}
      <p>Вы можете подать заявку повторно.</p>
    </div>
  `;

  return { subject, text, html };
}
