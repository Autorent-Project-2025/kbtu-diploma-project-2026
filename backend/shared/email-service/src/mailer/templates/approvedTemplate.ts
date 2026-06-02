import { escapeHtml } from "./escapeHtml.ts";
import {
  renderButton,
  renderCallout,
  renderDetailTable,
  renderEmailLayout,
  renderParagraph,
} from "./layout.ts";
import type { MailTemplate } from "./types.ts";

type ApprovedTemplateParams = {
  fullName: string;
  loginEmail: string;
  setPasswordUrl: string;
};

export function approvedTemplate(params: ApprovedTemplateParams): MailTemplate {
  const { fullName, loginEmail, setPasswordUrl } = params;

  const subject = "Ваша заявка одобрена";
  const text =
    `Здравствуйте, ${fullName}!\n\n` +
    `Ваша заявка одобрена.\n` +
    `Логин: ${loginEmail}\n` +
    `Установить пароль: ${setPasswordUrl}\n\n` +
    `Если это были не вы — проигнорируйте письмо.`;

  const html = renderEmailLayout({
    title: "Заявка одобрена",
    preheader: "Ваша заявка одобрена. Установите пароль для входа в AutoRent.",
    eyebrow: "Доступ открыт",
    tone: "success",
    bodyHtml:
      renderParagraph(`Здравствуйте, <strong>${escapeHtml(fullName)}</strong>!`) +
      renderParagraph("Ваша заявка одобрена. Осталось установить пароль для входа в кабинет.") +
      renderDetailTable([{ label: "Логин", value: loginEmail }]) +
      renderButton("Установить пароль", setPasswordUrl, "success") +
      renderCallout("Безопасность", "Если это были не вы — проигнорируйте письмо.", "info"),
  });

  return { subject, text, html };
}
