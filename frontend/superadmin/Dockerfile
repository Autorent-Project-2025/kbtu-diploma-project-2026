FROM node:24.8.0-alpine

WORKDIR /app
COPY superadmin/package*.json ./

RUN npm ci
COPY superadmin/. .
COPY shared /shared

EXPOSE 5173
