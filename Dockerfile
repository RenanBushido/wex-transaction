# Multi-stage build for WEX Transaction API
# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10 AS build

WORKDIR /src

# Copy solution and project files
COPY ["src/WexTransaction/WexTransaction.sln", "."]
COPY ["src/WexTransaction/WexTransaction.Api/WexTransaction.Api.csproj", "WexTransaction.Api/"]
COPY ["src/WexTransaction/WexTransaction.Application/WexTransaction.Application.csproj", "WexTransaction.Application/"]
COPY ["src/WexTransaction/WexTransaction.Domain/WexTransaction.Domain.csproj", "WexTransaction.Domain/"]
COPY ["src/WexTransaction/WexTransaction.Infra.Database/WexTransaction.Infra.Database.csproj", "WexTransaction.Infra.Database/"]
COPY ["src/WexTransaction/WexTransaction.CrossCutting/WexTransaction.CrossCutting.csproj", "WexTransaction.CrossCutting/"]

# Restore NuGet packages
RUN dotnet restore "WexTransaction.sln"

# Copy all source code
COPY ["src/", "."]

# Publish with Release configuration
RUN dotnet publish "WexTransaction.Api/WexTransaction.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10

WORKDIR /app

# Copy published application from build stage
COPY --from=build /app/publish .

# Expose port 5000
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Run application
ENTRYPOINT ["dotnet", "WexTransaction.Api.dll"]
