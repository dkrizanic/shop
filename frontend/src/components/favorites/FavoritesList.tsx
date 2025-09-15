import React, { useState, useEffect } from 'react';
import { UserFavorite, Product } from '../../types';
import { favoritesAPI, cartAPI, productsAPI } from '../../services/api-fixed';
import { useAuth } from '../../context/AuthContext';
import './Favorites.css';

const FavoritesList: React.FC = () => {
  const { user } = useAuth();
  const [favorites, setFavorites] = useState<UserFavorite[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [actionLoading, setActionLoading] = useState<number | null>(null);

  useEffect(() => {
    if (user) {
      fetchFavorites();
    } else {
      setFavorites([]);
      setLoading(false);
    }
  }, [user]);

  const fetchFavorites = async () => {
    if (!user) return;

    setLoading(true);
    setError('');

    try {
      const favoritesData: any = await favoritesAPI.getFavorites(user.id.toString());
      const productIds = favoritesData.favoriteProductIds || [];

      if (productIds.length === 0) {
        setFavorites([]);
        return;
      }

      // Fetch product details for each favorite
      const favoriteItems: UserFavorite[] = [];
      for (const productId of productIds) {
        try {
          const product: Product = await productsAPI.getProduct(productId);
          favoriteItems.push({
            id: productId, // Using productId as favorite ID for now
            userId: user.id,
            productId: productId,
            product: product
          });
        } catch (productErr) {
          console.error(`Failed to fetch product ${productId}:`, productErr);
        }
      }

      setFavorites(favoriteItems);
    } catch (err) {
      setError('Failed to fetch favorites');
      console.error('Error fetching favorites:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleRemoveFavorite = async (productId: number) => {
    if (!user) return;

    setActionLoading(productId);

    try {
      await favoritesAPI.removeFavorite(user.id.toString(), productId);
      setFavorites(prev => prev.filter(fav => fav.productId !== productId));
    } catch (error) {
      console.error('Failed to remove favorite:', error);
    } finally {
      setActionLoading(null);
    }
  };

  const handleAddToCart = async (productId: number) => {
    setActionLoading(productId);

    try {
      await cartAPI.addToCart({ productId, quantity: 1 });
      // Could show a success message here
    } catch (error) {
      console.error('Failed to add to cart:', error);
    } finally {
      setActionLoading(null);
    }
  };

  if (!user) {
    return (
      <div className="favorites-container">
        <div className="auth-required">
          <h2>Please log in to view your favorites</h2>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="favorites-container">
        <div className="loading">Loading favorites...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="favorites-container">
        <div className="error">Error: {error}</div>
      </div>
    );
  }

  if (favorites.length === 0) {
    return (
      <div className="favorites-container">
        <div className="empty-favorites">
          <h2>No favorites yet</h2>
          <p>Start browsing products and add some to your favorites!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="favorites-container">
      <div className="favorites-header">
        <h2>My Favorites ({favorites.length} items)</h2>
      </div>

      <div className="favorites-grid">
        {favorites.map(favorite => (
          <div key={favorite.id} className="favorite-item">
            <div className="favorite-image-container">
              <img
                src={favorite.product.thumbnail}
                alt={favorite.product.title}
                className="favorite-image"
              />
            </div>

            <div className="favorite-info">
              <h3 className="favorite-title">{favorite.product.title}</h3>
              <p className="favorite-brand">{favorite.product.brand}</p>
              <p className="favorite-category">{favorite.product.category}</p>
              <p className="favorite-description">{favorite.product.description}</p>

              <div className="favorite-price-rating">
                <span className="favorite-price">${favorite.product.price.toFixed(2)}</span>
                <span className="favorite-rating">⭐ {favorite.product.rating.toFixed(1)}</span>
              </div>
            </div>

            <div className="favorite-actions">
              <button
                className="add-to-cart-btn"
                onClick={() => handleAddToCart(favorite.product.id)}
                disabled={actionLoading === favorite.product.id}
              >
                {actionLoading === favorite.product.id ? 'Adding...' : 'Add to Cart'}
              </button>

              <button
                className="remove-favorite-btn"
                onClick={() => handleRemoveFavorite(favorite.productId)}
                disabled={actionLoading === favorite.productId}
                title="Remove from favorites"
              >
                {actionLoading === favorite.productId ? '...' : '🗑️'}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default FavoritesList;