import React, { useState, useEffect } from 'react';
import { ShoppingCart } from '../../types';
import { cartAPI } from '../../services/api-fixed';
import { useAuth } from '../../context/AuthContext';
import CartItem from './CartItem';
import './Cart.css';

const ShoppingCartComponent: React.FC = () => {
  const { user } = useAuth();
  const [cart, setCart] = useState<ShoppingCart | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [checkoutLoading, setCheckoutLoading] = useState(false);
  const [purchaseSuccess, setPurchaseSuccess] = useState(false);

  useEffect(() => {
    if (user) {
      fetchCart();
    } else {
      setCart(null);
      setLoading(false);
    }
  }, [user]);

  // Also refresh cart when component mounts or becomes visible
  useEffect(() => {
    if (user) {
      fetchCart();
    }
  }, []);

  const fetchCart = async () => {
    if (!user) return;

    setLoading(true);
    setError('');

    try {
      const cartData: any = await cartAPI.getCart();
      console.log('Cart data received:', cartData);
      setCart(cartData);
    } catch (err) {
      setError('Failed to fetch cart');
      console.error('Error fetching cart:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateQuantity = async (productId: number, quantity: number) => {
    if (!cart) return;

    try {
      if (quantity <= 0) {
        await cartAPI.removeFromCart(productId);
      } else {
        await cartAPI.updateCartItem(productId, { quantity });
      }
      await fetchCart(); // Refresh cart
    } catch (error) {
      console.error('Failed to update cart item:', error);
    }
  };

  const handleRemoveItem = async (productId: number) => {
    try {
      await cartAPI.removeFromCart(productId);
      await fetchCart(); // Refresh cart
    } catch (error) {
      console.error('Failed to remove cart item:', error);
    }
  };

  const handleClearCart = async () => {
    if (!window.confirm('Are you sure you want to clear your cart?')) return;

    try {
      await cartAPI.clearCart();
      await fetchCart(); // Refresh cart
    } catch (error) {
      console.error('Failed to clear cart:', error);
    }
  };

  const handleValidateCheckout = async () => {
    setCheckoutLoading(true);
    setPurchaseSuccess(false);

    try {
      // Simple validation: check if cart has items
      if (!cart || !cart.items || cart.items.length === 0) {
        setError('Cart is empty');
        return;
      }

      // Simulate successful purchase
      setPurchaseSuccess(true);
      // Clear the cart after successful purchase
      await cartAPI.clearCart();
      await fetchCart(); // Refresh to show empty cart

      // Hide success message after 5 seconds
      setTimeout(() => {
        setPurchaseSuccess(false);
      }, 5000);
    } catch (error) {
      console.error('Checkout failed:', error);
      setError('Checkout failed');
    } finally {
      setCheckoutLoading(false);
    }
  };

  if (!user) {
    return (
      <div className="cart-container">
        <div className="auth-required">
          <h2>Please log in to view your cart</h2>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="cart-container">
        <div className="loading">Loading cart...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="cart-container">
        <div className="error">Error: {error}</div>
      </div>
    );
  }

  if (!cart || !cart.items || cart.items.length === 0) {
    return (
      <div className="cart-container">
        {purchaseSuccess && (
          <div className="purchase-success">
            <h3>🎉 Items Purchased Successfully!</h3>
            <p>Thank you for your purchase. Your items have been bought!</p>
          </div>
        )}
        <div className="empty-cart">
          <h2>Your cart is empty</h2>
          <p>Start shopping to add items to your cart!</p>
        </div>
      </div>
    );
  }

  return (
    <div className="cart-container">
      <div className="cart-header">
        <h2>Shopping Cart ({cart.totalItems} items)</h2>
        <button className="clear-cart-btn" onClick={handleClearCart}>
          Clear Cart
        </button>
      </div>

      <div className="cart-items">
        {cart.items.map(item => (
          <CartItem
            key={item.id}
            item={item}
            onUpdateQuantity={handleUpdateQuantity}
            onRemove={handleRemoveItem}
          />
        ))}
      </div>

      <div className="cart-summary">
        <div className="summary-row">
          <span>Total Items:</span>
          <span>{cart.totalItems}</span>
        </div>
        <div className="summary-row total">
          <span>Total Amount:</span>
          <span>${cart.totalPrice.toFixed(2)}</span>
        </div>
      </div>


      {purchaseSuccess && (
        <div className="purchase-success">
          <h3>🎉 Items Purchased Successfully!</h3>
          <p>Thank you for your purchase. Your items have been bought!</p>
        </div>
      )}

      <div className="cart-actions">
        <button
          className="validate-btn"
          onClick={handleValidateCheckout}
          disabled={checkoutLoading}
        >
          {checkoutLoading ? 'Processing...' : 'Buy Items'}
        </button>
      </div>
    </div>
  );
};

export default ShoppingCartComponent;