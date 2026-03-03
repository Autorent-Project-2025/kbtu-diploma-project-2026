import path from "path";
import { randomUUID } from "crypto";

const MAX_EXTENSION_LENGTH = 10;

export const normalizeFileName = (fileName: string): string => {
  const trimmedFileName = fileName.trim();

  if (!trimmedFileName) {
    throw new Error("File name is required");
  }

  if (trimmedFileName.includes("\0")) {
    throw new Error("Invalid file name");
  }

  const safeFileName = path.basename(trimmedFileName);

  if (safeFileName !== trimmedFileName) {
    throw new Error("Invalid file name");
  }

  return safeFileName;
};

export const buildStoredFileName = (sourceFileName: string): string => {
  const normalizedFileName = normalizeFileName(sourceFileName);
  const fileExtension = path.extname(normalizedFileName).toLowerCase();
  const normalizedExtension =
    fileExtension && fileExtension.length <= MAX_EXTENSION_LENGTH ? fileExtension : ".bin";

  return `${randomUUID()}${normalizedExtension}`;
};
