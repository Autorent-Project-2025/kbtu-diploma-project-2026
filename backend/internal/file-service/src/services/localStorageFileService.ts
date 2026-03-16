import { mkdir, unlink, writeFile } from "fs/promises";
import path from "path";

import type IFileService from "../interfaces/IFileService";
import type {
  TemporaryFileLinkResult,
  UploadFilePayload,
  UploadFileResult
} from "../interfaces/IFileService";
import { buildStoredFileName, normalizeFileName } from "../utils/fileName";

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

class LocalStorageFileService implements IFileService {
  async saveFile(payload: UploadFilePayload): Promise<UploadFileResult> {
    await mkdir(UPLOADS_DIR, { recursive: true });

    const fileName = buildStoredFileName(payload.fileName);
    const filePath = path.join(UPLOADS_DIR, fileName);
    await writeFile(filePath, payload.buffer);

    return { fileName };
  }

  async getTemporaryReadLink(fileNameInput: string): Promise<TemporaryFileLinkResult> {
    const fileName = normalizeFileName(fileNameInput);

    return {
      fileName,
      url: buildPublicUrl(`/public/${encodeURIComponent(fileName)}`),
      expiresAtUtc: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
    };
  }

  async deleteFile(fileNameInput: string): Promise<void> {
    const fileName = normalizeFileName(fileNameInput);
    const filePath = path.join(UPLOADS_DIR, fileName);

    try {
      await unlink(filePath);
    } catch (error) {
      if ((error as NodeJS.ErrnoException).code === "ENOENT") {
        throw new Error("File not found");
      }

      throw error;
    }
  }
}

export default LocalStorageFileService;
