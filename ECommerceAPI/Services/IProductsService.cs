using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services;

public interface IProductsService
{
    Task<ProductResponse?> GetProductAsync(int id, string correlationId);
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, string correlationId);
    Task<IEnumerable<ProductResponse>> GetAllProductsAsync(string correlationId);
}
