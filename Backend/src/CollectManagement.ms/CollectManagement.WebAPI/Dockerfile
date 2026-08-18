# ============================
# BUILD
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore \
    src/CollectManagement.ms/CollectManagement.WebAPI/CollectManagement.WebAPI.csproj

RUN dotnet publish \
    src/CollectManagement.ms/CollectManagement.WebAPI/CollectManagement.WebAPI.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# ============================
# RUNTIME
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "CollectManagement.WebAPI.dll"]