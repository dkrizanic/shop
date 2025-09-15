import React, { useState, useEffect, useCallback } from 'react';
import { Product, ProductQueryParameters, PagedResult } from '../../types';
import { productsAPI, favoritesAPI } from '../../services/api';
import { useAuth } from '../../context/AuthContext';
import ProductCard from './ProductCard';
import ProductFilters from './ProductFilters';
import './Products.css';

const ProductList: React.FC = () => {
  const { user } = useAuth();
  const [products, setProducts] = useState<Product[]>([]);
  const [favorites, setFavorites] = useState<Set<number>>(new Set());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [totalPages, setTotalPages] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [filters, setFilters] = useState<ProductQueryParameters>({
    search: '',
    category: '',
    minPrice: undefined,
    maxPrice: undefined,
    sortBy: 'title',
    sortOrder: 'asc',
    pageSize: 12,
    pageNumber: 1,
  });

  useEffect(() => {
    fetchProducts();
    if (user) {
      loadFavorites();
    }
  }, [filters, user]);

  const loadFavorites = async () => {
    if (!user) return;

    try {
      const favoritesData: any = await favoritesAPI.getFavorites(user.id.toString());
      const favoriteIds = favoritesData.favoriteProductIds || [];
      setFavorites(new Set(favoriteIds));
    } catch (error) {
      console.error('Failed to load favorites:', error);
    }
  };

  const fetchProducts = async () => {
    setLoading(true);
    setError('');

    try {
      console.log('ProductList: Sending filters to API:', filters);
      const response: PagedResult<Product> = await productsAPI.getProducts(filters);
      console.log('ProductList: Received response:', response);
      setProducts(response.data || []);
      setTotalPages(response.totalPages || 1);
      setCurrentPage(response.pageNumber || 1);
    } catch (err) {
      setError('Failed to fetch products');
      console.error('Error fetching products:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = useCallback((newFilters: ProductQueryParameters) => {
    // Preserve all filter values and reset to first page when filters change
    const cleanedFilters: ProductQueryParameters = {
      ...newFilters,
      pageNumber: 1, // Reset to first page when filters change
      pageSize: 6, // Show 6 items per page for better pagination demonstration
    };

    console.log('Applying filters:', cleanedFilters);
    setFilters(cleanedFilters);
  }, []);

  const handlePageChange = (page: number) => {
    setFilters(prev => ({ ...prev, pageNumber: page }));
  };

  const handleAddToCart = (productId: number) => {
    // Show a success message or update cart count
    console.log(`Product ${productId} added to cart`);
  };

  const handleFavoriteToggle = (productId: number, isFavorite: boolean) => {
    if (isFavorite) {
      setFavorites(prev => {
        const newFavorites = new Set(prev);
        newFavorites.add(productId);
        return newFavorites;
      });
    } else {
      setFavorites(prev => {
        const newFavorites = new Set(prev);
        newFavorites.delete(productId);
        return newFavorites;
      });
    }
  };

  if (loading) {
    return <div className="loading">Loading products...</div>;
  }

  if (error) {
    return <div className="error">Error: {error}</div>;
  }

  return (
    <div className="product-list-container">
      <ProductFilters onFilterChange={handleFilterChange} currentFilters={filters} />

      <div className="product-grid">
        {products.length > 0 ? (
          products.map(product => (
            <ProductCard
              key={product.id}
              product={product}
              isFavorite={favorites.has(product.id)}
              onAddToCart={handleAddToCart}
              onFavoriteToggle={handleFavoriteToggle}
            />
          ))
        ) : (
          <div className="no-products">No products found</div>
        )}
      </div>

      {totalPages > 1 && (
        <div className="pagination">
          {Array.from({ length: totalPages }, (_, i) => i + 1).map(page => (
            <button
              key={page}
              className={`page-btn ${page === currentPage ? 'active' : ''}`}
              onClick={() => handlePageChange(page)}
            >
              {page}
            </button>
          ))}
        </div>
      )}
    </div>
  );
};

export default ProductList;