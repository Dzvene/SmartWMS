using SmartWMS.API.Common.Enums;

namespace SmartWMS.API.Modules.Fulfillment.DTOs;

/// <summary>
/// Shipment DTO for API responses
/// </summary>
public class ShipmentDto
{
    public Guid Id { get; set; }
    public required string ShipmentNumber { get; set; }

    // Order
    public Guid OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? CustomerName { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }

    // Status
    public ShipmentStatus Status { get; set; }

    // Carrier
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? ServiceLevel { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }

    // Package
    public int PackageCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    // Ship to address
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Full address for display
    public string? ShipToFullAddress => string.Join(", ",
        new[] { ShipToAddressLine1, ShipToCity, ShipToRegion, ShipToPostalCode, ShipToCountryCode }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

    // Timestamps
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Cost
    public decimal? ShippingCost { get; set; }
    public string? CurrencyCode { get; set; }

    // Label
    public string? LabelUrl { get; set; }
    public bool HasLabel => !string.IsNullOrEmpty(LabelUrl) || LabelData != null;
    public byte[]? LabelData { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request to create a shipment
/// </summary>
public class CreateShipmentRequest
{
    public string? ShipmentNumber { get; set; } // Auto-generated if not provided
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }

    // Carrier
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? ServiceLevel { get; set; }

    // Package
    public int PackageCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    // Override ship to address (optional, defaults to order address)
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a shipment
/// </summary>
public class UpdateShipmentRequest
{
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? ServiceLevel { get; set; }

    public int? PackageCount { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to add tracking information
/// </summary>
public class AddTrackingRequest
{
    public required string TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
}

/// <summary>
/// Request to mark shipment as shipped
/// </summary>
public class ShipShipmentRequest
{
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public decimal? ShippingCost { get; set; }
    public string? CurrencyCode { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to add/update label
/// </summary>
public class UpdateLabelRequest
{
    public string? LabelUrl { get; set; }
    public byte[]? LabelData { get; set; }
}

/// <summary>
/// Filter options for shipment queries
/// </summary>
public class ShipmentFilters
{
    public string? Search { get; set; }
    public ShipmentStatus? Status { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string? CarrierCode { get; set; }
    public DateTime? ShippedFrom { get; set; }
    public DateTime? ShippedTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
