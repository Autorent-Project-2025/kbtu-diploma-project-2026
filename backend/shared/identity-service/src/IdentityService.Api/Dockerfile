FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["shared/identity-service/src/IdentityService.Api/IdentityService.Api.csproj", "shared/identity-service/src/IdentityService.Api/"]
COPY ["shared/identity-service/src/IdentityService.Application/IdentityService.Application.csproj", "shared/identity-service/src/IdentityService.Application/"]
COPY ["shared/identity-service/src/IdentityService.Domain/IdentityService.Domain.csproj", "shared/identity-service/src/IdentityService.Domain/"]
COPY ["shared/identity-service/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "shared/identity-service/src/IdentityService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
COPY ["libraries/backend-shared-dotnet/src/AutoRent.Backend.Shared/AutoRent.Backend.Shared.csproj", "libraries/backend-shared-dotnet/src/AutoRent.Backend.Shared/"]
RUN dotnet restore "shared/identity-service/src/IdentityService.Api/IdentityService.Api.csproj"
COPY . .
WORKDIR "/src/shared/identity-service/src/IdentityService.Api"
RUN dotnet build "./IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
