# See https://aka.ms/customizecontainer to learn how to customize your debug container.

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
COPY ["external/car-service/src/CarService.Api/CarService.Api.csproj", "external/car-service/src/CarService.Api/"]
COPY ["external/car-service/src/CarService.Application/CarService.Application.csproj", "external/car-service/src/CarService.Application/"]
COPY ["external/car-service/src/CarService.Domain/CarService.Domain.csproj", "external/car-service/src/CarService.Domain/"]
COPY ["external/car-service/src/CarService.Infrastructure/CarService.Infrastructure.csproj", "external/car-service/src/CarService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
RUN dotnet restore "external/car-service/src/CarService.Api/CarService.Api.csproj"
COPY . .
WORKDIR "/src/external/car-service/src/CarService.Api"
RUN dotnet build "./CarService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CarService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarService.Api.dll"]
