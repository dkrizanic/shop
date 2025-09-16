import React, { useState, useEffect, useCallback } from 'react';
import { ProductQueryParameters, Category } from '../../types';
import { productsAPI } from '../../services/api';
import './Products.css';

interface ProductFiltersProps {
  onFilterChange: (filters: ProductQueryParameters) => void;
  currentFilters?: ProductQueryParameters;
}

const ProductFilters: React.FC<ProductFiltersProps> = ({ onFilterChange, currentFilters }) => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  // Local display state (what user sees)
  const [localFilters, setLocalFilters] = useState<ProductQueryParameters>({
    search: '',
    category: '',
    minPrice: undefined,
    maxPrice: undefined,
    sortBy: 'title',
    sortOrder: 'asc',
  });

  // Handle search button click
  const handleSearch = () => {
    console.log('Manual search triggered:', localFilters);
    onFilterChange(localFilters);
  };

  // Handle apply filters button click
  const handleApplyFilters = () => {
    console.log('Apply filters triggered:', localFilters);
    onFilterChange(localFilters);
    setIsDropdownOpen(false); // Close dropdown after applying
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  // Sync local filters with current filters from parent
  useEffect(() => {
    if (currentFilters) {
      setLocalFilters({
        search: currentFilters.search || '',
        category: currentFilters.category || '',
        minPrice: currentFilters.minPrice,
        maxPrice: currentFilters.maxPrice,
        sortBy: currentFilters.sortBy || '',
        sortOrder: currentFilters.sortOrder || 'asc',
      });
    }
  }, [currentFilters]);

  const fetchCategories = async () => {
    try {
      const categoriesData: any = await productsAPI.getCategories();
      setCategories(categoriesData || []);
    } catch (error) {
      console.error('Failed to fetch categories:', error);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;

    const newFilters = {
      ...localFilters,
      [name]: name === 'minPrice' || name === 'maxPrice'
        ? (value ? parseFloat(value) : undefined)
        : value,
    };

    // Only update local display state - no automatic filter application
    setLocalFilters(newFilters);
  };

  const handleReset = () => {
    const resetFilters: ProductQueryParameters = {
      search: '',
      category: '',
      minPrice: undefined,
      maxPrice: undefined,
      sortBy: 'title',
      sortOrder: 'asc',
    };

    setLocalFilters(resetFilters);
    onFilterChange(resetFilters);
  };

  return (
    <div className="filters-container">
      {/* Search bar always visible */}
      <div className="search-bar">
        <input
          type="text"
          id="search"
          name="search"
          value={localFilters.search || ''}
          onChange={handleInputChange}
          placeholder="Search products..."
          onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
          className="search-input"
        />
        <button type="button" onClick={handleSearch} className="search-btn">
          Search
        </button>
      </div>

      {/* Filters dropdown */}
      <div className="filters-dropdown">
        <button
          type="button"
          onClick={() => setIsDropdownOpen(!isDropdownOpen)}
          className="filters-toggle-btn"
        >
          Filters & Sort
          <span className={`dropdown-arrow ${isDropdownOpen ? 'open' : ''}`}>▼</span>
        </button>

        {isDropdownOpen && (
          <div className="filters-dropdown-content">
            <div className="filters-grid">
              <div className="filter-item">
                <label htmlFor="category">Category</label>
                <select
                  id="category"
                  name="category"
                  value={localFilters.category || ''}
                  onChange={handleInputChange}
                >
                  <option value="">All Categories</option>
                  {categories.map(category => (
                    <option key={category.name} value={category.name}>
                      {category.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="filter-item">
                <label htmlFor="sortBy">Sort By</label>
                <select
                  id="sortBy"
                  name="sortBy"
                  value={localFilters.sortBy || ''}
                  onChange={handleInputChange}
                >
                  <option value="">Default</option>
                  <option value="title">Name</option>
                  <option value="price">Price</option>
                  <option value="rating">Rating</option>
                  <option value="brand">Brand</option>
                </select>
              </div>

              <div className="filter-item">
                <label htmlFor="sortOrder">Sort Order</label>
                <select
                  id="sortOrder"
                  name="sortOrder"
                  value={localFilters.sortOrder || 'asc'}
                  onChange={handleInputChange}
                >
                  <option value="asc">Ascending</option>
                  <option value="desc">Descending</option>
                </select>
              </div>

              <div className="filter-item">
                <label htmlFor="minPrice">Min Price</label>
                <input
                  type="number"
                  id="minPrice"
                  name="minPrice"
                  value={localFilters.minPrice || ''}
                  onChange={handleInputChange}
                  min="0"
                  step="0.01"
                  placeholder="$0"
                />
              </div>

              <div className="filter-item">
                <label htmlFor="maxPrice">Max Price</label>
                <input
                  type="number"
                  id="maxPrice"
                  name="maxPrice"
                  value={localFilters.maxPrice || ''}
                  onChange={handleInputChange}
                  min="0"
                  step="0.01"
                  placeholder="$999"
                />
              </div>
            </div>

            <div className="filters-actions">
              <button type="button" onClick={handleReset} className="reset-btn">
                Reset Filters
              </button>
              <button type="button" onClick={handleApplyFilters} className="apply-filters-btn">
                Apply Filters
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ProductFilters;