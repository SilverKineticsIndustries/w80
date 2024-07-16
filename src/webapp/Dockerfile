# build
FROM node:latest AS build

ARG REACT_APP_BASE_API
ENV REACT_APP_BASE_API $REACT_APP_BASE_API

WORKDIR /build
COPY package.json package.json
COPY package-lock.json package-lock.json

RUN npm ci
COPY public/ public
COPY src/ src
RUN npm run build

# run
FROM nginx:stable-alpine
COPY --from=build /build/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
