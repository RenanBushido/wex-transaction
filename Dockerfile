# Multi-stage build for WEX Transaction API
# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy solution file first
COPY ["src/WexTransaction/WexTransaction.slnx", "WexTransaction/"]

# Copy all project files
COPY ["src/WexTransaction/", "WexTransaction/"]
COPY ["tests/", "../tests/"]

# Restore and build the solution
WORKDIR /src/WexTransaction
RUN dotnet restore "WexTransaction.slnx"

# Publish the API project
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
