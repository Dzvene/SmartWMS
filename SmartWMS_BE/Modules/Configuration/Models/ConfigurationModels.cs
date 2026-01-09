using SmartWMS.API.Common.Models;

namespace SmartWMS.API.Modules.Configuration.Models;

/// <summary>
/// Barcode prefix configuration
/// </summary>
public class BarcodePrefix : TenantEntity
{
    public required string Prefix { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // What this prefix identifies
    public BarcodePrefixType PrefixType { get; set; }

    // Format
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; } // Regex pattern

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}

/// <summary>
/// Reason code for various operations
/// </summary>
public class ReasonCode : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // What type of operation this reason is for
    public ReasonCodeType ReasonType { get; set; }

    // Settings
    public bool RequiresNotes { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public bool AffectsInventory { get; set; } = true;

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public string? Notes { get; set; }
}

/// <summary>
/// Document number sequence configuration
/// </summary>
public class NumberSequence : TenantEntity
{
    public required string SequenceType { get; set; } // SO, PO, GR, etc.
    public required string Prefix { get; set; }
    public int CurrentNumber { get; set; } = 0;
    public int IncrementBy { get; set; } = 1;
    public int MinDigits { get; set; } = 4;
    public string? Suffix { get; set; }
    public bool IncludeDate { get; set; } = true;
    public string DateFormat { get; set; } = "yyyyMMdd";
    public DateTime? LastResetDate { get; set; }
    public ResetFrequency ResetFrequency { get; set; } = ResetFrequency.Never;
}

/// <summary>
/// System setting
/// </summary>
public class SystemSetting : TenantEntity
{
    public required string Category { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
    public string? Description { get; set; }
    public SettingValueType ValueType { get; set; } = SettingValueType.String;
    public bool IsSystem { get; set; } = false; // System settings can't be deleted
}

#region Enums

public enum BarcodePrefixType
{
    Product,
    Location,
    Bin,
    Pallet,
    Container,
    User,
    Equipment,
    Document,
    Other
}

public enum ReasonCodeType
{
    StockAdjustment,
    Return,
    Damage,
    Scrap,
    Transfer,
    CycleCount,
    OrderCancellation,
    Receiving,
    Shipping,
    Other
}

public enum ResetFrequency
{
    Never,
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public enum SettingValueType
{
    String,
    Integer,
    Decimal,
    Boolean,
    DateTime,
    Json
}

#endregion
