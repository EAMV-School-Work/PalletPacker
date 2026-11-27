# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/PalletPacker.Core/*.csproj src/PalletPacker.Core/
COPY src/PalletPacker.Application/*.csproj src/PalletPacker.Application/
COPY src/PalletPacker.Infrastructure/*.csproj src/PalletPacker.Infrastructure/
COPY src/PalletPacker.Web/*.csproj src/PalletPacker.Web/

RUN dotnet restore src/PalletPacker.Web/PalletPacker.Web.csproj

COPY src/ src/
COPY mock-elements.json mock-orders.json ./

RUN dotnet publish src/PalletPacker.Web/PalletPacker.Web.csproj \
    -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "PalletPacker.Web.dll"]
