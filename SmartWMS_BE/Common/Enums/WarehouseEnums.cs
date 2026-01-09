namespace SmartWMS.API.Common.Enums;

/// <summary>
/// Types of storage locations in the warehouse
/// (From SmartWMS_UI: warehouse/location.types.ts)
/// </summary>
public enum LocationType
{
    Bulk,
    Pick,
    Staging,
    Receiving,
    Shipping,
    Returns,
    Quarantine,
    Reserve
}

/// <summary>
/// Types of warehouse zones
/// (From SmartWMS_UI: warehouse/location.types.ts)
/// </summary>
public enum ZoneType
{
    Storage,
    Picking,
    Packing,
    Staging,
    Shipping,
    Receiving,
    Returns
}
