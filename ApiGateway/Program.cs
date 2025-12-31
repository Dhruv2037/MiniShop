var builder = WebApplication.CreateBuilder(args);

// HTTP Client for downstream services
builder.Services.AddHttpClient();

// CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowReact");

// Gateway health check
app.MapGet("/health", () => "Gateway is healthy").WithName("Health").WithOpenApi();

// Forward products requests to CatalogService
app.MapGet("/api/products", async (HttpClient client) =>
{
    var response = await client.GetAsync("https://localhost:5198/api/products");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Get Products").WithOpenApi();

app.MapPost("/api/products", async (HttpClient client, HttpRequest request) =>
{
    var body = new StreamContent(request.Body);
    var response = await client.PostAsync("https://localhost:5198/api/products", body);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Create Product").WithOpenApi();

// Forward orders requests to OrdersService
app.MapGet("/api/orders", async (HttpClient client) =>
{
    var response = await client.GetAsync("https://localhost:5210/api/orders");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Get Orders").WithOpenApi();

app.MapPost("/api/orders", async (HttpClient client, HttpRequest request) =>
{
    var body = new StreamContent(request.Body);
    var response = await client.PostAsync("https://localhost:5210/api/orders", body);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Create Order").WithOpenApi();

app.Run();
