FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PartnerService.Api/PartnerService.Api.csproj", "PartnerService.Api/"]
COPY ["PartnerService.Application/PartnerService.Application.csproj", "PartnerService.Application/"]
COPY ["PartnerService.Domain/PartnerService.Domain.csproj", "PartnerService.Domain/"]
COPY ["PartnerService.Infrastructure/PartnerService.Infrastructure.csproj", "PartnerService.Infrastructure/"]
RUN dotnet restore "PartnerService.Api/PartnerService.Api.csproj"
COPY . .
WORKDIR "/src/PartnerService.Api"
RUN dotnet build "./PartnerService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PartnerService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PartnerService.Api.dll"]
