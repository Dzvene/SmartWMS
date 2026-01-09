using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Inventory.DTOs;

namespace SmartWMS.API.Modules.Inventory.Services;

public interface IStockService
{
    // Stock level queries
    Task<ApiResponse<PaginatedResult<StockLevelDto>>> GetStockLevelsAsync(
        Guid tenantId,
        StockLevelFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<StockLevelDto>> GetStockLevelByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<ProductStockSummaryDto>> GetProductStockSummaryAsync(Guid tenantId, Guid productId);
    Task<ApiResponse<IEnumerable<ProductStockSummaryDto>>> GetLowStockProductsAsync(Guid tenantId, Guid? warehouseId = null);

    // Stock movement queries
    Task<ApiResponse<PaginatedResult<StockMovementDto>>> GetStockMovementsAsync(
        Guid tenantId,
        StockMovementFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<IEnumerable<StockMovementDto>>> GetProductMovementHistoryAsync(
        Guid tenantId, Guid productId, int limit = 50);

    // Stock operations
    Task<ApiResponse<StockMovementDto>> ReceiveStockAsync(Guid tenantId, ReceiveStockRequest request);
    Task<ApiResponse<StockMovementDto>> IssueStockAsync(Guid tenantId, IssueStockRequest request);
    Task<ApiResponse<StockMovementDto>> TransferStockAsync(Guid tenantId, TransferStockRequest request);
    Task<ApiResponse<StockMovementDto>> AdjustStockAsync(Guid tenantId, AdjustStockRequest request);

    // Reservation operations
    Task<ApiResponse<StockLevelDto>> ReserveStockAsync(Guid tenantId, ReserveStockRequest request);
    Task<ApiResponse<StockLevelDto>> ReleaseReservationAsync(Guid tenantId, ReleaseReservationRequest request);

    // Availability check
    Task<ApiResponse<decimal>> GetAvailableQuantityAsync(
        Guid tenantId, Guid productId, Guid? locationId = null, string? batchNumber = null);
}
