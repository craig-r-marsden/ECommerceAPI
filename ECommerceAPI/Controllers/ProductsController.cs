using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        ApplicationDbContext context,
        IInventoryService inventoryService,
        ILogger<ProductsController> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a specific product by its ID, including live inventory data.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A product with its details, price, and stock information if available.</returns>
    /// <response code="200">Returns the requested product with inventory data.</response>
    /// <response code="404">Product with the specified ID was not found.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var inventoryData = await _inventoryService.GetInventoryDataAsync(id, correlationId);

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description
        };

        if (inventoryData != null)
        {
            response.Price = inventoryData.Price;
            response.Stock = inventoryData.Stock;
            response.DataStatus = "Live";
        }
        else
        {
            response.DataStatus = "Data Unavailable - External service error";
            _logger.LogWarning(
                "Unable to fetch inventory data for Product {ProductId}, returning local data only. Correlation-ID: {CorrelationId}",
                id,
                correlationId
            );
        }

        return Ok(response);
    }

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="request">The product creation request containing name and description.</param>
    /// <returns>The newly created product.</returns>
    /// <response code="201">Returns the newly created product.</response>
    /// <response code="400">The request data is invalid.</response>
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogInformation(
            "Created Product {ProductId} with Correlation-ID: {CorrelationId}",
            product.Id,
            correlationId
        );

        var response = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            DataStatus = "Local data only - Price and stock not available at creation"
        };

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, response);
    }

    /// <summary>
    /// Retrieves all products in the catalog with their inventory data.
    /// </summary>
    /// <returns>A list of all products with their details, prices, and stock information.</returns>
    /// <response code="200">Returns the list of all products.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
    {
        var products = await _context.Products.ToListAsync();

        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var response = new List<ProductResponse>();

        foreach (var product in products)
        {
            var inventoryData = await _inventoryService.GetInventoryDataAsync(product.Id, correlationId);

            var productResponse = new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description
            };

            if (inventoryData != null)
            {
                productResponse.Price = inventoryData.Price;
                productResponse.Stock = inventoryData.Stock;
                productResponse.DataStatus = "Live";
            }
            else
            {
                productResponse.DataStatus = "Data Unavailable";
            }

            response.Add(productResponse);
        }

        return Ok(response);
    }
}
