FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY services/Identity/Shop.Identity.Api/Shop.Identity.Api.csproj services/Identity/Shop.Identity.Api/
RUN dotnet restore services/Identity/Shop.Identity.Api/Shop.Identity.Api.csproj

COPY services/Identity/Shop.Identity.Api services/Identity/Shop.Identity.Api
RUN dotnet publish services/Identity/Shop.Identity.Api/Shop.Identity.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Identity.Api.dll"]
