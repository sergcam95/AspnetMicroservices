FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src/Services/Discount/Discount.API
COPY Discount.API.csproj .
RUN dotnet restore Discount.API.csproj
COPY . .
RUN dotnet publish Discount.API.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Discount.API.dll"]