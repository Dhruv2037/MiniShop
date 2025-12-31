using CatalogService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CatalogDb")));

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();

app.MapControllers();

// Ensure database is migrated and seed sample products if empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    try
    {
        db.Database.Migrate();

        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new CatalogService.Entities.Product { Name = "Laptop", Price = 999.99m, Stock = 10 },
                new CatalogService.Entities.Product { Name = "Mouse", Price = 29.99m, Stock = 50 },
                new CatalogService.Entities.Product { Name = "Keyboard", Price = 79.99m, Stock = 25 }
            );
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating or seeding the DB.");
    }
}

app.Run();
