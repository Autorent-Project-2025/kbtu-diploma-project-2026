import { escapeHtml } from "./escapeHtml.ts";

type EmailTone = "success" | "danger" | "info" | "warning";

type EmailLayoutParams = {
  title: string;
  preheader: string;
  bodyHtml: string;
  tone?: EmailTone;
  eyebrow?: string;
  footerNote?: string;
};

type DetailItem = {
  label: string;
  value: string;
};

const tones: Record<
  EmailTone,
  {
    accent: string;
    accentDark: string;
    accentSoft: string;
    text: string;
  }
> = {
  success: {
    accent: "#10b981",
    accentDark: "#047857",
    accentSoft: "#ecfdf5",
    text: "#065f46",
  },
  danger: {
    accent: "#ef4444",
    accentDark: "#b91c1c",
    accentSoft: "#fef2f2",
    text: "#991b1b",
  },
  info: {
    accent: "#2563eb",
    accentDark: "#1d4ed8",
    accentSoft: "#eff6ff",
    text: "#1e40af",
  },
  warning: {
    accent: "#f59e0b",
    accentDark: "#b45309",
    accentSoft: "#fffbeb",
    text: "#92400e",
  },
};

export function renderParagraph(html: string): string {
  return `<p style="margin:0 0 16px;color:#374151;font-size:16px;line-height:1.6;">${html}</p>`;
}

export function renderButton(label: string, href: string, tone: EmailTone = "info"): string {
  const palette = tones[tone];

  return `
    <table role="presentation" cellpadding="0" cellspacing="0" style="margin:26px 0 10px;">
      <tr>
        <td style="border-radius:12px;background:${palette.accentDark};">
          <a href="${escapeHtml(href)}" style="display:inline-block;padding:14px 20px;color:#ffffff;font-size:15px;font-weight:700;text-decoration:none;border-radius:12px;background:${palette.accentDark};">
            ${escapeHtml(label)}
          </a>
        </td>
      </tr>
    </table>
  `;
}

export function renderDetailTable(items: DetailItem[]): string {
  if (items.length === 0) return "";

  const rows = items
    .map(
      (item) => `
        <tr>
          <td style="padding:12px 0;border-bottom:1px solid #e5e7eb;color:#6b7280;font-size:13px;font-weight:700;text-transform:uppercase;letter-spacing:0.04em;vertical-align:top;width:38%;">
            ${escapeHtml(item.label)}
          </td>
          <td style="padding:12px 0;border-bottom:1px solid #e5e7eb;color:#111827;font-size:15px;font-weight:700;vertical-align:top;">
            ${escapeHtml(item.value)}
          </td>
        </tr>
      `,
    )
    .join("");

  return `
    <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;margin:22px 0;border-collapse:collapse;background:#f9fafb;border:1px solid #e5e7eb;border-radius:14px;">
      <tr>
        <td style="padding:6px 18px 6px;">
          <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;border-collapse:collapse;">
            ${rows}
          </table>
        </td>
      </tr>
    </table>
  `;
}

export function renderCallout(title: string, text: string, tone: EmailTone = "info"): string {
  const palette = tones[tone];

  return `
    <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;margin:22px 0;border-collapse:collapse;">
      <tr>
        <td style="padding:16px 18px;border-left:4px solid ${palette.accent};border-radius:12px;background:${palette.accentSoft};">
          <p style="margin:0 0 6px;color:${palette.text};font-size:13px;font-weight:800;text-transform:uppercase;letter-spacing:0.04em;">
            ${escapeHtml(title)}
          </p>
          <p style="margin:0;color:${palette.text};font-size:15px;line-height:1.55;">
            ${escapeHtml(text)}
          </p>
        </td>
      </tr>
    </table>
  `;
}

export function renderQuote(text: string): string {
  return `
    <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;margin:22px 0;border-collapse:collapse;">
      <tr>
        <td style="padding:18px 20px;border-left:4px solid #2563eb;border-radius:12px;background:#f9fafb;color:#374151;font-size:16px;line-height:1.6;font-style:italic;">
          “${escapeHtml(text)}”
        </td>
      </tr>
    </table>
  `;
}

export function renderEmailLayout(params: EmailLayoutParams): string {
  const tone = params.tone ?? "info";
  const palette = tones[tone];
  const eyebrow = params.eyebrow ?? "AutoRent";

  return `<!doctype html>
<html lang="ru">
  <head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width,initial-scale=1">
    <meta name="color-scheme" content="light">
    <title>${escapeHtml(params.title)}</title>
  </head>
  <body style="margin:0;padding:0;background:#f3f4f6;font-family:Arial,'Helvetica Neue',Helvetica,sans-serif;">
    <div style="display:none;max-height:0;overflow:hidden;opacity:0;color:transparent;">
      ${escapeHtml(params.preheader)}
    </div>
    <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;background:#f3f4f6;border-collapse:collapse;">
      <tr>
        <td align="center" style="padding:32px 16px;">
          <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;max-width:640px;border-collapse:collapse;">
            <tr>
              <td style="padding:0 0 16px;">
                <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;border-collapse:collapse;">
                  <tr>
                    <td style="vertical-align:middle;">
                      <div style="display:inline-block;width:42px;height:42px;border-radius:12px;background:${palette.accentDark};color:#ffffff;text-align:center;line-height:42px;font-weight:900;font-size:14px;">
                        AR
                      </div>
                    </td>
                    <td style="padding-left:12px;vertical-align:middle;">
                      <p style="margin:0;color:#111827;font-size:16px;font-weight:800;">AutoRent</p>
                      <p style="margin:2px 0 0;color:#6b7280;font-size:13px;">Уведомление платформы</p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td style="overflow:hidden;border-radius:22px;background:#ffffff;border:1px solid #e5e7eb;box-shadow:0 18px 45px rgba(15,23,42,0.10);">
                <table role="presentation" cellpadding="0" cellspacing="0" style="width:100%;border-collapse:collapse;">
                  <tr>
                    <td style="padding:30px 32px 24px;background:${palette.accentSoft};border-bottom:1px solid #e5e7eb;">
                      <p style="margin:0 0 10px;color:${palette.text};font-size:12px;font-weight:800;text-transform:uppercase;letter-spacing:0.12em;">
                        ${escapeHtml(eyebrow)}
                      </p>
                      <h1 style="margin:0;color:#111827;font-size:28px;line-height:1.18;font-weight:900;">
                        ${escapeHtml(params.title)}
                      </h1>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:30px 32px 26px;">
                      ${params.bodyHtml}
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
            <tr>
              <td style="padding:18px 8px 0;text-align:center;">
                <p style="margin:0;color:#6b7280;font-size:12px;line-height:1.6;">
                  ${escapeHtml(params.footerNote ?? "Если письмо пришло по ошибке, просто проигнорируйте его.")}
                </p>
                <p style="margin:8px 0 0;color:#9ca3af;font-size:12px;line-height:1.5;">
                  AutoRent • automated notification
                </p>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>`;
}
