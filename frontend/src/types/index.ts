export interface Product {
  id: number;
  title: string;
  description: string;
  category: string;
  price: number;
  rating: number;
  brand: string;
  thumbnail: string;
}

export interface ProductQueryParameters {
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface User {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
}

export interface UserRegistration {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UserLogin {
  email: string;
  password: string;
}

export interface AuthResponse {
  user: User;
  token: string;
}

export interface UserFavorite {
  id: number;
  userId: number;
  productId: number;
  product: Product;
}

export interface AddFavoriteRequest {
  userId: string;
  productId: number;
}

export interface ShoppingCartItem {
  id: number;
  userId: number;
  productId: number;
  quantity: number;
  product: Product;
}

export interface AddToCart {
  productId: number;
  quantity: number;
}

export interface UpdateCartItem {
  quantity: number;
}

export interface ShoppingCart {
  items: ShoppingCartItem[];
  totalPrice: number;
  totalItems: number;
}

export interface CheckoutValidationResult {
  isValid: boolean;
  errors: string[];
  validatedItems: ShoppingCartItem[];
}

export interface Category {
  name: string;
  count: number;
}