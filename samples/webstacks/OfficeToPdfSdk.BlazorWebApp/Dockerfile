FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy only project files and build files (for layer caching)
COPY Directory.Build.props ./
COPY src/OfficeToPdfSdk/OfficeToPdfSdk.csproj src/OfficeToPdfSdk/
COPY src/OfficeToPdfSdk.Hosting/OfficeToPdfSdk.Hosting.csproj src/OfficeToPdfSdk.Hosting/
COPY samples/webstacks/OfficeToPdfSdk.BlazorWebApp/OfficeToPdfSdk.BlazorWebApp.csproj samples/webstacks/OfficeToPdfSdk.BlazorWebApp/

# Restore dependencies (cached if .csproj files unchanged)
RUN dotnet restore "samples/webstacks/OfficeToPdfSdk.BlazorWebApp/OfficeToPdfSdk.BlazorWebApp.csproj" \
    /p:RestoreUseStaticGraphEvaluation=true \
    /p:TreatWarningsAsErrors=false

# Copy all source code (invalidates fewer layers)
COPY . .

# Build
RUN dotnet build "samples/webstacks/OfficeToPdfSdk.BlazorWebApp/OfficeToPdfSdk.BlazorWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "samples/webstacks/OfficeToPdfSdk.BlazorWebApp/OfficeToPdfSdk.BlazorWebApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "OfficeToPdfSdk.BlazorWebApp.dll"]
