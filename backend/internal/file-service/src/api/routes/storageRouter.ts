import { Router } from "express";
import express from "express";
import path from "path";

export const createStorageRouter = () => {
  const storageRouter = Router();

  storageRouter.use("/", express.static(path.resolve(process.cwd(), "uploads")));

  return storageRouter;
};
