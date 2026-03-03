import { RequestHandler, Router } from "express";

import FilesController from "../controllers/filesController";
import { requireInternalApiKey } from "../middlewares/internalApiKeyMiddleware";

export const createInternalFilesRouter = (
  filesController: FilesController,
  rawFileBodyParser: RequestHandler
) => {
  const filesRouter = Router();

  filesRouter.post(
    "/upload",
    requireInternalApiKey,
    rawFileBodyParser,
    filesController.uploadFile
  );

  filesRouter.post(
    "/temporary-link",
    requireInternalApiKey,
    filesController.getTemporaryLink
  );

  return filesRouter;
};
