FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClientService.Api/ClientService.Api.csproj", "ClientService.Api/"]
COPY ["ClientService.Application/ClientService.Application.csproj", "ClientService.Application/"]
COPY ["ClientService.Domain/ClientService.Domain.csproj", "ClientService.Domain/"]
COPY ["ClientService.Infrastructure/ClientService.Infrastructure.csproj", "ClientService.Infrastructure/"]
RUN dotnet restore "ClientService.Api/ClientService.Api.csproj"
COPY . .
WORKDIR "/src/ClientService.Api"
RUN dotnet build "./ClientService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClientService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClientService.Api.dll"]
