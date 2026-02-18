using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using ECommerceAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class ProductsServiceTests
{
    private readonly Mock<IInventoryService> _mockInventoryService;
    private readonly Mock<ILogger<ProductsService>> _mockLogger;
    private readonly ApplicationDbContext _context;
    private readonly ProductsService _service;

    public ProductsServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mockInventoryService = new Mock<IInventoryService>();
        _mockLogger = new Mock<ILogger<ProductsService>>();
        _service = new ProductsService(_context, _mockInventoryService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetProductAsync_WhenProductExists_WithInventoryData_ReturnsCompleteProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var inventoryData = new InventoryData
        {
            Price = 99.99m,
            Stock = 10
        };
        _mockInventoryService
            .Setup(s => s.GetInventoryDataAsync(1, It.IsAny<string>()))
            .ReturnsAsync(inventoryData);

        // Act
        var result = await _service.GetProductAsync(1, "test-correlation-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal("Test Description", result.Description);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(10, result.Stock);
        Assert.Equal("Live", result.DataStatus);
    }

    [Fact]
    public async Task GetProductAsync_WhenProductExists_WithoutInventoryData_ReturnsProductWithWarning()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Description = "Test Description"
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        _mockInventoryService
            .Setup(s => s.GetInventoryDataAsync(1, It.IsAny<string>()))
            .ReturnsAsync((InventoryData?)null);

        // Act
        var result = await _service.GetProductAsync(1, "test-correlation-id");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Product", result.Name);
        Assert.Null(result.Price);
        Assert.Null(result.Stock);
        Assert.Equal("Data Unavailable - External service error", result.DataStatus);
    }

    [Fact]
    public async Task GetProductAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Act
        var result = await _service.GetProductAsync(999, "test-correlation-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateProductAsync_CreatesProductSuccessfully()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "New Description"
        };

        // Act
        var result = await _service.CreateProductAsync(request, "test-correlation-id");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New Product", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Null(result.Price);
        Assert.Null(result.Stock);
        Assert.Equal("Local data only - Price and stock not available at creation", result.DataStatus);

        var productInDb = await _context.Products.FindAsync(result.Id);
        Assert.NotNull(productInDb);
        Assert.Equal("New Product", productInDb.Name);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithMultipleProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Description 1" },
            new Product { Id = 2, Name = "Product 2", Description = "Description 2" },
            new Product { Id = 3, Name = "Product 3", Description = "Description 3" }
        };
        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        _mockInventoryService
            .Setup(s => s.GetInventoryDataAsync(1, It.IsAny<string>()))
            .ReturnsAsync(new InventoryData { Price = 10.0m, Stock = 5 });
        _mockInventoryService
            .Setup(s => s.GetInventoryDataAsync(2, It.IsAny<string>()))
            .ReturnsAsync((InventoryData?)null);
        _mockInventoryService
            .Setup(s => s.GetInventoryDataAsync(3, It.IsAny<string>()))
            .ReturnsAsync(new InventoryData { Price = 30.0m, Stock = 15 });

        // Act
        var result = await _service.GetAllProductsAsync("test-correlation-id");

        // Assert
        var resultList = result.ToList();
        Assert.Equal(3, resultList.Count);
        
        Assert.Equal("Live", resultList[0].DataStatus);
        Assert.Equal(10.0m, resultList[0].Price);
        
        Assert.Equal("Data Unavailable", resultList[1].DataStatus);
        Assert.Null(resultList[1].Price);
        
        Assert.Equal("Live", resultList[2].DataStatus);
        Assert.Equal(30.0m, resultList[2].Price);
    }

    [Fact]
    public async Task GetAllProductsAsync_WithNoProducts_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetAllProductsAsync("test-correlation-id");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
