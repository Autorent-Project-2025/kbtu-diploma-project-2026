import type { IncomingMessage, ServerResponse } from "node:http";
import type { createMailer } from "../mailer/mailer.ts";
import {
  approvedTemplate,
  partnerCarApprovedTemplate,
  partnerCarRejectedTemplate,
  partnerApprovedTemplate,
  partnerRejectedTemplate,
  rejectedTemplate,
} from "../mailer/templates/index.ts";
import type { MailTemplate } from "../mailer/templates/types.ts";
import {
  BadRequestError,
  optionalString,
  readJsonBody,
  requiredString,
  sendJson,
} from "./httpUtils.ts";

type Mailer = ReturnType<typeof createMailer>;

async function sendTemplate(
  mailer: Mailer,
  res: ServerResponse,
  to: string,
  template: MailTemplate,
) {
  const result = await mailer.sendMail({
    to,
    subject: template.subject,
    text: template.text,
    html: template.html,
  });

  sendJson(res, 200, { message: "Email sent", ...result });
}

async function handleApprovedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const loginEmail = requiredString(body.loginEmail, "loginEmail");
  const setPasswordUrl = requiredString(body.setPasswordUrl, "setPasswordUrl");

  await sendTemplate(
    mailer,
    res,
    to,
    approvedTemplate({ fullName, loginEmail, setPasswordUrl }),
  );
}

async function handleRejectedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const reason = optionalString(body.reason, "reason");

  await sendTemplate(mailer, res, to, rejectedTemplate({ fullName, reason }));
}

async function handlePartnerApprovedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const loginEmail = requiredString(body.loginEmail, "loginEmail");
  const setPasswordUrl = requiredString(body.setPasswordUrl, "setPasswordUrl");

  await sendTemplate(
    mailer,
    res,
    to,
    partnerApprovedTemplate({ fullName, loginEmail, setPasswordUrl }),
  );
}

async function handlePartnerRejectedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const reason = optionalString(body.reason, "reason");

  await sendTemplate(mailer, res, to, partnerRejectedTemplate({ fullName, reason }));
}

async function handlePartnerCarApprovedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const carBrand = requiredString(body.carBrand, "carBrand");
  const carModel = requiredString(body.carModel, "carModel");
  const licensePlate = requiredString(body.licensePlate, "licensePlate");

  await sendTemplate(
    mailer,
    res,
    to,
    partnerCarApprovedTemplate({
      fullName,
      carBrand,
      carModel,
      licensePlate,
    }),
  );
}

async function handlePartnerCarRejectedEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const fullName = requiredString(body.fullName, "fullName");
  const carBrand = requiredString(body.carBrand, "carBrand");
  const carModel = requiredString(body.carModel, "carModel");
  const licensePlate = requiredString(body.licensePlate, "licensePlate");
  const reason = optionalString(body.reason, "reason");

  await sendTemplate(
    mailer,
    res,
    to,
    partnerCarRejectedTemplate({
      fullName,
      carBrand,
      carModel,
      licensePlate,
      reason,
    }),
  );
}

async function handleCustomEmail(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
) {
  const body = await readJsonBody(req);
  const to = requiredString(body.to, "to");
  const subject = requiredString(body.subject, "subject");
  const text = optionalString(body.text, "text");
  const html = optionalString(body.html, "html");
  const replyTo = optionalString(body.replyTo, "replyTo");

  if (!text && !html) {
    throw new BadRequestError("Either text or html must be provided");
  }

  const result = await mailer.sendMail({ to, subject, text, html, replyTo });
  sendJson(res, 200, { message: "Email sent", ...result });
}

export async function handleEmailRoute(
  req: IncomingMessage,
  res: ServerResponse,
  mailer: Mailer,
  url: URL,
): Promise<boolean> {
  if (req.method === "GET" && (url.pathname === "/health" || url.pathname === "/healthz")) {
    sendJson(res, 200, { status: "ok" });
    return true;
  }

  if (req.method !== "POST") return false;

  switch (url.pathname) {
    case "/emails/approved":
      await handleApprovedEmail(req, res, mailer);
      return true;

    case "/emails/rejected":
      await handleRejectedEmail(req, res, mailer);
      return true;

    case "/emails/partners/approved":
      await handlePartnerApprovedEmail(req, res, mailer);
      return true;

    case "/emails/partners/rejected":
      await handlePartnerRejectedEmail(req, res, mailer);
      return true;

    case "/emails/partners/cars/approved":
      await handlePartnerCarApprovedEmail(req, res, mailer);
      return true;

    case "/emails/partners/cars/rejected":
      await handlePartnerCarRejectedEmail(req, res, mailer);
      return true;

    case "/emails/custom":
      await handleCustomEmail(req, res, mailer);
      return true;

    default:
      return false;
  }
}
