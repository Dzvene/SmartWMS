using SmartWMS.API.Modules.Transfers.Models;

namespace SmartWMS.API.Modules.Transfers.DTOs;

#region Stock Transfer DTOs

public record StockTransferDto(
    Guid Id,
    string TransferNumber,
    TransferType TransferType,
    Guid FromWarehouseId,
    string? FromWarehouseName,
    Guid? FromZoneId,
    string? FromZoneName,
    Guid ToWarehouseId,
    string? ToWarehouseName,
    Guid? ToZoneId,
    string? ToZoneName,
    TransferStatus Status,
    TransferPriority Priority,
    DateTime? ScheduledDate,
    DateTime? RequiredByDate,
    Guid? ReasonCodeId,
    string? ReasonCodeName,
    string? ReasonNotes,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    string? SourceDocumentNumber,
    Guid CreatedByUserId,
    string? CreatedByUserName,
    Guid? AssignedToUserId,
    string? AssignedToUserName,
    Guid? PickedByUserId,
    DateTime? PickedAt,
    Guid? ReceivedByUserId,
    DateTime? ReceivedAt,
    string? Notes,
    int TotalLines,
    decimal TotalQuantity,
    int PickedLines,
    int ReceivedLines,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<StockTransferLineDto>? Lines
);

public record StockTransferLineDto(
    Guid Id,
    Guid TransferId,
    int LineNumber,
    Guid ProductId,
    string Sku,
    string? ProductName,
    Guid FromLocationId,
    string? FromLocationCode,
    Guid ToLocationId,
    string? ToLocationCode,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityRequested,
    decimal QuantityPicked,
    decimal QuantityReceived,
    decimal QuantityVariance,
    TransferLineStatus Status,
    DateTime? PickedAt,
    DateTime? ReceivedAt,
    string? Notes
);

public record StockTransferSummaryDto(
    Guid Id,
    string TransferNumber,
    TransferType TransferType,
    string? FromWarehouseName,
    string? ToWarehouseName,
    TransferStatus Status,
    TransferPriority Priority,
    DateTime? ScheduledDate,
    int TotalLines,
    decimal TotalQuantity,
    string? AssignedToUserName,
    DateTime CreatedAt
);

#endregion

#region Create/Update Requests

public record CreateStockTransferRequest(
    TransferType TransferType,
    Guid FromWarehouseId,
    Guid? FromZoneId,
    Guid ToWarehouseId,
    Guid? ToZoneId,
    TransferPriority Priority,
    DateTime? ScheduledDate,
    DateTime? RequiredByDate,
    Guid? ReasonCodeId,
    string? ReasonNotes,
    string? SourceDocumentType,
    Guid? SourceDocumentId,
    string? SourceDocumentNumber,
    string? Notes,
    List<CreateTransferLineRequest>? Lines
);

public record CreateTransferLineRequest(
    Guid ProductId,
    string Sku,
    Guid FromLocationId,
    Guid ToLocationId,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityRequested,
    string? Notes
);

public record UpdateStockTransferRequest(
    TransferPriority? Priority,
    DateTime? ScheduledDate,
    DateTime? RequiredByDate,
    Guid? ReasonCodeId,
    string? ReasonNotes,
    string? Notes
);

public record AddTransferLineRequest(
    Guid ProductId,
    string Sku,
    Guid FromLocationId,
    Guid ToLocationId,
    string? BatchNumber,
    string? SerialNumber,
    decimal QuantityRequested,
    string? Notes
);

public record UpdateTransferLineRequest(
    decimal QuantityRequested,
    string? Notes
);

#endregion

#region Action Requests

public record AssignTransferRequest(
    Guid AssignedToUserId
);

public record PickLineRequest(
    decimal QuantityPicked,
    string? Notes
);

public record ReceiveLineRequest(
    decimal QuantityReceived,
    string? Notes
);

public record CompletePickingRequest(
    string? Notes
);

public record CompleteReceivingRequest(
    string? Notes
);

#endregion

#region Query/Filter

public record TransferFilterRequest(
    Guid? FromWarehouseId = null,
    Guid? ToWarehouseId = null,
    TransferType? TransferType = null,
    TransferStatus? Status = null,
    TransferPriority? Priority = null,
    Guid? AssignedToUserId = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null,
    string? SearchTerm = null,
    int Page = 1,
    int PageSize = 20
);

#endregion

#region Replenishment

public record ReplenishmentRequest(
    Guid WarehouseId,
    Guid ProductId,
    Guid FromLocationId,
    Guid ToLocationId,
    decimal Quantity,
    TransferPriority Priority = TransferPriority.Normal,
    string? Notes = null
);

#endregion
