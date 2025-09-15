# Shop API

A .NET 9.0 ASP.NET Core Web API e-commerce application implementing Clean Architecture with DummyJSON API integration and user favorites functionality.

## Features

- **User Authentication** - JWT-based registration, login, and user management
- Product catalog with search, filtering, and pagination
- Category-based product browsing
- User favorites system with persistent storage
- Integration with DummyJSON API for product data
- Clean separation of concerns with layered architecture

## Quick Start

### Build and Run
```bash
# Build the solution
dotnet build Shop.sln

# Run the application
cd src
dotnet run

# Run with hot reload
cd src
dotnet watch run
```

## Docker Deployment

### Quick Start with Docker
```bash
# Build and run both frontend and backend services
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# Stop all services
docker-compose down
```

### Individual Service Deployment
```bash
# Build and run backend only
cd src
docker build -t shop-backend .
docker run -p 8080:8080 shop-backend

# Build and run frontend only
cd frontend
docker build -t shop-frontend .
docker run -p 3000:80 shop-frontend
```

### Production Deployment
```bash
# For production deployment (e.g., AWS, Azure, etc.)
docker-compose -f docker-compose.yml up -d --build

# View logs
docker-compose logs -f

# Update services
docker-compose pull
docker-compose up -d --build
```

### Service URLs (Docker)
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080
- **Swagger UI**: http://localhost:8080/swagger

### Database Setup
```bash
# Create and apply database migrations (first time setup)
cd src/Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../Shop.csproj
dotnet ef database update --startup-project ../Shop.csproj

# Apply migrations when database schema changes
dotnet ef database update --startup-project ../Shop.csproj
```

### API Endpoints
- Swagger UI: `http://localhost:5269/swagger`
- **Authentication API**: `http://localhost:5269/api/Auth`
  - `POST /api/auth/register` - User registration
  - `POST /api/auth/login` - User login
  - `GET /api/auth/me` - Get current user (requires JWT token)
- Products API: `http://localhost:5269/api/Product`
- User Favorites API: `http://localhost:5269/api/UserFavorite`

## Testing

### Run All Tests
```bash
dotnet test Shop.sln
```

### Run Unit Tests Only
```bash
dotnet test tests/Shop.UnitTests/Shop.UnitTests.csproj
```

### Run Integration Tests Only
```bash
dotnet test tests/Shop.IntegrationTests/Shop.IntegrationTests.csproj
```

### Test Coverage
```bash
# Run tests with coverage report
dotnet test --collect:"XPlat Code Coverage"

# Run tests with detailed output
dotnet test --verbosity normal
```

## Project Structure

### Backend (.NET API)
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
└── Shop.csproj           # Project file

tests/
├── Shop.UnitTests/        # Unit tests 
└── Shop.IntegrationTests/ # Integration tests (API tests)
```

### Frontend (React TypeScript)
```
frontend/
├── public/                # Static assets
├── src/
│   ├── components/
│   │   ├── auth/         # Authentication components
│   │   ├── cart/         # Shopping cart components
│   │   ├── common/       # Shared/reusable components
│   │   ├── favorites/    # User favorites components
│   │   └── products/     # Product listing and detail components
│   ├── context/          # React context providers
│   ├── services/         # API service layer
│   ├── types/           # TypeScript type definitions
│   ├── App.tsx          # Main application component
│   └── index.tsx        # Application entry point
├── package.json         # Dependencies and scripts
└── tsconfig.json       # TypeScript configuration
```

## Database Migrations

Entity Framework migrations manage database schema changes:

- **`dotnet ef migrations add <Name>`** - Creates a new migration file based on model changes
- **`dotnet ef database update`** - Applies pending migrations to the database
- **`dotnet ef migrations list`** - Shows all migrations and their status

The migration system:
- Compares current model vs. database schema
- Generates SQL scripts to update database structure
- Creates version-controlled migration files
- Supports rollback to previous versions

## Technology Stack

- .NET 9.0
- ASP.NET Core Web API
- Entity Framework Core (SQLite)
- JWT Authentication with BCrypt password hashing
- DummyJSON API integration
- xUnit, FluentAssertions, Moq (Testing)