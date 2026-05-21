# Build static web + nginx (compose service: web)
FROM node:20.19-alpine3.21 AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# Vite bakes env at build time — use .env.docker for Compose (see production/docker-compose.yml).
COPY .env.docker .env
RUN npm run build

FROM nginx:1.27.5-alpine3.21 AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=15s --timeout=3s --retries=3 \
  CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1
