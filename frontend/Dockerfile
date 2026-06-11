# ---- build ----
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build

# ---- runtime ----
FROM nginx:1.27-alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/eventosvivos/browser /usr/share/nginx/html
EXPOSE 80
