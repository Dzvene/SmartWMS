using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Transfers.DTOs;
using SmartWMS.API.Modules.Transfers.Models;

namespace SmartWMS.API.Modules.Transfers.Services;

public interface ITransfersService
{
    // Stock Transfers CRUD
    Task<ApiResponse<PaginatedResult<StockTransferSummaryDto>>> GetTransfersAsync(
        Guid tenantId, TransferFilterRequest filter);
    Task<ApiResponse<StockTransferDto>> GetTransferByIdAsync(Guid tenantId, Guid id);
    Task<ApiResponse<StockTransferDto>> GetTransferByNumberAsync(Guid tenantId, string transferNumber);
    Task<ApiResponse<StockTransferDto>> CreateTransferAsync(
        Guid tenantId, Guid userId, CreateStockTransferRequest request);
    Task<ApiResponse<StockTransferDto>> UpdateTransferAsync(
        Guid tenantId, Guid id, UpdateStockTransferRequest request);
    Task<ApiResponse<bool>> DeleteTransferAsync(Guid tenantId, Guid id);

    // Transfer Lines
    Task<ApiResponse<StockTransferLineDto>> AddLineAsync(
        Guid tenantId, Guid transferId, AddTransferLineRequest request);
    Task<ApiResponse<StockTransferLineDto>> UpdateLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, UpdateTransferLineRequest request);
    Task<ApiResponse<bool>> RemoveLineAsync(Guid tenantId, Guid transferId, Guid lineId);

    // Workflow Actions
    Task<ApiResponse<StockTransferDto>> ReleaseAsync(Guid tenantId, Guid id, Guid userId);
    Task<ApiResponse<StockTransferDto>> AssignAsync(
        Guid tenantId, Guid id, AssignTransferRequest request);
    Task<ApiResponse<StockTransferDto>> StartPickingAsync(Guid tenantId, Guid id, Guid userId);
    Task<ApiResponse<StockTransferLineDto>> PickLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, Guid userId, PickLineRequest request);
    Task<ApiResponse<StockTransferDto>> CompletePickingAsync(
        Guid tenantId, Guid id, Guid userId, CompletePickingRequest request);
    Task<ApiResponse<StockTransferDto>> MarkInTransitAsync(Guid tenantId, Guid id, Guid userId);
    Task<ApiResponse<StockTransferLineDto>> ReceiveLineAsync(
        Guid tenantId, Guid transferId, Guid lineId, Guid userId, ReceiveLineRequest request);
    Task<ApiResponse<StockTransferDto>> CompleteReceivingAsync(
        Guid tenantId, Guid id, Guid userId, CompleteReceivingRequest request);
    Task<ApiResponse<StockTransferDto>> CancelAsync(Guid tenantId, Guid id, Guid userId);

    // Replenishment shortcut
    Task<ApiResponse<StockTransferDto>> CreateReplenishmentAsync(
        Guid tenantId, Guid userId, ReplenishmentRequest request);

    // Quick transfer (for simple internal moves)
    Task<ApiResponse<StockTransferDto>> QuickTransferAsync(
        Guid tenantId, Guid userId, CreateStockTransferRequest request);
}
