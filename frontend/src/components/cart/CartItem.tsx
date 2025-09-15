import React, { useState } from 'react';
import { ShoppingCartItem } from '../../types';
import './Cart.css';

interface CartItemProps {
  item: ShoppingCartItem;
  onUpdateQuantity: (productId: number, quantity: number) => void;
  onRemove: (productId: number) => void;
}

const CartItem: React.FC<CartItemProps> = ({ item, onUpdateQuantity, onRemove }) => {
  const [updating, setUpdating] = useState(false);

  const handleQuantityChange = async (newQuantity: number) => {
    if (newQuantity < 0) return;

    setUpdating(true);
    try {
      await onUpdateQuantity(item.productId, newQuantity);
    } finally {
      setUpdating(false);
    }
  };

  const handleRemove = async () => {
    setUpdating(true);
    try {
      await onRemove(item.productId);
    } finally {
      setUpdating(false);
    }
  };

  const itemTotal = item.quantity * item.product.price;

  return (
    <div className={`cart-item ${updating ? 'updating' : ''}`}>
      <div className="item-image">
        <img src={item.product.thumbnail} alt={item.product.title} />
      </div>

      <div className="item-details">
        <h4 className="item-title">{item.product.title}</h4>
        <p className="item-brand">{item.product.brand}</p>
        <p className="item-category">{item.product.category}</p>
        <p className="item-price">${item.product.price.toFixed(2)} each</p>
      </div>

      <div className="item-quantity">
        <label htmlFor={`quantity-${item.id}`}>Quantity:</label>
        <div className="quantity-controls">
          <button
            className="quantity-btn"
            onClick={() => handleQuantityChange(item.quantity - 1)}
            disabled={updating || item.quantity <= 1}
          >
            -
          </button>
          <span className="quantity-value">{item.quantity}</span>
          <button
            className="quantity-btn"
            onClick={() => handleQuantityChange(item.quantity + 1)}
            disabled={updating}
          >
            +
          </button>
        </div>
      </div>

      <div className="item-total">
        <span className="total-label">Total:</span>
        <span className="total-amount">${itemTotal.toFixed(2)}</span>
      </div>

      <div className="item-actions">
        <button
          className="remove-btn"
          onClick={handleRemove}
          disabled={updating}
          title="Remove from cart"
        >
          🗑️
        </button>
      </div>
    </div>
  );
};

export default CartItem;