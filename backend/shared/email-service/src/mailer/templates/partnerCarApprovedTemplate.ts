import { escapeHtml } from "./escapeHtml.ts";
import type { MailTemplate } from "./types.ts";

type PartnerCarApprovedTemplateParams = {
  fullName: string;
  carBrand: string;
  carModel: string;
  licensePlate: string;
};

export function partnerCarApprovedTemplate(params: PartnerCarApprovedTemplateParams): MailTemplate {
  const subject = "Заявка на добавление машины одобрена";
  const text =
    `Здравствуйте, ${params.fullName}!\n\n` +
    `Ваша заявка на добавление машины одобрена.\n` +
    `Машина: ${params.carBrand} ${params.carModel}\n` +
    `Гос номер: ${params.licensePlate}\n\n` +
    `Автомобиль уже добавлен в ваш партнерский кабинет.`;

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Заявка на машину одобрена</h2>
      <p>Здравствуйте, <b>${escapeHtml(params.fullName)}</b>!</p>
      <p>Ваша заявка на добавление машины одобрена.</p>
      <p><b>Машина:</b> ${escapeHtml(params.carBrand)} ${escapeHtml(params.carModel)}</p>
      <p><b>Гос номер:</b> ${escapeHtml(params.licensePlate)}</p>
      <p>Автомобиль уже добавлен в ваш партнерский кабинет.</p>
    </div>
  `;

  return { subject, text, html };
}
