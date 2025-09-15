import React, { useState } from 'react';
import { Product } from '../../types';
import { cartAPI, favoritesAPI } from '../../services/api-fixed';
import { useAuth } from '../../context/AuthContext';
import './Products.css';

interface ProductCardProps {
  product: Product;
  isFavorite?: boolean;
  onFavoriteToggle?: (productId: number, isFavorite: boolean) => void;
  onAddToCart?: (productId: number) => void;
}

const ProductCard: React.FC<ProductCardProps> = ({
  product,
  isFavorite = false,
  onFavoriteToggle,
  onAddToCart,
}) => {
  const { user } = useAuth();
  const [addingToCart, setAddingToCart] = useState(false);
  const [cartMessage, setCartMessage] = useState('');
  const [togglingFavorite, setTogglingFavorite] = useState(false);

  const handleAddToCart = async () => {
    if (!user || addingToCart) return;

    setAddingToCart(true);
    setCartMessage('');
    try {
      await cartAPI.addToCart({ productId: product.id, quantity: 1 });
      setCartMessage('✓ Added to cart!');
      setTimeout(() => setCartMessage(''), 2000);
      onAddToCart?.(product.id);
    } catch (error: any) {
      console.error('Failed to add to cart:', error);
      const errorMsg = error.response?.data?.error || 'Failed to add to cart';
      setCartMessage(errorMsg);
      setTimeout(() => setCartMessage(''), 3000);
    } finally {
      setAddingToCart(false);
    }
  };

  const handleFavoriteToggle = async () => {
    if (!user || togglingFavorite) return;

    setTogglingFavorite(true);
    try {
      if (isFavorite) {
        await favoritesAPI.removeFavorite(user.id.toString(), product.id);
      } else {
        await favoritesAPI.addFavorite({ userId: user.id.toString(), productId: product.id });
      }
      onFavoriteToggle?.(product.id, !isFavorite);
    } catch (error) {
      console.error('Failed to toggle favorite:', error);
    } finally {
      setTogglingFavorite(false);
    }
  };

  return (
    <div className="product-card">
      <div className="product-image-container">
        <img src={product.thumbnail} alt={product.title} className="product-image" />
        {user && (
          <>
            <button
              className={`favorite-btn ${isFavorite ? 'favorite' : ''}`}
              onClick={handleFavoriteToggle}
              disabled={togglingFavorite}
              title={isFavorite ? 'Remove from favorites' : 'Add to favorites'}
            >
              ❤
            </button>
            <button
              className="cart-btn"
              onClick={handleAddToCart}
              disabled={addingToCart}
              title="Add to cart"
            >
              {addingToCart ? '...' : '🛒'}
            </button>
          </>
        )}
      </div>

      <div className="product-info">
        <h3 className="product-title">{product.title}</h3>
        <p className="product-brand">{product.brand}</p>
        <p className="product-category">{product.category}</p>
        <p className="product-description">{product.description}</p>

        {cartMessage && (
          <div className="cart-message" style={{
            padding: '0.5rem',
            margin: '0.5rem 0',
            borderRadius: '4px',
            fontSize: '0.8rem',
            textAlign: 'center',
            backgroundColor: cartMessage.includes('Added') ? '#d4edda' : '#f8d7da',
            color: cartMessage.includes('Added') ? '#155724' : '#721c24',
            border: `1px solid ${cartMessage.includes('Added') ? '#c3e6cb' : '#f5c6cb'}`
          }}>
            {cartMessage}
          </div>
        )}

        <div className="product-footer">
          <div className="product-price-rating">
            <span className="product-price">${product.price.toFixed(2)}</span>
            <span className="product-rating">⭐ {product.rating.toFixed(1)}</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProductCard;