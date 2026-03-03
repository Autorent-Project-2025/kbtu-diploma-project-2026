import { createVerify } from "crypto";
import { RequestHandler } from "express";

interface JwtPayload {
  sub?: unknown;
  exp?: unknown;
  nbf?: unknown;
  iss?: unknown;
  aud?: unknown;
  permissions?: unknown;
}

interface AuthenticatedUser {
  subject: string | null;
  permissions: Set<string>;
}

const parseBearerToken = (authorizationHeader: string | undefined): string | null => {
  if (!authorizationHeader) {
    return null;
  }

  const [scheme, token] = authorizationHeader.split(" ");

  if (!scheme || !token || scheme.toLowerCase() !== "bearer") {
    return null;
  }

  return token.trim();
};

const decodeBase64UrlToUtf8 = (value: string): string => {
  const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
  return Buffer.from(padded, "base64").toString("utf8");
};

const decodeBase64UrlToBuffer = (value: string): Buffer => {
  const base64 = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
  return Buffer.from(padded, "base64");
};

const getJwtPublicKey = (): string => {
  const rawPublicKey = process.env.JWT_PUBLIC_KEY ?? process.env.Jwt__PublicKey;

  if (!rawPublicKey) {
    throw new Error("JWT public key is not configured");
  }

  return rawPublicKey.replace(/\\n/g, "\n").trim();
};

const extractPayload = (token: string): JwtPayload => {
  const tokenParts = token.split(".");

  if (tokenParts.length !== 3) {
    throw new Error("Invalid token format");
  }

  const [encodedHeader, encodedPayload, encodedSignature] = tokenParts;
  const header = JSON.parse(decodeBase64UrlToUtf8(encodedHeader)) as { alg?: unknown };

  if (header.alg !== "RS256") {
    throw new Error("Unsupported token algorithm");
  }

  const signature = decodeBase64UrlToBuffer(encodedSignature);
  const signingInput = `${encodedHeader}.${encodedPayload}`;

  const verifier = createVerify("RSA-SHA256");
  verifier.update(signingInput);
  verifier.end();

  const isSignatureValid = verifier.verify(getJwtPublicKey(), signature);

  if (!isSignatureValid) {
    throw new Error("Invalid token signature");
  }

  const payload = JSON.parse(decodeBase64UrlToUtf8(encodedPayload)) as JwtPayload;
  const nowSeconds = Math.floor(Date.now() / 1000);

  if (typeof payload.nbf === "number" && payload.nbf > nowSeconds) {
    throw new Error("Token is not active yet");
  }

  if (typeof payload.exp === "number" && payload.exp <= nowSeconds) {
    throw new Error("Token is expired");
  }

  const expectedIssuer = process.env.JWT_ISSUER ?? process.env.Jwt__Issuer;
  const expectedAudience = process.env.JWT_AUDIENCE ?? process.env.Jwt__Audience;

  if (expectedIssuer && payload.iss !== expectedIssuer) {
    throw new Error("Invalid token issuer");
  }

  if (expectedAudience) {
    const audiences = Array.isArray(payload.aud) ? payload.aud : [payload.aud];
    const audienceMatch = audiences.some((audience) => audience === expectedAudience);

    if (!audienceMatch) {
      throw new Error("Invalid token audience");
    }
  }

  return payload;
};

const extractPermissions = (payload: JwtPayload): Set<string> => {
  if (Array.isArray(payload.permissions)) {
    return new Set(
      payload.permissions
        .filter((permission): permission is string => typeof permission === "string")
        .map((permission) => permission.trim())
        .filter(Boolean)
    );
  }

  if (typeof payload.permissions === "string") {
    const singlePermission = payload.permissions.trim();

    if (singlePermission) {
      return new Set([singlePermission]);
    }
  }

  return new Set<string>();
};

export const authenticateJwt: RequestHandler = (req, res, next) => {
  const token = parseBearerToken(req.header("authorization"));

  if (!token) {
    return res.status(401).json({ message: "Authorization header with Bearer token is required" });
  }

  try {
    const payload = extractPayload(token);
    const subject = typeof payload.sub === "string" ? payload.sub : null;
    const permissions = extractPermissions(payload);

    const authenticatedUser: AuthenticatedUser = {
      subject,
      permissions
    };

    res.locals.authenticatedUser = authenticatedUser;
    return next();
  } catch {
    return res.status(401).json({ message: "Unauthorized" });
  }
};

export const requirePermission = (permission: string): RequestHandler => {
  return (_req, res, next) => {
    const authenticatedUser = res.locals.authenticatedUser as AuthenticatedUser | undefined;

    if (!authenticatedUser) {
      return res.status(401).json({ message: "Unauthorized" });
    }

    if (!authenticatedUser.permissions.has(permission)) {
      return res.status(403).json({ message: "Forbidden" });
    }

    return next();
  };
};
