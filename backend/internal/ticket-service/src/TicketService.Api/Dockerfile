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
COPY ["TicketService.Api/TicketService.Api.csproj", "TicketService.Api/"]
COPY ["TicketService.Application/TicketService.Application.csproj", "TicketService.Application/"]
COPY ["TicketService.Domain/TicketService.Domain.csproj", "TicketService.Domain/"]
COPY ["TicketService.Infrastructure/TicketService.Infrastructure.csproj", "TicketService.Infrastructure/"]
RUN dotnet restore "TicketService.Api/TicketService.Api.csproj"
COPY . .
WORKDIR "/src/TicketService.Api"
RUN dotnet build "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TicketService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicketService.Api.dll"]
