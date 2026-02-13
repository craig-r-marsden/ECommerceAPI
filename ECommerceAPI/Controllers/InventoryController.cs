using ECommerceAPI.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly ILogger<InventoryController> _logger;
    private static readonly Random _random = new();

    public InventoryController(ILogger<InventoryController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves mock inventory data for a specific product.
    /// </summary>
    /// <param name="productId">The unique identifier of the product.</param>
    /// <returns>Inventory data including price and stock quantity.</returns>
    /// <response code="200">Returns the inventory data for the specified product.</response>
    /// <remarks>
    /// This is a mock endpoint that generates random inventory data for testing purposes.
    /// </remarks>
    [HttpGet("{productId}")]
    public ActionResult<InventoryData> GetInventory(int productId)
    {
        var correlationId = Request.Headers["X-Correlation-ID"].FirstOrDefault();

        _logger.LogInformation(
            "Mock Inventory API - Request for Product {ProductId} with Correlation-ID: {CorrelationId}",
            productId,
            correlationId
        );

        var inventoryData = new InventoryData
        {
            Price = Math.Round((decimal)(_random.NextDouble() * 1000 + 10), 2),
            Stock = _random.Next(0, 100)
        };

        return Ok(inventoryData);
    }
}
