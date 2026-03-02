FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IdentityService.Api/IdentityService.Api.csproj", "IdentityService.Api/"]
COPY ["IdentityService.Application/IdentityService.Application.csproj", "IdentityService.Application/"]
COPY ["IdentityService.Domain/IdentityService.Domain.csproj", "IdentityService.Domain/"]
COPY ["IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "IdentityService.Infrastructure/"]
RUN dotnet restore "IdentityService.Api/IdentityService.Api.csproj"
COPY . .
WORKDIR "/src/IdentityService.Api"
RUN dotnet build "./IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./IdentityService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
