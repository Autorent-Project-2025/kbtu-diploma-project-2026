import nodemailer from "nodemailer";

type SmtpConfig = {
  host: string;
  port: number;
  user: string;
  pass: string;
  from: string;
  secure: boolean;
};

function requireEnv(name: string): string {
  const v = process.env[name];
  if (!v) throw new Error(`Missing env: ${name}`);
  return v.trim();
}

function parsePort(value: string): number {
  const parsed = Number(value);
  if (!Number.isInteger(parsed) || parsed <= 0) {
    throw new Error(`Invalid SMTP_PORT value: "${value}"`);
  }

  return parsed;
}

function parseBoolean(value: string | undefined): boolean | undefined {
  if (value === undefined) return undefined;

  const normalized = value.trim().toLowerCase();
  if (normalized === "true") return true;
  if (normalized === "false") return false;

  throw new Error(`Invalid boolean value: "${value}"`);
}

export function createMailer(cfg?: Partial<SmtpConfig>) {
  const port = cfg?.port ?? parsePort(requireEnv("SMTP_PORT"));
  const secureFromEnv = parseBoolean(process.env.SMTP_SECURE);

  const config: SmtpConfig = {
    host: cfg?.host ?? requireEnv("SMTP_HOST"),
    port,
    user: cfg?.user ?? requireEnv("SMTP_USER"),
    pass: cfg?.pass ?? requireEnv("SMTP_PASS"),
    from: cfg?.from ?? requireEnv("SMTP_FROM"),
    secure: cfg?.secure ?? secureFromEnv ?? port === 465,
  };

  const transporter = nodemailer.createTransport({
    host: config.host,
    port: config.port,
    secure: config.secure,
    auth: {
      user: config.user,
      pass: config.pass,
    },
  });

  async function verify() {
    await transporter.verify();
  }

  async function sendMail(params: {
    to: string | string[];
    subject: string;
    text?: string;
    html?: string;
    replyTo?: string;
  }) {
    const info = await transporter.sendMail({
      from: config.from,
      to: params.to,
      subject: params.subject,
      text: params.text,
      html: params.html,
      replyTo: params.replyTo,
    });

    return {
      messageId: info.messageId,
      accepted: info.accepted,
      rejected: info.rejected,
    };
  }

  return { verify, sendMail };
}
