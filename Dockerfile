FROM node:24-bookworm-slim@sha256:c86ecead9e55a5210ac1727de38a4ca840bf03dcaf34dc9749addbf383b1920c AS assets
WORKDIR /src
COPY package*.json ./
RUN npm ci
COPY scripts/ ./scripts/
COPY assets/ ./assets/
RUN mkdir -p src/MerchantOS.Admin/wwwroot/js && npm run assets
FROM mcr.microsoft.com/dotnet/sdk:10.0@sha256:de4fe90a98845d75910b9769662465c84de1a934665c431279d1382b2bddc02b AS build
WORKDIR /src
COPY src/MerchantOS.Admin/MerchantOS.Admin.csproj ./src/MerchantOS.Admin/
RUN dotnet restore src/MerchantOS.Admin/MerchantOS.Admin.csproj
COPY src/ ./src/
COPY --from=assets /src/src/MerchantOS.Admin/wwwroot/vendor/ ./src/MerchantOS.Admin/wwwroot/vendor/
COPY --from=assets /src/src/MerchantOS.Admin/wwwroot/js/templates.js ./src/MerchantOS.Admin/wwwroot/js/templates.js
RUN dotnet publish src/MerchantOS.Admin/MerchantOS.Admin.csproj -c Release -o /publish --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0@sha256:de0ee358cdd672f7017a003bed21e9ac9ed48a27c922a59b18175b6e82fa7fee
RUN apt-get update && apt-get upgrade -y --no-install-recommends && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /publish/ ./
ENV ASPNETCORE_URLS=https://+:8443 DOTNET_EnableDiagnostics=0
USER $APP_UID
EXPOSE 8443
HEALTHCHECK --interval=10s --timeout=5s --start-period=30s CMD ["dotnet","MerchantOS.Admin.dll","--healthcheck"]
ENTRYPOINT ["dotnet","MerchantOS.Admin.dll"]
