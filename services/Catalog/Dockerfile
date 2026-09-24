FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY services/Catalog/Shop.Catalog.Api/Shop.Catalog.Api.csproj services/Catalog/Shop.Catalog.Api/
COPY shared/Shop.Shared/Shop.Shared.csproj shared/Shop.Shared/
RUN dotnet restore services/Catalog/Shop.Catalog.Api/Shop.Catalog.Api.csproj

COPY services/Catalog/Shop.Catalog.Api services/Catalog/Shop.Catalog.Api
COPY shared/Shop.Shared/ shared/Shop.Shared/
RUN dotnet publish services/Catalog/Shop.Catalog.Api/Shop.Catalog.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Catalog.Api.dll"]
