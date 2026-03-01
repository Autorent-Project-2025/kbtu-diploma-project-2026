import { Router, RequestHandler } from "express";

import ImagesController from "../controllers/imagesController";
import { createImagesRouter } from "./imagesRouter";
import { createStorageRouter } from "./storageRouter";

interface CreateApiRouterArgs {
  imagesController: ImagesController;
  rawImageBodyParser: RequestHandler;
}

export const createApiRouter = ({ imagesController, rawImageBodyParser }: CreateApiRouterArgs) => {
  const apiRouter = Router();

  apiRouter.use("/images", createImagesRouter(imagesController, rawImageBodyParser));

  return apiRouter;
};

export const createPublicRouter = () => createStorageRouter();
