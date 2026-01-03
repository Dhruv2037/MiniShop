$ErrorActionPreference = 'Continue'

Write-Host "=== Seeding MiniShop Demo Data ===" -ForegroundColor Cyan

# Products to add
$productData = @(
    @{name='MacBook Pro 16';price=2499.99;stock=5},
    @{name='iPhone 15 Pro';price=1299.99;stock=15},
    @{name='iPad Air';price=899.99;stock=8},
    @{name='Apple Watch Ultra';price=799.99;stock=12},
    @{name='AirPods Pro Max';price=549.99;stock=10},
    @{name='Magic Mouse';price=79.99;stock=20},
    @{name='USB-C Cable';price=19.99;stock=50}
)

# Create products
Write-Host "`n=== Adding Products ===" -ForegroundColor Yellow
foreach ($prod in $productData) {
    $json = @{name=$prod.name;price=$prod.price;stock=$prod.stock} | ConvertTo-Json
    try {
        $result = Invoke-RestMethod -Uri 'https://localhost:7000/api/products' -Method POST -ContentType 'application/json' -Body $json -SkipCertificateCheck
        Write-Host "✓ $($prod.name)" -ForegroundColor Green
    } catch {
        Write-Host "✗ $($prod.name) - $_" -ForegroundColor Red
    }
}

# Get all products
Write-Host "`n=== All Products ===" -ForegroundColor Yellow
try {
    $allProds = Invoke-RestMethod -Uri 'https://localhost:7000/api/products' -SkipCertificateCheck
    foreach ($p in $allProds) {
        Write-Host "  [$($p.id)] $($p.name) - `$$($p.price) (Stock: $($p.stock))"
    }
} catch {
    Write-Host "Error fetching products: $_" -ForegroundColor Red
}

# Create sample order
Write-Host "`n=== Creating Sample Order ===" -ForegroundColor Yellow
$orderBody = @{
    items = @(
        @{productId=1;productName='MacBook Pro 16';quantity=1;unitPrice=2499.99},
        @{productId=2;productName='iPhone 15 Pro';quantity=2;unitPrice=1299.99}
    )
} | ConvertTo-Json -Depth 10

try {
    $orderRes = Invoke-RestMethod -Uri 'https://localhost:7000/api/orders' -Method POST -ContentType 'application/json' -Body $orderBody -SkipCertificateCheck
    Write-Host "✓ Order Created: $($orderRes.orderNumber)" -ForegroundColor Green
    Write-Host "  Items: $($orderRes.items.Count)"
    Write-Host "  Total: `$$($orderRes.total)"
} catch {
    Write-Host "Error creating order: $_" -ForegroundColor Red
}

Write-Host "`n✓ Demo data seeded successfully!" -ForegroundColor Green
Write-Host "Open http://localhost:3001 to see the application" -ForegroundColor Cyan
