FROM node:24.8.0-alpine

WORKDIR /app
COPY package*.json ./

RUN npm i
COPY . .

EXPOSE 5173
