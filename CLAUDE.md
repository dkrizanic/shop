# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9.0 ASP.NET Core Web API project called "Shop" - an e-commerce application implementing Clean Architecture. The project integrates with DummyJSON API for product data and includes user favorites functionality.

## Architecture

**Clean Architecture Implementation:**
- **Domain Layer** (`src/Domain/`): Contains core business models, DTOs, and repository interfaces
- **Infrastructure Layer** (`src/Infrastructure/`): Implements repositories and external API services
- **Application Layer** (`src/Application/Controllers/`): API controllers (presentation layer)

**Key Components:**
- **Entry Point**: `src/Program.cs` - ASP.NET Core setup with Swagger
- **Models**: Product domain models with DTOs for API responses
- **Services**: ProductService handles business logic and integrates with DummyJSON API
- **Repositories**: UserFavoriteRepository manages user favorite products
- **External Integration**: DummyJSON API service for product catalog

## Development Commands

### Build and Run
```bash
# Build the solution
dotnet build Store.sln

# Build specific project
dotnet build src/Shop.csproj

# Run the application (from src directory)
cd src
dotnet run

# Run with hot reload
cd src
dotnet watch run
```

### Development Workflow
- Main project file: `src/Shop.csproj` (targets .NET 9.0)
- Solution file: `Store.sln`
- Swagger UI available at `/swagger` in development mode
- Uses HTTPS redirection and authorization middleware

## Project Structure
```
src/
├── Application/
│   └── Controllers/         # API endpoints
├── Domain/
│   ├── Models/             # Core business entities and DTOs
│   │   └── Read/          # Read-specific DTOs
│   └── Repositories/       # Repository interfaces
├── Infrastructure/
│   ├── Repositories/       # Repository implementations
│   └── Services/          # External service implementations
├── Program.cs             # Application entry point
├── DependencyInjection.cs # Service registration
└── Shop.csproj           # Project file
```

## Key Features
- Product catalog with search, filtering, and pagination
- Category-based product browsing
- User favorites system with persistent storage
- Integration with DummyJSON API for product data
- Clean separation of concerns with layered architecture

## Notes
- No test projects are currently configured
- Uses Entity Framework Core for data persistence (UserFavorites)
- Integrates with external DummyJSON API for product catalog
- Implements async/await patterns throughout
- Uses nullable reference types and proper error handling