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
COPY ["external/booking-service/src/BookingService.Api/BookingService.Api.csproj", "external/booking-service/src/BookingService.Api/"]
COPY ["external/booking-service/src/BookingService.Application/BookingService.Application.csproj", "external/booking-service/src/BookingService.Application/"]
COPY ["external/booking-service/src/BookingService.Domain/BookingService.Domain.csproj", "external/booking-service/src/BookingService.Domain/"]
COPY ["external/booking-service/src/BookingService.Infrastructure/BookingService.Infrastructure.csproj", "external/booking-service/src/BookingService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
RUN dotnet restore "external/booking-service/src/BookingService.Api/BookingService.Api.csproj"
COPY . .
WORKDIR "/src/external/booking-service/src/BookingService.Api"
RUN dotnet build "./BookingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "BookingService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookingService.Api.dll"]
