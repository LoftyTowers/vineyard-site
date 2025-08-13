# Build stage
FROM node:20 AS build
WORKDIR /app
COPY package*.json ./
COPY . .
RUN npm install
ARG CONFIG=production
RUN npx ng build --configuration=$CONFIG

# Serve stage
FROM nginx:alpine
COPY --from=build /app/dist/vineyard-site /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
