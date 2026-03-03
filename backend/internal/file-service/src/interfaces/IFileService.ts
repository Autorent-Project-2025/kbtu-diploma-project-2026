export interface UploadFilePayload {
  fileName: string;
  contentType: string;
  buffer: Buffer;
}

export interface UploadFileResult {
  fileName: string;
}

export interface TemporaryFileLinkResult {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

interface IFileService {
  saveFile(payload: UploadFilePayload): Promise<UploadFileResult>;
  getTemporaryReadLink(fileName: string, ttlSeconds?: number): Promise<TemporaryFileLinkResult>;
  deleteFile(fileName: string): Promise<void>;
}

export default IFileService;
