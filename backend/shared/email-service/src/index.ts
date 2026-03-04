import { createServer, type IncomingMessage, type ServerResponse } from "node:http";
import { createMailer } from "./mailer/mailer.ts";
import {
  approvedTemplate,
  partnerCarApprovedTemplate,
  partnerCarRejectedTemplate,
  partnerApprovedTemplate,
  partnerRejectedTemplate,
  rejectedTemplate,
} from "./mailer/templates.ts";

type JsonBody = Record<string, unknown>;

class BadRequestError extends Error {}

function sendJson(res: ServerResponse, statusCode: number, payload: Record<string, unknown>) {
  res.writeHead(statusCode, { "Content-Type": "application/json; charset=utf-8" });
  res.end(JSON.stringify(payload));
}

function requiredString(value: unknown, field: string): string {
  if (typeof value !== "string" || value.trim() === "") {
    throw new BadRequestError(`${field} is required`);
  }

  return value.trim();
}

function optionalString(value: unknown, field: string): string | undefined {
  if (value === undefined || value === null || value === "") return undefined;
  if (typeof value !== "string") throw new BadRequestError(`${field} must be a string`);

  return value.trim();
}

function parsePort(rawPort: string | undefined): number {
  if (!rawPort) return 8080;

  const port = Number(rawPort);
  if (!Number.isInteger(port) || port <= 0 || port > 65535) {
    throw new Error(`Invalid PORT value: "${rawPort}"`);
  }

  return port;
}

async function readJsonBody(req: IncomingMessage): Promise<JsonBody> {
  const chunks: Buffer[] = [];
  let size = 0;
  const maxSize = 1024 * 1024;

  for await (const chunk of req) {
    const bufferChunk = Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk);
    size += bufferChunk.length;

    if (size > maxSize) {
      throw new BadRequestError("Request body is too large");
    }

    chunks.push(bufferChunk);
  }

  if (chunks.length === 0) return {};

  let parsed: unknown;

  try {
    parsed = JSON.parse(Buffer.concat(chunks).toString("utf8"));
  } catch {
    throw new BadRequestError("Invalid JSON body");
  }

  if (typeof parsed !== "object" || parsed === null || Array.isArray(parsed)) {
    throw new BadRequestError("JSON body must be an object");
  }

  return parsed as JsonBody;
}

async function main() {
  const mailer = createMailer();
  const port = parsePort(process.env.PORT);

  if (process.env.SMTP_VERIFY_ON_STARTUP === "true") {
    await mailer.verify();
    console.log("SMTP connection verified");
  }

  const server = createServer(async (req, res) => {
    res.setHeader("Access-Control-Allow-Origin", "*");
    res.setHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
    res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

    if (req.method === "OPTIONS") {
      res.writeHead(204);
      res.end();
      return;
    }

    const url = new URL(req.url ?? "/", "http://localhost");

    try {
      if (req.method === "GET" && url.pathname === "/health") {
        sendJson(res, 200, { status: "ok" });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/approved") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const loginEmail = requiredString(body.loginEmail, "loginEmail");
        const setPasswordUrl = requiredString(body.setPasswordUrl, "setPasswordUrl");

        const template = approvedTemplate({ fullName, loginEmail, setPasswordUrl });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/rejected") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const reason = optionalString(body.reason, "reason");

        const template = rejectedTemplate({ fullName, reason });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/partners/approved") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const loginEmail = requiredString(body.loginEmail, "loginEmail");
        const setPasswordUrl = requiredString(body.setPasswordUrl, "setPasswordUrl");

        const template = partnerApprovedTemplate({ fullName, loginEmail, setPasswordUrl });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/partners/rejected") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const reason = optionalString(body.reason, "reason");

        const template = partnerRejectedTemplate({ fullName, reason });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/partners/cars/approved") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const carBrand = requiredString(body.carBrand, "carBrand");
        const carModel = requiredString(body.carModel, "carModel");
        const licensePlate = requiredString(body.licensePlate, "licensePlate");

        const template = partnerCarApprovedTemplate({
          fullName,
          carBrand,
          carModel,
          licensePlate,
        });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/partners/cars/rejected") {
        const body = await readJsonBody(req);
        const to = requiredString(body.to, "to");
        const fullName = requiredString(body.fullName, "fullName");
        const carBrand = requiredString(body.carBrand, "carBrand");
        const carModel = requiredString(body.carModel, "carModel");
        const licensePlate = requiredString(body.licensePlate, "licensePlate");
        const reason = optionalString(body.reason, "reason");

        const template = partnerCarRejectedTemplate({
          fullName,
          carBrand,
          carModel,
          licensePlate,
          reason,
        });
        const result = await mailer.sendMail({
          to,
          subject: template.subject,
          text: template.text,
          html: template.html,
        });

        sendJson(res, 200, { message: "Email sent", ...result });
        return;
      }

      if (req.method === "POST" && url.pathname === "/emails/custom") {
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
        return;
      }

      sendJson(res, 404, { message: "Route not found" });
    } catch (error) {
      if (error instanceof BadRequestError) {
        sendJson(res, 400, { message: error.message });
        return;
      }

      console.error(error);
      sendJson(res, 500, { message: "Internal server error" });
    }
  });

  server.listen(port, () => {
    console.log(`Email service is running on port ${port}`);
  });
}

main().catch((error) => {
  console.error(error);
  process.exit(1);
});
