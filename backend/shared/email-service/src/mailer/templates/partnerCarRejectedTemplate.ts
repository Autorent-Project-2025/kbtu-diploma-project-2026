import { escapeHtml } from "./escapeHtml.ts";
import type { MailTemplate } from "./types.ts";

type PartnerCarRejectedTemplateParams = {
  fullName: string;
  carBrand: string;
  carModel: string;
  licensePlate: string;
  reason?: string;
};

export function partnerCarRejectedTemplate(params: PartnerCarRejectedTemplateParams): MailTemplate {
  const subject = "Заявка на добавление машины отклонена";
  const text =
    `Здравствуйте, ${params.fullName}!\n\n` +
    `К сожалению, заявка на добавление машины отклонена.\n` +
    `Машина: ${params.carBrand} ${params.carModel}\n` +
    `Гос номер: ${params.licensePlate}\n` +
    (params.reason ? `Причина: ${params.reason}\n` : "") +
    `\nВы можете отправить заявку повторно.`;

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Заявка на машину отклонена</h2>
      <p>Здравствуйте, <b>${escapeHtml(params.fullName)}</b>!</p>
      <p>К сожалению, заявка на добавление машины отклонена.</p>
      <p><b>Машина:</b> ${escapeHtml(params.carBrand)} ${escapeHtml(params.carModel)}</p>
      <p><b>Гос номер:</b> ${escapeHtml(params.licensePlate)}</p>
      ${params.reason ? `<p><b>Причина:</b> ${escapeHtml(params.reason)}</p>` : ""}
      <p>Вы можете отправить заявку повторно.</p>
    </div>
  `;

  return { subject, text, html };
}
