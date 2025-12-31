import React, { useState } from 'react';
import { Provider } from 'react-redux';
import { store } from './redux/store';
import ProductList from './components/ProductList';
import OrderList from './components/OrderList';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState<'products' | 'orders'>('products');

  return (
    <Provider store={store}>
      <div className="App">
        <header className="app-header">
          <h1>MiniShop</h1>
          <p>E-Commerce Platform</p>
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

        <footer className="app-footer">
          <p>&copy; 2025 MiniShop. All rights reserved.</p>
        </footer>
      </div>
    </Provider>
  );
}

export default App;

