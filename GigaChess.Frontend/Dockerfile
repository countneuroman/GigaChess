FROM nginx:1.26.3 AS base
WORKDIR /app
RUN apt update && apt install -y curl gnupg2 ca-certificates lsb-release debian-archive-keyring && \
    curl https://nginx.org/keys/nginx_signing.key | gpg --dearmor > /tmp/nginx-key.gpg && \
    mv /tmp/nginx-key.gpg /usr/share/keyrings/nginx-archive-keyring.gpg 

RUN echo "deb [signed-by=/usr/share/keyrings/nginx-archive-keyring.gpg] \
    http://nginx.org/packages/debian `lsb_release -cs` nginx" \
        | tee /etc/apt/sources.list.d/nginx.list 

RUN echo "Package: *\nPin: origin nginx.org\nPin: release o=nginx\nPin-Priority: 900\n" | tee /etc/apt/preferences.d/99nginx

RUN apt update && apt install -y nginx-module-otel

EXPOSE 8080

FROM node:22.13.1 AS build
WORKDIR /src
COPY GigaChess.Frontend/ GigaChess.Frontend/
WORKDIR "/src/GigaChess.Frontend"
RUN npm install
RUN npm run build

FROM base AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /src/GigaChess.Frontend/nginx.conf /etc/nginx/nginx.conf
COPY --from=build /src/GigaChess.Frontend/bin .
ENTRYPOINT ["nginx", "-g", "daemon off;"]
