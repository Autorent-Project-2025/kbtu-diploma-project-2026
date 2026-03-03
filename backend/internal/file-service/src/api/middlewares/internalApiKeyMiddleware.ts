import { createHash, timingSafeEqual } from "crypto";
import { RequestHandler } from "express";

const InternalApiKeyHeader = "x-internal-api-key";

const sha256 = (value: string): Buffer => {
  return createHash("sha256").update(value, "utf8").digest();
};

export const requireInternalApiKey: RequestHandler = (req, res, next) => {
  const configuredApiKey = process.env.INTERNAL_API_KEY ?? process.env.InternalAuth__ApiKey;
  if (!configuredApiKey || !configuredApiKey.trim()) {
    return res.status(500).json({ message: "Internal API key is not configured" });
  }

  const receivedApiKey = req.header(InternalApiKeyHeader);
  if (!receivedApiKey) {
    return res.status(401).json({ message: "Internal API key is required" });
  }

  const expectedHash = sha256(configuredApiKey.trim());
  const receivedHash = sha256(receivedApiKey.trim());

  if (!timingSafeEqual(expectedHash, receivedHash)) {
    return res.status(401).json({ message: "Invalid internal API key" });
  }

  return next();
};
