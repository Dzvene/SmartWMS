namespace SmartWMS.API.Common.Enums;

/// <summary>
/// Types of stock movements
/// (From SmartWMS_UI: inventory/stock.types.ts)
/// </summary>
public enum MovementType
{
    Receipt,
    Issue,
    Transfer,
    Adjustment,
    Return,
    WriteOff,
    CycleCount
}
