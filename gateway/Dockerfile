FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props ./

COPY gateway/Shop.Gateway/Shop.Gateway.csproj gateway/Shop.Gateway/
RUN dotnet restore gateway/Shop.Gateway/Shop.Gateway.csproj

COPY gateway/Shop.Gateway gateway/Shop.Gateway
RUN dotnet publish gateway/Shop.Gateway/Shop.Gateway.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT [ "dotnet", "Shop.Gateway.dll" ]