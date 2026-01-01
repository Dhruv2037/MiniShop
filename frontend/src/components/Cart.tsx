import React from 'react';
import { useAppDispatch, useAppSelector } from '../redux';
import {
  createOrder,
  addToCart,
  decreaseFromCart,
  removeFromCart,
  clearCart,
} from '../redux';
import type { OrderItem } from '../services/api';
import './Cart.css';

interface CartProps {
  onSuccess: () => void;
  onClose: () => void;
}

const Cart: React.FC<CartProps> = ({ onSuccess, onClose }) => {
  const dispatch = useAppDispatch();
  const { cart, loading, error } = useAppSelector((state: any) => state.orders);

  const handleIncrease = (item: OrderItem) => {
    dispatch(addToCart({ ...item, quantity: 1 }));
  };

  const handleDecrease = (item: OrderItem) => {
    dispatch(decreaseFromCart({ productId: item.productId, quantity: 1 }));
  };

  const handleRemove = (productId: number) => {
    dispatch(removeFromCart(productId));
  };

  const total = cart.reduce((s: number, i: OrderItem) => s + i.quantity * i.unitPrice, 0);

  const handleCheckout = async () => {
    if (cart.length === 0) return;
    try {
      const items = cart.map((i: OrderItem) => ({ ...i }));
      await dispatch(createOrder(items)).unwrap();
      onSuccess();
      dispatch(clearCart());
      alert('Order placed successfully');
    } catch (err: any) {
      console.error('Checkout error:', err);
      const payload = err?.payload ?? err?.response ?? err?.message ?? 'Failed to place order';
      const message = typeof payload === 'string' ? payload : JSON.stringify(payload);
      alert(message);
    }
  };

  return (
    <div className="cart-panel">
      <div className="cart-header">
        <h3>Your Cart</h3>
        <button className="btn btn-close" onClick={onClose}>Close</button>
      </div>

      {error && <div className="error">{typeof error === 'string' ? error : JSON.stringify(error)}</div>}

      {cart.length === 0 ? (
        <div className="empty">Your cart is empty.</div>
      ) : (
        <div className="cart-items">
          {cart.map((item: OrderItem) => (
            <div key={item.productId} className="cart-item">
              <div className="item-info">
                <div className="name">{item.productName}</div>
                <div className="price">${item.unitPrice.toFixed(2)}</div>
              </div>
              <div className="item-controls">
                <button className="btn" onClick={() => handleDecrease(item)}>-</button>
                <span className="qty">{item.quantity}</span>
                <button className="btn" onClick={() => handleIncrease(item)}>+</button>
                <button className="btn btn-remove" onClick={() => handleRemove(item.productId)}>Remove</button>
              </div>
            </div>
          ))}

          <div className="cart-footer">
            <div className="total">Total: ${total.toFixed(2)}</div>
            <div className="actions">
              <button className="btn btn-checkout" onClick={handleCheckout} disabled={loading}>
                {loading ? 'Placing Order...' : 'Checkout'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Cart;
