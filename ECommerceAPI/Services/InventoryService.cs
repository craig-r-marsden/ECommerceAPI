using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services;

public class InventoryService : IInventoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(HttpClient httpClient, ILogger<InventoryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<InventoryData?> GetInventoryDataAsync(int productId, string correlationId)
    {
        try
        {
            _logger.LogInformation(
                "Fetching inventory data for Product {ProductId} with Correlation-ID: {CorrelationId}",
                productId,
                correlationId
            );

            _httpClient.DefaultRequestHeaders.Remove("X-Correlation-ID");
            _httpClient.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            var response = await _httpClient.GetAsync($"/api/inventory/{productId}");

            if (response.IsSuccessStatusCode)
            {
                var inventoryData = await response.Content.ReadFromJsonAsync<InventoryData>();
                _logger.LogInformation(
                    "Successfully retrieved inventory data for Product {ProductId} with Correlation-ID: {CorrelationId}",
                    productId,
                    correlationId
                );
                return inventoryData;
            }

            _logger.LogWarning(
                "Failed to fetch inventory data for Product {ProductId}. Status: {StatusCode}, Correlation-ID: {CorrelationId}",
                productId,
                response.StatusCode,
                correlationId
            );
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP error fetching inventory data for Product {ProductId}, Correlation-ID: {CorrelationId}",
                productId,
                correlationId
            );
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error fetching inventory data for Product {ProductId}, Correlation-ID: {CorrelationId}",
                productId,
                correlationId
            );
            return null;
        }
    }
}
