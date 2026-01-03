FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["MiniShop.sln", "."]
COPY ["CatalogService/", "CatalogService/"]
COPY ["OrdersService/", "OrdersService/"]
COPY ["ApiGateway/", "ApiGateway/"]
COPY ["MiniShop.Contracts/", "MiniShop.Contracts/"]

RUN dotnet build "MiniShop.sln" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatalogService/CatalogService.csproj" -c Release -o /app/publish/CatalogService
RUN dotnet publish "OrdersService/OrdersService.csproj" -c Release -o /app/publish/OrdersService
RUN dotnet publish "ApiGateway/ApiGateway.csproj" -c Release -o /app/publish/ApiGateway

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80

# Default to ApiGateway, override with docker run -c parameter
ENTRYPOINT ["dotnet", "ApiGateway/ApiGateway.dll"]
