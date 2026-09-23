FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj Gateway/Shop.Gateway.Api
RUN Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj

COPY Gateway/Shop.Gateway.Api Gateway/Shop.Gateway.Api
RUN dotnet publish Gateway/Shop.Gateway.Api/Shop.Gateway.Api.csproj \ 
-c Release -o app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Gateway.Api.dll"]
