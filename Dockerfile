# Multi-stage build for WEX Transaction API
# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution and project files
COPY ["src/WexTransaction/WexTransaction.slnx", "WexTransaction/"]
COPY ["src/WexTransaction/WexTransaction.Api/WexTransaction.Api.csproj", "WexTransaction/WexTransaction.Api/"]
COPY ["src/WexTransaction/WexTransaction.Application/WexTransaction.Application.csproj", "WexTransaction/WexTransaction.Application/"]
COPY ["src/WexTransaction/WexTransaction.Domain/WexTransaction.Domain.csproj", "WexTransaction/WexTransaction.Domain/"]
COPY ["src/WexTransaction/WexTransaction.Infra.Database/WexTransaction.Infra.Database.csproj", "WexTransaction/WexTransaction.Infra.Database/"]
COPY ["src/WexTransaction/WexTransaction.CrossCutting/WexTransaction.CrossCutting.csproj", "WexTransaction/WexTransaction.CrossCutting/"]
COPY ["src/WexTransaction/WexTransaction.Infra.Services.RatesExchange/WexTransaction.Infra.Services.RatesExchange.csproj", "WexTransaction/WexTransaction.Infra.Services.RatesExchange/"]

# Restore NuGet packages
WORKDIR /src/WexTransaction
RUN dotnet restore "WexTransaction.slnx"

# Copy remaining source code
COPY ["src/WexTransaction/", "."]

# Publish with Release configuration
RUN dotnet publish "WexTransaction.Api/WexTransaction.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0

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
