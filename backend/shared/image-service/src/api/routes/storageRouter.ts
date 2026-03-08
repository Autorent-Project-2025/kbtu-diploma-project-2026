import { Router } from "express";
import express from "express";
import path from "path";

export const createStorageRouter = () => {
  const storageRouter = Router();
  const uploadsDir = path.resolve(process.cwd(), "uploads");
  const bundledPublicDir = path.resolve(process.cwd(), "public");

  storageRouter.use("/", express.static(uploadsDir));
  storageRouter.use("/", express.static(bundledPublicDir));

  return storageRouter;
};
