import React from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Common.css';

const HomePage: React.FC = () => {
  const { user } = useAuth();

  return (
    <div className="home-page">
      <div className="hero-section">
        <div className="hero-content">
          <h1>Welcome to Shop</h1>
          <p>Discover amazing products and great deals!</p>

          <div className="hero-actions">
            {user ? (
              <>
                <Link to="/products" className="cta-button primary">
                  Browse Products
                </Link>
                <Link to="/favorites" className="cta-button secondary">
                  My Favorites
                </Link>
              </>
            ) : (
              <>
                <Link to="/products" className="cta-button primary">
                  Browse Products
                </Link>
                <Link to="/auth" className="cta-button secondary">
                  Sign Up / Login
                </Link>
              </>
            )}
          </div>
        </div>
      </div>

      <div className="features-section">
        <div className="features-container">
          <h2>Why Choose Our Shop?</h2>

          <div className="features-grid">
            <div className="feature-item">
              <div className="feature-icon">🛍️</div>
              <h3>Wide Selection</h3>
              <p>Browse through thousands of products across various categories</p>
            </div>

            <div className="feature-item">
              <div className="feature-icon">❤️</div>
              <h3>Save Favorites</h3>
              <p>Keep track of products you love with our favorites system</p>
            </div>

            <div className="feature-item">
              <div className="feature-icon">🛒</div>
              <h3>Easy Shopping</h3>
              <p>Simple and intuitive shopping cart with checkout validation</p>
            </div>

            <div className="feature-item">
              <div className="feature-icon">🔒</div>
              <h3>Secure Account</h3>
              <p>Your data is protected with secure authentication</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default HomePage;