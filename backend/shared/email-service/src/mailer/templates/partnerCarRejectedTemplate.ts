import { escapeHtml } from "./escapeHtml.ts";
import {
  renderCallout,
  renderDetailTable,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
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

  const html = renderEmailLayout({
    title: "Заявка на машину отклонена",
    preheader: "К сожалению, заявка на добавление машины отклонена.",
    eyebrow: "Статус автомобиля",
    tone: "danger",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(params.fullName)}</strong>!`) +
      renderParagraph("К сожалению, заявка на добавление машины отклонена.") +
      renderDetailTable([
        { label: "Машина", value: `${params.carBrand} ${params.carModel}` },
        { label: "Гос номер", value: params.licensePlate },
      ]) +
      (params.reason ? renderCallout("Причина", params.reason, "danger") : "") +
      renderParagraph("Вы можете исправить данные и отправить заявку повторно."),
  });

  return { subject, text, html };
}
