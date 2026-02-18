using ECommerceAPI.Controllers;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductsService> _mockProductsService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductsService = new Mock<IProductsService>();
        _controller = new ProductsController(_mockProductsService.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.HttpContext.Items["CorrelationId"] = "test-correlation-id";
    }

    [Fact]
    public async Task GetProduct_WhenProductExists_ReturnsOkResult()
    {
        // Arrange
        var productId = 1;
        var expectedProduct = new ProductResponse
        {
            Id = productId,
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10,
            DataStatus = "Live"
        };

        _mockProductsService
            .Setup(s => s.GetProductAsync(productId, It.IsAny<string>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var okResult = Assert.IsType<ActionResult<ProductResponse>>(result);
        var okObjectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var returnedProduct = Assert.IsType<ProductResponse>(okObjectResult.Value);
        Assert.Equal(productId, returnedProduct.Id);
        Assert.Equal("Test Product", returnedProduct.Name);
    }

    [Fact]
    public async Task GetProduct_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = 999;
        _mockProductsService
            .Setup(s => s.GetProductAsync(productId, It.IsAny<string>()))
            .ReturnsAsync((ProductResponse?)null);

        // Act
        var result = await _controller.GetProduct(productId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ProductResponse>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task GetProduct_UsesCorrelationIdFromHttpContext()
    {
        // Arrange
        var productId = 1;
        var correlationId = "specific-correlation-id";
        _controller.HttpContext.Items["CorrelationId"] = correlationId;
        string? capturedCorrelationId = null;

        _mockProductsService
            .Setup(s => s.GetProductAsync(productId, It.IsAny<string>()))
            .Callback<int, string>((id, corrId) => capturedCorrelationId = corrId)
            .ReturnsAsync(new ProductResponse { Id = productId });

        // Act
        await _controller.GetProduct(productId);

        // Assert
        Assert.Equal(correlationId, capturedCorrelationId);
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "New Description"
        };

        var createdProduct = new ProductResponse
        {
            Id = 1,
            Name = request.Name,
            Description = request.Description,
            DataStatus = "Local data only - Price and stock not available at creation"
        };

        _mockProductsService
            .Setup(s => s.CreateProductAsync(request, It.IsAny<string>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ProductResponse>>(result);
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        Assert.Equal(nameof(_controller.GetProduct), createdAtActionResult.ActionName);
        
        var returnedProduct = Assert.IsType<ProductResponse>(createdAtActionResult.Value);
        Assert.Equal(1, returnedProduct.Id);
        Assert.Equal("New Product", returnedProduct.Name);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            Description = "Description"
        };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateProduct(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<ProductResponse>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.IsType<SerializableError>(badRequestResult.Value);
    }

    [Fact]
    public async Task GetAllProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<ProductResponse>
        {
            new ProductResponse { Id = 1, Name = "Product 1", Description = "Desc 1", Price = 10m, Stock = 5 },
            new ProductResponse { Id = 2, Name = "Product 2", Description = "Desc 2", Price = 20m, Stock = 10 },
            new ProductResponse { Id = 3, Name = "Product 3", Description = "Desc 3", Price = 30m, Stock = 15 }
        };

        _mockProductsService
            .Setup(s => s.GetAllProductsAsync(It.IsAny<string>()))
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductResponse>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Equal(3, returnedProducts.Count());
    }

    [Fact]
    public async Task GetAllProducts_WhenNoProducts_ReturnsEmptyList()
    {
        // Arrange
        _mockProductsService
            .Setup(s => s.GetAllProductsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ProductResponse>());

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<ProductResponse>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductResponse>>(okResult.Value);
        Assert.Empty(returnedProducts);
    }
}
