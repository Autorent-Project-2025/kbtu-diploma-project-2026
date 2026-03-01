import { Request, Response } from "express";

import type ImageSaveRequest from "../dtos/imageSaveRequest";
import type ImageFile from "../../models/ImageFile";
import type IImageService from "../../interfaces/IImageService";

class ImagesController {
  constructor(private readonly imageService: IImageService) {}

  saveImage = async (req: Request, res: Response) => {
    const imageFile = res.locals.imageFile as ImageFile | undefined;

    if (!imageFile) {
      return res.status(400).json({ message: "File is required" });
    }

    const payload: ImageSaveRequest = {
      imageFile
    };

    try {
      const result = await this.imageService.saveImage(payload.imageFile);
      return res.status(201).json(result);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Internal server error";
      return res.status(500).json({ message });
    }
  };

  deleteImage = async (req: Request, res: Response) => {
    const imageIdParam = req.params.imageId;
    const imageId = Array.isArray(imageIdParam) ? imageIdParam[0] : imageIdParam;

    if (!imageId) {
      return res.status(400).json({ message: "imageId is required" });
    }

    try {
      await this.imageService.deleteImage(imageId);
      return res.status(200).json({ message: "Image deleted", imageId });
    } catch (error) {
      const message = error instanceof Error ? error.message : "Internal server error";
      const statusCode = message === "Image not found" ? 404 : 500;
      return res.status(statusCode).json({ message });
    }
  };
}

export default ImagesController;
