export { store } from './store';
export { useAppDispatch, useAppSelector } from './hooks';
export * from './productSlice';

// Re-export order slice symbols explicitly and alias conflicting names
export {
	fetchOrders,
	fetchOrderById,
	createOrder,
	updateOrderStatus,
	deleteOrder,
	addToCart,
	removeFromCart,
	decreaseFromCart,
	clearCart,
	clearError as clearOrderError,
} from './orderSlice';

// (If you need the reducer) export { default as orderReducer } from './orderSlice';
