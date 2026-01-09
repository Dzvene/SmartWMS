namespace SmartWMS.API.Common.Enums;

/// <summary>
/// Fulfillment batch status workflow
/// (From SmartWMS_UI: orders/fulfillment.types.ts)
/// </summary>
public enum FulfillmentStatus
{
    Created,
    Released,
    InProgress,
    PartiallyComplete,
    Complete,
    Cancelled
}

/// <summary>
/// Types of fulfillment batches
/// (From SmartWMS_UI: orders/fulfillment.types.ts)
/// </summary>
public enum BatchType
{
    Single,
    Multi,
    Zone,
    Wave
}

/// <summary>
/// Pick task status workflow
/// (From SmartWMS_UI: orders/fulfillment.types.ts)
/// </summary>
public enum PickTaskStatus
{
    Pending,
    Assigned,
    InProgress,
    Complete,
    ShortPicked,
    Cancelled
}

/// <summary>
/// Shipment status workflow
/// </summary>
public enum ShipmentStatus
{
    Created,
    Packed,
    LabelPrinted,
    PickedUp,
    InTransit,
    Delivered,
    Cancelled
}
