# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY DBAHound.sln .
COPY src/Core/Core.csproj src/Core/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Scraper/Scraper.csproj src/Scraper/
COPY src/Web/Web.csproj src/Web/

# Restore dependencies
RUN dotnet restore src/Web/Web.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish src/Web/Web.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create data directory
RUN mkdir -p /srv/data

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Web.dll"]