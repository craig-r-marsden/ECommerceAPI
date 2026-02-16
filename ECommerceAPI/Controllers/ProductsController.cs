using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    /// <summary>
    /// Retrieves a specific product by its ID, including live inventory data.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>A product with its details, price, and stock information if available.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Products/3
    /// </remarks>
    /// <response code="200">Returns the requested product with inventory data.</response>
    /// <response code="404">Product with the specified ID was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var response = await _productsService.GetProductAsync(id, correlationId);

        if (response == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        return Ok(response);
    }

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="request">The product creation request containing name and description.</param>
    /// <returns>The newly created product.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/Products
    ///     {
    ///         "name":"Laptop",
    ///         "description":"Gaming laptop with RTX 4080"
    ///     }
    /// </remarks>
    /// <response code="201">Returns the newly created product.</response>
    /// <response code="400">The request data is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var response = await _productsService.CreateProductAsync(request, correlationId);

        return CreatedAtAction(nameof(GetProduct), new { id = response.Id }, response);
    }

    /// <summary>
    /// Retrieves all products in the catalog with their inventory data.
    /// </summary>
    /// <returns>A list of all products with their details, prices, and stock information.</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/Products
    /// </remarks>
    /// <response code="200">Returns the list of all products.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAllProducts()
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        var response = await _productsService.GetAllProductsAsync(correlationId);

        return Ok(response);
    }
}
