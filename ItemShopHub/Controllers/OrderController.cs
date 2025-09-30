using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ItemShopHub.Data;
using ItemShopHub.Shared.Models;
using ItemShopHub.Services;
using System.Security.Claims;

namespace ItemShopHub.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("Fixed")]
public class OrderController(ApplicationDbContext ctx, IEmailNotificationService emailService, IPdfGenerationService pdfService, INotificationService notificationService) : ControllerBase
{
    [HttpGet("")]
    [EnableQuery]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IQueryable<Order>> Get()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Administrator");
        var orders = isAdmin 
            ? ctx.Order.Include(x => x.Items).ThenInclude(x => x.Product)
            : ctx.Order.Where(x => x.UserId == userId).Include(x => x.Items).ThenInclude(x => x.Product);

        return Ok(orders);
    }

    [HttpGet("{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> GetAsync(long key)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Administrator");
        var order = await ctx.Order
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == key && (isAdmin || x.UserId == userId));

        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost("create-from-cart")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Order>> CreateFromCartAsync([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var cart = await ctx.ShoppingCart
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || cart.Items == null || !cart.Items.Any())
            return BadRequest("Cart is empty");

        // Calculate totals
        var subtotal = cart.Items.Sum(item => (item.UnitPrice ?? 0) * item.Quantity);
        
        // Calculate tax
        var taxRate = await ctx.TaxRate.FirstOrDefaultAsync(x => x.StateCode == request.BillingStateCode && x.IsActive);
        var taxAmount = taxRate != null ? subtotal * taxRate.CombinedTaxRate / 100 : 0;
        
        var total = subtotal + taxAmount + (request.ShippingAmount ?? 0);

        // Create order
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            ShippingAmount = request.ShippingAmount ?? 0,
            TotalAmount = total,
            PaymentMethod = request.PaymentMethod,
            ShippingAddress = request.ShippingAddress,
            BillingAddress = request.BillingAddress,
            Notes = request.Notes,
            EstimatedDeliveryDate = DateTime.UtcNow.AddDays(7), // Default 7 days
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = (item.UnitPrice ?? 0) * item.Quantity
            }).ToList()
        };

        ctx.Order.Add(order);

        // Clear the cart
        ctx.ShoppingCartItem.RemoveRange(cart.Items);
        cart.Items.Clear();

        await ctx.SaveChangesAsync();

        // Create notification for new order
        await CreateOrderStatusNotificationAsync(order, OrderStatus.Pending);

        return CreatedAtAction(nameof(GetAsync), new { key = order.Id }, order);
    }

    [HttpPut("{key}/status")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> UpdateStatusAsync(long key, [FromBody] UpdateOrderStatusRequest request)
    {
        var order = await ctx.Order.FindAsync(key);
        if (order == null)
            return NotFound();

        order.Status = request.Status;
        
        if (request.TrackingNumber != null)
            order.TrackingNumber = request.TrackingNumber;

        if (request.Status == OrderStatus.Shipped && order.ActualDeliveryDate == null)
            order.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3);

        if (request.Status == OrderStatus.Delivered)
            order.ActualDeliveryDate = DateTime.UtcNow;

        await ctx.SaveChangesAsync();

        // Create notification for order status update
        await CreateOrderStatusNotificationAsync(order, request.Status);

        return Ok(order);
    }

    [HttpPost("{key}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Order>> CancelOrderAsync(long key)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Administrator");
        var order = await ctx.Order.FirstOrDefaultAsync(x => x.Id == key && (isAdmin || x.UserId == userId));

        if (order == null)
            return NotFound();

        if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            return BadRequest("Cannot cancel shipped or delivered orders");

        order.Status = OrderStatus.Cancelled;
        await ctx.SaveChangesAsync();

        // Create notification for order cancellation
        await CreateOrderStatusNotificationAsync(order, OrderStatus.Cancelled);

        return Ok(order);
    }

    [HttpPost("send-confirmation-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SendConfirmationEmailAsync([FromBody] SendOrderEmailRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Administrator");
        var order = await ctx.Order
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == request.OrderId && (isAdmin || x.UserId == userId));

        if (order == null)
            return NotFound();

        try
        {
            await emailService.SendOrderConfirmationAsync(order, request.CustomerEmail);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to send email: {ex.Message}");
        }
    }

    [HttpGet("{key}/pdf")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadOrderPdfAsync(long key)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var isAdmin = User.IsInRole("Administrator");
        var order = await ctx.Order
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == key && (isAdmin || x.UserId == userId));

        if (order == null)
            return NotFound();

        try
        {
            var pdfBytes = pdfService.GenerateOrderPdf(order);
            var fileName = $"Order-{order.OrderNumber}.pdf";
            
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Failed to generate PDF: {ex.Message}");
        }
    }

    private async Task CreateOrderStatusNotificationAsync(Order order, OrderStatus status)
    {
        try
        {
            var (title, message, notificationType) = status switch
            {
                OrderStatus.Pending => (
                    "Order Created",
                    $"Your order {order.OrderNumber} has been created successfully. Total: ${order.TotalAmount:F2}",
                    "Success"
                ),
                OrderStatus.Processing => (
                    "Order Processing",
                    $"Your order {order.OrderNumber} is now being processed.",
                    "Info"
                ),
                OrderStatus.Shipped => (
                    "Order Shipped",
                    $"Good news! Your order {order.OrderNumber} has been shipped. Tracking: {order.TrackingNumber ?? "Available soon"}",
                    "Success"
                ),
                OrderStatus.Delivered => (
                    "Order Delivered",
                    $"Your order {order.OrderNumber} has been delivered successfully!",
                    "Success"
                ),
                OrderStatus.Cancelled => (
                    "Order Cancelled",
                    $"Order {order.OrderNumber} has been cancelled.",
                    "Warning"
                ),
                OrderStatus.Refunded => (
                    "Order Refunded",
                    $"Your refund for order {order.OrderNumber} has been processed.",
                    "Info"
                ),
                _ => (
                    "Order Updated",
                    $"Order {order.OrderNumber} status has been updated to {status}.",
                    "Info"
                )
            };

            await notificationService.CreateNotificationAsync(
                title: title,
                message: message,
                type: notificationType,
                userId: order.UserId,
                actionUrl: $"/orders/{order.Id}",
                notes: $"Order #{order.OrderNumber}"
            );
        }
        catch (Exception ex)
        {
            // Error logged internally but don't fail the order status update process
        }
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(100000, 999999)}";
    }
}

public class CreateOrderRequest
{
    public string? ShippingAddress { get; set; }
    public string? BillingAddress { get; set; }
    public string BillingStateCode { get; set; } = "";
    public decimal? ShippingAmount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? TrackingNumber { get; set; }
}

public class SendOrderEmailRequest
{
    public long OrderId { get; set; }
    public string CustomerEmail { get; set; } = "";
}
