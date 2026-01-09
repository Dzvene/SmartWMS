namespace SmartWMS.API.Common.Enums;

/// <summary>
/// Sales order status workflow
/// (From SmartWMS_UI: orders/sales-order.types.ts)
/// </summary>
public enum SalesOrderStatus
{
    Draft,
    Pending,
    Confirmed,
    Allocated,
    PartiallyPicked,
    Picked,
    Packed,
    Shipped,
    Delivered,
    Cancelled,
    OnHold
}

/// <summary>
/// Purchase order status workflow
/// (From SmartWMS_UI: orders/purchase-order.types.ts)
/// </summary>
public enum PurchaseOrderStatus
{
    Draft,
    Pending,
    Confirmed,
    PartiallyReceived,
    Received,
    Closed,
    Cancelled
}

/// <summary>
/// Order priority levels
/// </summary>
public enum OrderPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
