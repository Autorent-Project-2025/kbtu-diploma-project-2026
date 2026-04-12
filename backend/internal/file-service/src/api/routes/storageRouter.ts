import { Router } from "express";
import express from "express";
import path from "path";

export const createStorageRouter = () => {
  const storageRouter = Router();

  // Uploaded files take precedence; bundled demo assets are fallback for seeded tickets.
  storageRouter.use("/", express.static(path.resolve(process.cwd(), "uploads")));
  storageRouter.use("/", express.static(path.resolve(process.cwd(), "public")));

  return storageRouter;
};
