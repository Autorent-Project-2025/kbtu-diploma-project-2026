import { Request, Response } from "express";

import type IFileService from "../../interfaces/IFileService";

type FileLinkRequestBody = {
  fileName?: unknown;
  ttlSeconds?: unknown;
};

class FilesController {
  constructor(private readonly fileService: IFileService) {}

  uploadFile = async (req: Request, res: Response) => {
    const sourceFileName = req.header("x-file-name");
    const fileBody = req.body;

    if (!sourceFileName) {
      return res.status(400).json({ message: "x-file-name header is required" });
    }

    if (!Buffer.isBuffer(fileBody) || fileBody.length === 0) {
      return res.status(400).json({ message: "File body is required" });
    }

    try {
      const result = await this.fileService.saveFile({
        fileName: sourceFileName,
        contentType: req.header("content-type") || "application/octet-stream",
        buffer: fileBody
      });

      return res.status(201).json(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Internal server error";
      const statusCode = message === "Invalid file name" ? 400 : 500;
      return res.status(statusCode).json({ message });
    }
  };

  getTemporaryLink = async (req: Request, res: Response) => {
    const body = (req.body || {}) as FileLinkRequestBody;
    const fileName = typeof body.fileName === "string" ? body.fileName : "";
    const ttlSeconds =
      typeof body.ttlSeconds === "number" && Number.isFinite(body.ttlSeconds)
        ? body.ttlSeconds
        : undefined;

    if (!fileName.trim()) {
      return res.status(400).json({ message: "fileName is required" });
    }

    try {
      const result = await this.fileService.getTemporaryReadLink(fileName, ttlSeconds);
      return res.status(200).json(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Internal server error";
      const statusCode = this.resolveStatusCode(message);
      return res.status(statusCode).json({ message });
    }
  };

  deleteFile = async (req: Request, res: Response) => {
    const fileNameParam = req.params.fileName;
    const fileName = Array.isArray(fileNameParam) ? fileNameParam[0] : fileNameParam;

    if (!fileName) {
      return res.status(400).json({ message: "fileName is required" });
    }

    try {
      await this.fileService.deleteFile(fileName);
      return res.status(200).json({ message: "File deleted", fileName });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Internal server error";
      const statusCode = this.resolveStatusCode(message);
      return res.status(statusCode).json({ message });
    }
  };

  private resolveStatusCode(errorMessage: string): number {
    if (errorMessage === "File not found") {
      return 404;
    }

    if (errorMessage === "Invalid file name" || errorMessage === "File name is required") {
      return 400;
    }

    return 500;
  }
}

export default FilesController;
