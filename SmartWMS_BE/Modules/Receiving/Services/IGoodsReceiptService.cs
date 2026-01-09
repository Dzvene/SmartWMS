using SmartWMS.API.Common.Models;
using SmartWMS.API.Modules.Receiving.DTOs;

namespace SmartWMS.API.Modules.Receiving.Services;

public interface IGoodsReceiptService
{
    Task<ApiResponse<PaginatedResult<GoodsReceiptDto>>> GetReceiptsAsync(
        Guid tenantId,
        GoodsReceiptFilters? filters = null,
        int page = 1,
        int pageSize = 25);

    Task<ApiResponse<GoodsReceiptDto>> GetReceiptByIdAsync(Guid tenantId, Guid id, bool includeLines = true);
    Task<ApiResponse<GoodsReceiptDto>> GetReceiptByNumberAsync(Guid tenantId, string receiptNumber, bool includeLines = true);
    Task<ApiResponse<GoodsReceiptDto>> CreateReceiptAsync(Guid tenantId, CreateGoodsReceiptRequest request);
    Task<ApiResponse<GoodsReceiptDto>> UpdateReceiptAsync(Guid tenantId, Guid id, UpdateGoodsReceiptRequest request);
    Task<ApiResponse> DeleteReceiptAsync(Guid tenantId, Guid id);

    // Line operations
    Task<ApiResponse<GoodsReceiptDto>> AddLineAsync(Guid tenantId, Guid receiptId, AddGoodsReceiptLineRequest request);
    Task<ApiResponse> RemoveLineAsync(Guid tenantId, Guid receiptId, Guid lineId);

    // Receiving workflow
    Task<ApiResponse<GoodsReceiptDto>> StartReceivingAsync(Guid tenantId, Guid id);
    Task<ApiResponse<GoodsReceiptLineDto>> ReceiveLineAsync(Guid tenantId, Guid receiptId, Guid lineId, ReceiveLineRequest request);
    Task<ApiResponse<GoodsReceiptDto>> CompleteReceiptAsync(Guid tenantId, Guid id);
    Task<ApiResponse<GoodsReceiptDto>> CancelReceiptAsync(Guid tenantId, Guid id, string? reason = null);

    // Create from PO
    Task<ApiResponse<GoodsReceiptDto>> CreateFromPurchaseOrderAsync(Guid tenantId, Guid purchaseOrderId, Guid warehouseId, Guid? receivingLocationId = null);
}
