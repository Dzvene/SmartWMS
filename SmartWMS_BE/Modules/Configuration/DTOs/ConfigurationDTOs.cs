using SmartWMS.API.Modules.Configuration.Models;

namespace SmartWMS.API.Modules.Configuration.DTOs;

#region Barcode Prefix DTOs

public class BarcodePrefixDto
{
    public Guid Id { get; set; }
    public required string Prefix { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public BarcodePrefixType PrefixType { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateBarcodePrefixRequest
{
    public required string Prefix { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public BarcodePrefixType PrefixType { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBarcodePrefixRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Reason Code DTOs

public class ReasonCodeDto
{
    public Guid Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ReasonCodeType ReasonType { get; set; }
    public bool RequiresNotes { get; set; }
    public bool RequiresApproval { get; set; }
    public bool AffectsInventory { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateReasonCodeRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public ReasonCodeType ReasonType { get; set; }
    public bool RequiresNotes { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public bool AffectsInventory { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? Notes { get; set; }
}

public class UpdateReasonCodeRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool RequiresNotes { get; set; }
    public bool RequiresApproval { get; set; }
    public bool AffectsInventory { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Number Sequence DTOs

public class NumberSequenceDto
{
    public Guid Id { get; set; }
    public required string SequenceType { get; set; }
    public required string Prefix { get; set; }
    public int CurrentNumber { get; set; }
    public int IncrementBy { get; set; }
    public int MinDigits { get; set; }
    public string? Suffix { get; set; }
    public bool IncludeDate { get; set; }
    public string DateFormat { get; set; } = "yyyyMMdd";
    public DateTime? LastResetDate { get; set; }
    public ResetFrequency ResetFrequency { get; set; }
    public string NextNumber { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateNumberSequenceRequest
{
    public required string SequenceType { get; set; }
    public required string Prefix { get; set; }
    public int StartNumber { get; set; } = 1;
    public int IncrementBy { get; set; } = 1;
    public int MinDigits { get; set; } = 4;
    public string? Suffix { get; set; }
    public bool IncludeDate { get; set; } = true;
    public string DateFormat { get; set; } = "yyyyMMdd";
    public ResetFrequency ResetFrequency { get; set; } = ResetFrequency.Never;
}

public class UpdateNumberSequenceRequest
{
    public required string Prefix { get; set; }
    public int IncrementBy { get; set; }
    public int MinDigits { get; set; }
    public string? Suffix { get; set; }
    public bool IncludeDate { get; set; }
    public string DateFormat { get; set; } = "yyyyMMdd";
    public ResetFrequency ResetFrequency { get; set; }
}

#endregion

#region System Setting DTOs

public class SystemSettingDto
{
    public Guid Id { get; set; }
    public required string Category { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public SettingValueType ValueType { get; set; }
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateSystemSettingRequest
{
    public required string Category { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public SettingValueType ValueType { get; set; } = SettingValueType.String;
}

public class UpdateSystemSettingRequest
{
    public required string Value { get; set; }
    public string? Description { get; set; }
}

#endregion
