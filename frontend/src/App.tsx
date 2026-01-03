import React, { useState } from 'react';
import { Provider } from 'react-redux';
import { store } from './redux/store';
import { useAppSelector } from './redux';
import ProductList from './components/ProductList';
import OrderList from './components/OrderList';
import Cart from './components/Cart';
import './App.css';

function AppContent() {
  const [activeTab, setActiveTab] = useState<'products' | 'orders'>('products');
  const [showCart, setShowCart] = useState(false);
  const cart = useAppSelector((state: any) => state.orders.cart);

  const onCheckoutSuccess = () => {
    setShowCart(false);
    setActiveTab('orders');
  };

  return (
    <div className="App">
      <header className="app-header">
        <h1>MiniShop</h1>
        <p>E-Commerce Platform</p>
        <div className="cart-toggle">
          <button className="btn btn-cart" onClick={() => setShowCart((s) => !s)}>
            Cart ({cart.reduce((sum: number, i: any) => sum + i.quantity, 0)})
          </button>
        </div>
      </header>

      <nav className="app-nav">
        <button
          className={`nav-button ${activeTab === 'products' ? 'active' : ''}`}
          onClick={() => setActiveTab('products')}
        >
          Products
        </button>
        <button
          className={`nav-button ${activeTab === 'orders' ? 'active' : ''}`}
          onClick={() => setActiveTab('orders')}
        >
          Orders
        </button>
      </nav>

      <main className="app-main">
        {activeTab === 'products' && <ProductList />}
        {activeTab === 'orders' && <OrderList />}
      </main>

      {showCart && <Cart onSuccess={onCheckoutSuccess} onClose={() => setShowCart(false)} />}

      <footer className="app-footer">
        <p>&copy; 2025 MiniShop. All rights reserved.</p>
      </footer>
    </div>
  );
}

function App() {
  return (
    <Provider store={store}>
      <AppContent />
    </Provider>
  );
}

export default App;

