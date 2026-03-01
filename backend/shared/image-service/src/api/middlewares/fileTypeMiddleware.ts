import { Request, Response, NextFunction } from "express";
import sharp from "sharp";

import type ImageFile from "../../models/ImageFile";

const ALLOWED_FORMATS = new Set(["jpeg", "png", "webp"]);

const parseContentType = (contentTypeHeader: string | string[] | undefined) => {
  const contentTypeValue = Array.isArray(contentTypeHeader) ? contentTypeHeader[0] : contentTypeHeader;
  return contentTypeValue?.split(";")[0].trim().toLowerCase();
};

const toMimeType = (format: string) => (format === "jpeg" ? "image/jpeg" : `image/${format}`);

export const fileTypeMiddleware = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  const contentType = parseContentType(req.headers["content-type"]);

  if (contentType !== "application/octet-stream") {
    return res.status(415).json({ message: "Content-Type must be application/octet-stream" });
  }

  if (!Buffer.isBuffer(req.body) || req.body.length === 0) {
    return res.status(400).json({ message: "File is required" });
  }

  try {
    const metadata = await sharp(req.body).metadata();

    if (!metadata.format || !ALLOWED_FORMATS.has(metadata.format)) {
      return res.status(400).json({ message: "Invalid file type" });
    }

    const file: ImageFile = {
      buffer: req.body,
      mimetype: toMimeType(metadata.format),
      size: req.body.length
    };

    res.locals.imageFile = file;
    return next();
  } catch {
    return res.status(400).json({ message: "Invalid file type" });
  }
};
