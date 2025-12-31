import React from 'react';
import { useAppDispatch, useAppSelector } from '../redux';
import { fetchOrders } from '../redux/orderSlice';
import type { Order } from '../services/api';
import './OrderList.css';

export const OrderList: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, loading, error } = useAppSelector((state: any) => state.orders);

  React.useEffect(() => {
    dispatch(fetchOrders());
  }, [dispatch]);

  if (loading) return <div className="loading">Loading orders...</div>;

  return (
    <div className="order-list-container">
      <h2>My Orders</h2>
      {error && <div className="error">{error}</div>}
      {items.length === 0 ? (
        <p>No orders found.</p>
      ) : (
        <div className="orders-table">
          <table>
            <thead>
              <tr>
                <th>Order Number</th>
                <th>Date</th>
                <th>Status</th>
                <th>Total</th>
              </tr>
            </thead>
            <tbody>
              {items.map((order: Order) => (
                <tr key={order.id}>
                  <td>{order.orderNumber}</td>
                  <td>{new Date(order.createdDate).toLocaleDateString()}</td>
                  <td>
                    <span className={`status-badge status-${order.status}`}>
                      {order.status === 0 ? 'Pending' : order.status === 1 ? 'Processing' : order.status === 2 ? 'Completed' : 'Cancelled'}
                    </span>
                  </td>
                  <td>${order.total.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default OrderList;
