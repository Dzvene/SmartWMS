using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Equipment.Models;

/// <summary>
/// Equipment types supported in the warehouse
/// </summary>
public enum EquipmentType
{
    Forklift,
    PalletJack,
    Scanner,
    Printer,
    Trolley
}

/// <summary>
/// Equipment operational status
/// </summary>
public enum EquipmentStatus
{
    Available,
    InUse,
    Maintenance,
    OutOfService
}

/// <summary>
/// Equipment - warehouse equipment and devices.
/// Supports different equipment types with type-specific specifications stored as JSON.
/// </summary>
public class Equipment : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public EquipmentType Type { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

    // Location assignment
    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }

    // User assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    // Maintenance tracking
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceNotes { get; set; }

    // Type-specific specifications stored as JSON
    // Examples:
    // Forklift: {"loadCapacityKg": 2000, "liftHeightMm": 5000, "fuelType": "Electric"}
    // Scanner: {"model": "Zebra MC9300", "connectionType": "WiFi", "batteryCapacity": "5200mAh"}
    // Printer: {"model": "Zebra ZT411", "printType": "Thermal", "dpi": 300, "labelWidthMm": 102}
    public string? Specifications { get; set; }

    // Asset tracking
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Warehouse.Models.Warehouse? Warehouse { get; set; }
    public virtual Warehouse.Models.Zone? Zone { get; set; }
}
