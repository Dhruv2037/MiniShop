# 🚀 Next Steps to Run the Application

## Prerequisites Check

Before starting, verify you have:
- [ ] .NET 9.0 SDK installed (`dotnet --version` should show 9.0.x)
- [ ] Node.js 18+ installed (`node --version` should show v18+)
- [ ] SQL Server 2019+ running
- [ ] VS Code or Visual Studio installed

**Need help?** See [ENVIRONMENT_SETUP.md](ENVIRONMENT_SETUP.md)

---

## Step 1: Setup (5 minutes)

### Windows
```bash
cd c:\DhruvsStudy\MiniShop
setup.bat
```

### macOS/Linux
```bash
cd ~/DhruvsStudy/MiniShop
chmod +x setup.sh
./setup.sh
```

This will:
- Build the .NET solution
- Install npm dependencies
- Verify your environment

---

## Step 2: Create Databases (2 minutes)

Use **SQL Server Management Studio** or **Azure Data Studio**:

```sql
CREATE DATABASE MiniShop_Catalog;
CREATE DATABASE MiniShop_Orders;
```

Or use EF Core to auto-create:
```bash
dotnet ef database update --project CatalogService
dotnet ef database update --project OrdersService
```

---

## Step 3: Trust HTTPS Certificate (1 minute)

```bash
dotnet dev-certs https --trust
```

Accept any prompts to trust the development certificate.

---

## Step 4: Start Backend Services (In 3 Separate Terminals)

### Terminal 1: CatalogService
```bash
cd c:\DhruvsStudy\MiniShop
dotnet run --project CatalogService
```

You should see:
```
info: Microsoft.AspNetCore.Hosting.Diagnostics
      Application started. Press Ctrl+C to shut down.
```

### Terminal 2: OrdersService
```bash
cd c:\DhruvsStudy\MiniShop
dotnet run --project OrdersService
```

### Terminal 3: ApiGateway
```bash
cd c:\DhruvsStudy\MiniShop
dotnet run --project ApiGateway
```

**Wait for all 3 to start successfully before moving to Step 5.**

---

## Step 5: Start React Frontend

### Terminal 4: Frontend
```bash
cd c:\DhruvsStudy\MiniShop\frontend
npm start
```

The browser should automatically open to http://localhost:3000

If not, navigate to: **http://localhost:3000**

---

## ✅ Verify Everything Works

### Check Backend Services
- CatalogService: https://localhost:7001/swagger
- OrdersService: https://localhost:7002/swagger
- ApiGateway: https://localhost:7000/api/products

### Check Frontend
- Main app: http://localhost:3000
- Click "Products" tab → should see empty list (no products yet)
- Click "Orders" tab → should see empty list (no orders yet)

---

## 🎮 Try It Out

### Create a Product
1. Open https://localhost:7001/swagger
2. Click on "POST /api/products"
3. Click "Try it out"
4. Enter JSON:
```json
{
  "name": "Laptop",
  "price": 999.99,
  "stock": 10
}
```
5. Click "Execute"
6. Should see 200 response with product ID 1

### View Products in Frontend
1. Go to http://localhost:3000
2. Click "Products" tab
3. Should see the Laptop card with price and stock

### Create an Order
1. Open https://localhost:7002/swagger
2. Click on "POST /api/orders"
3. Click "Try it out"
4. Enter JSON:
```json
{
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 999.99
    }
  ]
}
```
5. Click "Execute"
6. Should see 201 response with order ID

### View Orders in Frontend
1. Go to http://localhost:3000
2. Click "Orders" tab
3. Should see the order with status "Pending" (orange badge)

---

## 🐛 If Something Goes Wrong

### Backend Won't Start
```bash
# Check port is not in use
netstat -ano | findstr :7001

# Try this if port is stuck
taskkill /PID [PID] /F
```

### Database Connection Error
```
Make sure:
1. SQL Server is running
2. Windows Authentication is enabled
3. You created the databases
```

### Frontend Can't Connect to Backend
```
Make sure:
1. All 3 backend services are running
2. Check browser console (F12) for errors
3. Check REACT_APP_API_URL in frontend/.env
```

### HTTPS Certificate Error
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

**See [ENVIRONMENT_SETUP.md](ENVIRONMENT_SETUP.md) for more troubleshooting**

---

## 📝 What Each Tab Does

### Products Tab
- **View:** Grid of products with price and stock
- **Features:** Shows all products from CatalogService
- **URL:** GET https://localhost:7000/api/products

### Orders Tab
- **View:** Table of orders with status badges
- **Features:** Shows all orders with color-coded status
  - Orange = Pending
  - Blue = Processing
  - Green = Completed
  - Red = Cancelled
- **URL:** GET https://localhost:7000/api/orders

---

## 🎯 Common Tasks

### Add More Products
```bash
# Use Swagger at https://localhost:7001/swagger
# Or use REST Client extension in VS Code
# Or use curl:

curl -X POST https://localhost:7001/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Mouse","price":29.99,"stock":50}'
```

### Check API Gateway
```bash
# Should route to CatalogService:
curl https://localhost:7000/api/products

# Should route to OrdersService:
curl https://localhost:7000/api/orders
```

### View Databases
Use **SQL Server Management Studio** or **Azure Data Studio**:
- Server: localhost
- Database: MiniShop_Catalog or MiniShop_Orders
- Tables: Products, Orders, OrderItems

---

## 📚 Need Help?

| Issue | Solution |
|-------|----------|
| Setup questions | [ENVIRONMENT_SETUP.md](ENVIRONMENT_SETUP.md) |
| API not working | [API_CONTRACTS.md](API_CONTRACTS.md) |
| Development tips | [DEVELOPMENT.md](DEVELOPMENT.md) |
| Architecture | [.github/copilot-instructions.md](.github/copilot-instructions.md) |
| Complete overview | [README.md](README.md) |

---

## ✨ Success Criteria

You'll know everything is working when:

✅ All 3 backend services start without errors  
✅ Frontend opens at http://localhost:3000  
✅ Products tab shows empty grid  
✅ Orders tab shows empty table  
✅ Can create a product via Swagger  
✅ Can see the product in Products tab  
✅ Can create an order via Swagger  
✅ Can see the order in Orders tab with correct status  

---

## 📋 Checklist

- [ ] Prerequisites installed (.NET, Node, SQL Server)
- [ ] Run setup script
- [ ] Create SQL databases
- [ ] Trust HTTPS certificate
- [ ] Start CatalogService (Terminal 1)
- [ ] Start OrdersService (Terminal 2)
- [ ] Start ApiGateway (Terminal 3)
- [ ] Start React frontend (Terminal 4)
- [ ] Test via Swagger UI
- [ ] Test via React frontend
- [ ] Create a product and order
- [ ] Verify everything works

---

## 🎉 You're Ready!

The application is fully built and ready to run.

**Time estimate for this setup:** ~10-15 minutes (depending on your system)

**Let's go!** Start with the setup script above.

---

**Questions?** Check the documentation files linked above.

**Ready to develop?** See [DEVELOPMENT.md](DEVELOPMENT.md) for next steps.

Built: December 31, 2025
