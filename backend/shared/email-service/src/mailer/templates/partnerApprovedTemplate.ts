import { escapeHtml } from "./escapeHtml.ts";
import {
  renderButton,
  renderCallout,
  renderDetailTable,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
import type { MailTemplate } from "./types.ts";

type PartnerApprovedTemplateParams = {
  fullName: string;
  loginEmail: string;
  setPasswordUrl: string;
};

export function partnerApprovedTemplate(params: PartnerApprovedTemplateParams): MailTemplate {
  const { fullName, loginEmail, setPasswordUrl } = params;

  const subject = "Ваша партнерская заявка одобрена";
  const text =
    `Здравствуйте, ${fullName}!\n\n` +
    `Ваша заявка на партнерство одобрена.\n` +
    `Логин: ${loginEmail}\n` +
    `Установить пароль: ${setPasswordUrl}\n\n` +
    `После входа вы увидите карточку партнера.\n` +
    `Если это были не вы — проигнорируйте письмо.`;

  const html = renderEmailLayout({
    title: "Партнерская заявка одобрена",
    preheader: "Ваша заявка на партнерство одобрена. Установите пароль для входа.",
    eyebrow: "Партнерский доступ",
    tone: "success",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(fullName)}</strong>!`) +
      renderParagraph("Ваша заявка на партнерство одобрена. Теперь можно войти в партнерский кабинет.") +
      renderDetailTable([
        { label: "Логин", value: loginEmail },
        { label: "Раздел", value: "Партнерский кабинет" },
      ]) +
      renderButton("Установить пароль", setPasswordUrl, "success") +
      renderCallout("Что дальше", "После входа вы увидите карточку партнера и сможете управлять автомобилями.", "info"),
  });

  return { subject, text, html };
}
