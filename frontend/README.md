# Shop Frontend

A React.js frontend application for the Shop e-commerce platform.

## Features

- **User Authentication**: Registration and login with JWT tokens
- **Product Browsing**: Browse products with search and filtering
- **Shopping Cart**: Add products to cart, update quantities, checkout validation
- **Favorites**: Save favorite products for later
- **Responsive Design**: Mobile-friendly interface

## Tech Stack

- **React 18** with TypeScript
- **React Router** for navigation
- **Axios** for API calls
- **CSS3** for styling
- **Context API** for state management

## Prerequisites

- Node.js 16+ and npm
- Backend API running on https://localhost:7292

## Setup

1. **Install dependencies**:
   ```bash
   npm install
   ```

2. **Environment configuration**:
   The `.env` file is already configured with:
   ```
   REACT_APP_API_URL=https://localhost:7292
   ```

3. **Start the development server**:
   ```bash
   npm start
   ```

   The app will open at `http://localhost:3000`

## Available Scripts

- `npm start` - Start development server
- `npm run build` - Create production build
- `npm test` - Run tests
- `npm run eject` - Eject from Create React App (irreversible)

## Project Structure

```
src/
├── components/
│   ├── auth/           # Authentication components
│   ├── cart/           # Shopping cart components
│   ├── common/         # Shared components (Header, HomePage)
│   ├── favorites/      # User favorites components
│   └── products/       # Product listing and filtering
├── context/
│   └── AuthContext.tsx # Authentication state management
├── services/
│   └── api.ts          # API client and endpoints
├── types/
│   └── index.ts        # TypeScript type definitions
├── App.tsx             # Main app component with routing
└── index.tsx           # App entry point
```

## Key Components

### Authentication
- `LoginForm` - User login
- `RegisterForm` - User registration
- `AuthPage` - Combined auth interface

### Products
- `ProductList` - Product grid with pagination
- `ProductCard` - Individual product display
- `ProductFilters` - Search and filter controls

### Shopping Cart
- `ShoppingCart` - Cart overview and checkout
- `CartItem` - Individual cart item management

### Favorites
- `FavoritesList` - User's saved products

## API Integration

The frontend communicates with the backend through:

- **Auth API**: Registration, login
- **Products API**: Browse products, categories, search/filter
- **Cart API**: Add/remove items, update quantities, checkout validation
- **Favorites API**: Add/remove favorite products

## Authentication Flow

1. User registers/logs in
2. JWT token stored in localStorage
3. Token automatically added to API requests
4. Protected routes require authentication
5. User redirected to login if not authenticated

## Running with Backend

1. Start the .NET backend API (port 7292)
2. Start the React frontend (port 3000)
3. Backend CORS is configured to allow frontend origin

## Notes

- The app uses React Context for authentication state
- All API calls include error handling
- Components are responsive and mobile-friendly
- TypeScript provides type safety throughout