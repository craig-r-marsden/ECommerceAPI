using ECommerceAPI.Data;
using ECommerceAPI.DTOs;
using ECommerceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Services;

public class ProductsService : IProductsService
{
    private readonly ApplicationDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<ProductsService> _logger;

    public ProductsService(
        ApplicationDbContext context,
        IInventoryService inventoryService,
        ILogger<ProductsService> logger)
    {
        _context = context;
        _inventoryService = inventoryService;
        _logger = logger;
    }

    public async Task<ProductResponse?> GetProductAsync(int id, string correlationId)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return null;
        }

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

        return response;
    }

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, string correlationId)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

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

        return response;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllProductsAsync(string correlationId)
    {
        var products = await _context.Products.ToListAsync();

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

        return response;
    }
}
