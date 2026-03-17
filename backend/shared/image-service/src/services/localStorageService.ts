import { mkdir, unlink } from "fs/promises";
import path from "path";
import { randomUUID } from "crypto";
import sharp from "sharp";

import type ImageSaveResponse from "../api/dtos/imageSaveResponse";
import type ImageFile from "../models/ImageFile";
import type IImageService from "../interfaces/IImageService";

const UPLOADS_DIR = path.resolve(process.cwd(), "uploads");

const buildPublicUrl = (relativePath: string): string => {
  const publicBaseUrl = process.env.PUBLIC_BASE_URL?.trim();
  if (!publicBaseUrl) {
    return relativePath;
  }

  const normalizedBaseUrl = publicBaseUrl.replace(/\/+$/, "");
  const normalizedRelativePath = relativePath.replace(/^\/+/, "");
  return new URL(normalizedRelativePath, `${normalizedBaseUrl}/`).toString();
};

class LocalStorageService implements IImageService {
  async saveImage(file: ImageFile): Promise<ImageSaveResponse> {
    await mkdir(UPLOADS_DIR, { recursive: true });

    const imageId = `${randomUUID()}.webp`;
    const filePath = path.join(UPLOADS_DIR, imageId);

    await sharp(file.buffer).rotate().webp({ quality: 85 }).toFile(filePath);

    return {
      imageId,
      imageUrl: buildPublicUrl(`/public/${imageId}`)
    };
  }

  async deleteImage(imageId: string): Promise<void> {
    const safeImageId = path.basename(imageId);

    if (safeImageId !== imageId) {
      throw new Error("Invalid image id");
    }

    const filePath = path.join(UPLOADS_DIR, safeImageId);

    try {
      await unlink(filePath);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        throw new Error("Image not found");
      }

      throw error;
    }
  }
}

export default LocalStorageService;
