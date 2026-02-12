using ECommerceAPI.DTOs;

namespace ECommerceAPI.Services;

public interface IInventoryService
{
    Task<InventoryData?> GetInventoryDataAsync(int productId, string correlationId);
}
