import type { IncomingMessage, ServerResponse } from "node:http";

export type JsonBody = Record<string, unknown>;

export class BadRequestError extends Error {}

export function sendJson(
  res: ServerResponse,
  statusCode: number,
  payload: Record<string, unknown>,
) {
  res.writeHead(statusCode, { "Content-Type": "application/json; charset=utf-8" });
  res.end(JSON.stringify(payload));
}

export function requiredString(value: unknown, field: string): string {
  if (typeof value !== "string" || value.trim() === "") {
    throw new BadRequestError(`${field} is required`);
  }

  return value.trim();
}

export function optionalString(value: unknown, field: string): string | undefined {
  if (value === undefined || value === null || value === "") return undefined;
  if (typeof value !== "string") throw new BadRequestError(`${field} must be a string`);

  return value.trim();
}

export function parsePort(rawPort: string | undefined): number {
  if (!rawPort) return 8080;

  const port = Number(rawPort);
  if (!Number.isInteger(port) || port <= 0 || port > 65535) {
    throw new Error(`Invalid PORT value: "${rawPort}"`);
  }

  return port;
}

export async function readJsonBody(req: IncomingMessage): Promise<JsonBody> {
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
