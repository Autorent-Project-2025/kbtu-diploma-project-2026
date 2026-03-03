import cors from "cors";
import express, { NextFunction, Request, Response } from "express";

import FilesController from "./api/controllers/filesController";
import { createApiRouter, createPublicRouter } from "./api/routes";
import GoogleCloudStorageService from "./services/googleCloudStorageService";
import LocalStorageFileService from "./services/localStorageFileService";

const configuredMaxFileSizeMb = Number(process.env.MAX_FILE_SIZE_MB);
const maxFileSizeMb =
  Number.isFinite(configuredMaxFileSizeMb) && configuredMaxFileSizeMb > 0
    ? configuredMaxFileSizeMb
    : 10;
const maxFileSizeBytes = Math.floor(maxFileSizeMb * 1024 * 1024);

const app = express();
const rawFileBodyParser = express.raw({
  // Upload endpoints accept arbitrary binary content types (pdf/image/etc.).
  // Restricting only to application/octet-stream leaves req.body empty for application/pdf.
  type: () => true,
  limit: maxFileSizeBytes
});

const useWebStorage = process.env.USE_WEB_STORAGE !== "false";
const fileService = useWebStorage
  ? new GoogleCloudStorageService()
  : new LocalStorageFileService();
const filesController = new FilesController(fileService);

app.use(cors());
app.use(express.json());
app.use("/api", createApiRouter({ filesController, rawFileBodyParser }));

if (!useWebStorage) {
  app.use("/public", createPublicRouter());
}

app.use((error: unknown, _req: Request, res: Response, _next: NextFunction) => {
  const bodyParserError = error as { type?: string; status?: number };

  if (bodyParserError?.type === "entity.too.large" || bodyParserError?.status === 413) {
    return res.status(400).json({ message: `File size exceeds limit (${maxFileSizeMb}MB)` });
  }

  return res.status(500).json({ message: "Internal server error" });
});

const port = Number(process.env.PORT || 8080);

app.listen(port, () => {
  console.log(`File service is running on port ${port}`);
});
