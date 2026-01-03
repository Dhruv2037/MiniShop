using System.Net.Http;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// HTTP Client for downstream services - bypass cert validation for dev
var insecureHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true
};
builder.Services.AddSingleton(new HttpClient(insecureHandler));

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// app.UseHttpsRedirection(); // Disabled for dev - no HTTPS port configured in HTTP profile
app.UseCors("AllowReact");

// Gateway health check
app.MapGet("/health", () => "Gateway is healthy").WithName("Health").WithOpenApi();

// ============================================
// PRODUCTS ENDPOINTS (Forward to CatalogService:7001)
// ============================================

// GET /api/products - Get all products
app.MapGet("/api/products", async (HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync("http://localhost:7001/api/products");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Get Products").WithOpenApi();

// GET /api/products/{id} - Get product by id
app.MapGet("/api/products/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync($"http://localhost:7001/api/products/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Get Product By Id").WithOpenApi();

// POST /api/products - Create product
app.MapPost("/api/products", async (HttpClient client, HttpRequest request) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var bodyString = await reader.ReadToEndAsync();
        var ct = request.ContentType ?? "application/json";
        var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
        var response = await client.PostAsync("http://localhost:7001/api/products", contentMsg);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Create Product").WithOpenApi();

// PUT /api/products/{id} - Update product
app.MapPut("/api/products/{id}", async (int id, HttpClient client, HttpRequest request) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var bodyString = await reader.ReadToEndAsync();
        var ct = request.ContentType ?? "application/json";
        var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
        var response = await client.PutAsync($"http://localhost:7001/api/products/{id}", contentMsg);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Update Product").WithOpenApi();

// DELETE /api/products/{id} - Delete product
app.MapDelete("/api/products/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.DeleteAsync($"http://localhost:7001/api/products/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Delete Product").WithOpenApi();

// ============================================
// ORDERS ENDPOINTS (Forward to OrdersService:7002)
// ============================================

// GET /api/orders - Get all orders
app.MapGet("/api/orders", async (HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync("http://localhost:7002/api/orders");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Get Orders").WithOpenApi();

// GET /api/orders/{id} - Get order by id
app.MapGet("/api/orders/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.GetAsync($"http://localhost:7002/api/orders/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Get Order By Id").WithOpenApi();

// POST /api/orders - Create order
app.MapPost("/api/orders", async (HttpClient client, HttpRequest request) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var bodyString = await reader.ReadToEndAsync();
        var ct = request.ContentType ?? "application/json";
        var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
        var response = await client.PostAsync("http://localhost:7002/api/orders", contentMsg);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Create Order").WithOpenApi();

// PUT /api/orders/{id}/status - Update order status
app.MapPut("/api/orders/{id}/status", async (int id, HttpClient client, HttpRequest request) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var bodyString = await reader.ReadToEndAsync();
        var ct = request.ContentType ?? "application/json";
        var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
        var response = await client.PutAsync($"http://localhost:7002/api/orders/{id}/status", contentMsg);
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Update Order Status").WithOpenApi();

// DELETE /api/orders/{id} - Delete order
app.MapDelete("/api/orders/{id}", async (int id, HttpClient client) =>
{
    try
    {
        var response = await client.DeleteAsync($"http://localhost:7002/api/orders/{id}");
        var content = await response.Content.ReadAsStringAsync();
        return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 503);
    }
}).WithName("Delete Order").WithOpenApi();

app.Run();
