import { RequestHandler, Router } from "express";

import FilesController from "../controllers/filesController";
import { createFilesRouter } from "./filesRouter";
import { createInternalFilesRouter } from "./internalFilesRouter";
import { createStorageRouter } from "./storageRouter";

interface CreateApiRouterArgs {
  filesController: FilesController;
  rawFileBodyParser: RequestHandler;
}

export const createApiRouter = ({ filesController, rawFileBodyParser }: CreateApiRouterArgs) => {
  const apiRouter = Router();

  apiRouter.use("/files", createFilesRouter(filesController, rawFileBodyParser));
  apiRouter.use("/internal/files", createInternalFilesRouter(filesController, rawFileBodyParser));

  return apiRouter;
};

export const createPublicRouter = () => createStorageRouter();
