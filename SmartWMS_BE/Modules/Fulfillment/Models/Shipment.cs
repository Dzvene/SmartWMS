using SmartWMS.API.Common.Enums;
using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Fulfillment.Models;

/// <summary>
/// Shipment - represents a physical shipment to customer.
/// (Matches SmartWMS_UI: orders/fulfillment.types.ts - Shipment type)
/// </summary>
public class Shipment : TenantEntity
{
    public required string ShipmentNumber { get; set; }

    // Order reference
    public Guid OrderId { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }

    // Status
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

    // Carrier information
    public string? CarrierCode { get; set; }
    public string? CarrierName { get; set; }
    public string? ServiceLevel { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }

    // Package details
    public int PackageCount { get; set; } = 1;
    public decimal? TotalWeightKg { get; set; }

    // Dimensions of largest package
    public int? WidthMm { get; set; }
    public int? HeightMm { get; set; }
    public int? DepthMm { get; set; }

    // Shipping address (copied from order at shipment creation)
    public string? ShipToName { get; set; }
    public string? ShipToAddressLine1 { get; set; }
    public string? ShipToAddressLine2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToRegion { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountryCode { get; set; }

    // Timestamps
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    // Cost
    public decimal? ShippingCost { get; set; }
    public string? CurrencyCode { get; set; }

    // Label
    public string? LabelUrl { get; set; }
    public byte[]? LabelData { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation properties
    public virtual Orders.Models.SalesOrder Order { get; set; } = null!;
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
}
