# API Contract Documentation

## Overview

All API endpoints are accessed through the **ApiGateway** (Port 7000) which routes requests to:
- **CatalogService** (Port 7001) for product endpoints
- **OrdersService** (Port 7002) for order endpoints

Base URL: `https://localhost:7000/api`

## Data Models

### Product
```typescript
{
  id: number;
  name: string;
  price: decimal;
  stock: number;
}
```

### Order
```typescript
{
  id: number;
  orderNumber: string;
  createdDate: string (ISO 8601);
  status: OrderStatus;
  items: OrderItem[];
  total: decimal;
}
```

### OrderItem
```typescript
{
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: decimal;
}
```

### OrderStatus (Enum)
```
0 = Pending
1 = Processing
2 = Completed
3 = Cancelled
```

## Product Endpoints

### GET /api/products
Get all products

**Response:**
```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 999.99,
    "stock": 10
  }
]
```

**Status:** 200 OK

---

### GET /api/products/{id}
Get product by ID

**Parameters:**
- `id` (int, path): Product ID

**Response:**
```json
{
  "id": 1,
  "name": "Laptop",
  "price": 999.99,
  "stock": 10
}
```

**Status:** 200 OK | 404 Not Found

---

### POST /api/products
Create new product

**Request Body:**
```json
{
  "name": "Mouse",
  "price": 29.99,
  "stock": 50
}
```

**Response:**
```json
{
  "id": 2,
  "name": "Mouse",
  "price": 29.99,
  "stock": 50
}
```

**Status:** 201 Created

---

### PUT /api/products/{id}
Update product

**Parameters:**
- `id` (int, path): Product ID

**Request Body:**
```json
{
  "name": "Updated Mouse",
  "price": 34.99,
  "stock": 45
}
```

**Response:** Empty body

**Status:** 204 No Content | 404 Not Found

---

### DELETE /api/products/{id}
Delete product

**Parameters:**
- `id` (int, path): Product ID

**Response:** Empty body

**Status:** 204 No Content | 404 Not Found

---

## Order Endpoints

### GET /api/orders
Get all orders

**Response:**
```json
[
  {
    "id": 1,
    "orderNumber": "ORD-132524865012345",
    "createdDate": "2025-01-01T10:30:00Z",
    "status": 0,
    "items": [
      {
        "productId": 1,
        "productName": "Laptop",
        "quantity": 1,
        "unitPrice": 999.99
      }
    ],
    "total": 999.99
  }
]
```

**Status:** 200 OK

---

### GET /api/orders/{id}
Get order by ID

**Parameters:**
- `id` (int, path): Order ID

**Response:**
```json
{
  "id": 1,
  "orderNumber": "ORD-132524865012345",
  "createdDate": "2025-01-01T10:30:00Z",
  "status": 0,
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 999.99
    }
  ],
  "total": 999.99
}
```

**Status:** 200 OK | 404 Not Found

---

### POST /api/orders
Create new order

**Request Body:**
```json
{
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 999.99
    },
    {
      "productId": 2,
      "productName": "Mouse",
      "quantity": 2,
      "unitPrice": 29.99
    }
  ]
}
```

**Response:**
```json
{
  "id": 1,
  "orderNumber": "ORD-132524865012345",
  "createdDate": "2025-01-01T10:30:00Z",
  "status": 0,
  "items": [
    {
      "productId": 1,
      "productName": "Laptop",
      "quantity": 1,
      "unitPrice": 999.99
    },
    {
      "productId": 2,
      "productName": "Mouse",
      "quantity": 2,
      "unitPrice": 29.99
    }
  ],
  "total": 1059.97
}
```

**Status:** 201 Created

---

### PUT /api/orders/{id}/status
Update order status

**Parameters:**
- `id` (int, path): Order ID

**Request Body:**
```json
{
  "status": 1
}
```

**Response:** Empty body

**Status:** 204 No Content | 404 Not Found

---

### DELETE /api/orders/{id}
Delete order

**Parameters:**
- `id` (int, path): Order ID

**Response:** Empty body

**Status:** 204 No Content | 404 Not Found

---

## HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200  | OK - Request successful |
| 201  | Created - Resource created successfully |
| 204  | No Content - Successful update/delete |
| 400  | Bad Request - Invalid input |
| 404  | Not Found - Resource doesn't exist |
| 500  | Internal Server Error |

## Error Responses

**400 Bad Request:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "price": ["The field price must be between 0.01 and 999999.99."]
  }
}
```

**404 Not Found:**
```json
null
```

## Testing with cURL

### Get All Products
```bash
curl -X GET https://localhost:7000/api/products \
  -H "Content-Type: application/json"
```

### Create Product
```bash
curl -X POST https://localhost:7000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"Mouse","price":29.99,"stock":50}'
```

### Create Order
```bash
curl -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "items": [
      {"productId":1,"productName":"Laptop","quantity":1,"unitPrice":999.99}
    ]
  }'
```

### Update Order Status
```bash
curl -X PUT https://localhost:7000/api/orders/1/status \
  -H "Content-Type: application/json" \
  -d '{"status":1}'
```

## Testing with REST Client (VS Code)

Create `requests.rest`:

```rest
### Get all products
GET https://localhost:7000/api/products

### Create product
POST https://localhost:7000/api/products
Content-Type: application/json

{
  "name": "Keyboard",
  "price": 79.99,
  "stock": 25
}

### Get all orders
GET https://localhost:7000/api/orders

### Create order
POST https://localhost:7000/api/orders
Content-Type: application/json

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

### Update order status
PUT https://localhost:7000/api/orders/1/status
Content-Type: application/json

{
  "status": 1
}
```

## Swagger Documentation

Access API documentation at service endpoints:

- **CatalogService:** https://localhost:7001/swagger
- **OrdersService:** https://localhost:7002/swagger

Swagger UI provides interactive testing of all endpoints with live requests.

## Rate Limiting

Currently: **None** (development)

For production, consider implementing:
- 100 requests per minute per IP
- 1000 requests per minute per authenticated user

## Authentication

Currently: **None** (development)

For production, implement:
- JWT Bearer tokens
- API key authentication
- OAuth2 integration

## CORS Headers

Response headers from services:
```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization
```

For production:
```
Access-Control-Allow-Origin: https://yourdomain.com
Access-Control-Max-Age: 3600
```

## Versioning Strategy

Currently: **API v1** (no versioning in URL)

For future versions:
- `/api/v1/products`
- `/api/v2/products`

---

**Last Updated:** December 31, 2025
**API Version:** 1.0.0
