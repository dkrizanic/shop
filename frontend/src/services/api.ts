import axios from 'axios';
import {
  Product,
  ProductQueryParameters,
  PagedResult,
  UserRegistration,
  UserLogin,
  AuthResponse,
  UserFavorite,
  AddFavoriteRequest,
  ShoppingCartItem,
  AddToCart,
  UpdateCartItem,
  ShoppingCart,
  CheckoutValidationResult,
  Category
} from '../types';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7292';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if available
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth API
export const authAPI = {
  register: async (userData: UserRegistration): Promise<AuthResponse> => {
    const response = await api.post('/api/auth/register', userData);
    return response.data as AuthResponse;
  },

  login: async (credentials: UserLogin): Promise<AuthResponse> => {
    const response = await api.post('/api/auth/login', credentials);
    return response.data as AuthResponse;
  },
};

// Products API
export const productsAPI = {
  getProducts: async (params?: ProductQueryParameters): Promise<PagedResult<Product>> => {
    const response = await api.get('/api/product', { params });
    return response.data as PagedResult<Product>;
  },

  getProduct: async (id: number): Promise<Product> => {
    const response = await api.get(`/api/product/${id}`);
    return response.data as Product;
  },

  getCategories: async (): Promise<Category[]> => {
    const response = await api.get('/api/product/categories');
    return response.data as Category[];
  },
};

// User Favorites API
export const favoritesAPI = {
  getFavorites: async (): Promise<UserFavorite[]> => {
    const response = await api.get('/api/userfavorite');
    return response.data as UserFavorite[];
  },

  addFavorite: async (request: AddFavoriteRequest): Promise<UserFavorite> => {
    const response = await api.post('/api/userfavorite', request);
    return response.data as UserFavorite;
  },

  removeFavorite: async (favoriteId: number): Promise<void> => {
    await api.delete(`/api/userfavorite/${favoriteId}`);
  },

  isFavorite: async (productId: number): Promise<boolean> => {
    const response = await api.get(`/api/userfavorite/is-favorite/${productId}`);
    return response.data as boolean;
  },
};

// Shopping Cart API
export const cartAPI = {
  getCart: async (): Promise<ShoppingCart> => {
    const response = await api.get('/api/shoppingcart');
    return response.data as ShoppingCart;
  },

  addToCart: async (request: AddToCart): Promise<ShoppingCartItem> => {
    const response = await api.post('/api/shoppingcart/add', request);
    return response.data as ShoppingCartItem;
  },

  updateCartItem: async (itemId: number, request: UpdateCartItem): Promise<ShoppingCartItem> => {
    const response = await api.put(`/api/shoppingcart/update/${itemId}`, request);
    return response.data as ShoppingCartItem;
  },

  removeFromCart: async (itemId: number): Promise<void> => {
    await api.delete(`/api/shoppingcart/remove/${itemId}`);
  },

  clearCart: async (): Promise<void> => {
    await api.delete('/api/shoppingcart/clear');
  },

  validateCheckout: async (): Promise<CheckoutValidationResult> => {
    const response = await api.post('/api/shoppingcart/validate-checkout');
    return response.data as CheckoutValidationResult;
  },
};

export default api;