# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["SigitTuning.API/SigitTuning.API.csproj", "SigitTuning.API/"]
RUN dotnet restore "SigitTuning.API/SigitTuning.API.csproj"

# Copy remaining files and build
COPY . .
WORKDIR "/src/SigitTuning.API"
RUN dotnet build "SigitTuning.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "SigitTuning.API.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SigitTuning.API.dll"]