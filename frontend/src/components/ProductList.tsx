import React, { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '../redux';
import { fetchProducts } from '../redux/productSlice';
import type { Product } from '../services/api';
import './ProductList.css';

export const ProductList: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, loading, error } = useAppSelector((state: any) => state.products);

  useEffect(() => {
    dispatch(fetchProducts());
  }, [dispatch]);

  if (loading) return <div className="loading">Loading products...</div>;

  return (
    <div className="product-list-container">
      <h2>Products</h2>
      {error && <div className="error">{error}</div>}
      <div className="product-grid">
        {items.map((product: Product) => (
          <div key={product.id} className="product-card">
            <h3>{product.name}</h3>
            <p className="price">${product.price.toFixed(2)}</p>
            <p className="stock">Stock: {product.stock}</p>
            <button className="btn btn-add">Add to Cart</button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default ProductList;
