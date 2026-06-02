import { escapeHtml } from "./escapeHtml.ts";
import {
  renderCallout,
  renderDetailTable,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
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

  const html = renderEmailLayout({
    title: "Заявка на машину одобрена",
    preheader: "Автомобиль добавлен в ваш партнерский кабинет.",
    eyebrow: "Автомобиль одобрен",
    tone: "success",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(params.fullName)}</strong>!`) +
      renderParagraph("Ваша заявка на добавление машины одобрена.") +
      renderDetailTable([
        { label: "Машина", value: `${params.carBrand} ${params.carModel}` },
        { label: "Гос номер", value: params.licensePlate },
      ]) +
      renderCallout("Готово", "Автомобиль уже добавлен в ваш партнерский кабинет.", "success"),
  });

  return { subject, text, html };
}
