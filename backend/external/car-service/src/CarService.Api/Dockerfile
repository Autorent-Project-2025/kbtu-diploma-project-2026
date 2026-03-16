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
COPY ["CarService.Api/CarService.Api.csproj", "CarService.Api/"]
COPY ["CarService.Application/CarService.Application.csproj", "CarService.Application/"]
COPY ["CarService.Domain/CarService.Domain.csproj", "CarService.Domain/"]
COPY ["CarService.Infrastructure/CarService.Infrastructure.csproj", "CarService.Infrastructure/"]
RUN dotnet restore "CarService.Api/CarService.Api.csproj"
COPY . .
WORKDIR "/src/CarService.Api"
RUN dotnet build "./CarService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CarService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarService.Api.dll"]
