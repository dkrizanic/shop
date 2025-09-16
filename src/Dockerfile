# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Use the runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install SQLite (needed for the database)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copy the published app
COPY --from=build /app/out .

# Create directory for database
RUN mkdir -p /app/data

# Expose port
EXPOSE 80
EXPOSE 5269

# Set environment variables (PORT will be provided by Render)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:$PORT

# Run the application
ENTRYPOINT ["dotnet", "Shop.dll"]