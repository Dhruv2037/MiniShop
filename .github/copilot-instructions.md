# MiniShop AI Coding Agent Instructions

## Project Overview

MiniShop is a full-stack microservices-based e-commerce application:
- **Frontend**: React 18 + Redux Toolkit (Port 3000)
- **Backend**: .NET 9 microservices with SQL Server
  - **CatalogService** (Port 7001): Product catalog management
  - **OrdersService** (Port 7002): Order processing
  - **ApiGateway** (Port 7000): Entry point routing requests to services
  - **MiniShop.Contracts**: Shared DTOs for inter-service communication

## Quick Links for Developers

- **Getting Started**: See [GETTING_STARTED.md](../GETTING_STARTED.md) for running the app
- **API Reference**: See [API_CONTRACTS.md](../API_CONTRACTS.md) for endpoint details
- **Setup Help**: See [ENVIRONMENT_SETUP.md](../ENVIRONMENT_SETUP.md) for installation
- **Development**: See [DEVELOPMENT.md](../DEVELOPMENT.md) for workflow
- **Overview**: See [README.md](../README.md) for complete documentation

## Architecture

```
React App (Redux) --HTTP--> ApiGateway --routes--> CatalogService (SQL)
                                            |-----> OrdersService (SQL)
```

## Backend Architecture

### Database & ORM
- **Entity Framework Core 8.0.15** with SQL Server
- Each service has its own database (CatalogDb, OrdersDb) with Windows authentication
- Entities use POCO pattern with auto-properties and nullable reference types enabled
- Migrations in `[ServiceName]/Migrations/` directory

### Service Structure (Program.cs Pattern)
```csharp
// 1. DbContext registration
builder.Services.AddDbContext<ServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceDb")));

// 2. Controllers
builder.Services.AddControllers();

// 3. Swagger (Development only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Use CORS, routing, etc.
app.UseCors("AllowReact");
app.MapControllers();
```

### Controllers Pattern
- Route: `[Route("api/[controller]")]`
- All methods async (Task<ActionResult<T>>)
- Use `AsNoTracking()` for read operations
- DI via constructor for DbContext

### Data Models
```csharp
// Entities (simple POCO)
public class Product { int Id; string Name; decimal Price; int Stock; }
public class Order { int Id; string OrderNumber; OrderStatus Status; List<OrderItem> Items; }

// Shared DTOs in MiniShop.Contracts
public class ProductDto { int Id; string Name; decimal Price; int Stock; }
public class OrderDto { int Id; string OrderNumber; OrderStatus Status; List<OrderItemDto> Items; decimal Total; }
```

## Frontend Architecture

### Redux Store Structure
```
store/
├── productSlice.ts
│   ├── State: { items, loading, error, selectedProduct }
│   ├── Thunks: fetchProducts, fetchProductById, createProduct, updateProduct, deleteProduct
│   └── Reducers: clearError
├── orderSlice.ts
│   ├── State: { items, cart, loading, error, selectedOrder }
│   ├── Thunks: fetchOrders, createOrder, updateOrderStatus, deleteOrder
│   └── Reducers: addToCart, removeFromCart, clearCart, clearError
└── hooks.ts (useAppDispatch, useAppSelector)
```

### API Client (services/api.ts)
- Base URL from `REACT_APP_API_URL` environment variable
- Axios instance for HTTP calls
- Typed interfaces for Product, Order, OrderItem, OrderStatus
- Exported functions: `productAPI.getAll()`, `orderAPI.create()`, etc.

### Components
- **ProductList.tsx**: Displays products in grid, uses Redux to fetch products
- **OrderList.tsx**: Displays orders in table, shows status with color coding
- **App.tsx**: Main component with tab navigation (Products/Orders)
- CSS files provide styling with flexbox/grid layouts

### Styling Conventions
- CSS Grid for product display (minmax 250px columns)
- Flexbox for layouts
- Color scheme: #2c3e50 (dark blue), #3498db (bright blue), #27ae60 (green)
- Status badges with enum-based color: Pending (orange), Processing (blue), Completed (green), Cancelled (red)

## Development Workflows

### Running Services
```bash
# Backend - each in separate terminal from repo root
dotnet run --project CatalogService    # https://localhost:7001
dotnet run --project OrdersService     # https://localhost:7002
dotnet run --project ApiGateway        # https://localhost:7000

# Frontend - from frontend/ directory
npm start                               # http://localhost:3000
```

### Database Operations
```bash
# Create migration
dotnet ef migrations add "MigrationName" --project CatalogService

# Apply migration
dotnet ef database update --project CatalogService

# View database
# CatalogService: Database=MiniShop_Catalog
# OrdersService: Database=MiniShop_Orders
```

### API Testing
- Use Swagger UI at service URLs: `https://localhost:7001/swagger`
- Or use REST Client extensions with `.http` files

## Project-Specific Conventions

### Backend
- Nullable ref types: `<Nullable>enable</Nullable>`
- Implicit usings enabled
- .NET 9.0 target framework
- Folder structure: Controllers/, Data/, Entities/, Migrations/
- CORS policy name: "AllowReact"
- Connection strings use Windows authentication + TrustServerCertificate=True

### Frontend
- TypeScript strict mode
- Redux Toolkit for state management
- Functional components with hooks
- CSS modules or plain CSS per component
- Axios for HTTP (base URL from env)
- Redux hooks: `useAppDispatch`, `useAppSelector` (typed)

## Key Files

**Backend:**
- [../CatalogService/Program.cs](../CatalogService/Program.cs) - Service template
- [../CatalogService/Controllers/ProductsController.cs](../CatalogService/Controllers/ProductsController.cs) - CRUD example
- [../OrdersService/Data/OrdersDbContext.cs](../OrdersService/Data/OrdersDbContext.cs) - DbContext with relationships
- [../MiniShop.Contracts/Class1.cs](../MiniShop.Contracts/Class1.cs) - Shared DTOs

**Frontend:**
- [../frontend/src/redux/store.ts](../frontend/src/redux/store.ts) - Store configuration
- [../frontend/src/redux/productSlice.ts](../frontend/src/redux/productSlice.ts) - Product state management
- [../frontend/src/redux/orderSlice.ts](../frontend/src/redux/orderSlice.ts) - Order state management
- [../frontend/src/services/api.ts](../frontend/src/services/api.ts) - API client
- [../frontend/src/components/ProductList.tsx](../frontend/src/components/ProductList.tsx) - Product display
- [../frontend/src/App.tsx](../frontend/src/App.tsx) - Main app layout

## Development Notes

- Services use Windows auth for local dev (`Trusted_Connection=True`)
- HTTPS enabled in all services; trust dev certs with `dotnet dev-certs https --trust`
- AllowedHosts = "*" in appsettings.json (review for production)
- React dev server runs on http://localhost:3000, backend on https://localhost:7xxx
- Redux actions auto-clear errors on new request; use `clearError()` action if needed
- Product and Order entities use decimal for prices; OrderStatus is enum (0-3)

## Cross-Service Communication

- ApiGateway routes `/api/products/*` to CatalogService (port 7001)
- ApiGateway routes `/api/orders/*` to OrdersService (port 7002)
- Frontend calls through ApiGateway (port 7000) via axios client
- Shared DTOs in MiniShop.Contracts ensure type consistency across services
