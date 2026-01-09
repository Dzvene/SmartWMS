using SmartWMS.API.Modules.Carriers.Models;

namespace SmartWMS.API.Modules.Carriers.DTOs;

#region Carrier DTOs

public class CarrierDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AccountNumber { get; set; }
    public CarrierIntegrationType IntegrationType { get; set; }
    public bool IsActive { get; set; }
    public string? DefaultServiceCode { get; set; }
    public string? Notes { get; set; }
    public List<CarrierServiceDto> Services { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CarrierListDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public CarrierIntegrationType IntegrationType { get; set; }
    public bool IsActive { get; set; }
    public int ServiceCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Carrier Service DTOs

public class CarrierServiceDto
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? MinTransitDays { get; set; }
    public int? MaxTransitDays { get; set; }
    public ServiceType ServiceType { get; set; }
    public bool HasTracking { get; set; }
    public string? TrackingUrlTemplate { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? MaxLengthCm { get; set; }
    public decimal? MaxWidthCm { get; set; }
    public decimal? MaxHeightCm { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

#endregion

#region Request DTOs

public class CreateCarrierRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AccountNumber { get; set; }
    public CarrierIntegrationType IntegrationType { get; set; } = CarrierIntegrationType.Manual;
    public string? DefaultServiceCode { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCarrierRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? AccountNumber { get; set; }
    public CarrierIntegrationType IntegrationType { get; set; }
    public bool IsActive { get; set; }
    public string? DefaultServiceCode { get; set; }
    public string? Notes { get; set; }
}

public class CreateCarrierServiceRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? MinTransitDays { get; set; }
    public int? MaxTransitDays { get; set; }
    public ServiceType ServiceType { get; set; } = ServiceType.Ground;
    public bool HasTracking { get; set; } = true;
    public string? TrackingUrlTemplate { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? MaxLengthCm { get; set; }
    public decimal? MaxWidthCm { get; set; }
    public decimal? MaxHeightCm { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCarrierServiceRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? MinTransitDays { get; set; }
    public int? MaxTransitDays { get; set; }
    public ServiceType ServiceType { get; set; }
    public bool HasTracking { get; set; }
    public string? TrackingUrlTemplate { get; set; }
    public decimal? MaxWeightKg { get; set; }
    public decimal? MaxLengthCm { get; set; }
    public decimal? MaxWidthCm { get; set; }
    public decimal? MaxHeightCm { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Filter DTOs

public class CarrierFilters
{
    public CarrierIntegrationType? IntegrationType { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
}

#endregion
