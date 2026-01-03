@echo off
title MiniShop Backend Services
color 0A

echo ============================================
echo Starting MiniShop Backend Services
echo ============================================

REM Open PowerShell windows for each service
echo Starting CatalogService (Port 7001)...
start "CatalogService" powershell -NoExit -Command "cd 'c:\DhruvsStudy\MiniShop'; dotnet run --project CatalogService"

timeout /t 2 /nobreak

echo Starting OrdersService (Port 7002)...
start "OrdersService" powershell -NoExit -Command "cd 'c:\DhruvsStudy\MiniShop'; dotnet run --project OrdersService"

timeout /t 2 /nobreak

echo Starting ApiGateway (Port 7000)...
start "ApiGateway" powershell -NoExit -Command "cd 'c:\DhruvsStudy\MiniShop'; dotnet run --project ApiGateway"

timeout /t 3 /nobreak

echo.
echo ============================================
echo Services should be running:
echo - CatalogService: http://localhost:7001
echo - OrdersService:  http://localhost:7002
echo - ApiGateway:     http://localhost:7000
echo ============================================
echo.
pause
