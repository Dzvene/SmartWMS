using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Orders.DTOs;

/// <summary>
/// Sales order DTO for API responses
/// </summary>
public class SalesOrderDto
{
    public Guid Id { get; set; }
    public required string OrderNumber { get; set; }
    public string? ExternalReference { get; set; }

    // Customer
    public Guid CustomerId { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerName { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }

    // Status
    public SalesOrderStatus Status { get; set; }
    public OrderPriority Priority { get; set; }

    // Dates
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public DateTime? ShippedDate { get; set; }

    // Shipping address
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Shipping info
    public string? CarrierCode { get; set; }
    public string? ServiceLevel { get; set; }
    public string? ShippingInstructions { get; set; }

    // Totals
    public int TotalLines { get; set; }
    public decimal TotalQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal PickedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Lines (optional, loaded on detail view)
    public List<SalesOrderLineDto>? Lines { get; set; }
}

/// <summary>
/// Sales order line DTO
/// </summary>
public class SalesOrderLineDto
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
    public decimal QuantityAllocated { get; set; }
    public decimal QuantityPicked { get; set; }
    public decimal QuantityShipped { get; set; }
    public decimal QuantityCancelled { get; set; }
    public decimal QuantityOutstanding { get; set; }

    // Batch/Serial requirements
    public string? RequiredBatchNumber { get; set; }
    public DateTime? RequiredExpiryDate { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to create a new sales order
/// </summary>
public class CreateSalesOrderRequest
{
    public string? OrderNumber { get; set; } // Auto-generated if not provided
    public string? ExternalReference { get; set; }

    public Guid CustomerId { get; set; }
    public Guid WarehouseId { get; set; }

    public OrderPriority Priority { get; set; } = OrderPriority.Normal;
    public DateTime? RequiredDate { get; set; }

    // Shipping address (optional, uses customer default if not provided)
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Shipping info
    public string? CarrierCode { get; set; }
    public string? ServiceLevel { get; set; }
    public string? ShippingInstructions { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }

    // Lines
    public List<CreateSalesOrderLineRequest>? Lines { get; set; }
}

/// <summary>
/// Request to create a sales order line
/// </summary>
public class CreateSalesOrderLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public string? RequiredBatchNumber { get; set; }
    public DateTime? RequiredExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a sales order
/// </summary>
public class UpdateSalesOrderRequest
{
    public string? ExternalReference { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }

    public OrderPriority? Priority { get; set; }
    public DateTime? RequiredDate { get; set; }

    // Shipping address
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Shipping info
    public string? CarrierCode { get; set; }
    public string? ServiceLevel { get; set; }
    public string? ShippingInstructions { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
}

/// <summary>
/// Request to update sales order status
/// </summary>
public class UpdateSalesOrderStatusRequest
{
    public SalesOrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add a line to an existing order
/// </summary>
public class AddSalesOrderLineRequest
{
    public Guid ProductId { get; set; }
    public decimal QuantityOrdered { get; set; }
    public string? RequiredBatchNumber { get; set; }
    public DateTime? RequiredExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a sales order line
/// </summary>
public class UpdateSalesOrderLineRequest
{
    public decimal? QuantityOrdered { get; set; }
    public string? RequiredBatchNumber { get; set; }
    public DateTime? RequiredExpiryDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Filter options for sales order queries
/// </summary>
public class SalesOrderFilters
{
    public string? Search { get; set; }
    public SalesOrderStatus? Status { get; set; }
    public OrderPriority? Priority { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? WarehouseId { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo { get; set; }
    public DateTime? RequiredDateFrom { get; set; }
    public DateTime? RequiredDateTo { get; set; }
}
