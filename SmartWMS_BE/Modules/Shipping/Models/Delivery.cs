using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Shipping.Models;

/// <summary>
/// Delivery Route - groups shipments for delivery planning
/// </summary>
public class DeliveryRoute : TenantEntity
{
    public required string RouteNumber { get; set; }
    public string? RouteName { get; set; }

    // Warehouse
    public Guid WarehouseId { get; set; }

    // Carrier
    public Guid? CarrierId { get; set; }
    public Guid? CarrierServiceId { get; set; }

    // Driver/Vehicle
    public string? DriverName { get; set; }
    public string? VehicleNumber { get; set; }
    public string? VehicleType { get; set; }

    // Planning
    public DateTime PlannedDate { get; set; }
    public DateTime? PlannedDepartureTime { get; set; }
    public DateTime? PlannedReturnTime { get; set; }
    public int? EstimatedStops { get; set; }
    public decimal? EstimatedDistance { get; set; } // in km
    public int? EstimatedDuration { get; set; } // in minutes

    // Actual times
    public DateTime? ActualDepartureTime { get; set; }
    public DateTime? ActualReturnTime { get; set; }

    // Status
    public RouteStatus Status { get; set; } = RouteStatus.Draft;

    // Capacity
    public decimal? MaxWeight { get; set; }
    public decimal? MaxVolume { get; set; }
    public int? MaxStops { get; set; }
    public decimal CurrentWeight { get; set; }
    public decimal CurrentVolume { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation
    public virtual Warehouse.Models.Warehouse Warehouse { get; set; } = null!;
    public virtual Carriers.Models.Carrier? Carrier { get; set; }
    public virtual Carriers.Models.CarrierService? CarrierService { get; set; }
    public virtual List<DeliveryStop> Stops { get; set; } = new();
}

/// <summary>
/// Delivery Stop - individual stop on a delivery route
/// </summary>
public class DeliveryStop : TenantEntity
{
    public Guid RouteId { get; set; }
    public int StopSequence { get; set; }

    // Location
    public string? CustomerName { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    // Contact
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? DeliveryInstructions { get; set; }

    // Time window
    public DateTime? TimeWindowStart { get; set; }
    public DateTime? TimeWindowEnd { get; set; }
    public int? EstimatedServiceTime { get; set; } // minutes at stop

    // Actual
    public DateTime? ArrivalTime { get; set; }
    public DateTime? DepartureTime { get; set; }

    // Status
    public StopStatus Status { get; set; } = StopStatus.Pending;

    // Proof of delivery
    public string? SignedBy { get; set; }
    public DateTime? SignedAt { get; set; }
    public string? ProofOfDeliveryUrl { get; set; }

    // Issues
    public DeliveryIssue? IssueType { get; set; }
    public string? IssueNotes { get; set; }

    // Reference to shipments at this stop
    public Guid? ShipmentId { get; set; }
    public Guid? CustomerId { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Navigation
    public virtual DeliveryRoute Route { get; set; } = null!;
    public virtual Fulfillment.Models.Shipment? Shipment { get; set; }
    public virtual Orders.Models.Customer? Customer { get; set; }
}

/// <summary>
/// Delivery Tracking Event - real-time tracking updates
/// </summary>
public class DeliveryTrackingEvent : TenantEntity
{
    public Guid RouteId { get; set; }
    public Guid? StopId { get; set; }
    public Guid? ShipmentId { get; set; }

    // Event
    public TrackingEventType EventType { get; set; }
    public DateTime EventTime { get; set; }
    public string? EventDescription { get; set; }

    // Location
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationDescription { get; set; }

    // Additional info
    public string? PerformedBy { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public virtual DeliveryRoute Route { get; set; } = null!;
    public virtual DeliveryStop? Stop { get; set; }
}

#region Enums

public enum RouteStatus
{
    Draft,
    Planned,
    Released,
    Loading,
    InTransit,
    Delivering,
    Returning,
    Complete,
    Cancelled
}

public enum StopStatus
{
    Pending,
    EnRoute,
    Arrived,
    Delivering,
    Complete,
    PartialDelivery,
    Failed,
    Skipped,
    Rescheduled
}

public enum DeliveryIssue
{
    None,
    CustomerNotAvailable,
    WrongAddress,
    RefusedDelivery,
    DamagedGoods,
    MissingItems,
    AccessRestricted,
    Weather,
    VehicleBreakdown,
    Other
}

public enum TrackingEventType
{
    Created,
    Released,
    LoadingStarted,
    LoadingComplete,
    Departed,
    LocationUpdate,
    ArrivedAtStop,
    DeliveryStarted,
    DeliveryComplete,
    PartialDelivery,
    DeliveryFailed,
    DepartedStop,
    Returning,
    ReturnedToWarehouse,
    Cancelled
}

#endregion
