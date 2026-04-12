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
COPY ["shared/chat-service/src/ChatService.Api/ChatService.Api.csproj", "shared/chat-service/src/ChatService.Api/"]
COPY ["shared/chat-service/src/ChatService.Application/ChatService.Application.csproj", "shared/chat-service/src/ChatService.Application/"]
COPY ["shared/chat-service/src/ChatService.Domain/ChatService.Domain.csproj", "shared/chat-service/src/ChatService.Domain/"]
COPY ["shared/chat-service/src/ChatService.Infrastructure/ChatService.Infrastructure.csproj", "shared/chat-service/src/ChatService.Infrastructure/"]
COPY ["libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj", "libraries/messaging-dotnet/src/AutoRent.Messaging/"]
RUN dotnet restore "shared/chat-service/src/ChatService.Api/ChatService.Api.csproj"
COPY . .
WORKDIR "/src/shared/chat-service/src/ChatService.Api"
RUN dotnet build "./ChatService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ChatService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatService.Api.dll"]
