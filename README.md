# MiniShop - Full Stack E-Commerce Application

A complete microservices-based e-commerce platform built with **.NET 9** (backend) and **React + Redux** (frontend).

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                  React Frontend                         │
│              (Port 3000, Redux Store)                   │
└────────────────────────┬────────────────────────────────┘
                         │
                         │ HTTP/REST
                         ▼
┌─────────────────────────────────────────────────────────┐
│             API Gateway (Port 7000)                      │
│      Routes requests to downstream services             │
└─┬──────────────────────────────────────────────────────┬┘
  │                                                      │
  │ /api/products                              /api/orders
  ▼                                                      ▼
┌──────────────────────┐              ┌──────────────────────┐
│ CatalogService       │              │ OrdersService        │
│  (Port 7001)         │              │  (Port 7002)         │
│  - Product CRUD      │              │  - Order CRUD        │
│  - SQL Server DB     │              │  - SQL Server DB     │
└──────────────────────┘              └──────────────────────┘
      MiniShop_Catalog                     MiniShop_Orders
```

## Project Structure

```
MiniShop/
├── frontend/                    # React + Redux application
│   ├── public/
│   ├── src/
│   │   ├── components/         # React components (ProductList, OrderList)
│   │   ├── redux/              # Redux store, slices, hooks
│   │   ├── services/           # API client (axios)
│   │   ├── pages/              # Page components
│   │   └── App.tsx
│   └── package.json
│
├── CatalogService/             # .NET 9 Web API
│   ├── Controllers/            # Product endpoints
│   ├── Data/                   # Entity Framework DbContext
│   ├── Entities/               # Product entity
│   ├── Migrations/             # EF migrations
│   └── Program.cs
│
├── OrdersService/              # .NET 9 Web API
│   ├── Controllers/            # Order endpoints
│   ├── Data/                   # Entity Framework DbContext
│   ├── Entities/               # Order, OrderItem entities
│   └── Program.cs
│
├── ApiGateway/                 # .NET 9 API Gateway
│   └── Program.cs              # Route mapping to services
│
├── MiniShop.Contracts/         # Shared DTOs
│   ├── ProductDto
│   ├── OrderDto
│   └── OrderStatus enum
│
└── MiniShop.sln
```

## Prerequisites

- **.NET 9.0** SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **Node.js** 18+ with npm
- **SQL Server 2019+** (Local instance with Windows Authentication)
- **Visual Studio Code** or **Visual Studio**

## Setup Instructions

### 1. Database Setup

Create two SQL Server databases:

```sql
-- SQL Server Management Studio or sqlcmd
CREATE DATABASE MiniShop_Catalog;
CREATE DATABASE MiniShop_Orders;
```

Or let EF Core create them via migrations:

```bash
# From repository root
dotnet ef database update --project CatalogService
dotnet ef database update --project OrdersService
```

### 2. Backend Services Setup

```bash
# From repository root

# Install dependencies (already in .csproj files)
dotnet restore

# Build solution
dotnet build

# Run services in separate terminals

# Terminal 1: CatalogService (Port 7001)
dotnet run --project CatalogService

# Terminal 2: OrdersService (Port 7002)
dotnet run --project OrdersService

# Terminal 3: ApiGateway (Port 7000)
dotnet run --project ApiGateway
```

### 3. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start React development server (Port 3000)
npm start
```

The application will open at `http://localhost:3000`

## API Endpoints

### Via ApiGateway (Recommended)

**Base URL:** `https://localhost:7000/api`

#### Products
- `GET /products` - List all products
- `GET /products/{id}` - Get product by ID
- `POST /products` - Create new product
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Delete product

#### Orders
- `GET /orders` - List all orders
- `GET /orders/{id}` - Get order by ID
- `POST /orders` - Create new order
- `PUT /orders/{id}/status` - Update order status
- `DELETE /orders/{id}` - Delete order

### Direct Service Access

**CatalogService:** `https://localhost:7001/api`
**OrdersService:** `https://localhost:7002/api`

