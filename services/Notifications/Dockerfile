FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY services/Notifications/Shop.Notifications.Api/Shop.Notifications.Api.csproj services/Notifications/Shop.Notifications.Api
COPY shared/Shop.Shared/Shop.Shared.csproj shared/Shop.Shared
RUN services/Notifications/Shop.Notifications.Api/Shop.Notifications.Api.csproj

COPY services/Notifications/Shop.Notifications.Api services/Notifications/Shop.Notifications.Api
COPY shared/Shop.Shared/ shared/Shop.Shared
RUN dotnet publish services/Notifications/Shop.Notifications.Api/Shop.Notifications.Api.csproj \ 
-c Release -o app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Notifications.Api.dll"]
