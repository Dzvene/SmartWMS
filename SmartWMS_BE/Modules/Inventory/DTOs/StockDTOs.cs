using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Inventory.DTOs;

/// <summary>
/// Stock level DTO for API responses
/// </summary>
public class StockLevelDto
{
    public Guid Id { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }

    // Location
    public Guid LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? WarehouseName { get; set; }
    public string? ZoneName { get; set; }

    // Quantities
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }

    // Batch/Serial
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Audit
    public DateTime? LastMovementAt { get; set; }
    public DateTime? LastCountAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Aggregated stock summary by product
/// </summary>
public class ProductStockSummaryDto
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }

    public decimal TotalOnHand { get; set; }
    public decimal TotalReserved { get; set; }
    public decimal TotalAvailable { get; set; }

    public int LocationCount { get; set; }
    public List<StockLevelDto>? Locations { get; set; }
}

/// <summary>
/// Stock movement DTO for API responses
/// </summary>
public class StockMovementDto
{
    public Guid Id { get; set; }
    public string? MovementNumber { get; set; }
    public MovementType MovementType { get; set; }

    // Product
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }

    // Locations
    public Guid? FromLocationId { get; set; }
    public string? FromLocationCode { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? ToLocationCode { get; set; }

    // Quantity
    public decimal Quantity { get; set; }

    // Batch/Serial
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Reference
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }

    // Details
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
    public DateTime MovementDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to receive stock (from goods receipt)
/// </summary>
public class ReceiveStockRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Reference to source document
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to issue/pick stock (for sales orders)
/// </summary>
public class IssueStockRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }

    // Reference to source document
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to transfer stock between locations
/// </summary>
public class TransferStockRequest
{
    public Guid ProductId { get; set; }
    public Guid FromLocationId { get; set; }
    public Guid ToLocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to adjust stock (inventory count adjustment)
/// </summary>
public class AdjustStockRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal NewQuantity { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public string? ReasonCode { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to reserve stock for an order
/// </summary>
public class ReserveStockRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }

    // Reference to order
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Request to release reserved stock
/// </summary>
public class ReleaseReservationRequest
{
    public Guid ProductId { get; set; }
    public Guid LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? BatchNumber { get; set; }

    // Reference to order
    public Guid? ReferenceId { get; set; }
}

/// <summary>
/// Filter options for stock level queries
/// </summary>
public class StockLevelFilters
{
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public string? Sku { get; set; }
    public string? BatchNumber { get; set; }
    public bool? HasAvailableStock { get; set; }
    public bool? IsExpiringSoon { get; set; }
    public int? ExpiringWithinDays { get; set; }
}

/// <summary>
/// Filter options for stock movement queries
/// </summary>
public class StockMovementFilters
{
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public MovementType? MovementType { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
