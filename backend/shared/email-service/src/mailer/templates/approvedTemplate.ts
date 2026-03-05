import { escapeHtml } from "./escapeHtml.ts";
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

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Заявка одобрена</h2>
      <p>Здравствуйте, <b>${escapeHtml(fullName)}</b>!</p>
      <p>Ваша заявка одобрена.</p>
      <p><b>Логин:</b> ${escapeHtml(loginEmail)}</p>
      <p>
        <a href="${setPasswordUrl}" style="display:inline-block;padding:10px 14px;text-decoration:none;border-radius:6px;border:1px solid #ccc;">
          Установить пароль
        </a>
      </p>
      <p style="color:#666;font-size:12px">Если это были не вы — проигнорируйте письмо.</p>
    </div>
  `;

  return { subject, text, html };
}
