using SmartWMS.API.Modules.Receiving.Models;

namespace SmartWMS.API.Modules.Receiving.DTOs;

/// <summary>
/// Goods receipt DTO for API responses
/// </summary>
public class GoodsReceiptDto
{
    public Guid Id { get; set; }
    public required string ReceiptNumber { get; set; }

    // Source PO
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }

    // Supplier
    public Guid? SupplierId { get; set; }
    public string? SupplierName { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    // Receiving location
    public Guid? ReceivingLocationId { get; set; }
    public string? ReceivingLocationCode { get; set; }

    // Status
    public GoodsReceiptStatus Status { get; set; }

    // Dates
    public DateTime ReceiptDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Carrier
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? DeliveryNote { get; set; }

    // Totals
    public int TotalLines { get; set; }
    public decimal TotalQuantityExpected { get; set; }
    public decimal TotalQuantityReceived { get; set; }

    // Progress
    public decimal ProgressPercent => TotalQuantityExpected > 0
        ? Math.Round(TotalQuantityReceived / TotalQuantityExpected * 100, 1)
        : 0;

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Lines (optional)
    public List<GoodsReceiptLineDto>? Lines { get; set; }
}

/// <summary>
/// Goods receipt line DTO
/// </summary>
public class GoodsReceiptLineDto
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }

    // Quantities
    public decimal QuantityExpected { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityRejected { get; set; }
    public decimal QuantityRemaining => QuantityExpected - QuantityReceived - QuantityRejected;

    // Lot/Batch
    public string? BatchNumber { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }

    // Put-away
    public Guid? PutawayLocationId { get; set; }
    public string? PutawayLocationCode { get; set; }

    // Status
    public GoodsReceiptLineStatus Status { get; set; }
    public string? QualityStatus { get; set; }
    public string? RejectionReason { get; set; }

    // Source PO line
    public Guid? PurchaseOrderLineId { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to create a goods receipt
/// </summary>
public class CreateGoodsReceiptRequest
{
    public string? ReceiptNumber { get; set; } // Auto-generated if not provided
    public Guid? PurchaseOrderId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? ReceivingLocationId { get; set; }
    public DateTime? ReceiptDate { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? DeliveryNote { get; set; }
    public string? Notes { get; set; }

    // Lines to create
    public List<CreateGoodsReceiptLineRequest>? Lines { get; set; }
}

/// <summary>
/// Request to create a goods receipt line
/// </summary>
public class CreateGoodsReceiptLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityExpected { get; set; }
    public string? BatchNumber { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid? PurchaseOrderLineId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a goods receipt
/// </summary>
public class UpdateGoodsReceiptRequest
{
    public Guid? ReceivingLocationId { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNumber { get; set; }
    public string? DeliveryNote { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to receive a line (record actual receipt)
/// </summary>
public class ReceiveLineRequest
{
    public decimal QuantityReceived { get; set; }
    public decimal QuantityRejected { get; set; }
    public string? BatchNumber { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public Guid? PutawayLocationId { get; set; }
    public string? QualityStatus { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add a line to existing receipt
/// </summary>
public class AddGoodsReceiptLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityExpected { get; set; }
    public string? BatchNumber { get; set; }
    public string? LotNumber { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public Guid? PurchaseOrderLineId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Filter options for goods receipt queries
/// </summary>
public class GoodsReceiptFilters
{
    public string? Search { get; set; }
    public GoodsReceiptStatus? Status { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public DateTime? ReceiptDateFrom { get; set; }
    public DateTime? ReceiptDateTo { get; set; }
}
