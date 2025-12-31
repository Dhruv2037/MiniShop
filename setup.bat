@echo off
REM MiniShop Full-Stack Setup Script

echo.
echo ================================
echo MiniShop Setup Script
echo ================================
echo.

REM Check for .NET
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET SDK not found. Please install .NET 9.0
    pause
    exit /b 1
)

REM Check for Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js not found. Please install Node.js 18+
    pause
    exit /b 1
)

echo [✓] .NET SDK found: 
dotnet --version
echo.

echo [✓] Node.js found:
node --version
echo.

echo ================================
echo 1. Building .NET Solution
echo ================================
dotnet build MiniShop.sln
if errorlevel 1 (
    echo Build failed!
    pause
    exit /b 1
)
echo.

echo ================================
echo 2. Setting up React Frontend
echo ================================
cd frontend
call npm install
if errorlevel 1 (
    echo npm install failed!
    cd ..
    pause
    exit /b 1
)
echo.

echo ================================
echo SETUP COMPLETE!
echo ================================
echo.
echo Next steps:
echo.
echo 1. Trust HTTPS certificate:
echo    dotnet dev-certs https --trust
echo.
echo 2. Create SQL Server databases (run in SSMS):
echo    CREATE DATABASE MiniShop_Catalog;
echo    CREATE DATABASE MiniShop_Orders;
echo.
echo 3. Run migrations:
echo    dotnet ef database update --project CatalogService
echo    dotnet ef database update --project OrdersService
echo.
echo 4. Start backend services (in separate terminals from repo root):
echo    dotnet run --project CatalogService
echo    dotnet run --project OrdersService
echo    dotnet run --project ApiGateway
echo.
echo 5. Start frontend (from frontend directory):
echo    npm start
echo.
echo Application will be available at http://localhost:3000
echo.
cd ..
pause
