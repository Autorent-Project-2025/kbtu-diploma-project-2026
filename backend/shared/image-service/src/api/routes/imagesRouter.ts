import { Router, RequestHandler } from "express";

import ImagesController from "../controllers/imagesController";
import { fileTypeMiddleware } from "../middlewares/fileTypeMiddleware";
import { authenticateJwt, requirePermission } from "../middlewares/jwtPermissionMiddleware";
import { PermissionConstants } from "../../constants/permissionConstants";

export const createImagesRouter = (
  imagesController: ImagesController,
  rawImageBodyParser: RequestHandler
) => {
  const imagesRouter = Router();

  imagesRouter.post(
    "/",
    authenticateJwt,
    requirePermission(PermissionConstants.imageCreate),
    rawImageBodyParser,
    fileTypeMiddleware,
    imagesController.saveImage
  );
  imagesRouter.delete(
    "/:imageId",
    authenticateJwt,
    requirePermission(PermissionConstants.imageDelete),
    imagesController.deleteImage
  );

  return imagesRouter;
};
