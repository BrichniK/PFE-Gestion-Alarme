# ============================
# BUILD ANGULAR
# ============================
FROM node:20-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm install --legacy-peer-deps

COPY . .

RUN npm run build -- --configuration production


# ============================
# NGINX
# ============================
FROM nginx:alpine

COPY --from=build /app/dist/*/browser /usr/share/nginx/html

# Configuration runtime Angular
COPY public/config/setting-config.json \
     /usr/share/nginx/html/config/setting-config.json

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]