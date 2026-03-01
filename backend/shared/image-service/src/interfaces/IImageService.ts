import type ImageSaveResponse from "../api/dtos/imageSaveResponse";
import type ImageFile from "../models/ImageFile";

interface IImageService {
  saveImage(file: ImageFile): Promise<ImageSaveResponse>;
  deleteImage(imageId: string): Promise<void>;
}

export default IImageService;
