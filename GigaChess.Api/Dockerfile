FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
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
