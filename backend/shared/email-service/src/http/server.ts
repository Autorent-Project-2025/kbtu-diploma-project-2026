import { createServer, type IncomingMessage, type ServerResponse } from "node:http";
import type { createMailer } from "../mailer/mailer.ts";
import { observabilityLogger } from "../observability/logger.ts";
import { handleEmailRoute } from "./emailRoutes.ts";
import { BadRequestError, sendJson } from "./httpUtils.ts";

type Mailer = ReturnType<typeof createMailer>;

function setCorsHeaders(res: ServerResponse) {
  res.setHeader("Access-Control-Allow-Origin", "*");
  res.setHeader("Access-Control-Allow-Headers", "Content-Type, Authorization");
  res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
}

function handleOptions(req: IncomingMessage, res: ServerResponse): boolean {
  if (req.method !== "OPTIONS") return false;

  res.writeHead(204);
  res.end();
  return true;
}

export function createEmailHttpServer(mailer: Mailer) {
  return createServer(async (req, res) => {
    setCorsHeaders(res);

    if (handleOptions(req, res)) {
      return;
    }

    const url = new URL(req.url ?? "/", "http://localhost");

    try {
      const handled = await handleEmailRoute(req, res, mailer, url);
      if (!handled) {
        sendJson(res, 404, { message: "Route not found" });
      }
    } catch (error) {
      if (error instanceof BadRequestError) {
        sendJson(res, 400, { message: error.message });
        return;
      }

      console.error(error);
      observabilityLogger.error("http_request_failed", error, {
        method: req.method ?? null,
        path: url.pathname,
      });
      sendJson(res, 500, { message: "Internal server error" });
    }
  });
}
