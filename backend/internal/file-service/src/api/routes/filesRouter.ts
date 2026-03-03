import { RequestHandler, Router } from "express";

import FilesController from "../controllers/filesController";
import { PermissionConstants } from "../../constants/permissionConstants";
import { authenticateJwt, requirePermission } from "../middlewares/jwtPermissionMiddleware";

export const createFilesRouter = (
  filesController: FilesController,
  rawFileBodyParser: RequestHandler
) => {
  const filesRouter = Router();

  filesRouter.post(
    "/",
    authenticateJwt,
    requirePermission(PermissionConstants.fileCreate),
    rawFileBodyParser,
    filesController.uploadFile
  );

  filesRouter.post(
    "/temporary-link",
    authenticateJwt,
    requirePermission(PermissionConstants.fileRead),
    filesController.getTemporaryLink
  );

  filesRouter.delete(
    "/:fileName",
    authenticateJwt,
    requirePermission(PermissionConstants.fileDelete),
    filesController.deleteFile
  );

  return filesRouter;
};
