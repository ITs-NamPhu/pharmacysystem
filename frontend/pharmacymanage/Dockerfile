# Stage 1: Build static files
FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./

RUN npm ci

COPY . .

ENV REACT_APP_API_URL=/
ENV REACT_APP_AI_URL=/
RUN npm run build

# Stage 2: Serve bằng Nginx (Siêu nhẹ, siêu nhanh)
FROM nginx:alpine

RUN rm /etc/nginx/conf.d/default.conf

COPY nginx.conf /etc/nginx/conf.d/

COPY --from=build /app/build /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]