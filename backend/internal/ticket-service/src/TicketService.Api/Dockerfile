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
COPY ["internal/ticket-service/src/TicketService.Api/TicketService.Api.csproj", "internal/ticket-service/src/TicketService.Api/"]
COPY ["internal/ticket-service/src/TicketService.Application/TicketService.Application.csproj", "internal/ticket-service/src/TicketService.Application/"]
COPY ["internal/ticket-service/src/TicketService.Domain/TicketService.Domain.csproj", "internal/ticket-service/src/TicketService.Domain/"]
COPY ["internal/ticket-service/src/TicketService.Infrastructure/TicketService.Infrastructure.csproj", "internal/ticket-service/src/TicketService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
RUN dotnet restore "internal/ticket-service/src/TicketService.Api/TicketService.Api.csproj"
COPY . .
WORKDIR "/src/internal/ticket-service/src/TicketService.Api"
RUN dotnet build "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketService.Api.dll"]
