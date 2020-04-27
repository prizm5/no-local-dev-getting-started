FROM node:10.13.0-alpine as node
WORKDIR /app
COPY ClientApp/public ./public
COPY ClientApp/src ./src
COPY ClientApp/package*.json ./
RUN npm install --progress=true --loglevel=silent
RUN npm run build

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS builder
WORKDIR /source
COPY Controllers ./Controllers
COPY Pages ./Pages
COPY ./*.* .
RUN dotnet restore
RUN dotnet publish -c Release -r linux-musl-x64 -o ./app

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
WORKDIR /app
COPY --from=builder /app .
COPY --from=node /app/build ./wwwroot
CMD ASPNETCORE_URLS=http://*:$PORT ./AspNetCoreDemoApp
