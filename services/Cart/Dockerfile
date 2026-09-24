FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY services/Cart/Shop.Cart.Api/Shop.Cart.Api.csproj services/Cart/Shop.Cart.Api/
RUN dotnet restore services/Cart/Shop.Cart.Api/Shop.Cart.Api.csproj

COPY services/Cart/Shop.Cart.Api services/Cart/Shop.Cart.Api
RUN dotnet publish services/Cart/Shop.Cart.Api/Shop.Cart.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Cart.Api.dll"]
