import path from "path";
import { randomBytes } from "crypto";
import { Bucket, Storage } from "@google-cloud/storage";
import sharp from "sharp";

import type IImageService from "../interfaces/IImageService";
import type ImageFile from "../models/ImageFile";
import type ImageSaveResponse from "../api/dtos/imageSaveResponse";

const IMAGE_EXTENSION = "webp";
const IMAGE_CONTENT_TYPE = "image/webp";

class GoogleCloudService implements IImageService {
  private readonly bucket: Bucket;

  constructor() {
    let bucketName = process.env.GCLOUD_BUCKET;

    if (!bucketName) {
      throw new Error("GCLOUD_BUCKET is required when USE_WEB_STORAGE=true");
    }

    bucketName = bucketName;

    const storageOptions: ConstructorParameters<typeof Storage>[0] = {};

    if (process.env.GCLOUD_PROJECT_ID) {
      storageOptions.projectId = process.env.GCLOUD_PROJECT_ID;
    }

    if (process.env.GCLOUD_CLIENT_EMAIL && process.env.GCLOUD_PRIVATE_KEY) {
      storageOptions.credentials = {
        client_email: process.env.GCLOUD_CLIENT_EMAIL,
        private_key: process.env.GCLOUD_PRIVATE_KEY.replace(/\\n/g, "\n")
      };
    }

    const storage = new Storage(storageOptions);
    this.bucket = storage.bucket(bucketName);
  }

  async saveImage(file: ImageFile): Promise<ImageSaveResponse> {
    const imageId = `${randomBytes(16).toString("hex")}.${IMAGE_EXTENSION}`;
    const fileRef = this.bucket.file(imageId);

    const imageBuffer = await sharp(file.buffer)
      .rotate()
      .webp({ quality: 85 })
      .toBuffer();

    await fileRef.save(imageBuffer, {
      contentType: IMAGE_CONTENT_TYPE,
      resumable: false,
      metadata: {
        cacheControl: "public, max-age=31536000, immutable"
      }
    });

    return {
      imageId,
      imageUrl: fileRef.publicUrl()
    };
  }

  async deleteImage(imageId: string): Promise<void> {
    const safeImageId = path.basename(imageId);

    if (safeImageId !== imageId) {
      throw new Error("Invalid image id");
    }

    try {
      await this.bucket.file(safeImageId).delete();
    } catch (error) {
      const statusCode = (error as { code?: number | string }).code;

      if (statusCode === 404 || statusCode === "404") {
        throw new Error("Image not found");
      }

      throw error;
    }
  }
}

export default GoogleCloudService;
