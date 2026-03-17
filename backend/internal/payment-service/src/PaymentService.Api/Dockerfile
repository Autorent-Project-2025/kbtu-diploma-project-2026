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
COPY ["internal/payment-service/src/PaymentService.Api/PaymentService.Api.csproj", "internal/payment-service/src/PaymentService.Api/"]
COPY ["internal/payment-service/src/PaymentService.Application/PaymentService.Application.csproj", "internal/payment-service/src/PaymentService.Application/"]
COPY ["internal/payment-service/src/PaymentService.Domain/PaymentService.Domain.csproj", "internal/payment-service/src/PaymentService.Domain/"]
COPY ["internal/payment-service/src/PaymentService.Infrastructure/PaymentService.Infrastructure.csproj", "internal/payment-service/src/PaymentService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
RUN dotnet restore "internal/payment-service/src/PaymentService.Api/PaymentService.Api.csproj"
COPY . .
WORKDIR "/src/internal/payment-service/src/PaymentService.Api"
RUN dotnet build "./PaymentService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "PaymentService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.Api.dll"]
