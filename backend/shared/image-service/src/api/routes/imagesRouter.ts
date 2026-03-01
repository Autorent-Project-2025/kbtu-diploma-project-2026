import { Router, RequestHandler } from "express";

import ImagesController from "../controllers/imagesController";
import { fileTypeMiddleware } from "../middlewares/fileTypeMiddleware";

export const createImagesRouter = (
  imagesController: ImagesController,
  rawImageBodyParser: RequestHandler
) => {
  const imagesRouter = Router();

  imagesRouter.post(
    "/",
    rawImageBodyParser,
    fileTypeMiddleware,
    imagesController.saveImage
  );
  imagesRouter.delete("/:imageId", imagesController.deleteImage);

  return imagesRouter;
};
