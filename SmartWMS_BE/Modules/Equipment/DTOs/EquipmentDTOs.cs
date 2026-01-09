using System.Text.Json;
using SmartWMS.API.Modules.Equipment.Models;

namespace SmartWMS.API.Modules.Equipment.DTOs;

/// <summary>
/// Equipment response DTO
/// </summary>
public class EquipmentDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public EquipmentType Type { get; set; }
    public string TypeName => Type.ToString();

    public EquipmentStatus Status { get; set; }
    public string StatusName => Status.ToString();

    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public Guid? ZoneId { get; set; }
    public string? ZoneName { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceNotes { get; set; }

    // Type-specific specifications as JSON object
    public JsonDocument? Specifications { get; set; }

    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Create equipment request
/// </summary>
public class CreateEquipmentRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    public EquipmentType Type { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceNotes { get; set; }

    // Type-specific specifications as JSON string
    public string? Specifications { get; set; }

    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Update equipment request
/// </summary>
public class UpdateEquipmentRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }

    public EquipmentType? Type { get; set; }
    public EquipmentStatus? Status { get; set; }

    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public string? MaintenanceNotes { get; set; }

    public string? Specifications { get; set; }

    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpiryDate { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// Assign equipment to user request
/// </summary>
public class AssignEquipmentRequest
{
    public Guid? UserId { get; set; }
}

/// <summary>
/// Update equipment status request
/// </summary>
public class UpdateEquipmentStatusRequest
{
    public EquipmentStatus Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Equipment list filters
/// </summary>
public class EquipmentFilters
{
    public string? Search { get; set; }
    public EquipmentType? Type { get; set; }
    public EquipmentStatus? Status { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? ZoneId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public bool? IsAssigned { get; set; }
    public bool? IsActive { get; set; }
}
