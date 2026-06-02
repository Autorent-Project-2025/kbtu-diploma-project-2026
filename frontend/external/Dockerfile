FROM node:24.8.0-alpine

WORKDIR /app
COPY external/package*.json ./

RUN npm ci
COPY external/. .
COPY shared /shared

EXPOSE 5173
