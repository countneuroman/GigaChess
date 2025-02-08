FROM nginx:1.27.4 AS base
WORKDIR /app
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
