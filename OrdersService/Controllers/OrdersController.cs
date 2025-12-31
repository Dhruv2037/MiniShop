using OrdersService.Data;
using OrdersService.Entities;
using MiniShop.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OrdersService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrdersDbContext _db;

    public OrdersController(OrdersDbContext db)
    {
        _db = db;
    }

    // GET: api/orders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        var orders = await _db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .ToListAsync();

        var result = orders.Select(o => MapToDto(o)).ToList();
        return Ok(result);
    }

    // GET: api/orders/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetById(int id)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Ok(MapToDto(order));
    }

    // POST: api/orders
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderRequest request)
    {
        var order = new Order
        {
            OrderNumber = $"ORD-{DateTime.UtcNow.Ticks}",
            CreatedDate = DateTime.UtcNow,
            Status = Entities.OrderStatus.Pending
        };

        foreach (var item in request.Items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
            order.Total += item.Quantity * item.UnitPrice;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToDto(order));
    }

    // PUT: api/orders/{id}/status
    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        order.Status = (Entities.OrderStatus)request.Status;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/orders/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CreatedDate = order.CreatedDate,
            Status = (MiniShop.Contracts.OrderStatus)order.Status,
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            Total = order.Total
        };
    }
}

public class CreateOrderRequest
{
    public List<OrderItemDto> Items { get; set; } = new();
}

public class UpdateStatusRequest
{
    public MiniShop.Contracts.OrderStatus Status { get; set; }
}
