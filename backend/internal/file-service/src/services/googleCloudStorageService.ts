import { Bucket, Storage } from "@google-cloud/storage";

import type IFileService from "../interfaces/IFileService";
import type {
  TemporaryFileLinkResult,
  UploadFilePayload,
  UploadFileResult
} from "../interfaces/IFileService";
import { buildStoredFileName, normalizeFileName } from "../utils/fileName";

const MAX_SIGNED_LINK_TTL_SECONDS = 24 * 60 * 60;
const DEFAULT_SIGNED_LINK_TTL_SECONDS = 15 * 60;

class GoogleCloudStorageService implements IFileService {
  private readonly bucket: Bucket;
  private readonly defaultSignedLinkTtlSeconds: number;

  constructor() {
    let bucketName = process.env.GCLOUD_BUCKET;

    if (!bucketName) {
      throw new Error("GCLOUD_BUCKET is required");
    }

    bucketName = bucketName.trim();
    this.defaultSignedLinkTtlSeconds = this.resolveSignedLinkTtl(
      Number(process.env.SIGNED_URL_TTL_SECONDS)
    );

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

  async saveFile(payload: UploadFilePayload): Promise<UploadFileResult> {
    const fileName = buildStoredFileName(payload.fileName);
    const fileRef = this.bucket.file(fileName);

    await fileRef.save(payload.buffer, {
      contentType: payload.contentType || "application/octet-stream",
      resumable: false,
      metadata: {
        cacheControl: "private, max-age=0, no-cache"
      }
    });

    return { fileName };
  }

  async getTemporaryReadLink(
    inputFileName: string,
    ttlSeconds?: number
  ): Promise<TemporaryFileLinkResult> {
    const fileName = normalizeFileName(inputFileName);
    const fileRef = this.bucket.file(fileName);
    const [exists] = await fileRef.exists();

    if (!exists) {
      throw new Error("File not found");
    }

    const effectiveTtlSeconds = this.resolveSignedLinkTtl(ttlSeconds ?? this.defaultSignedLinkTtlSeconds);
    const expiresAt = new Date(Date.now() + effectiveTtlSeconds * 1000);
    const [url] = await fileRef.getSignedUrl({
      version: "v4",
      action: "read",
      expires: expiresAt
    });

    return {
      fileName,
      url,
      expiresAtUtc: expiresAt.toISOString()
    };
  }

  async deleteFile(inputFileName: string): Promise<void> {
    const fileName = normalizeFileName(inputFileName);

    try {
      await this.bucket.file(fileName).delete();
    } catch (error) {
      const statusCode = (error as { code?: number | string }).code;

      if (statusCode === 404 || statusCode === "404") {
        throw new Error("File not found");
      }

      throw error;
    }
  }

  private resolveSignedLinkTtl(ttlSeconds: number): number {
    if (!Number.isFinite(ttlSeconds) || ttlSeconds <= 0) {
      return DEFAULT_SIGNED_LINK_TTL_SECONDS;
    }

    return Math.min(Math.floor(ttlSeconds), MAX_SIGNED_LINK_TTL_SECONDS);
  }
}

export default GoogleCloudStorageService;
