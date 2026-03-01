import express, { NextFunction, Request, Response } from "express";
import cors from "cors";

import ImagesController from "./api/controllers/imagesController";
import { createApiRouter, createPublicRouter } from "./api/routes";
import GoogleCloudService from "./services/googleCloudService";
import LocalStorageService from "./services/localStorageService";

const configuredMaxFileSizeMb = Number(process.env.MAX_FILE_SIZE_MB);
const maxFileSizeMb =
  Number.isFinite(configuredMaxFileSizeMb) && configuredMaxFileSizeMb > 0
    ? configuredMaxFileSizeMb
    : 10;
const maxFileSizeBytes = Math.floor(maxFileSizeMb * 1024 * 1024);

const app = express();
const rawImageBodyParser = express.raw({
  type: "application/octet-stream",
  limit: maxFileSizeBytes
});

const useWebStorage = process.env.USE_WEB_STORAGE === "true";
const imageService = useWebStorage ? new GoogleCloudService() : new LocalStorageService();
const imagesController = new ImagesController(imageService);

app.use(cors());
app.use(express.json());
app.use("/api", createApiRouter({ imagesController, rawImageBodyParser }));
app.use("/public", createPublicRouter());

app.use((error: unknown, _req: Request, res: Response, _next: NextFunction) => {
  const bodyParserError = error as { type?: string; status?: number };

  if (bodyParserError?.type === "entity.too.large" || bodyParserError?.status === 413) {
    return res.status(400).json({ message: `File size exceeds limit (${maxFileSizeMb}MB)` });
  }
  
  return res.status(500).json({ message: "Internal server error" });
});

const PORT = Number(process.env.PORT || 8080);

app.listen(PORT, () => {
  console.log(`Image service is running on port ${PORT}`);
});
