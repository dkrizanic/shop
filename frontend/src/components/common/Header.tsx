import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Common.css';

const Header: React.FC = () => {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <header className="header">
      <div className="header-container">
        <div className="logo">
          <Link to="/">Shop</Link>
        </div>

        <nav className="nav-menu">
          <Link to="/products" className="nav-link">Products</Link>
          {user && (
            <>
              <Link to="/favorites" className="nav-link">Favorites</Link>
              <Link to="/cart" className="nav-link">Cart</Link>
            </>
          )}
        </nav>

        <div className="user-menu">
          {user ? (
            <div className="user-info">
              <span className="welcome-text">
                Welcome, {user.firstName} {user.lastName}
              </span>
              <button className="logout-btn" onClick={handleLogout}>
                Logout
              </button>
            </div>
          ) : (
            <Link to="/auth" className="login-link">
              Login / Register
            </Link>
          )}
        </div>
      </div>
    </header>
  );
};

export default Header;