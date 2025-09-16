import axios from 'axios';
import { ProductQueryParameters, PagedResult, Product } from '../types';

// Use relative URLs in production (same origin), localhost in development
const API_BASE_URL = process.env.NODE_ENV === 'production' ? '' : 'http://localhost:5269';

console.log('API_BASE_URL:', API_BASE_URL);
console.log('NODE_ENV:', process.env.NODE_ENV);

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to requests if available
api.interceptors.request.use((config) => {
  console.log('Making request to:', config.baseURL + config.url);
  const token = localStorage.getItem('token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Auth API
export const authAPI = {
  register: async (userData: any) => {
    const response = await api.post('/api/auth/register', userData);
    return response.data;
  },

  login: async (credentials: any) => {
    const response = await api.post('/api/auth/login', credentials);
    return response.data;
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

  getCategories: async () => {
    const response = await api.get('/api/product/categories');
    return response.data;
  },
};

// User Favorites API
export const favoritesAPI = {
  getFavorites: async (userId: string) => {
    const response = await api.get(`/api/userfavorite/user/${userId}`);
    return response.data;
  },

  addFavorite: async (request: any) => {
    const response = await api.post('/api/userfavorite', request);
    return response.data;
  },

  removeFavorite: async (userId: string, productId: number) => {
    await api.delete(`/api/userfavorite?userId=${userId}&productId=${productId}`);
  },

  isFavorite: async (userId: string, productId: number) => {
    const response = await api.get(`/api/userfavorite/check?userId=${userId}&productId=${productId}`);
    return response.data;
  },
};

// Shopping Cart API
export const cartAPI = {
  getCart: async () => {
    const response = await api.get('/api/shoppingcart');
    return response.data;
  },

  addToCart: async (request: any) => {
    const response = await api.post('/api/shoppingcart/items', request);
    return response.data;
  },

  updateCartItem: async (itemId: number, request: any) => {
    const response = await api.put(`/api/shoppingcart/items/${itemId}`, request);
    return response.data;
  },

  removeFromCart: async (itemId: number) => {
    await api.delete(`/api/shoppingcart/items/${itemId}`);
  },

  clearCart: async () => {
    await api.delete('/api/shoppingcart');
  },
};

export default api;