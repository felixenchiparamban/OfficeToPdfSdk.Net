# Multi-stage build for Office to PDF Conversion Service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy only project files and build files (for layer caching)
COPY Directory.Build.props ./
COPY src/OfficeToPdfSdk/OfficeToPdfSdk.csproj src/OfficeToPdfSdk/
COPY src/OfficeToPdfSdk.Hosting/OfficeToPdfSdk.Hosting.csproj src/OfficeToPdfSdk.Hosting/
COPY samples/webstacks/OfficeToPdfSdk.ConversionService/OfficeToPdfSdk.ConversionService.csproj samples/webstacks/OfficeToPdfSdk.ConversionService/

# Restore dependencies (cached if .csproj files unchanged)
RUN dotnet restore "samples/webstacks/OfficeToPdfSdk.ConversionService/OfficeToPdfSdk.ConversionService.csproj" \
    /p:RestoreUseStaticGraphEvaluation=true \
    /p:TreatWarningsAsErrors=false

# Copy all source code (invalidates fewer layers)
COPY . .

# Build and publish
RUN dotnet build "samples/webstacks/OfficeToPdfSdk.ConversionService/OfficeToPdfSdk.ConversionService.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "samples/webstacks/OfficeToPdfSdk.ConversionService/OfficeToPdfSdk.ConversionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM dotnet-libreoffice:latest
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OfficeToPdfSdk.ConversionService.dll"]
