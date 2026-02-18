using ECommerceAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerceAPI.Tests.Middleware;

public class CorrelationIdMiddlewareTests
{
    private readonly Mock<ILogger<CorrelationIdMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly CorrelationIdMiddleware _middleware;

    public CorrelationIdMiddlewareTests()
    {
        _mockLogger = new Mock<ILogger<CorrelationIdMiddleware>>();
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new CorrelationIdMiddleware(_mockNext.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenCorrelationIdExists_UsesExistingId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var existingCorrelationId = "existing-correlation-id";
        context.Request.Headers["X-Correlation-ID"] = existingCorrelationId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.Equal(existingCorrelationId, context.Items["CorrelationId"]);
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-ID"));
        Assert.Equal(existingCorrelationId, context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_WhenNoCorrelationId_GeneratesNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.NotNull(context.Items["CorrelationId"]);
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.False(string.IsNullOrEmpty(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_AddsCorrelationIdToResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var correlationId = "test-correlation-id";
        context.Request.Headers["X-Correlation-ID"] = correlationId;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-ID"));
        Assert.Equal(correlationId, context.Response.Headers["X-Correlation-ID"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var nextCalled = false;
        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                 .Callback(() => nextCalled = true)
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        _mockNext.Verify(next => next(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsIncomingRequest()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/products";

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Incoming request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_LogsOutgoingResponse()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/products";
        context.Response.StatusCode = 200;

        _mockNext.Setup(next => next(It.IsAny<HttpContext>()))
                 .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Outgoing response")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenEmptyCorrelationId_GeneratesNewId()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-ID"] = string.Empty;

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var correlationId = context.Items["CorrelationId"]?.ToString();
        Assert.False(string.IsNullOrEmpty(correlationId));
        Assert.True(Guid.TryParse(correlationId, out _));
    }
}
