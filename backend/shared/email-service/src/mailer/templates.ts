export function approvedTemplate(params: {
  fullName: string;
  loginEmail: string;
  setPasswordUrl: string; // лучше ссылку, а не пароль
}) {
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

export function rejectedTemplate(params: { fullName: string; reason?: string }) {
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

export function partnerApprovedTemplate(params: {
  fullName: string;
  loginEmail: string;
  setPasswordUrl: string;
}) {
  const { fullName, loginEmail, setPasswordUrl } = params;

  const subject = "Ваша партнерская заявка одобрена";
  const text =
    `Здравствуйте, ${fullName}!\n\n` +
    `Ваша заявка на партнерство одобрена.\n` +
    `Логин: ${loginEmail}\n` +
    `Установить пароль: ${setPasswordUrl}\n\n` +
    `После входа вы увидите карточку партнера.\n` +
    `Если это были не вы — проигнорируйте письмо.`;

  const html = `
    <div style="font-family: Arial, sans-serif; line-height: 1.5">
      <h2>Партнерская заявка одобрена</h2>
      <p>Здравствуйте, <b>${escapeHtml(fullName)}</b>!</p>
      <p>Ваша заявка на партнерство одобрена.</p>
      <p><b>Логин:</b> ${escapeHtml(loginEmail)}</p>
      <p>
        <a href="${setPasswordUrl}" style="display:inline-block;padding:10px 14px;text-decoration:none;border-radius:6px;border:1px solid #ccc;">
          Установить пароль
        </a>
      </p>
      <p>После входа вы увидите карточку партнера.</p>
      <p style="color:#666;font-size:12px">Если это были не вы — проигнорируйте письмо.</p>
    </div>
  `;

  return { subject, text, html };
}

export function partnerRejectedTemplate(params: { fullName: string; reason?: string }) {
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

// минимальная защита от инъекций в HTML
function escapeHtml(s: string) {
  return s
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}
