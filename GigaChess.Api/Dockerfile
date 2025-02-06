FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GigaChess.Api/GigaChess.Api.csproj", "GigaChess.Api/"]
RUN dotnet restore "GigaChess.Api/GigaChess.Api.csproj"
COPY . .
WORKDIR "/src/GigaChess.Api"
RUN dotnet build "GigaChess.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GigaChess.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GigaChess.Api.dll"]
