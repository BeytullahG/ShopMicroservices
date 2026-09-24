FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY services/Orders/Shop.Orders.Api/Shop.Orders.Api.csproj services/Orders/Shop.Orders.Api/
COPY shared/Shop.Shared/Shop.Shared.csproj shared/Shop.Shared/
RUN dotnet restore services/Orders/Shop.Orders.Api/Shop.Orders.Api.csproj

COPY services/Orders/Shop.Orders.Api services/Orders/Shop.Orders.Api
COPY shared/Shop.Shared/ shared/Shop.Shared/
RUN dotnet publish services/Orders/Shop.Orders.Api/Shop.Orders.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Orders.Api.dll"]
