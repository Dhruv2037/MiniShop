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

// Forward products requests to CatalogService (HTTPS port 7001)
app.MapGet("/api/products", async (HttpClient client) =>
{
    var response = await client.GetAsync("http://localhost:5198/api/products");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Get Products").WithOpenApi();

app.MapPost("/api/products", async (HttpClient client, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var bodyString = await reader.ReadToEndAsync();
    var ct = request.ContentType ?? "application/json";
    var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
    var response = await client.PostAsync("http://localhost:5198/api/products", contentMsg);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Create Product").WithOpenApi();

app.MapGet("/api/orders", async (HttpClient client) =>
{
    var response = await client.GetAsync("http://localhost:5210/api/orders");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Get Orders").WithOpenApi();

app.MapPost("/api/orders", async (HttpClient client, HttpRequest request) =>
{
    using var reader = new StreamReader(request.Body);
    var bodyString = await reader.ReadToEndAsync();
    var ct = request.ContentType ?? "application/json";
    var contentMsg = new StringContent(bodyString, System.Text.Encoding.UTF8, ct);
    var response = await client.PostAsync("http://localhost:5210/api/orders", contentMsg);
    var content = await response.Content.ReadAsStringAsync();
    return Results.Text(content, contentType: "application/json", statusCode: (int)response.StatusCode);
}).WithName("Create Order").WithOpenApi();

app.Run();
