# ===============================
# BUILD
# ===============================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src


COPY . .


RUN dotnet restore CST.LePoint.sln


RUN dotnet build CST.LePoint.sln \
    --configuration Release \
    --no-restore



# ===============================
# TESTS
# ===============================

RUN dotnet test tests/CollectManagement.Tests/CollectManagement.Tests.csproj \
    --configuration Release \
    --no-build \
    --logger "trx"



# ===============================
# PUBLISH API
# ===============================

RUN dotnet publish \
    src/CollectManagement.ms/CollectManagement.WebAPI/CollectManagement.WebAPI.csproj \
    --configuration Release \
    --output /app/publish



# ===============================
# RUNTIME
# ===============================

FROM mcr.microsoft.com/dotnet/aspnet:8.0


WORKDIR /app


COPY --from=build /app/publish .


EXPOSE 8080


ENTRYPOINT ["dotnet","CollectManagement.WebAPI.dll"]