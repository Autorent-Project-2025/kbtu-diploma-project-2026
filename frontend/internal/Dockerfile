FROM node:24.8.0-alpine

WORKDIR /app
COPY internal/package*.json ./

RUN npm ci
COPY internal/. .
COPY shared /shared

EXPOSE 5173
