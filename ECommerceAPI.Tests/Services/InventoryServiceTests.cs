using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ECommerceAPI.DTOs;
using ECommerceAPI.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace ECommerceAPI.Tests.Services;

public class InventoryServiceTests
{
    private readonly Mock<ILogger<InventoryService>> _mockLogger;
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly InventoryService _service;

    public InventoryServiceTests()
    {
        _mockLogger = new Mock<ILogger<InventoryService>>();
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("https://localhost:7000")
        };
        _service = new InventoryService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetInventoryDataAsync_WhenSuccessful_ReturnsInventoryData()
    {
        // Arrange
        var productId = 1;
        var correlationId = "test-correlation-id";
        var expectedData = new InventoryData
        {
            Price = 99.99m,
            Stock = 50
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(expectedData)
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains($"/api/inventory/{productId}")),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetInventoryDataAsync(productId, correlationId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal(50, result.Stock);
    }

    [Fact]
    public async Task GetInventoryDataAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        var productId = 999;
        var correlationId = "test-correlation-id";

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.NotFound
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _service.GetInventoryDataAsync(productId, correlationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInventoryDataAsync_WhenHttpRequestException_ReturnsNull()
    {
        // Arrange
        var productId = 1;
        var correlationId = "test-correlation-id";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetInventoryDataAsync(productId, correlationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInventoryDataAsync_WhenGeneralException_ReturnsNull()
    {
        // Arrange
        var productId = 1;
        var correlationId = "test-correlation-id";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _service.GetInventoryDataAsync(productId, correlationId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetInventoryDataAsync_SetsCorrelationIdHeader()
    {
        // Arrange
        var productId = 1;
        var correlationId = "test-correlation-id-123";
        HttpRequestMessage? capturedRequest = null;

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new InventoryData { Price = 10m, Stock = 5 })
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(responseMessage);

        // Act
        await _service.GetInventoryDataAsync(productId, correlationId);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.True(capturedRequest.Headers.Contains("X-Correlation-ID"));
        Assert.Equal(correlationId, capturedRequest.Headers.GetValues("X-Correlation-ID").First());
    }
}
