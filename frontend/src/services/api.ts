import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'https://localhost:7000/api';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface Product {
  id: number;
  name: string;
  price: number;
  stock: number;
}

export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  createdDate: string;
  status: OrderStatus;
  items: OrderItem[];
  total: number;
}

export enum OrderStatus {
  Pending = 0,
  Processing = 1,
  Completed = 2,
  Cancelled = 3,
}

// Product API calls
export const productAPI = {
  getAll: async (): Promise<Product[]> => {
    const response = await apiClient.get('/products');
    return response.data;
  },

  getById: async (id: number): Promise<Product> => {
    const response = await apiClient.get(`/products/${id}`);
    return response.data;
  },

  create: async (product: Omit<Product, 'id'>): Promise<Product> => {
    const response = await apiClient.post('/products', product);
    return response.data;
  },

  update: async (id: number, product: Omit<Product, 'id'>): Promise<void> => {
    await apiClient.put(`/products/${id}`, product);
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/products/${id}`);
  },
};

// Order API calls
export const orderAPI = {
  getAll: async (): Promise<Order[]> => {
    const response = await apiClient.get('/orders');
    return response.data;
  },

  getById: async (id: number): Promise<Order> => {
    const response = await apiClient.get(`/orders/${id}`);
    return response.data;
  },

  create: async (items: OrderItem[]): Promise<Order> => {
    const response = await apiClient.post('/orders', { items });
    return response.data;
  },

  updateStatus: async (id: number, status: OrderStatus): Promise<void> => {
    await apiClient.put(`/orders/${id}/status`, { status });
  },

  delete: async (id: number): Promise<void> => {
    await apiClient.delete(`/orders/${id}`);
  },
};

export default apiClient;
