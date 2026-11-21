# Builds and publishes the backend.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source

# Copies project files first to cache dependency restoration.
COPY ["src/Qwizzi.Api/Qwizzi.Api.csproj", "src/Qwizzi.Api/"]
COPY ["src/Qwizzi.Application/Qwizzi.Application.csproj", "src/Qwizzi.Application/"]
COPY ["src/Qwizzi.Domain/Qwizzi.Domain.csproj", "src/Qwizzi.Domain/"]
COPY ["src/Qwizzi.Infrastructure/Qwizzi.Infrastructure.csproj", "src/Qwizzi.Infrastructure/"]

RUN dotnet restore "src/Qwizzi.Api/Qwizzi.Api.csproj"

# Copies source code and publishes the API with its dependencies.
COPY src/ src/

RUN dotnet publish "src/Qwizzi.Api/Qwizzi.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


# Runs the published backend without the SDK.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish/ ./

# Runs the application as the built-in non-root user.
USER $APP_UID

ENTRYPOINT ["dotnet", "Qwizzi.Api.dll"]