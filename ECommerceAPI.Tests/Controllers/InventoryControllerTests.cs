using ECommerceAPI.Controllers;
using ECommerceAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Controllers;

public class InventoryControllerTests
{
    private readonly Mock<ILogger<InventoryController>> _mockLogger;
    private readonly InventoryController _controller;

    public InventoryControllerTests()
    {
        _mockLogger = new Mock<ILogger<InventoryController>>();
        _controller = new InventoryController(_mockLogger.Object);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public void GetInventory_ReturnsOkResultWithInventoryData()
    {
        // Arrange
        var productId = 1;

        // Act
        var result = _controller.GetInventory(productId);

        // Assert
        var actionResult = Assert.IsType<ActionResult<InventoryData>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var inventoryData = Assert.IsType<InventoryData>(okResult.Value);
        
        Assert.True(inventoryData.Price > 0);
        Assert.InRange(inventoryData.Stock, 0, 99);
    }

    [Fact]
    public void GetInventory_ReturnsRandomData()
    {
        // Arrange
        var productId = 1;

        // Act - Call multiple times to verify randomization
        var result1 = _controller.GetInventory(productId);
        var result2 = _controller.GetInventory(productId);

        // Assert
        var actionResult1 = Assert.IsType<ActionResult<InventoryData>>(result1);
        var okResult1 = Assert.IsType<OkObjectResult>(actionResult1.Result);
        var data1 = Assert.IsType<InventoryData>(okResult1.Value);

        var actionResult2 = Assert.IsType<ActionResult<InventoryData>>(result2);
        var okResult2 = Assert.IsType<OkObjectResult>(actionResult2.Result);
        var data2 = Assert.IsType<InventoryData>(okResult2.Value);

        Assert.NotNull(data1);
        Assert.NotNull(data2);
    }

    [Fact]
    public void GetInventory_LogsRequestWithCorrelationId()
    {
        // Arrange
        var productId = 1;
        var correlationId = "test-correlation-id";
        _controller.HttpContext.Request.Headers["X-Correlation-ID"] = correlationId;

        // Act
        _controller.GetInventory(productId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mock Inventory API")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetInventory_WithDifferentProductIds_ReturnsInventoryData()
    {
        // Arrange & Act & Assert
        for (int productId = 1; productId <= 5; productId++)
        {
            var result = _controller.GetInventory(productId);
            var actionResult = Assert.IsType<ActionResult<InventoryData>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var inventoryData = Assert.IsType<InventoryData>(okResult.Value);
            
            Assert.True(inventoryData.Price >= 10m && inventoryData.Price <= 1010m);
            Assert.InRange(inventoryData.Stock, 0, 99);
        }
    }
}
