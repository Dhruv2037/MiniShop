#!/bin/bash

# MiniShop Full-Stack Setup Script for Linux/macOS

echo ""
echo "================================"
echo "MiniShop Setup Script"
echo "================================"
echo ""

# Check for .NET
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Please install .NET 9.0"
    exit 1
fi

# Check for Node.js
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js not found. Please install Node.js 18+"
    exit 1
fi

echo "[✓] .NET SDK found:"
dotnet --version
echo ""

echo "[✓] Node.js found:"
node --version
echo ""

echo "================================"
echo "1. Building .NET Solution"
echo "================================"
dotnet build MiniShop.sln
if [ $? -ne 0 ]; then
    echo "Build failed!"
    exit 1
fi
echo ""

echo "================================"
echo "2. Setting up React Frontend"
echo "================================"
cd frontend
npm install
if [ $? -ne 0 ]; then
    echo "npm install failed!"
    exit 1
fi
echo ""

echo "================================"
echo "SETUP COMPLETE!"
echo "================================"
echo ""
echo "Next steps:"
echo ""
echo "1. Trust HTTPS certificate (macOS):"
echo "   dotnet dev-certs https --trust"
echo ""
echo "2. Create SQL Server databases:"
echo "   Note: Requires SQL Server on Linux/macOS or cloud instance"
echo "   CREATE DATABASE MiniShop_Catalog;"
echo "   CREATE DATABASE MiniShop_Orders;"
echo ""
echo "3. Run migrations:"
echo "   dotnet ef database update --project CatalogService"
echo "   dotnet ef database update --project OrdersService"
echo ""
echo "4. Start backend services (in separate terminals from repo root):"
echo "   dotnet run --project CatalogService"
echo "   dotnet run --project OrdersService"
echo "   dotnet run --project ApiGateway"
echo ""
echo "5. Start frontend (from frontend directory):"
echo "   npm start"
echo ""
echo "Application will be available at http://localhost:3000"
echo ""

cd ..
