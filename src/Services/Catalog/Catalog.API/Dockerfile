FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src/Services/Catalog/Catalog.API
COPY Services/Catalog/Catalog.API/Catalog.API.csproj .
RUN dotnet restore Catalog.API.csproj
COPY Services/Catalog/Catalog.API .
RUN dotnet publish Catalog.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
