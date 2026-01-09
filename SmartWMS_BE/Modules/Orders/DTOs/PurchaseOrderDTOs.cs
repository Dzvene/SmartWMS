using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Orders.DTOs;

/// <summary>
/// Purchase order DTO for API responses
/// </summary>
public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public string? ExternalReference { get; set; }

    // Supplier
    public Guid SupplierId { get; set; }
    public string? SupplierCode { get; set; }
    public string? SupplierName { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    // Status
    public PurchaseOrderStatus Status { get; set; }

    // Dates
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }

    // Receiving dock
    public Guid? ReceivingDockId { get; set; }
    public string? ReceivingDockCode { get; set; }

    // Totals
    public int TotalLines { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Lines (optional, loaded on detail view)
    public List<PurchaseOrderLineDto>? Lines { get; set; }
}

/// <summary>
/// Purchase order line DTO
/// </summary>
public class PurchaseOrderLineDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public int LineNumber { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public required string Sku { get; set; }
    public string? ProductName { get; set; }

    // Quantities
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal QuantityCancelled { get; set; }
    public decimal QuantityOutstanding { get; set; }

    // Expected batch/serial
    public string? ExpectedBatchNumber { get; set; }
    public DateTime? ExpectedExpiryDate { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to create a new purchase order
/// </summary>
public class CreatePurchaseOrderRequest
{
    public string? OrderNumber { get; set; } // Auto-generated if not provided
    public string? ExternalReference { get; set; }

    public Guid SupplierId { get; set; }
    public Guid WarehouseId { get; set; }

    public DateTime? ExpectedDate { get; set; }
    public Guid? ReceivingDockId { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Lines
    public List<CreatePurchaseOrderLineRequest>? Lines { get; set; }
}

/// <summary>
/// Request to create a purchase order line
/// </summary>
public class CreatePurchaseOrderLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public string? ExpectedBatchNumber { get; set; }
    public DateTime? ExpectedExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a purchase order
/// </summary>
public class UpdatePurchaseOrderRequest
{
    public string? ExternalReference { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }

    public DateTime? ExpectedDate { get; set; }
    public Guid? ReceivingDockId { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}

/// <summary>
/// Request to update purchase order status
/// </summary>
public class UpdatePurchaseOrderStatusRequest
{
    public PurchaseOrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add a line to an existing order
/// </summary>
public class AddPurchaseOrderLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public string? ExpectedBatchNumber { get; set; }
    public DateTime? ExpectedExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a purchase order line
/// </summary>
public class UpdatePurchaseOrderLineRequest
{
    public decimal? QuantityOrdered { get; set; }
    public string? ExpectedBatchNumber { get; set; }
    public DateTime? ExpectedExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to receive items on a purchase order line
/// </summary>
public class ReceivePurchaseOrderLineRequest
{
    public decimal QuantityReceived { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? LocationId { get; set; } // Where to put the received items
    public string? Notes { get; set; }
}

/// <summary>
/// Filter options for purchase order queries
/// </summary>
public class PurchaseOrderFilters
{
    public string? Search { get; set; }
    public PurchaseOrderStatus? Status { get; set; }
    public Guid? SupplierId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo { get; set; }
    public DateTime? ExpectedDateFrom { get; set; }
    public DateTime? ExpectedDateTo { get; set; }
}