## Frontend Features

### Redux State Management

**Products Slice:**
- `fetchProducts()` - Load all products
- `fetchProductById(id)` - Load single product
- `createProduct()` - Create new product
- `updateProduct()` - Update product
- `deleteProduct()` - Remove product

**Orders Slice:**
- `fetchOrders()` - Load all orders
- `fetchOrderById(id)` - Load single order
- `createOrder()` - Create new order (from cart)
- `updateOrderStatus()` - Change order status
- `deleteOrder()` - Remove order
- `addToCart()` - Add item to cart
- `removeFromCart()` - Remove item from cart
- `clearCart()` - Empty cart

### Components

1. **ProductList.tsx**
   - Displays all products in grid layout
   - Shows price and stock levels
   - Add to cart functionality

2. **OrderList.tsx**
   - Displays user's orders in table format
   - Shows order status with color coding
   - Links to order details

## Environment Configuration

### Frontend (.env)
```
REACT_APP_API_URL=https://localhost:7000/api
```

### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "CatalogDb": "Server=localhost;Database=MiniShop_Catalog;Trusted_Connection=True;TrustServerCertificate=True",
    "OrdersDb": "Server=localhost;Database=MiniShop_Orders;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

## Development Workflows

### Add Database Migration

```bash
# CatalogService
dotnet ef migrations add "MigrationName" --project CatalogService
dotnet ef database update --project CatalogService

# OrdersService
dotnet ef migrations add "MigrationName" --project OrdersService
dotnet ef database update --project OrdersService
```

### Test APIs with Swagger UI

When services run in Development mode:
- CatalogService: `https://localhost:7001/swagger`
- OrdersService: `https://localhost:7002/swagger`

### Debugging

Use VS Code launch configurations or Visual Studio debugger:
```json
// .vscode/launch.json example
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Attach",
      "type": "coreclr",
      "request": "attach",
      "processId": "${command:pickProcess}"
    }
  ]
}
```

## Database Schema

### Products Table
```
Id (int, PK)
Name (nvarchar(max))
Price (decimal)
Stock (int)
```

### Orders Table
```
Id (int, PK)
OrderNumber (nvarchar(max))
CreatedDate (datetime)
Status (int) - 0:Pending, 1:Processing, 2:Completed, 3:Cancelled
Total (decimal)
```

### OrderItems Table
```
Id (int, PK)
OrderId (int, FK)
ProductId (int)
ProductName (nvarchar(max))
Quantity (int)
UnitPrice (decimal)
```

## Technologies Used

### Backend
- **.NET 9** - Framework
- **Entity Framework Core 8.0.15** - ORM
- **SQL Server** - Database
- **Swagger/Swashbuckle** - API Documentation

### Frontend
- **React 18** - UI Framework
- **Redux Toolkit** - State Management
- **React-Redux** - React bindings
- **Axios** - HTTP Client
- **TypeScript** - Type Safety
- **CSS3** - Styling

## Troubleshooting

### Certificate Errors
```bash
# Trust localhost certificate
dotnet dev-certs https --trust
```

### Database Connection Issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure Windows Authentication is enabled

### CORS Errors in Frontend
- Verify ApiGateway and services have CORS enabled
- Check frontend .env REACT_APP_API_URL

### Frontend Won't Connect to Backend
- Ensure all backend services are running
- Check ports: 7000 (Gateway), 7001 (Catalog), 7002 (Orders)
- Verify SSL certificate trust: `dotnet dev-certs https --trust`

## Future Enhancements

- [ ] Authentication/Authorization (JWT)
- [ ] Shopping cart persistence
- [ ] Payment integration
- [ ] Order tracking
- [ ] Admin dashboard
- [ ] Product search and filtering
- [ ] Unit tests
- [ ] Docker containerization
- [ ] CI/CD pipeline

## License

MIT License - See LICENSE file for details

## Support

For issues or questions, please create an issue in the repository.
