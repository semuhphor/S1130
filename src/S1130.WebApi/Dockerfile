# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY S1130.sln ./
COPY src/S1130.SystemObjects/S1130.SystemObjects.csproj ./src/S1130.SystemObjects/
COPY src/S1130.WebApi/S1130.WebApi.csproj ./src/S1130.WebApi/

# Restore dependencies
RUN dotnet restore src/S1130.WebApi/S1130.WebApi.csproj

# Copy source code
COPY src/S1130.SystemObjects/ ./src/S1130.SystemObjects/
COPY src/S1130.WebApi/ ./src/S1130.WebApi/

# Build the application
WORKDIR /src/src/S1130.WebApi
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "S1130.WebApi.dll"]
