using SmartWMS.API.Modules.Adjustments.Models;

namespace SmartWMS.API.Modules.Adjustments.DTOs;

#region Stock Adjustment DTOs

public record StockAdjustmentDto(
    Guid Id,
    string AdjustmentNumber,
    Guid WarehouseId,
    string? WarehouseName,
    AdjustmentStatus Status,
    AdjustmentType AdjustmentType,
    Guid? ReasonCodeId,
    string? ReasonCodeName,
    string? ReasonNotes,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    string? SourceDocumentNumber,
    Guid CreatedByUserId,
    string? CreatedByUserName,
    Guid? ApprovedByUserId,
    string? ApprovedByUserName,
    DateTime? ApprovedAt,
    Guid? PostedByUserId,
    string? PostedByUserName,
    DateTime? PostedAt,
    string? Notes,
    int TotalLines,
    decimal TotalQuantityChange,
    decimal? TotalValueChange,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<StockAdjustmentLineDto>? Lines
);

public record StockAdjustmentLineDto(
    Guid Id,
    Guid AdjustmentId,
    int LineNumber,
    Guid ProductId,
    string Sku,
    string? ProductName,
    Guid LocationId,
    string? LocationCode,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityBefore,
    decimal QuantityAdjustment,
    decimal QuantityAfter,
    decimal? UnitCost,
    decimal? ValueChange,
    Guid? ReasonCodeId,
    string? ReasonCodeName,
    string? ReasonNotes,
    bool IsProcessed,
    DateTime? ProcessedAt
);

public record StockAdjustmentSummaryDto(
    Guid Id,
    string AdjustmentNumber,
    string? WarehouseName,
    AdjustmentStatus Status,
    AdjustmentType AdjustmentType,
    string? ReasonCodeName,
    int TotalLines,
    decimal TotalQuantityChange,
    string? CreatedByUserName,
    DateTime CreatedAt
);

#endregion

#region Create/Update Requests

public record CreateStockAdjustmentRequest(
    Guid WarehouseId,
    AdjustmentType AdjustmentType,
    Guid? ReasonCodeId,
    string? ReasonNotes,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    string? SourceDocumentNumber,
    string? Notes,
    List<CreateAdjustmentLineRequest>? Lines
);

public record CreateAdjustmentLineRequest(
    Guid ProductId,
    string Sku,
    Guid LocationId,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityAdjustment,
    decimal? UnitCost,
    Guid? ReasonCodeId,
    string? ReasonNotes
);

public record UpdateStockAdjustmentRequest(
    Guid? ReasonCodeId,
    string? ReasonNotes,
    string? Notes
);

public record AddAdjustmentLineRequest(
    Guid ProductId,
    string Sku,
    Guid LocationId,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityAdjustment,
    decimal? UnitCost,
    Guid? ReasonCodeId,
    string? ReasonNotes
);

public record UpdateAdjustmentLineRequest(
    decimal QuantityAdjustment,
    decimal? UnitCost,
    Guid? ReasonCodeId,
    string? ReasonNotes
);

#endregion

#region Action Requests

public record SubmitForApprovalRequest(
    string? Notes
);

public record ApproveAdjustmentRequest(
    string? ApprovalNotes
);

public record RejectAdjustmentRequest(
    string RejectionReason
);

public record PostAdjustmentRequest(
    string? PostingNotes
);

#endregion

#region Query/Filter

public record AdjustmentFilterRequest(
    Guid? WarehouseId = null,
    AdjustmentStatus? Status = null,
    AdjustmentType? AdjustmentType = null,
    Guid? ReasonCodeId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    Guid? CreatedByUserId = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);

#endregion
