# Use Node.js for building React frontend
FROM node:18 AS frontend
WORKDIR /frontend
COPY ["frontend/package*.json", "./"]
RUN npm install
COPY frontend ./
RUN npm run build

# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies
COPY ["src/*.csproj", "./"]
COPY ["src/Domain/Domain.csproj", "Domain/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet publish -c Release -o out

# Copy frontend build to wwwroot
COPY --from=frontend /frontend/build ./out/wwwroot

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install SQLite (needed for the database)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy the published app
COPY --from=build /app/out .

# Note: Using /tmp directory for SQLite database on Render (writable filesystem)

# Expose port (will be set by Render via PORT env var)
EXPOSE 5000

# Set environment variables (PORT will be provided by Render)
ENV ASPNETCORE_ENVIRONMENT=Production

# Run the application
ENTRYPOINT ["dotnet", "Shop.dll"]