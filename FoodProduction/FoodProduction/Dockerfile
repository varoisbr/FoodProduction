# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY ["FoodProduction.csproj", "./"]

# Restore dependencies
RUN dotnet restore "FoodProduction.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "FoodProduction.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "FoodProduction.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for logs and label files
RUN mkdir -p /app/logs /app/labels

# Expose port
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD dotnet /app/FoodProduction.dll

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Run application
ENTRYPOINT ["dotnet", "FoodProduction.dll"]
